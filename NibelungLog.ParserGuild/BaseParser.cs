using System.Net;
using System.Text;
using System.Text.Json;
using NibelungLog.ParserGuild.Models;

namespace NibelungLog.ParserGuild;

public abstract class BaseParser
{
    private readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private int requestIdentifier;
    private HttpClient? authorizedHttpClient;

    public abstract Task InvokeAsync(GuildParserOptions options, CancellationToken cancellationToken = default);

    public void SetAuthorizedHttpClient(HttpClient httpClient)
    {
        authorizedHttpClient = httpClient;
    }

    protected RequestEnvelope<TRequestData> CreateRequest<TRequestData>(string method, TRequestData[] data)
    {
        return new RequestEnvelope<TRequestData>
        {
            RequestIdentifier = Interlocked.Increment(ref requestIdentifier),
            Method = method,
            Data = data.ToList()
        };
    }

    protected async Task<List<ResponseEnvelope<TResult>>?> SendRequestAsync<TResult, TRequestData>(
        int serverId,
        RequestEnvelope<TRequestData>[] requests,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = GetHttpClient();
            var shouldDisposeHttpClient = authorizedHttpClient == null;

            try
            {
                var serializedRequests = JsonSerializer.Serialize(requests, jsonSerializerOptions);
                var requestUri = $"/main.php?1&serverId={serverId}";
                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri)
                {
                    Content = new StringContent(serializedRequests, Encoding.UTF8, "application/json")
                };
                requestMessage.Headers.TryAddWithoutValidation("Origin", "https://cp.wowcircle.net");
                requestMessage.Headers.TryAddWithoutValidation("Referer", "https://cp.wowcircle.net/guilds");
                requestMessage.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

                using var httpResponseMessage = await httpClient.SendAsync(requestMessage, cancellationToken);

                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    return null;
                }

                var responseContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

                if (responseContent.Contains("Please enable Javascript", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                using var jsonDocument = JsonDocument.Parse(responseContent);

                if (jsonDocument.RootElement.ValueKind == JsonValueKind.Array)
                {
                    return JsonSerializer.Deserialize<List<ResponseEnvelope<TResult>>>(responseContent, jsonSerializerOptions);
                }

                if (jsonDocument.RootElement.ValueKind == JsonValueKind.Object)
                {
                    var singleResponse = JsonSerializer.Deserialize<ResponseEnvelope<TResult>>(responseContent, jsonSerializerOptions);

                    if (singleResponse == null)
                    {
                        return null;
                    }

                    return [singleResponse];
                }

                return null;
            }
            finally
            {
                if (shouldDisposeHttpClient)
                {
                    httpClient.Dispose();
                }
            }
        }
        catch
        {
            return null;
        }
    }

    protected ResponseEnvelope<TResult>? GetResponseByRequestIdentifier<TResult>(
        IEnumerable<ResponseEnvelope<TResult>>? responses,
        int requestIdentifier)
    {
        return responses?.FirstOrDefault(response => response.RequestIdentifier == requestIdentifier);
    }

    protected virtual HttpClient GetHttpClient()
    {
        if (authorizedHttpClient != null)
        {
            return authorizedHttpClient;
        }

        var cookieContainer = new CookieContainer();
        var baseUri = new Uri("https://cp.wowcircle.net");
        cookieContainer.Add(baseUri, new Cookie("PMBC", "dcc7064c9be35b3f13eb06b50690024b", "/"));
        cookieContainer.Add(baseUri, new Cookie("PHPSESSID", "60g5hvurs1bff75r47q3e1jck9", "/"));

        var httpClientHandler = new HttpClientHandler
        {
            CookieContainer = cookieContainer,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };

        var httpClient = new HttpClient(httpClientHandler)
        {
            BaseAddress = baseUri,
            Timeout = TimeSpan.FromMinutes(5)
        };

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36");
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "ru-RU,ru;q=0.9");
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://cp.wowcircle.net/guilds");
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://cp.wowcircle.net");

        return httpClient;
    }
}
