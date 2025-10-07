using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Scv.Api.Controllers;
using Scv.Api.Helpers.Extensions;
using Scv.Api.Infrastructure.Authentication;
using Scv.Api.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Scv.TdApi.Infrastructure.Authorization.TDProviderAuthorizationHandler;

namespace Scv.TdApi.Infrastructure.Authorization
{
    public class TDProviderAuthorizationHandler : AuthorizationHandler<TdProviderRequirement>, IAuthorizationRequirement
    {
        public sealed class TdProviderRequirement : IAuthorizationRequirement
        {
            public TdProviderRequirement() { }
        }

        /// <summary>
        /// </summary>
        protected async override Task HandleRequirementAsync(AuthorizationHandlerContext context, TdProviderRequirement requirement)
        {
            var user = context.User;
            var httpContext = context.Resource as DefaultHttpContext;
            var endpoint = httpContext?.GetEndpoint();
            var actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

            context.Succeed(requirement);
        }
    }
}
