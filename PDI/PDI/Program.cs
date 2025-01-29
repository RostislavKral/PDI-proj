using MassTransit;
using PDI.Consumers;
using PDI.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables(); 

var config = builder.Configuration;

builder.WebHost.UseUrls(config["HOST"]);

builder.Services.AddMassTransit(busConfigurator =>
{

    busConfigurator.AddConsumer<PrimeResultConsumer>();

    busConfigurator.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(config["RabbitMQUri"], config["RabbitMQVirtualHost"], h =>
        {
            h.Username(config["RabbitMQUsername"]);
            h.Password(config["RabbitMQPassword"]);
        });
        cfg.ConfigureEndpoints(context);
    });
});


builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins(config["FRONT_URL"]) 
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
var app = builder.Build();
app.UseCors("AllowSpecificOrigin");

app.MapHub<PrimeHub>("/primeHub"); // Map SignalR Hub endpoint

app.UseHttpsRedirection();
app.MapControllers();
app.Run();

