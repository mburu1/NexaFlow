using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NexaFlow.Application.Common.Interfaces;
using NexaFlow.Domain.Interfaces;
using NexaFlow.Infrastructure.Auth;
using NexaFlow.Infrastructure.Persistence;
using NexaFlow.Infrastructure.Persistence.Repositories;

namespace NexaFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<NexaFlowDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("MsSql"),
                sql => sql.MigrationsAssembly(typeof(NexaFlowDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options => options.Configuration = redisConnectionString);
        }

        return services;
    }
}
