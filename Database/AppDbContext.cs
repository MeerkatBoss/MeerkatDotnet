using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using MeerkatDotnet.Database.Models;

namespace MeerkatDotnet.Database;

public sealed class AppDbContext : DbContext
{
    public DbSet<UserModel> Users { get; set; } = null!;
    public DbSet<RefreshTokenModel> Tokens { get; set; } = null!;

    public AppDbContext(DbContextOptions options)
        : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

}