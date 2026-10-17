// GolBet.Repositories/Data/DbSeeder.cs
using GolBet.Entities;
using GolBet.Entities.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GolBet.Repositories.Data;

public static class DbSeeder
{
    public const string AdminRole = "Admin";
    public const string BettorRole = "Apostador";

    public static async Task SeedAsync(AppDbContext context,
        RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager,
        bool seedDemoUsers = false)
    {
        // Applies any pending migration (creates the DB if it does not exist)
        await context.Database.MigrateAsync();

        await SeedDomainAsync(context);
        await SeedIdentityAsync(roleManager, userManager, seedDemoUsers);
    }

    private static async Task SeedDomainAsync(AppDbContext context)
    {

        if (await context.Teams.AnyAsync()) return;   // idempotence guard

        using var transaction = await context.Database.BeginTransactionAsync();

        // ---- Teams ----
        var teams = new List<Team>
        {
            new() {
                Name = "Atlético Nacional",
                City = "Medellín",
                CrestUrl = "/images/crests/nac.svg"
            },
            new() { Name = "Independiente Medellín",  City = "Medellín",     CrestUrl = "/images/crests/dim.svg" },
            new() { Name = "Millonarios",             City = "Bogotá",       CrestUrl = "/images/crests/mil.svg" },
            new() { Name = "Independiente Santa Fe",  City = "Bogotá",       CrestUrl = "/images/crests/sfe.svg" },
            new() { Name = "América de Cali",         City = "Cali",         CrestUrl = "/images/crests/ame.svg" },
            new() { Name = "Deportivo Cali",          City = "Cali",         CrestUrl = "/images/crests/cal.svg" },
            new() { Name = "Junior de Barranquilla",  City = "Barranquilla", CrestUrl = "/images/crests/jun.svg" },
            new() { Name = "Once Caldas",             City = "Manizales",    CrestUrl = "/images/crests/onc.svg" }
        };

        context.Teams.AddRange(teams);
        await context.SaveChangesAsync();   // CreatedDate stamped automatically

        // ---- Matches ----
        var today = DateTime.UtcNow.Date;

        var matches = new List<Match>
        {
            // Scheduled: open for betting
            new()
            {
                HomeTeamId = teams[0].Id, AwayTeamId = teams[1].Id,
                Date = today.AddDays(3).AddHours(20),
                Status = MatchStatus.Scheduled,
                HomeOdds = 2.10m,
                DrawOdds = 3.20m,
                AwayOdds = 3.60m
            },
            new()
            {
                HomeTeamId = teams[2].Id, AwayTeamId = teams[3].Id,
                Date = today.AddDays(5).AddHours(18),
                Status = MatchStatus.Scheduled,
                HomeOdds = 2.45m, DrawOdds = 3.00m, AwayOdds = 2.95m
            },
            new()
            {
                HomeTeamId = teams[4].Id, AwayTeamId = teams[5].Id,
                Date = today.AddDays(7).AddHours(19),
                Status = MatchStatus.Scheduled,
                HomeOdds = 2.30m, DrawOdds = 3.10m, AwayOdds = 3.15m
            },
            new()
            {
                HomeTeamId = teams[6].Id, AwayTeamId = teams[7].Id,
                Date = today.AddDays(10).AddHours(16),
                Status = MatchStatus.Scheduled,
                HomeOdds = 1.85m, DrawOdds = 3.40m, AwayOdds = 4.20m
            },

            // InProgress: betting closed, no result yet
            new()
            {
                HomeTeamId = teams[3].Id, AwayTeamId = teams[6].Id,
                Date = DateTime.UtcNow.AddHours(-1),
                Status = MatchStatus.InProgress,
                HomeOdds = 2.60m, DrawOdds = 3.05m, AwayOdds = 2.80m
            },

            // Finished: has a final score
            new()
            {
                HomeTeamId = teams[1].Id, AwayTeamId = teams[2].Id,
                Date = today.AddDays(-4).AddHours(20),
                Status = MatchStatus.Finished,
                HomeGoals = 2, AwayGoals = 1,
                HomeOdds = 2.75m, DrawOdds = 3.10m, AwayOdds = 2.70m
            }
        };

        context.Matches.AddRange(matches);
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private static async Task SeedIdentityAsync(RoleManager<IdentityRole> roleManager,
        UserManager<AppUser> userManager, bool seedDemoUsers)
    {
        foreach (var role in new[] { AdminRole, BettorRole })
        {
            if (!await roleManager.RoleExistsAsync(role))
                EnsureSucceeded(await roleManager.CreateAsync(new IdentityRole(role)));
        }

        // The course accounts are only created for local development.
        if (!seedDemoUsers) return;

        var admin = await userManager.FindByEmailAsync("admin@golbet.com");
        if (admin is null)
        {
            admin = new AppUser
            {
                UserName = "admin@golbet.com",
                Email = "admin@golbet.com",
                FullName = "Administrador GolBet",
                Balance = 0m,
                EmailConfirmed = true
            };
            EnsureSucceeded(await userManager.CreateAsync(admin, "Admin123*"));
        }

        // A restart can repair a missing role without resetting the account.
        if (!await userManager.IsInRoleAsync(admin, AdminRole))
            EnsureSucceeded(await userManager.AddToRoleAsync(admin, AdminRole));

        var bettor = await userManager.FindByEmailAsync("bettor@golbet.com");
        if (bettor is null)
        {
            bettor = new AppUser
            {
                UserName = "bettor@golbet.com",
                Email = "bettor@golbet.com",
                FullName = "Apostador GolBet",
                Balance = 100_000m,
                EmailConfirmed = true
            };
            EnsureSucceeded(await userManager.CreateAsync(bettor, "Bettor123*"));
        }

        // The welcome balance is assigned once, even if the app restarts.
        if (!await userManager.IsInRoleAsync(bettor, BettorRole))
            EnsureSucceeded(await userManager.AddToRoleAsync(bettor, BettorRole));
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException("Identity seed failed: " +
                string.Join("; ", result.Errors.Select(error => error.Code)));
    }
}
