using Microsoft.EntityFrameworkCore;

namespace JarconiRestaurant.Data;

public class AppDbContext : DbContext {

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {
    }

    public DbSet<Todo> Todos => Set<Todo>();
}