using GolBet.Repositories.Data;
using Microsoft.AspNetCore.Authorization;
using GolBet.Entities.Enums;
using GolBet.Services.DTOs;
using GolBet.Services.Exceptions;
using GolBet.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GolBet.Web.Controllers;

public class MatchesController(IMatchService matches, ITeamService teams) : Controller
{
    public async Task<IActionResult> Index(MatchStatus? status, int? teamId, string? search)
    {
        if (!ModelState.IsValid || (status.HasValue && !Enum.IsDefined(status.Value)) || teamId <= 0)
            return BadRequest("Los filtros no son válidos.");
        ViewBag.CurrentStatus = status;
        ViewBag.CurrentTeamId = teamId;
        ViewBag.CurrentSearch = search;
        await LoadTeamsAsync();
        return View(await matches.GetBoardAsync(status, teamId, search));
    }

    public async Task<IActionResult> Detail(int id)
    {
        var match = await matches.GetDetailAsync(id);
        return match is null ? NotFound() : View(match);
    }

    [Authorize(Roles = DbSeeder.AdminRole)]
    public async Task<IActionResult> Create()
    {
        await LoadTeamsAsync();
        return View(new MatchFormDto());
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = DbSeeder.AdminRole)]
    public async Task<IActionResult> Create(MatchFormDto dto)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await matches.CreateAsync(dto);
                TempData["Success"] = "Partido creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (BusinessRuleException ex) { ModelState.AddModelError("", ex.Message); }
        }
        await LoadTeamsAsync();
        return View(dto);
    }

    [Authorize(Roles = DbSeeder.AdminRole)]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var dto = await matches.GetForEditAsync(id);
            if (dto is null) return NotFound();
            await LoadTeamsAsync();
            return View(dto);
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Detail), new { id });
        }
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = DbSeeder.AdminRole)]
    public async Task<IActionResult> Edit([FromRoute] int id, MatchFormDto dto)
    {
        if (id != dto.Id) return BadRequest();
        if (ModelState.IsValid)
        {
            try
            {
                await matches.UpdateAsync(dto);
                TempData["Success"] = "Partido actualizado correctamente.";
                return RedirectToAction(nameof(Detail), new { id });
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (BusinessRuleException ex) { ModelState.AddModelError("", ex.Message); }
        }
        await LoadTeamsAsync();
        return View(dto);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = DbSeeder.AdminRole)]
    public async Task<IActionResult> Deactivate(int id)
    {
        try
        {
            await matches.DeactivateAsync(id);
            TempData["Success"] = "Partido desactivado. Su historial se conserva.";
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (BusinessRuleException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = DbSeeder.AdminRole)]
    public async Task<IActionResult> Start(int id)
    {
        try
        {
            await matches.StartMatchAsync(id);
            TempData["Success"] = "¡Partido en juego! Las apuestas quedaron cerradas.";
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (BusinessRuleException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Detail), new { id });
    }

    // The form opens for any match; the state rules live in the service.
    [HttpGet, Authorize(Roles = DbSeeder.AdminRole)]
    public async Task<IActionResult> Close(int id)
    {
        var match = await matches.GetDetailAsync(id);
        if (match is null) return NotFound();
        ViewBag.Match = match;
        return View(new MatchResultFormDto { MatchId = id });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = DbSeeder.AdminRole)]
    public async Task<IActionResult> Close(MatchResultFormDto dto)
    {
        if (!ModelState.IsValid)
        {
            var match = await matches.GetDetailAsync(dto.MatchId);
            if (match is null) return NotFound();
            ViewBag.Match = match;
            return View(dto);
        }

        try
        {
            var summary = await matches.CloseMatchAsync(dto);
            TempData["Success"] =
                $"Partido cerrado {dto.HomeGoals}-{dto.AwayGoals}. " +
                $"{summary.ResolvedBets} apuestas resueltas: {summary.WonBets} ganadas, " +
                $"{summary.TotalPaid:N2} FutCoins pagados.";
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (BusinessRuleException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Detail), new { id = dto.MatchId });
    }

    private async Task LoadTeamsAsync()
        => ViewBag.Teams = new SelectList(await teams.GetAllAsync(), "Id", "Name");
}
