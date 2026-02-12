using Microsoft.EntityFrameworkCore;
using MyRemoteApi.Models;

namespace MyRemoteApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<TodoTask> Tasks => Set<TodoTask>();
}