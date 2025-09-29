using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Copilot suggestted adding Cors to fix connection
// Configure CORS to allow the Blazor ClientApp origin during development
var AllowClientCorsPolicy = "AllowClient";
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        name: AllowClientCorsPolicy,
        policy =>
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
    );
});

// Copilot added caching to optimize repeated requests
// register in-memory cache
builder.Services.AddMemoryCache();

var app = builder.Build();

app.UseCors(AllowClientCorsPolicy);

// Cache key and TTL
const string cacheKey = "productList:json";
var cacheTtl = TimeSpan.FromSeconds(30);

app.MapGet(
    "/api/productList",
    (HttpContext ctx, IMemoryCache cache) =>
    {
        // Try get cached serialized bytes and etag
        if (cache.TryGetValue(cacheKey, out CachedPayload cached) && cached != null)
        {
            // If client provided If-None-Match and it matches, return 304
            var ifNoneMatch = ctx.Request.Headers["If-None-Match"].ToString();
            if (!string.IsNullOrEmpty(ifNoneMatch) && ifNoneMatch == cached.ETag)
            {
                ctx.Response.Headers["X-Cache"] = "HIT";
                ctx.Response.Headers["ETag"] = cached.ETag;
                return Results.StatusCode(StatusCodes.Status304NotModified);
            }

            ctx.Response.Headers["Cache-Control"] = "public, max-age=30";
            ctx.Response.Headers["X-Cache"] = "HIT";
            ctx.Response.Headers["ETag"] = cached.ETag;
            return Results.File(cached.JsonBytes, "application/json");
        }

        // Build the payload (in real app, fetch from DB/service)
        var products = new[]
        {
            new
            {
                id = 1,
                name = "Laptop",
                price = 1200.50,
                stock = 25,
                category = new { id = 101, name = "Electronics" },
            },
            new
            {
                id = 2,
                name = "Headphones",
                price = 50.00,
                stock = 100,
                category = new { id = 102, name = "Accessories" },
            },
        };

        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
        };
        var jsonBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(products, options);

        // Compute a simple ETag from the payload
        using var sha = System.Security.Cryptography.SHA256.Create();
        var hash = sha.ComputeHash(jsonBytes);
        var etag = "\"" + Convert.ToBase64String(hash) + "\""; // quoted string per RFC

        // Cache the serialized bytes along with ETag
        cached = new CachedPayload(jsonBytes, etag);
        cache.Set(cacheKey, cached, cacheTtl);

        ctx.Response.Headers["Cache-Control"] = "public, max-age=30";
        ctx.Response.Headers["X-Cache"] = "MISS";
        ctx.Response.Headers["ETag"] = etag;

        return Results.File(jsonBytes, "application/json");
    }
);

app.Run();

// small helper record used for cache storage
internal sealed record CachedPayload(byte[] JsonBytes, string ETag);
