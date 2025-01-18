using CooP_WS.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.StackExchangeRedis;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SignalR and configure Redis backplane
builder.Services.AddSignalR()
    .AddStackExchangeRedis("10.220.249.99:6379"); // Replace with your Redis endpoint

// Add CORS services and define a named policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOriginsPolicy", builder =>
    {
        builder.WithOrigins("https://coopfront-674574933021.europe-central2.run.app") // Replace with your frontend URL
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials(); // Allow credentials if needed
    });
});

// Connect to Redis with logging
var redisConnectionString = "10.220.249.99:6379"; // Replace with your Redis endpoint
try
{
    Console.WriteLine("Attempting to connect to Redis...");
    var redis = ConnectionMultiplexer.Connect(redisConnectionString);
    Console.WriteLine("Successfully connected to Redis.");
    builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to connect to Redis: {ex.Message}");
    throw; // Re-throw to stop the application if Redis connection is critical
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use CORS policy
app.UseCors("AllowedOriginsPolicy");

// Map SignalR hubs
app.MapHub<CanvasHub>("/canvasHub");

app.Run();
