using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Scv.Api.Infrastructure.Authorization;
using Scv.Api.Models.Location;
using Scv.Api.Services;
using TDCommon.Clients;
using TDCommon.Clients.DocumentsServices;

namespace Scv.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransitoryDocumentsController(TransitoryDocumentsClient client) : ControllerBase
    {

        [HttpGet]
        public async Task<ActionResult> GetDocuments()
        {
            var user = HttpContext.User;
            var bearerToken = await HttpContext.GetTokenAsync("access_token");

            client.SetBearerToken(bearerToken);
            var result = await client.TestAsync();

            return Ok(result);
        }
    }
}