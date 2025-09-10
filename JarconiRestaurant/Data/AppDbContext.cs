using System.Reflection;
using JarconiRestaurant.Domain.Menu;
using JarconiRestaurant.Domain.News;
using JarconiRestaurant.Domain.Reservations;
using JarconiRestaurant.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace JarconiRestaurant.Data;

public class AppDbContext : DbContext {

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<NewsPost> NewsPosts => Set<NewsPost>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}