using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebAPI.Common.Messaging;
using WebAPI.Data;
using WebAPI.Data.Repository;

namespace WebAPI.Framework.Startup;

public static class DataServiceCollectionExtensions
{
    public static IServiceCollection AddWebAPIData(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default") 
            ?? throw new InvalidOperationException("Configure ConnectionStrings:Default (PostgreSQL).");

        services.AddDbContext<WebAPIContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IOutbox, Outbox>();
        return services;
    }
}
