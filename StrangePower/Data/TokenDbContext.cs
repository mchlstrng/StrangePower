using Microsoft.EntityFrameworkCore;

namespace StrangePower.Data;

public class TokenDbContext : DbContext
{
    public DbSet<Token> Tokens { get; set; }
    public DbSet<MeteringPoint> MeteringPoints { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=tokens.db");
    }
}