using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Globalization;
using System.Threading.Tasks;
using Xunit;
using System.Linq;

namespace MetalsApiTestsProject
{
    public class MetalsApiTests : IDisposable
    {
        private readonly HttpClient _httpClient;
        private const string Token = "52148956-5625-417d-88ca-0f6486edc857"; 
        private const string BaseUrl = "https://api.nfusionsolutions.biz/api/v1/Metals/spot/summary";

        public MetalsApiTests()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        private HttpRequestMessage CreateRequest()
        {
            var urlWithParams = $"{BaseUrl}?metals=gold,silver,platinum&token={Token}";
            return new HttpRequestMessage(HttpMethod.Get, urlWithParams);
        }

        [Fact]
        public async Task GetMetalsSummary_ReturnsSuccessStatusCode()
        {
            var request = CreateRequest();
            var response = await _httpClient.SendAsync(request);
            string responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Actual status code: {(int)response.StatusCode}");
            Console.WriteLine("API Response: " + responseBody);

            Assert.True(response.IsSuccessStatusCode, $"Expected success but got {(int)response.StatusCode} - Response: {responseBody}");
        }

        [Fact]
        public async Task GetMetalsSummary_ReturnsValidJson()
        {
            var request = CreateRequest();
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
        public async Task GetMetalsSummary_TimestampsAreValid()
        {
            var request = CreateRequest();
            var response = await _httpClient.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode, $" API request failed with status code {(int)response.StatusCode}");
            
            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine("API Response: " + responseBody);
            var json = JsonDocument.Parse(responseBody);
            
            foreach (var item in json.RootElement.EnumerateArray())
            {
                if (item.TryGetProperty("data", out JsonElement dataElement))
                {
                    if (dataElement.TryGetProperty("timeStamp", out JsonElement timestampElement) && timestampElement.ValueKind == JsonValueKind.String)
                    {
                        string? timestampStr = timestampElement.GetString();
                        Assert.False(string.IsNullOrWhiteSpace(timestampStr), " 'timeStamp' field is empty.");

                        bool isValidDate = DateTime.TryParse(timestampStr, null, DateTimeStyles.RoundtripKind, out DateTime parsedDate);
                        Assert.True(isValidDate, $" Invalid timestamp format: {timestampStr}");

                        Assert.True(parsedDate <= DateTime.UtcNow, $" Timestamp {parsedDate} is in the future.");
                    }
                    else
                    {
                        Assert.Fail(" 'timeStamp' field is missing or not a string inside 'data'.");
                    }
                }
                else
                {
                    Assert.Fail(" 'data' object is missing in API response.");
                }
            }
        }


        //Added more tests here for the Metals API

        [Fact]
        public async Task GetMetalsSummary_SymbolsAreValid()
        {
            var request = CreateRequest();
            var response = await _httpClient.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode, $" API request failed with status code {(int)response.StatusCode}");

            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine("API Response: " + responseBody);
            var json = JsonDocument.Parse(responseBody);

            foreach (var item in json.RootElement.EnumerateArray())
            {
                if (item.TryGetProperty("data", out JsonElement dataElement) && dataElement.TryGetProperty("symbol", out JsonElement symbolElement))
                {
                    string? symbol = symbolElement.GetString();
                    Assert.False(string.IsNullOrWhiteSpace(symbol), " 'symbol' field is empty or missing.");
                }
                else
                {
                    Assert.Fail(" 'symbol' field is missing in 'data' object.");
                }
            }
        }

        [Fact]
        public async Task GetMetalsSummary_BidPriceIsLessThanAskPrice()
        {
            var request = CreateRequest();
            var response = await _httpClient.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode, $" API request failed with status code {(int)response.StatusCode}");

            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine("API Response: " + responseBody);
            var json = JsonDocument.Parse(responseBody);

            foreach (var item in json.RootElement.EnumerateArray())
            {
                if (item.TryGetProperty("data", out JsonElement dataElement))
                {
                    if (dataElement.TryGetProperty("bid", out JsonElement bidElement) &&
                        dataElement.TryGetProperty("ask", out JsonElement askElement) &&
                        bidElement.ValueKind == JsonValueKind.Number &&
                        askElement.ValueKind == JsonValueKind.Number)
                    {
                        double bid = bidElement.GetDouble();
                        double ask = askElement.GetDouble();
                        Assert.True(bid < ask, $" Bid price ({bid}) is not less than ask price ({ask}).");
                    }
                    else
                    {
                        Assert.Fail(" 'bid' or 'ask' field is missing or not a valid number in 'data'.");
                    }
                }
                else
                {
                    Assert.Fail(" 'data' object is missing in API response.");
                }
            }
        }

        [Fact]
        public async Task GetMetalsSummary_ResponseTimeUnder2Seconds()
        {
            var request = CreateRequest();
            var startTime = DateTime.UtcNow;
            var response = await _httpClient.SendAsync(request);
            var duration = DateTime.UtcNow - startTime;

            Console.WriteLine($"Response time: {duration.TotalSeconds} seconds");
            Assert.True(duration.TotalSeconds < 2, $"Response time exceeded: {duration.TotalSeconds} seconds");
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}