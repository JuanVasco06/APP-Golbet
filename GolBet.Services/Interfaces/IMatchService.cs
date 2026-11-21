// GolBet.Services/Interfaces/IMatchService.cs
using GolBet.Entities.Enums;
using GolBet.Services.DTOs;

namespace GolBet.Services.Interfaces;

public interface IMatchService
{
    /// <summary>Match board: all active matches ordered by date.</summary>
    Task<IEnumerable<MatchDto>> GetBoardAsync(MatchStatus? status = null, int? teamId = null, string? search = null);
    Task<MatchDetailDto?> GetDetailAsync(int id);
    Task<MatchFormDto?> GetForEditAsync(int id);
    Task CreateAsync(MatchFormDto dto);
    Task UpdateAsync(MatchFormDto dto);
    Task DeactivateAsync(int id);

    /// <summary>Scheduled -> InProgress. Betting closes.</summary>
    Task StartMatchAsync(int id);

    /// <summary>InProgress -> Finished: saves the score and resolves every pending bet.</summary>
    Task<ResolutionSummaryDto> CloseMatchAsync(MatchResultFormDto dto);
}
