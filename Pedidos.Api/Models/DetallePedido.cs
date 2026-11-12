using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Pedidos.Api.Models;

[Table("DetallePedido")]
public class DetallePedido
{
    [Key]
    public int IdDetalle { get; set; }

    public int IdPedido { get; set; }

    [Required(ErrorMessage = "El identificador del producto es requerido.")]
    [Range(1, int.MaxValue, ErrorMessage = "El IdProducto debe ser un entero mayor a cero.")]
    public int IdProducto { get; set; }

    [Required(ErrorMessage = "Debe indicar la cantidad de unidades solicitadas.")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad mínima permitida es 1.")]
    public int Cantidad { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, 999999.99, ErrorMessage = "El precio unitario debe ser mayor a cero.")]
    public decimal PrecioUnitario { get; set; }

    [JsonIgnore]
    [ForeignKey(nameof(IdPedido))]
    public Pedido? Pedido { get; set; }
}
