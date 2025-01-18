using CooP_WS;
using CooP_WS.Hubs;
using Microsoft.AspNetCore.SignalR;

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

builder.Services.AddSingleton<PubSubSubscriber>(serviceProvider =>
{
    var hubContext = serviceProvider.GetRequiredService<IHubContext<CanvasHub>>();
    return new PubSubSubscriber("coop-443623", "wscanvas", hubContext);
});

var app = builder.Build();

// Start the Pub/Sub subscriber
var cts = new CancellationTokenSource();

var pubSubSubscriber = app.Services.GetRequiredService<PubSubSubscriber>();
Task.Run(() => pubSubSubscriber.StartAsync(cts.Token));


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
