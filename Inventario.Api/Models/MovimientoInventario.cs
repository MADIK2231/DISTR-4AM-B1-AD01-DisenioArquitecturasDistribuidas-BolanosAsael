using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventario.Api.Models;

[Table("MovimientoInventario")]
public class MovimientoInventario
{
    [Key]
    public int IdMovimiento { get; set; }

    [Required(ErrorMessage = "El identificador del producto es requerido.")]
    [Range(1, int.MaxValue, ErrorMessage = "El IdProducto debe ser un entero mayor a cero.")]
    public int IdProducto { get; set; }

    [Required(ErrorMessage = "El identificador de la bodega es requerido.")]
    [Range(1, int.MaxValue, ErrorMessage = "El IdBodega debe ser un entero mayor a cero.")]
    public int IdBodega { get; set; }

    [Required(ErrorMessage = "Debe especificar el tipo de movimiento (ENTRADA, SALIDA o AJUSTE).")]
    [RegularExpression("^(ENTRADA|SALIDA|AJUSTE)$", ErrorMessage = "Valores permitidos para Tipo: ENTRADA, SALIDA, AJUSTE.")]
    [StringLength(50)]
    public string Tipo { get; set; } = "ENTRADA";

    [Required(ErrorMessage = "Debe indicar la cantidad de unidades del movimiento.")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1.")]
    public int Cantidad { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    [ForeignKey(nameof(IdBodega))]
    public Bodega? Bodega { get; set; }
}
