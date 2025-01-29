using MassTransit;
using PDI_node.Consumers;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables(); 
var config = builder.Configuration;

builder.Services.AddMassTransit(busConfigurator =>
{
    busConfigurator.AddConsumer<PrimeTaskConsumer>();
        
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

var app = builder.Build();
app.UseHttpsRedirection();



app.Run();

