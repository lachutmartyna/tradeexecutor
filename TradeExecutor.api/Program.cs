using Microsoft.EntityFrameworkCore;
using TradeExecutor.api.Data;

namespace TradeExecutor.api;

public static class Program
{
    public static Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var startup = new ApiStartup(builder.Configuration);
        startup.ConfigureServices(builder.Services);

        var app = builder.Build();
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TradeDbContext>();
            db.Database.Migrate();
        }

        
        startup.Configure(app, app.Environment);

        return app.RunAsync();
    }
}