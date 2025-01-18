using CooP_WS;
using CooP_WS.Hubs;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

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

// Connect to Redis
var redisConnectionString = "10.220.249.99:6379"; // Replace with your Redis endpoint
var redis = ConnectionMultiplexer.Connect(redisConnectionString);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use the named CORS policy
app.UseCors("AllowedOriginsPolicy");

app.UseHttpsRedirection();
app.MapHub<ChatHub>("/signalr");
app.MapHub<CanvasHub>("/canvasHub");

app.Run();
