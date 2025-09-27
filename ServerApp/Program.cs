var builder = WebApplication.CreateBuilder(args);

// Configure CORS to allow the Blazor ClientApp origin during development
var AllowClientCorsPolicy = "AllowClient";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowClientCorsPolicy, policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
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
                Id = 1,
                Name = "Laptop",
                Price = 1200.50,
                Stock = 25,
            },
            new
            {
                Id = 2,
                Name = "Headphones",
                Price = 50.00,
                Stock = 100,
            },
        };
    }
);

app.Run();
