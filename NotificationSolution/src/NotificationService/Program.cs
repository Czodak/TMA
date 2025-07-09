using System.Text.Json.Serialization;
using System.Text.Json;
using NotificationService.EventHandling;
using System.Text.Json.Serialization.Metadata;
using System.Net.Mail;
using NotificationService.MessageClient;
using Serilog;

namespace NotificationService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<Worker>();
            builder.Services.AddScoped<IMessageEventHandler, MessageEventHandler>();
            builder.Services.AddSingleton<IMessageClient, RabbitMqMessageClient>();
            builder.Services.Configure<JsonPolymorphismOptions>(options =>
            {
                options.TypeDiscriminatorPropertyName = "eventType";
            });

            var logger = new LoggerConfiguration()
               .WriteTo.Console()
               .WriteTo.Seq("http://seq:5341")
               .CreateLogger();

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger);

            builder.Services.Configure<JsonSerializerOptions>(options =>
            {
                options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.Converters.Add(new JsonStringEnumConverter());
            });

            builder.Services
                .AddFluentEmail("test@test.pl")
                .AddSmtpSender(() => new SmtpClient("smtp4dev")
                {
                    Port = 25,
                    EnableSsl = false
                });
                

            var host = builder.Build();
            host.Run();
        }
    }
}