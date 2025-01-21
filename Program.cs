using CooP_WS;
using CooP_WS.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.StackExchangeRedis;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Redis connection with a health check
var redisConnectionString = "10.220.249.99:6379"; // Replace with your Redis endpoint
try
{
    Console.WriteLine("Attempting to connect to Redis...");
    var redis = ConnectionMultiplexer.Connect(redisConnectionString);

    // Test Redis connection with a PING command
    var redisDatabase = redis.GetDatabase();
    var redisPingResponse = redisDatabase.Ping();
    Console.WriteLine($"Connected to Redis. Ping response: {redisPingResponse}");

    builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to connect to Redis: {ex.Message}");
    throw; // Stop the application if Redis connection is critical
}

// Add SignalR and configure Redis backplane
builder.Services.AddSignalR()
    .AddStackExchangeRedis(redisConnectionString); // Replace with your Redis endpoint

// Add CORS services and define a named policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOriginsPolicy", builder =>
    {
        builder.WithOrigins(
            "https://coopfront-674574933021.europe-central2.run.app" // Frontend
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});


// Add RedisSubscriberService
builder.Services.AddSingleton<RedisSubscriberService>();

var app = builder.Build();

// Start RedisSubscriberService
app.Services.GetRequiredService<RedisSubscriberService>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok("Healthy")); // Responds with 200 OK


// Use CORS policy
app.UseCors("AllowedOriginsPolicy");

// Map SignalR hubs
app.MapHub<CanvasHub>("/canvasHub");

app.Run();
