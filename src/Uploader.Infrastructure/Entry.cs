using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Uploader.Core.Abstractions;
using Uploader.Infrastructure.Data;
using Uploader.Infrastructure.Data.Repositories;
using Uploader.Infrastructure.Storage;

namespace Uploader.Infrastructure;

public static class Entry
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<DapperContext>();
        services.AddScoped<IUploadRepository, UploadRepositoryDapper>();
        
        services.AddScoped<IFileStorage, LocalFileStorage>();
        
        return services;
    }
}