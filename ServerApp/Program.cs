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

var app = builder.Build();

app.UseCors(AllowClientCorsPolicy);

app.MapGet(
    "/api/productList",
    () =>
    {
        return new[]
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
    }
);

app.Run();
