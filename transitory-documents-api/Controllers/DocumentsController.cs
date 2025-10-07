using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Scv.Api.Helpers;
using Scv.Api.Helpers.Extensions;
using Scv.Api.Infrastructure.Authorization;
using Scv.Api.Infrastructure.Encryption;
using Scv.Api.Models.auth;
using Scv.Db.Models;
using Scv.Db.Models.Auth;
using Scv.TdApi.Infrastructure.Authorization;

namespace Scv.TdApi.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer", Policy = nameof(TDProviderAuthorizationHandler))]
    [Route("api/[controller]")]
    public class DocumentsController(IConfiguration configuration) : ControllerBase
    {
        public IConfiguration Configuration { get; } = configuration;

        [HttpGet("test")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public ActionResult<string> Test()
        {
            var userGuid = HttpContext.User.UserGuid;
            return $"P: drive access granted to user: {HttpContext.User.PreferredUsername()}";
        }
    }
}
