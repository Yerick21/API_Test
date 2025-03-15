# Metals API Test Project

This project contains **XUnit tests** for validating the Metals Spot Price API from **nFusion Solutions**. The tests ensure API reliability, response validation, and performance.

## 📌 Overview
- **Language:** C#
- **Framework:** .NET 6+/7+
- **Testing Framework:** XUnit
- **API Endpoint:** [Metals Spot Summary](https://api.nfusionsolutions.biz/api/v1/Metals/spot/summary)

## 🚀 Setup Instructions
### **Prerequisites**
- Install **.NET SDK** ([Download Here](https://dotnet.microsoft.com/en-us/download))
- Install **VS Code** or **Visual Studio**
- Ensure **XUnit dependencies** are installed:
  ```sh
  dotnet add package xunit
  dotnet add package Microsoft.NET.Test.Sdk
  dotnet add package xunit.runner.visualstudio
  ```

## 📋 Test Cases

| Test Name | Description |
|-----------|-------------|
| **GetMetalsSummary_ReturnsSuccessStatusCode** | Ensures the API returns a **200 OK** response. |
| **GetMetalsSummary_ReturnsValidJson** | Verifies that the response is **valid JSON** and can be parsed correctly. |
| **GetMetalsSummary_InvalidApiKey_ShouldReturnUnauthorized** | Tests if an **invalid API key** results in a **401 Unauthorized** or **400 Bad Request** error. |
| **GetMetalsSummary_ResponseTimeUnder2Seconds** | Checks if the API **responds within 2 seconds** to meet performance expectations. |
| **GetMetalsSummary_HandlesMultipleConcurrentRequests** | Ensures the API can handle **multiple parallel requests** without failures. |
| **GetMetalsSummary_TimestampsAreValid** | Ensures the `timeStamp` field inside `data` exists, is formatted correctly, and is **not in the future**. |
| **GetMetalsSummary_SymbolsAreValid** | Validates that the `symbol` field inside `data` exists and is **not empty**. |
| **GetMetalsSummary_BidPriceIsLessThanAskPrice** | Ensures **bid price is always less than the ask price**, and both values exist as valid numbers. |

## 🛠 Troubleshooting
- **Getting 401 Unauthorized?** Check if your API token is valid.
- **Slow API response?** Increase timeout in `HttpClient`.
- **Tests not detected in VS Code?** Install the **C# Dev Kit** and enable **Test Explorer**.


