using GolBet.Entities;
using GolBet.Repositories.Data;
using GolBet.Services.DTOs;
using GolBet.Services.Exceptions;
using GolBet.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GolBet.Web.Controllers;

[Authorize(Roles = DbSeeder.BettorRole)]
public class BetsController(IBetService bets, UserManager<AppUser> userManager) : Controller
{
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Place(BetFormDto dto)
    {
        var userId = userManager.GetUserId(User);
        if (userId is null) return Challenge();

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Revisa el monto y la selección de la apuesta.";
            return RedirectToAction("Detail", "Matches", new { id = dto.MatchId });
        }

        try
        {
            await bets.PlaceBetAsync(userId, dto);
            TempData["Success"] = $"¡Apuesta registrada! {dto.Amount:N2} FutCoins en juego. Suerte 🍀";
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Detail", "Matches", new { id = dto.MatchId });
    }

    [HttpGet]
    public async Task<IActionResult> MyBets()
    {
        var userId = userManager.GetUserId(User);
        if (userId is null) return Challenge();
        return View(await bets.GetMyBetsAsync(userId));
    }
}
