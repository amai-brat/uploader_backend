using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Uploader.Core.Abstractions;
using Uploader.Infrastructure.Data;
using Uploader.Infrastructure.Data.Models;
using Uploader.Infrastructure.Data.Repositories;
using Uploader.Infrastructure.Storage;

namespace Uploader.Infrastructure;

public static class Entry
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => 
            options
                .UseSqlite(configuration.GetConnectionString("Sqlite"))
                .UseModel(AppDbContextModel.Instance));
        services.AddScoped<IUploadRepository, UploadRepository>();
        
        services.AddScoped<IFileStorage, LocalFileStorage>();
        
        return services;
    }
}