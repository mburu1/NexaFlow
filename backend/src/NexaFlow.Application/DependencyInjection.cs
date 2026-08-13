using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NexaFlow.Application.Interfaces;
using NexaFlow.Application.Services;

namespace NexaFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IWorkflowService, WorkflowService>();
        services.AddScoped<IWorkflowTaskService, WorkflowTaskService>();

        return services;
    }
}
