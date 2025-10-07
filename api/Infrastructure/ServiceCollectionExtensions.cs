﻿using System;
using System.Net.Http;
using System.Reflection;
using GdPicture14;
using Hangfire;
using Hangfire.PostgreSql;
using JCCommon.Clients.FileServices;
using JCCommon.Clients.LocationServices;
using JCCommon.Clients.LookupCodeServices;
using JCCommon.Clients.UserService;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Scv.Api.Documents;
using Scv.Api.Documents.Parsers;
using Scv.Api.Documents.Strategies;
using Scv.Api.Helpers;
using Scv.Api.Helpers.Extensions;
using Scv.Api.Infrastructure.Authorization;
using Scv.Api.Infrastructure.Encryption;
using Scv.Api.Infrastructure.Handler;
using Scv.Api.Jobs;
using Scv.Api.Models;
using Scv.Api.Models.AccessControlManagement;
using Scv.Api.Processors;
using Scv.Api.Services;
using Scv.Api.Services.Files;
using Scv.Db.Contexts;
using Scv.Db.Repositories;
using Scv.Db.Seeders;
using BasicAuthenticationHeaderValue = JCCommon.Framework.BasicAuthenticationHeaderValue;
using PCSSConfigServices = PCSSCommon.Clients.ConfigurationServices;
using PCSSCourtCalendarServices = PCSSCommon.Clients.CourtCalendarServices;
using PCSSFileDetailServices = PCSSCommon.Clients.FileDetailServices;
using PCSSJudicialCalendarServices = PCSSCommon.Clients.JudicialCalendarServices;
using PCSSLocationServices = PCSSCommon.Clients.LocationServices;
using PCSSLookupServices = PCSSCommon.Clients.LookupServices;
using PCSSPersonServices = PCSSCommon.Clients.PersonServices;
using PCSSReportServices = PCSSCommon.Clients.ReportServices;
using PCSSSearchDateServices = PCSSCommon.Clients.SearchDateServices;
using PCSSAuthorizationServices = PCSSCommon.Clients.AuthorizationServices;
using TdDocumentsServices = TDCommon.Clients.DocumentsServices;

namespace Scv.Api.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        const string X_APIGW_KEY_HEADER = "x-api-key";
        const string X_ORIGIN_VERIFY_HEADER = "x-origin-verify";
        const string X_TARGET_APP = "x-target-app";

        public static void AddNutrient(this IServiceCollection services)
        {
            LicenseManager licenseManager = new();
            // For now, use the trial license.
            // https://www.nutrient.io/sdk/dotnet/getting-started/integrate/#activating-the-trial-license
            licenseManager.RegisterKEY("");

            services.AddScoped<IDocumentMerger, DocumentMerger>();
            services.AddScoped<IDocumentRetriever, DocumentRetriever>();
            services.AddScoped<IDocumentConverter, DocumentConverter>();
            services.AddScoped<IDocumentStrategy, FileStrategy>();
            services.AddScoped<IDocumentStrategy, ROPStrategy>();
            services.AddScoped<IDocumentStrategy, ReportStrategy>();
            services.AddScoped<IDocumentStrategy, CourtSummaryReportStrategy>();
        }

        public static IServiceCollection AddMapster(this IServiceCollection services, Action<TypeAdapterConfig> options = null)
        {
            var config = TypeAdapterConfig.GlobalSettings;
            config.Scan(Assembly.GetAssembly(typeof(Startup)) ?? throw new InvalidOperationException());

            options?.Invoke(config);

            services.AddSingleton(config);
            services.AddScoped<IMapper>(sp => new Mapper(sp.GetRequiredService<TypeAdapterConfig>()));

            return services;
        }

        public static IServiceCollection AddJasperDb(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetValue<string>("MONGODB_CONNECTION_STRING");
            var dbName = configuration.GetValue<string>("MONGODB_NAME");

            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(dbName))
            {
                return services;
            }

            services.AddSingleton<IMongoClient>(m =>
            {
                var logger = m.GetRequiredService<ILogger<MongoClient>>();
                var settings = MongoClientSettings.FromConnectionString(connectionString);
                var client = new MongoClient(settings);

                try
                {
                    var database = client.GetDatabase(dbName);
                    database.RunCommand<BsonDocument>(new BsonDocument("ping", 1));
                    logger.LogInformation("MongoDB connection established successfully.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to connect to MongoDB.");
                    throw new InvalidOperationException("Failed to establish MongoDB connection.", ex);
                }

                return client;

            });
            services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(dbName));

            services.AddScoped<PermissionSeeder>();
            services.AddScoped<RoleSeeder>();
            services.AddScoped<GroupSeeder>();
            services.AddScoped<UserSeeder>();
            services.AddScoped<GroupAliasSeeder>();

            services.AddDbContext<JasperDbContext>((serviceProvider, options) =>
            {
                var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
                options.UseMongoDB(mongoClient, dbName);
            });

            services.AddTransient<SeederFactory<JasperDbContext>>();

            services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));
            services.AddScoped<IPermissionRepository, PermissionRepository>();

            return services;
        }

        public static IServiceCollection AddHttpClientsAndScvServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<TimingHandler>();

            // JC Interface
            services
                .AddHttpClient<FileServicesClient>(client => { ConfigureHttpClient(client, configuration, "FileServicesClient", 300); })
                .AddHttpMessageHandler<TimingHandler>();
            services
                .AddHttpClient<LookupCodeServicesClient>(client => { ConfigureHttpClient(client, configuration, "LookupServicesClient"); })
                .AddHttpMessageHandler<TimingHandler>();
            services
                .AddHttpClient<LocationServicesClient>(client => { ConfigureHttpClient(client, configuration, "LocationServicesClient"); })
                .AddHttpMessageHandler<TimingHandler>();
            services
                .AddHttpClient<UserServiceClient>(client => { ConfigureHttpClient(client, configuration, "UserServicesClient"); })
                .AddHttpMessageHandler<TimingHandler>();

            // PCSS
            services
                .AddHttpClient<PCSSLocationServices.LocationServicesClient>(
                    typeof(PCSSLocationServices.LocationServicesClient).FullName,
                    (client) => { ConfigureHttpClient(client, configuration, "PCSS"); })
                .AddHttpMessageHandler<TimingHandler>();
            services
                .AddHttpClient<PCSSCourtCalendarServices.CourtCalendarClientServicesClient>(client => { ConfigureHttpClient(client, configuration, "PCSS"); })
                .AddHttpMessageHandler<TimingHandler>();
            services
                .AddHttpClient<PCSSJudicialCalendarServices.JudicialCalendarServicesClient>(client => { ConfigureHttpClient(client, configuration, "PCSS"); })
                .AddHttpMessageHandler<TimingHandler>();
            services
                .AddHttpClient<PCSSSearchDateServices.SearchDateClient>(client => { ConfigureHttpClient(client, configuration, "PCSS"); })
                .AddHttpMessageHandler<TimingHandler>();
            services
                .AddHttpClient<PCSSFileDetailServices.FileDetailClient>(client => { ConfigureHttpClient(client, configuration, "PCSS"); })
                .AddHttpMessageHandler<TimingHandler>();
            services
                .AddHttpClient<PCSSLookupServices.LookupServicesClient>(client => { ConfigureHttpClient(client, configuration, "PCSS"); })
                .AddHttpMessageHandler<TimingHandler>();
            services
                .AddHttpClient<PCSSReportServices.ReportServicesClient>(client => { ConfigureHttpClient(client, configuration, "PCSS"); })
                .AddHttpMessageHandler<TimingHandler>();
            services
                .AddHttpClient<PCSSConfigServices.ConfigurationServicesClient>(client => { ConfigureHttpClient(client, configuration, "PCSS"); })
                .AddHttpMessageHandler<TimingHandler>();
            services
                .AddHttpClient<PCSSPersonServices.PersonServicesClient>(client => { ConfigureHttpClient(client, configuration, "PCSS"); })
                .AddHttpMessageHandler<TimingHandler>();
            services
                .AddHttpClient<PCSSAuthorizationServices.AuthorizationServicesClient>(client => { ConfigureHttpClient(client, configuration, "PCSS"); })
                .AddHttpMessageHandler<TimingHandler>();

            // Transitory Documents
            services
                .AddHttpClient<TdDocumentsServices.TransitoryDocumentsClient>(client => { ConfigureHttpClient(client, configuration, "TD"); })
                .AddHttpMessageHandler<TimingHandler>();

            services.AddHttpContextAccessor();
            services.AddTransient(s => s.GetService<IHttpContextAccessor>().HttpContext.User);
            services.AddScoped<FilesService>();
            services.AddScoped<LookupService>();
            services.AddScoped<LocationService>();
            services.AddScoped<AuthorizationService>();
            services.AddScoped<CourtListService>();
            services.AddScoped<VcCivilFileAccessHandler>();
            services.AddSingleton<JCUserService>();
            services.AddSingleton<AesGcmEncryption>();
            services.AddSingleton<JudicialCalendarService>();

            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IDocumentCategoryService, DocumentCategoryService>();
            services.AddScoped<ICsvParser, CsvParser>();

            var connectionString = configuration.GetValue<string>("MONGODB_CONNECTION_STRING");
            if (!string.IsNullOrEmpty(connectionString))
            {
                services.AddScoped<ICrudService<PermissionDto>, PermissionService>();
                services.AddScoped<ICrudService<RoleDto>, RoleService>();
                services.AddScoped<ICrudService<GroupDto>, GroupService>();
                services.AddScoped<ICrudService<ReservedJudgementDto>, ReservedJudgementService>();
                services.AddScoped<IUserService, UserService>();
                services.AddScoped<IBinderFactory, BinderFactory>();
                services.AddScoped<IBinderService, BinderService>();
                services.AddScoped<IGroupService, GroupService>();
                services.AddTransient<IRecurringJob, SyncDocumentCategoriesJob>();
            }

            return services;
        }

        public static IServiceCollection AddHangfire(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetValue<string>("DatabaseConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                return services;
            }

            services.AddHangfire(config => config
                .UsePostgreSqlStorage(c => c
                    .UseNpgsqlConnection(connectionString), new PostgreSqlStorageOptions
                    {
                        SchemaName = "hangfire"
                    }));
            services.AddHangfireServer();
            return services;
        }

        public static void ConfigureHttpClient(HttpClient client, IConfiguration configuration, string prefix, int timeoutInSecs = 100)
        {
            var apigwUrl = configuration.GetValue<string>("AWS_API_GATEWAY_URL");
            var apigwKey = configuration.GetValue<string>("AWS_API_GATEWAY_API_KEY");
            var authorizerKey = configuration.GetValue<string>("AuthorizerKey");

            client.Timeout = TimeSpan.FromSeconds(timeoutInSecs);

            // Defaults to BC Gov API if any config setting is missing
            if (string.IsNullOrWhiteSpace(apigwUrl) || string.IsNullOrWhiteSpace(apigwKey) || string.IsNullOrWhiteSpace(authorizerKey))
            {
                Console.WriteLine($"Redirecting traffic to: {configuration.GetNonEmptyValue($"{prefix}:Url")}");
                client.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(
                    configuration.GetNonEmptyValue($"{prefix}:Username"),
                    configuration.GetNonEmptyValue($"{prefix}:Password"));
                client.BaseAddress = new Uri(configuration.GetNonEmptyValue($"{prefix}:Url").EnsureEndingForwardSlash());
            }
            // Requests are routed to JASPER's API Gateway. Lambda functions are triggered by these requests and are responsible for communicating with the BC Gov API.
            else
            {
                Console.WriteLine($"Redirecting traffic to: {apigwUrl}");
                client.BaseAddress = new Uri(apigwUrl.EnsureEndingForwardSlash());
                client.DefaultRequestHeaders.Add(X_APIGW_KEY_HEADER, apigwKey);
                client.DefaultRequestHeaders.Add(X_ORIGIN_VERIFY_HEADER, authorizerKey);
                // The prefix will help determine where will the request is routed (e.g. lookup, CatsAPI or DARS)
                client.DefaultRequestHeaders.Add(X_TARGET_APP, prefix);
            }
        }
    }
}