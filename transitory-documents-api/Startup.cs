using ColeSoft.Extensions.Logging.Splunk;
using FluentValidation;
using JCCommon.Clients.FileServices;
using JCCommon.Clients.UserService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Console;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Scv.Api.Helpers;
using Scv.Api.Helpers.ContractResolver;
using Scv.Api.Infrastructure;
using Scv.Api.Infrastructure.Authentication;
using Scv.Api.Infrastructure.Authorization;
using Scv.Api.Infrastructure.Encryption;
using Scv.Api.Infrastructure.Handler;
using Scv.Api.Infrastructure.Middleware;
using Scv.Api.Services;
using Scv.Db.Models;
using Scv.TdApi.Infrastructure.Authorization;
using Scv.TdApi.Infrastructure.Middleware;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using static Scv.Api.Infrastructure.Authorization.ProviderAuthorizationHandler;
using static Scv.TdApi.Infrastructure.Authorization.TDProviderAuthorizationHandler;

namespace Scv.TdApi
{
    public class Startup
    {
        private IWebHostEnvironment CurrentEnvironment { get; }

        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            Configuration = configuration;
            CurrentEnvironment = env;
        }

        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(options =>
            {
                options.Services.Configure<SimpleConsoleFormatterOptions>(c =>
                {
                    c.ColorBehavior = LoggerColorBehavior.Enabled;
                    c.TimestampFormat = "[yyyy-MM-dd HH:mm:ss.fff] ";
                });

                options.AddSplunk(c =>
                {
                    c.SplunkCollectorUrl = Configuration.GetNonEmptyValue("SplunkCollectorUrl");
                    c.AuthenticationToken = Configuration.GetNonEmptyValue("SplunkToken");
                });
            });

            services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthorizationRedirectMiddlewareResultHandler>();

            #region Cors

            string corsDomain = Configuration.GetValue<string>("CORS_DOMAIN");

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins(corsDomain);
                });
            });

            services.AddHealthChecks();

            #endregion Cors

            #region Setup Services

            services
                .AddHttpClient<UserServiceClient>(client => { Api.Infrastructure.ServiceCollectionExtensions.ConfigureHttpClient(client, Configuration, "UserServicesClient"); })
                .AddHttpMessageHandler<TimingHandler>();

            services.AddHttpContextAccessor();
            services.AddTransient(s => s.GetService<IHttpContextAccessor>().HttpContext.User);
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddSingleton<JCUserService>();

            #endregion Setup Services

            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

            #region Authentication & Authorization

            services.AddScoped<IAuthorizationHandler, PermissionHandler>();
            services.AddScvAuthentication(CurrentEnvironment, Configuration, JwtBearerDefaults.AuthenticationScheme);

            services.AddScoped<IAuthorizationHandler, TDProviderAuthorizationHandler>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(nameof(TDProviderAuthorizationHandler), policy =>
                    policy.Requirements.Add(new TdProviderRequirement()));
            });

            services.AddTransient<TimingHandler>();
            #endregion

            #region Newtonsoft

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new SafeContractResolver { NamingStrategy = new CamelCaseNamingStrategy() };
                options.SerializerSettings.Formatting = Formatting.Indented;
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });

            #endregion Newtonsoft

            #region Swagger

            services.AddSwaggerGen(options =>
            {
                var thisAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                options.EnableAnnotations(true, true);
                options.CustomSchemaIds(o => o.FullName);
                var xmlFile = $"{thisAssemblyName}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
                options.DocInclusionPredicate((docName, apiDesc) =>
                {
                    if (apiDesc.ActionDescriptor is ControllerActionDescriptor cad)
                    {
                        var controllerAsm = cad.ControllerTypeInfo.Assembly.GetName().Name;
                        return string.Equals(controllerAsm, thisAssemblyName, StringComparison.OrdinalIgnoreCase);
                    }

                    return false;
                });
            });

            services.AddSwaggerGenNewtonsoftSupport();

            #endregion Swagger

            services.AddLazyCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Use((context, next) =>
            {
                context.Request.EnableBuffering();
                context.Request.Scheme = "https";
                if (context.Request.Headers.ContainsKey("X-Forwarded-Host") && !env.IsDevelopment())
                {
                    var baseUrl = context.Request.Headers["X-Base-Href"].ToString();
                    context.Request.PathBase = new PathString(baseUrl.Remove(baseUrl.Length - 1));
                }
                return next();
            });

            app.UseForwardedHeaders();
            app.UseCors();

            app.UseSwagger(options =>
            {
                options.RouteTemplate = "api/swagger/{documentname}/swagger.json";
                options.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    if (!httpReq.Headers.ContainsKey("X-Forwarded-Host"))
                        return;

                    var forwardedHost = httpReq.Headers["X-Forwarded-Host"];
                    var forwardedPort = httpReq.Headers["X-Forwarded-Port"];
                    var baseUrl = httpReq.Headers["X-Base-Href"];
                    swaggerDoc.Servers = new List<OpenApiServer>
                    {
                        new OpenApiServer { Url = XForwardedForHelper.BuildUrlString(forwardedHost, forwardedPort, baseUrl) }
                    };
                });
            });

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("swagger/v1/swagger.json", "DOCUMENT_PROXY.API");
                options.RoutePrefix = "api";
            });

            app.UseMiddleware(typeof(TdErrorHandlingMiddleware));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // Move health check mapping inside UseEndpoints
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}