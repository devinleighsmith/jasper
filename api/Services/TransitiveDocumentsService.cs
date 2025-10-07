using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DnsClient.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Scv.Api.Services
{
    public class TransitiveDocumentsService
    {
        private readonly HttpClient _httpClient;
        private readonly Lazy<JsonSerializerOptions> _jsonSerializerOptions;
        private ILogger<TransitiveDocumentsService> _logger;

        public TransitiveDocumentsService(HttpClient httpClient, ILogger<TransitiveDocumentsService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _jsonSerializerOptions = new Lazy<JsonSerializerOptions>(CreateJsonSerializerOptions);
            _logger = logger;
        }

        private JsonSerializerOptions CreateJsonSerializerOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            return options;
        }

        public JsonSerializerOptions JsonSerializerOptions => _jsonSerializerOptions.Value;

        /// <summary>
        /// Calls the Test endpoint in the Transitive Documents API.
        /// </summary>
        /// <param name="bearerToken">The bearer token for authentication.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The response from the Test endpoint.</returns>
        /// <exception cref="ApiException">A server-side error occurred.</exception>
        public async Task<string> CallTestEndpointAsync(string bearerToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(bearerToken))
            {
                throw new ArgumentException("Bearer token must not be null or empty.", nameof(bearerToken));
            }

            var urlBuilder = new Uri("api/documents/test");

            using var request = new HttpRequestMessage(HttpMethod.Get, urlBuilder);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            var headers = response.Headers;

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to query transitive documents, {StatusCode}", response.StatusCode);
            }

            var responseStream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<string>(responseStream, JsonSerializerOptions, cancellationToken);
        }
    }

    public class ApiException : Exception
    {
        public int StatusCode { get; }
        public string Response { get; }
        public System.Net.Http.Headers.HttpResponseHeaders Headers { get; }

        public ApiException(string message, int statusCode, string response, System.Net.Http.Headers.HttpResponseHeaders headers, Exception innerException)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            Response = response;
            Headers = headers;
        }

        public override string ToString()
        {
            return $"{base.ToString()}\n\nStatusCode: {StatusCode}\nResponse: {Response}";
        }
    }
}