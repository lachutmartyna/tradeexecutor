using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TradeExecutor.api.Data;
using TradeExecutor.api.Messaging;
using TradeExecutor.api.Services;

namespace TradeExecutor.api;

public class ApiStartup(IConfiguration configuration)
{
    private IConfiguration Configuration { get; } = configuration;
    
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            });
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddDbContext<TradeDbContext>(opts =>
            opts.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<ITradeService, TradeService>();
        services.AddSingleton<IMessagingService, MessagingService>();

        services.Configure<KafkaConfig>(Configuration.GetSection("Kafka"));
    }
    
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseRouting();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}