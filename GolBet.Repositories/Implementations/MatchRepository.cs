 // GolBet.Repositories/Implementations/MatchRepository.cs
using GolBet.Entities;
using GolBet.Entities.Enums;
using GolBet.Repositories.Data;
using GolBet.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GolBet.Repositories.Implementations;

public class MatchRepository : GenericRepository<Match>, IMatchRepository
{
    public MatchRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Match>> GetAllWithTeamsAsync(MatchStatus? status = null, int? teamId = null, string? search = null)
    {
        var query = _dbSet
            .Include(m => m.HomeTeam) // Load the team used by the match card.
            .Include(m => m.AwayTeam)
            .Where(m => m.IsActive)
            .AsNoTracking()
            .AsQueryable(); // Keep the query composable until all filters have been applied.

        if (status.HasValue)
            query = query.Where(m => m.Status == status.Value);

        if (teamId.HasValue)
            query = query.Where(m => m.HomeTeamId == teamId || m.AwayTeamId == teamId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            // Team names already use a case-insensitive SQL collation.
            query = query.Where(m => m.HomeTeam.Name.Contains(term) || m.AwayTeam.Name.Contains(term));
        }

        return await query.OrderBy(m => m.Date).ToListAsync();
    }

    public async Task<Match?> GetByIdWithDetailsAsync(int id)
        => await _dbSet
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.Bets)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);

    public Task<bool> HasTeamMatchesAsync(int teamId)
        => _dbSet.AnyAsync(m => m.HomeTeamId == teamId || m.AwayTeamId == teamId);
}
