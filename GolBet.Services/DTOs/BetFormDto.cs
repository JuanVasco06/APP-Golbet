using System.ComponentModel.DataAnnotations;
using GolBet.Entities.Enums;

namespace GolBet.Services.DTOs;

public class BetFormDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un partido válido.")]
    public int MatchId { get; set; }

    [EnumDataType(typeof(BetPick), ErrorMessage = "Seleccione una cuota válida.")]
    public BetPick Pick { get; set; } = (BetPick)(-1);

    [Display(Name = "Monto")]
    [Range(typeof(decimal), "1", "9999999999", ErrorMessage = "El monto debe estar entre 1 y 9.999.999.999 FutCoins.")]
    public decimal Amount { get; set; }
}
