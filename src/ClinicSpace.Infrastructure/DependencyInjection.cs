using ClinicSpace.Application.Abstractions;
using ClinicSpace.Infrastructure.Identity;
using ClinicSpace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicSpace.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "DefaultConnection";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // The connection string is resolved lazily so test hosts can swap the provider
        // without needing a SQL Server connection string.
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString(ConnectionStringName)
                ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is not configured.")));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        // Minimal Identity registration so the Identity schema is part of the initial migration.
        // Sign-in, cookies, password policy and roles are configured in Step 1.
        services.AddIdentityCore<AppUser>()
            .AddEntityFrameworkStores<AppDbContext>();

        return services;
    }
}
