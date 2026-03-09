using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
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
    protected ILogger? Logger { get; private set; }

    public abstract Task InvokeAsync(GuildParserOptions options, CancellationToken cancellationToken = default);

    public void SetAuthorizedHttpClient(HttpClient httpClient)
    {
        authorizedHttpClient = httpClient;
    }

    public void SetLogger(ILogger logger)
    {
        Logger = logger;
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
        const int maxRetries = 3;
        const int retryDelayMs = 65000;

        for (var retryAttempt = 0; retryAttempt < maxRetries; retryAttempt++)
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
                    requestMessage.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
                    requestMessage.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
                    requestMessage.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");

                    using var httpResponseMessage = await httpClient.SendAsync(requestMessage, cancellationToken);

                    if (!httpResponseMessage.IsSuccessStatusCode)
                    {
                        var statusCode = (int)httpResponseMessage.StatusCode;
                        var errorContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
                        
                        if (statusCode == 503)
                        {
                            if (retryAttempt < maxRetries - 1)
                            {
                                Logger?.LogWarning(
                                    "Получена ошибка 503 (лимит запросов). Попытка {Attempt}/{MaxAttempts}. Ожидание {DelayMs}ms перед повтором",
                                    retryAttempt + 1,
                                    maxRetries,
                                    retryDelayMs);

                                await Task.Delay(TimeSpan.FromMilliseconds(retryDelayMs), cancellationToken);
                                continue;
                            }

                            Logger?.LogError(
                                "HTTP запрос завершился с ошибкой 503 после {MaxAttempts} попыток: Uri={Uri}",
                                maxRetries,
                                requestUri);
                        }
                        else
                        {
                            Logger?.LogError(
                                "HTTP запрос завершился с ошибкой: StatusCode={StatusCode}, Uri={Uri}",
                                statusCode,
                                requestUri);

                            if (statusCode == 443)
                            {
                                Logger?.LogWarning("Обнаружена ошибка 443. Возможные причины: блокировка Cloudflare, SSL/TLS проблемы, недостаточно заголовков для имитации браузера");
                            }
                        }

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
            catch (HttpRequestException httpException)
            {
                Logger?.LogError(httpException, "HTTP исключение при выполнении запроса: {Message}", httpException.Message);
                
                if (retryAttempt < maxRetries - 1)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(retryDelayMs), cancellationToken);
                    continue;
                }
                
                return null;
            }
            catch (TaskCanceledException canceledException)
            {
                Logger?.LogWarning(canceledException, "Запрос был отменен: {Message}", canceledException.Message);
                return null;
            }
            catch (Exception exception)
            {
                Logger?.LogError(exception, "Неожиданное исключение при выполнении запроса: {Message}", exception.Message);
                
                if (retryAttempt < maxRetries - 1)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(retryDelayMs), cancellationToken);
                    continue;
                }
                
                return null;
            }
        }

        return null;
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
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            UseCookies = true,
            AllowAutoRedirect = true
        };

        var httpClient = new HttpClient(httpClientHandler)
        {
            BaseAddress = baseUri,
            Timeout = TimeSpan.FromMinutes(5)
        };

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Connection", "keep-alive");
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36");
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://cp.wowcircle.net/guilds");
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://cp.wowcircle.net");

        return httpClient;
    }
}
