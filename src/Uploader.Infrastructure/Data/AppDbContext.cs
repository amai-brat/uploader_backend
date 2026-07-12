using Microsoft.EntityFrameworkCore;
using Uploader.Core.Entities;

namespace Uploader.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Upload> Uploads => Set<Upload>();
    
    
}