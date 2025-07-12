using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Synergy.Framework.Auth.Entities;

namespace Synergy.Framework.Auth.Data;

public class SynergyIdentityDbContext : IdentityDbContext<SynergyIdentityUser>
{
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<UserTokenSession> UserTokenSessions { get; set; } = null!;

    public SynergyIdentityDbContext(DbContextOptions<SynergyIdentityDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Ek model konfigürasyonlarý yapýlabilir
        builder.Entity<RefreshToken>().ToTable("RefreshTokens");
        builder.Entity<UserTokenSession>().ToTable("UserTokenSessions");
    }
}
