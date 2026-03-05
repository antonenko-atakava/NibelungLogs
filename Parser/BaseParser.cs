using System.Net;
using System.Text;
using Newtonsoft.Json;
using Parser.Models.Rpc;

namespace Parser;

public abstract class BaseParser
{
    private int _tId;
    private HttpClient? _authorizedHttpClient;
    public abstract Task InvokeAsync(ParserOptions options, CancellationToken cancellationToken = default);

    public void SetAuthorizedHttpClient(HttpClient httpClient)
    {
        _authorizedHttpClient = httpClient;
    }

    protected virtual HttpClient GetHttpClient()
    {
        if (_authorizedHttpClient != null)
            return _authorizedHttpClient;

        var cookieContainer = new CookieContainer();
        var uri = new Uri("https://cp.wowcircle.net");
        cookieContainer.Add(uri, new Cookie("PMBC", "dcc7064c9be35b3f13eb06b50690024b", "/"));
        cookieContainer.Add(uri, new Cookie("PHPSESSID", "60g5hvurs1bff75r47q3e1jck9", "/"));

        var httpClientHandler = new HttpClientHandler
        {
            CookieContainer = cookieContainer,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };

        var httpClient = new HttpClient(httpClientHandler)
        {
            BaseAddress = new Uri("https://cp.wowcircle.net")
        };

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36");
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "ru");
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://cp.wowcircle.net/pveladder");
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://cp.wowcircle.net");

        return httpClient;
    }

    protected RpcRequest CreateRequest(string method, RequestData[] data)
    {
        return new RpcRequest
        {
            TId = ++_tId,
            Method = method,
            Data = data
        };
    }

    protected async Task<List<RpcResponseModel<TResult>>?> SendRequestAsync<TResult>(int serverId,
        params RpcRequest[] requests)
    {
        try
        {
            var httpClient = GetHttpClient();
            var shouldDispose = _authorizedHttpClient == null;

            try
            {
                var serialized = JsonConvert.SerializeObject(requests);
                var content = new StringContent(serialized, Encoding.UTF8, "application/json");

                var url = $"/main.php?1&serverId={serverId}";
                Console.WriteLine($"Sending request to: {url}");

                var httpResponseMessage = await httpClient.PostAsync(url, content);

                Console.WriteLine($"Response status: {httpResponseMessage.StatusCode}");

                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    var errorContent = await httpResponseMessage.Content.ReadAsStringAsync();
                    Console.WriteLine(
                        $"ERROR: HTTP {httpResponseMessage.StatusCode}, Response: {errorContent.Substring(0, Math.Min(500, errorContent.Length))}");
                    return null;
                }

                var response = await httpResponseMessage.Content.ReadAsStringAsync();
                Console.WriteLine($"Response length: {response.Length} chars");

                if (response.Contains("Please enable Javascript"))
                {
                    Console.WriteLine(
                        "ERROR: Server still requires JavaScript. Cookie PMBC may be invalid or expired.");
                    return null;
                }

                try
                {
                    return JsonConvert.DeserializeObject<List<RpcResponseModel<TResult>>>(response);
                }
                catch (Exception ex1)
                {
                    Console.WriteLine($"Failed to deserialize as List: {ex1.Message}");
                    try
                    {
                        var model = JsonConvert.DeserializeObject<RpcResponseModel<TResult>>(response);

                        if (model != null)
                            return [model];

                        Console.WriteLine("Deserialized as single object but result is null");
                        return null;
                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine($"Failed to deserialize as single object: {ex2.Message}");
                        Console.WriteLine(
                            $"Response preview (first 500 chars): {response.Substring(0, Math.Min(500, response.Length))}");
                        return null;
                    }
                }
            }
            finally
            {
                if (shouldDispose)
                {
                    httpClient.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EXCEPTION in SendRequestAsync: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    protected RpcResponseModel<TResult>? GetResponseByRequestTid<TResult>(
        IEnumerable<RpcResponseModel<TResult>>? responses, int requestTid)
        => responses?.FirstOrDefault(response => response.Tid == requestTid);
}