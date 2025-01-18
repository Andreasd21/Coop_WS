using CooP_WS;
using CooP_WS.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.StackExchangeRedis;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var redisConnectionString = "10.220.249.99:6379"; // Replace with your Redis endpoint
var redis = ConnectionMultiplexer.Connect(redisConnectionString);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);


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
builder.Services.AddSingleton<RedisSubscriberService>();


var app = builder.Build();


app.Services.GetRequiredService<RedisSubscriberService>();

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
