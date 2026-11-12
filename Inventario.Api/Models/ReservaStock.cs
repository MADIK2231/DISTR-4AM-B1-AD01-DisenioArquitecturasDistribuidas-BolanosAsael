using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventario.Api.Models;

[Table("ReservaStock")]
public class ReservaStock
{
    [Key]
    public int IdReserva { get; set; }

    [Required(ErrorMessage = "El identificador del producto es requerido.")]
    [Range(1, int.MaxValue, ErrorMessage = "El IdProducto debe ser un entero mayor a cero.")]
    public int IdProducto { get; set; }

    [Required(ErrorMessage = "El identificador de la bodega es requerido.")]
    [Range(1, int.MaxValue, ErrorMessage = "El IdBodega debe ser un entero mayor a cero.")]
    public int IdBodega { get; set; }

    [Required(ErrorMessage = "Debe especificar la cantidad a reservar.")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad reservada debe ser mayor a cero.")]
    public int Cantidad { get; set; }

    [Required(ErrorMessage = "El estado de la reserva es requerido.")]
    [RegularExpression("^(PENDIENTE|CONFIRMADA|CANCELADA)$", ErrorMessage = "Valores permitidos para Estado: PENDIENTE, CONFIRMADA, CANCELADA.")]
    [StringLength(50)]
    public string Estado { get; set; } = "PENDIENTE";

    [ForeignKey(nameof(IdBodega))]
    public Bodega? Bodega { get; set; }
}
