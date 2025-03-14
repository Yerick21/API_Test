using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace MetalsApiTestsProject
{
    public class MetalsApiTests : IDisposable
    {
        private readonly HttpClient _httpClient;
        private const string Token = "52148956-5625-417d-88ca-0f6486edc857"; 
        private const string BaseUrl = "https://api.nfusionsolutions.biz/api/v1/Metals/spot/summary";

        public MetalsApiTests()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5) // Prevent hanging requests
            };
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        private HttpRequestMessage CreateRequest(string baseUrl)
        {
            // Append token as a query parameter
            var urlWithParams = $"{baseUrl}?metals=gold,silver,platinum&token={Token}";
            return new HttpRequestMessage(HttpMethod.Get, urlWithParams);
        }

        [Fact]
        public async Task GetMetalsSummary_ReturnsSuccessStatusCode()
        {
            var request = CreateRequest(BaseUrl);
            var response = await _httpClient.SendAsync(request);
            string responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Actual status code: {(int)response.StatusCode}");
            Console.WriteLine("API Response: " + responseBody);

            Assert.True(response.IsSuccessStatusCode, $"Expected success but got {(int)response.StatusCode} - Response: {responseBody}");
        }

        [Fact]
        public async Task GetMetalsSummary_ReturnsValidJson()
        {
            var request = CreateRequest(BaseUrl);
            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine("API Response: " + responseBody);
            Assert.False(string.IsNullOrWhiteSpace(responseBody));

            try
            {
                JsonDocument.Parse(responseBody);
            }
            catch (JsonException)
            {
                Assert.True(false, "Response is not valid JSON");
            }
        }


        [Fact]
        public async Task GetMetalsSummary_InvalidApiKey_ShouldReturnUnauthorized()
        {
            var invalidToken = "invalid-token";
            var invalidUrl = $"{BaseUrl}?metals=gold,silver,platinum&token={invalidToken}";

            var request = new HttpRequestMessage(HttpMethod.Get, invalidUrl);
            request.Headers.Add("Accept", "application/json");

            var response = await _httpClient.SendAsync(request);
            string responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Invalid API Key Test - Status Code: {(int)response.StatusCode}");
            Console.WriteLine("API Response: " + responseBody);

            Assert.Contains(response.StatusCode, new[] { HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest });
        }

        [Fact]
        public async Task GetMetalsSummary_ResponseTimeUnder2Seconds()
        {
            var request = CreateRequest(BaseUrl);
            var startTime = DateTime.UtcNow;
            var response = await _httpClient.SendAsync(request);
            var duration = DateTime.UtcNow - startTime;

            Console.WriteLine($"Response time: {duration.TotalSeconds} seconds");
            Assert.True(duration.TotalSeconds < 2, $"Response time exceeded: {duration.TotalSeconds} seconds");
        }

        [Fact]
        public async Task GetMetalsSummary_HandlesMultipleConcurrentRequests()
        {
            var tasks = new List<Task<HttpResponseMessage>>();
            int failureCount = 0;

            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(100);
                var request = CreateRequest(BaseUrl);
                tasks.Add(_httpClient.SendAsync(request));
            }

            var responses = await Task.WhenAll(tasks);

            foreach (var response in responses)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Parallel request returned: {(int)response.StatusCode} - Response: {responseBody}");

                if (!response.IsSuccessStatusCode)
                {
                    failureCount++;
                    if (failureCount > 3)
                    {
                        Assert.True(false, "More than 3 parallel requests failed.");
                    }

                    Console.WriteLine("Retrying failed request...");
                    await Task.Delay(500);
                    var retryResponse = await _httpClient.SendAsync(CreateRequest(BaseUrl));
                    Console.WriteLine($"Retry response status: {(int)retryResponse.StatusCode}");

                    Assert.True(retryResponse.IsSuccessStatusCode,
                        $"One request failed even after retry. Final status: {(int)retryResponse.StatusCode}");
                }
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}