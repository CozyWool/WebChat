using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using WebChatApplication.DataAccess.Contexts;

namespace WebChatApplication.DataAccess.Factories;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        const string connectionString =
            "Host=localhost;Port=5432;Username=postgres;Password=1;Database=WebChat;";
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}