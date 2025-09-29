using System.Net.Http;
using System.Net.Http.Json;

namespace ClientApp.Services;

// Copilot added caching with TTL and request coalescing to reduce redundant network calls with implementing ProductService
public class ProductService
{
    private readonly HttpClient _http;
    private readonly TimeSpan _ttl = TimeSpan.FromSeconds(30);

    // cached payload and timestamp
    private Product[]? _cache;
    private DateTime _cachedAt;

    // coalescing task - when not null, a fetch is in progress
    private Task<Product[]>? _inflight;

    public ProductService(HttpClient http)
    {
        _http = http;
    }

    public Task<Product[]> GetProductsAsync()
    {
        // if cache is fresh, return it
        if (_cache is not null && (DateTime.UtcNow - _cachedAt) < _ttl)
        {
            return Task.FromResult(_cache);
        }

        // if there's an inflight fetch, return that task to coalesce requests
        if (_inflight is not null)
        {
            return _inflight;
        }

        // start a new fetch
        _inflight = FetchAndCacheAsync();
        return _inflight;
    }

    private async Task<Product[]> FetchAndCacheAsync()
    {
        try
        {
            var products =
                await _http.GetFromJsonAsync<Product[]>("api/productList")
                ?? Array.Empty<Product>();
            _cache = products;
            _cachedAt = DateTime.UtcNow;
            return products;
        }
        finally
        {
            // clear inflight so subsequent calls can start new fetch when needed
            _inflight = null;
        }
    }

    public void Invalidate()
    {
        _cache = null;
        _cachedAt = default;
    }

    public record Product(int Id, string? Name, double Price, int Stock);
}
