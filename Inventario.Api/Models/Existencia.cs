using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventario.Api.Models;

[Table("Existencia")]
public class Existencia
{
    [Key]
    public int IdExistencia { get; set; }

    [Required(ErrorMessage = "El identificador del producto es requerido.")]
    [Range(1, int.MaxValue, ErrorMessage = "El IdProducto debe ser un entero mayor a cero.")]
    public int IdProducto { get; set; }

    [Required(ErrorMessage = "El identificador de la bodega es requerido.")]
    [Range(1, int.MaxValue, ErrorMessage = "El IdBodega debe ser un entero mayor a cero.")]
    public int IdBodega { get; set; }

    [Required(ErrorMessage = "Debe indicar la cantidad de stock disponible.")]
    [Range(0, int.MaxValue, ErrorMessage = "El stock disponible no puede ser menor que cero.")]
    public int Stock { get; set; }

    [ForeignKey(nameof(IdBodega))]
    public Bodega? Bodega { get; set; }
}
