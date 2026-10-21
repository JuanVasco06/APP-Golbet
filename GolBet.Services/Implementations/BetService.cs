using AutoMapper;
using GolBet.Entities;
using GolBet.Entities.Enums;
using GolBet.Repositories.Exceptions;
using GolBet.Repositories.Interfaces;
using GolBet.Services.DTOs;
using GolBet.Services.Exceptions;
using GolBet.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace GolBet.Services.Implementations;

public class BetService(IBetRepository bets, IMatchRepository matches,
    UserManager<AppUser> userManager, IMapper mapper) : IBetService
{
    public async Task PlaceBetAsync(string userId, BetFormDto dto)
    {
        if (!Enum.IsDefined(dto.Pick))
            throw new BusinessRuleException("Selección inválida.");

        if (dto.Amount < 1m || dto.Amount > 9_999_999_999m || decimal.Round(dto.Amount, 2) != dto.Amount)
            throw new BusinessRuleException("Ingrese un monto válido, desde 1 FutCoin y con máximo dos decimales.");

        var match = await matches.GetByIdAsync(dto.MatchId);
        if (match is null || !match.IsActive)
            throw new BusinessRuleException("El partido no existe.");

        if (match.Status != MatchStatus.Scheduled || match.Date <= DateTime.UtcNow)
            throw new BusinessRuleException("Este partido ya no acepta apuestas.");

        if (await bets.ExistsAsync(dto.MatchId, userId, dto.Pick))
            throw new BusinessRuleException("Ya tienes una apuesta con esta selección en este partido.");

        var user = await userManager.FindByIdAsync(userId)
            ?? throw new BusinessRuleException("Usuario no encontrado.");

        if (dto.Amount > user.Balance)
            throw new BusinessRuleException($"Saldo insuficiente: tienes {user.Balance:N2} FutCoins.");

        var oddsAtPlacement = dto.Pick switch
        {
            BetPick.Home => match.HomeOdds,
            BetPick.Draw => match.DrawOdds,
            BetPick.Away => match.AwayOdds,
            _ => throw new BusinessRuleException("Selección inválida.")
        };

        user.Balance -= dto.Amount;
        // Reject a second request that read the same balance before this bet was saved.
        user.ConcurrencyStamp = Guid.NewGuid().ToString();

        var bet = new Bet
        {
            MatchId = dto.MatchId,
            UserId = userId,
            Pick = dto.Pick,
            Amount = dto.Amount,
            OddsAtPlacement = oddsAtPlacement,
            Status = BetStatus.Pending
        };

        try
        {
            // Identity and the repository share the tracked context: one save commits both changes.
            await bets.AddAsync(bet);
        }
        catch (DuplicateRecordException)
        {
            throw new BusinessRuleException("Ya tienes una apuesta con esta selección en este partido.");
        }
        catch (ConcurrentUpdateException)
        {
            throw new BusinessRuleException("Tu saldo cambió mientras apostabas. Revisa el saldo e inténtalo de nuevo.");
        }
    }

    public async Task<IEnumerable<BetDto>> GetMyBetsAsync(string userId)
        => mapper.Map<IEnumerable<BetDto>>(await bets.GetByUserAsync(userId));
}
