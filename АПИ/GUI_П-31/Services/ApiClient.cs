using GuiP31.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GuiP31.Services
{
    internal static class ApiClient
    {
        private static readonly HttpClient HttpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5000/")
        };

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public static async Task<T?> GetAsync<T>(string url)
        {
            using var request = CreateRequest(HttpMethod.Get, url);
            using var response = await HttpClient.SendAsync(request);
            return await ReadResponse<T>(response);
        }

        public static async Task<TResponse?> SendAsync<TRequest, TResponse>(HttpMethod method, string url, TRequest payload)
        {
            using var request = CreateRequest(method, url);
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            using var response = await HttpClient.SendAsync(request);
            return await ReadResponse<TResponse>(response);
        }

        public static async Task SendAsync<TRequest>(HttpMethod method, string url, TRequest payload)
        {
            using var request = CreateRequest(method, url);
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            using var response = await HttpClient.SendAsync(request);
            await EnsureSuccess(response);
        }

        public static async Task DeleteAsync(string url)
        {
            using var request = CreateRequest(HttpMethod.Delete, url);
            using var response = await HttpClient.SendAsync(request);
            await EnsureSuccess(response);
        }

        private static HttpRequestMessage CreateRequest(HttpMethod method, string url)
        {
            var request = new HttpRequestMessage(method, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (SessionContext.CurrentUser != null)
            {
                request.Headers.Add("X-User-Id", SessionContext.CurrentUser.UserId.ToString());
                request.Headers.Add("X-User-Role", SessionContext.CurrentUser.Role);
            }

            return request;
        }

        private static async Task<T?> ReadResponse<T>(HttpResponseMessage response)
        {
            await EnsureSuccess(response);
            var json = await response.Content.ReadAsStringAsync();
            return string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json, JsonOptions);
        }

        private static async Task EnsureSuccess(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var content = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(content))
            {
                try
                {
                    var error = JsonSerializer.Deserialize<ApiErrorResponse>(content, JsonOptions);
                    if (error != null && !string.IsNullOrWhiteSpace(error.Message))
                    {
                        var details = string.IsNullOrWhiteSpace(error.Details) ? string.Empty : Environment.NewLine + error.Details;
                        throw new InvalidOperationException(error.Message + details);
                    }
                }
                catch (JsonException)
                {
                }
            }

            throw new InvalidOperationException($"Ошибка API: {(int)response.StatusCode}");
        }
    }
}
