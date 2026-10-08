using System.Net.Http.Headers;
using System.Net.Mime;

namespace ServiceLib.Helper;

/// <summary>
/// HTTP access to the local Clash API. Every request carries the Clash API secret as a Bearer token.
/// </summary>
public sealed class ClashApiHttpHelper
{
    private static readonly Lazy<ClashApiHttpHelper> _instance = new(() => new(new SocketsHttpHandler { UseCookies = false }));
    public static ClashApiHttpHelper Instance => _instance.Value;

    private readonly HttpClient _httpClient;

    internal ClashApiHttpHelper(HttpMessageHandler handler)
    {
        _httpClient = new HttpClient(handler);
    }

    internal static HttpRequestMessage BuildRequest(HttpMethod method, string url, string secret, HttpContent? content = null)
    {
        var request = new HttpRequestMessage(method, url) { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secret);
        return request;
    }

    public async Task<string?> TryGetAsync(string url, CancellationToken cancellationToken = default)
    {
        if (url.IsNullOrEmpty())
        {
            return null;
        }

        try
        {
            using var request = BuildRequest(HttpMethod.Get, url, ClashApiSecret.Active);
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }

    public async Task PutAsync(string url, Dictionary<string, string> headers)
    {
        var jsonContent = JsonUtils.Serialize(headers);
        var content = new StringContent(jsonContent, Encoding.UTF8, MediaTypeNames.Application.Json);

        using var request = BuildRequest(HttpMethod.Put, url, ClashApiSecret.Active, content);
        using var response = await _httpClient.SendAsync(request);
    }

    public async Task PatchAsync(string url, Dictionary<string, string> headers)
    {
        var myContent = JsonUtils.Serialize(headers);
        var buffer = Encoding.UTF8.GetBytes(myContent);
        var byteContent = new ByteArrayContent(buffer);
        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        using var request = BuildRequest(HttpMethod.Patch, url, ClashApiSecret.Active, byteContent);
        using var response = await _httpClient.SendAsync(request);
    }

    public async Task DeleteAsync(string url)
    {
        using var request = BuildRequest(HttpMethod.Delete, url, ClashApiSecret.Active);
        using var response = await _httpClient.SendAsync(request);
    }
}
