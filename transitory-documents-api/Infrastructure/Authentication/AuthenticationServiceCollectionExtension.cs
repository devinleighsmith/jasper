using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using PCSSCommon.Clients.AuthorizationServices;
using PCSSCommon.Models;
using Scv.Api.Helpers;
using Scv.Api.Helpers.Extensions;
using Scv.Api.Infrastructure.Encryption;
using Scv.Api.Models.AccessControlManagement;
using Scv.Api.Services;
using Scv.Db.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ApiException = Scv.Api.Services.ApiException;

namespace Scv.Api.Infrastructure.Authentication
{
    public static class AuthenticationServiceCollectionExtension
    {
        public static IServiceCollection AddScvAuthentication(this IServiceCollection services,
            IWebHostEnvironment env, IConfiguration configuration, string scheme)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = scheme ?? CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = scheme ?? CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = scheme ?? CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var key = Encoding.ASCII.GetBytes(configuration.GetNonEmptyValue("Keycloak:Secret"));
                options.Authority = configuration.GetNonEmptyValue("Keycloak:Authority");
                options.Audience = configuration.GetNonEmptyValue("Keycloak:Audience");
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.FromSeconds(5)
                };
                if (key.Length > 0) options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(key);
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        context.NoResult();
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    },
                    OnForbidden = context =>
                    {
                        context.NoResult();
                        context.Response.StatusCode = 403;
                        return Task.CompletedTask;
                    }
                };
            })
            .AddScheme<AuthenticationSchemeOptions, SiteMinderAuthenticationHandler>(
                SiteMinderAuthenticationHandler.SiteMinder, null);

            return services;
        }

        private static async Task OnPostAuthSuccess(
            IConfiguration configuration,
            Microsoft.AspNetCore.Authentication.OpenIdConnect.TokenValidatedContext context,
            ILogger logger,
            List<Claim> claims)
        {
            var judgeId = configuration.GetNonEmptyValue("PCSS:JudgeId");
            var homeLocationId = configuration.GetNonEmptyValue("PCSS:JudgeHomeLocationId");
            PersonSearchItem judge = null;
            try
            {
                if (int.TryParse(context.Principal.ExternalJudgeId(), out int externalJudgeId))
                {
                    judge = await GetJudgeById(context, externalJudgeId);
                }

                if (IsMongoDbConfigured(configuration))
                {
                    var userDto = await HandleMongoDbUser(context, logger, claims);

                    if (userDto?.JudgeId.HasValue == true)
                    {
                        judge = (await GetJudgeById(context, userDto.JudgeId.Value)) ?? judge; // Attempt to override the default Judge Id and HomeLocationId if the current user has been mapped.
                    }
                }
            }
            finally
            {
                judgeId = judge?.PersonId.ToString() ?? judgeId;
                homeLocationId = judge?.HomeLocationId.ToString() ?? homeLocationId;
                AddDefaultJudgeClaims(logger, claims, judgeId, homeLocationId);
            }
        }

        private static async Task<UserDto> HandleMongoDbUser(
            Microsoft.AspNetCore.Authentication.OpenIdConnect.TokenValidatedContext context,
            ILogger logger,
            List<Claim> claims)
        {
            UserDto userDto = null;
            try
            {
                var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                userDto = await userService.GetWithPermissionsAsync(context.Principal.Email());
                if (userDto == null)
                {
                    var newUser = await BuildNewUser(context, logger);
                    var result = await userService.AddAsync(newUser);
                    if (result.Payload == null || result.Errors.Any())
                    {
                        logger.LogError("Error creating new user in SCV database: {Errors}", string.Join(", ", result.Errors));
                        return null;
                    }
                    userDto = await userService.GetByIdWithPermissionsAsync(result.Payload.Id); // re-fetch to get permissions and roles;
                }
                AddUserClaims(claims, userDto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Something went wrong during post authentication process: {Exception}", ex);
            }
            return userDto;
        }

        private static async Task<UserDto> BuildNewUser(
            Microsoft.AspNetCore.Authentication.OpenIdConnect.TokenValidatedContext context,
            ILogger logger)
        {
            UserDto userDto = new()
            {
                FirstName = context.Principal.FindFirstValue(ClaimTypes.GivenName),
                LastName = context.Principal.FindFirstValue(ClaimTypes.Surname),
                Email = context.Principal.Email(),
                UserGuid = context.Principal.UserGuid(),
                IsActive = false,
            };

            var pcssAuthServiceClient = context.HttpContext.RequestServices.GetRequiredService<AuthorizationServicesClient>();
            var authorizationService = context.HttpContext.RequestServices.GetRequiredService<AuthorizationService>();
            var groupService = context.HttpContext.RequestServices.GetRequiredService<IGroupService>();

            UserItem matchingUser = null;
            if(context.Principal.UserGuid() == null)
            {
                logger.LogInformation("No GUID claim found for user with email {Email}. Cannot look up user in PCSS.", userDto.Email);
                return userDto;
            }
            try
            {
                logger.LogInformation("Requesting user information from PCSS."); //TODO: only do this for ProvJud users.
                var pcssUsers = await pcssAuthServiceClient.GetUsersAsync();
                matchingUser = pcssUsers?.FirstOrDefault(u => u.GUID == context.Principal.UserGuid());
                logger.LogDebug("PCSS user lookup by GUID {UserGuid} returned: {MatchingUser}",
                    context.Principal.UserGuid(),
                    matchingUser != null ? JsonConvert.SerializeObject(matchingUser) : "No match");

                if (matchingUser == null || !matchingUser.UserId.HasValue)
                {
                    return userDto;
                }

                var roleNameResult = await authorizationService.GetPcssUserRoleNames(matchingUser.UserId.Value);
                if (roleNameResult == null || roleNameResult.Errors.Any())
                {
                    return userDto;
                }

                var groupResult = await groupService.GetGroupsByAliases(roleNameResult.Payload);
                if (groupResult == null || groupResult.Errors.Any())
                {
                    return userDto;
                }

                var groupIds = groupResult.Payload.Select(g => g.Id).ToList();
                userDto.GroupIds = groupIds;
                userDto.IsActive = groupIds.Count > 0;

            }
            catch (ApiException e)
            {
                logger.LogError(e, "Unable to get user or groups from PCSS.");
            }

            return userDto;
        }

        private static async Task<PersonSearchItem> GetJudgeById(Microsoft.AspNetCore.Authentication.OpenIdConnect.TokenValidatedContext context, int judgeId)
        {
            var dashboardService = context.HttpContext.RequestServices.GetRequiredService<IDashboardService>();
            var judges = await dashboardService.GetJudges();

            return judges.FirstOrDefault(j => j.PersonId == judgeId);
        }

        private static bool IsMongoDbConfigured(IConfiguration configuration)
        {
            var connectionString = configuration.GetValue<string>("MONGODB_CONNECTION_STRING");
            return !string.IsNullOrWhiteSpace(connectionString);
        }

        private static void AddDefaultJudgeClaims(ILogger logger, List<Claim> claims, string judgeId, string homeLocationId)
        {
            logger.LogInformation("Acting as Judge Id - {JudgeId} with Home Location Id - {HomeLocationId}.", judgeId, homeLocationId);
            claims.Add(new Claim(CustomClaimTypes.JudgeId, judgeId));
            claims.Add(new Claim(CustomClaimTypes.JudgeHomeLocationId, homeLocationId));
        }

        private static void AddUserClaims(List<Claim> claims, UserDto userDto)
        {
            claims.Add(new Claim(CustomClaimTypes.UserId, userDto.Id));
            claims.Add(new Claim(CustomClaimTypes.IsActive, userDto.IsActive.ToString()));

            claims.AddRange(userDto.Permissions.Select(p => new Claim(CustomClaimTypes.Permission, p)));
            claims.AddRange(userDto.Roles.Select(r => new Claim(CustomClaimTypes.Role, r)));
        }
    }
}
