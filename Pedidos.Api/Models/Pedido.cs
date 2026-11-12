using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pedidos.Api.Models;

[Table("Pedido")]
public class Pedido
{
    [Key]
    public int IdPedido { get; set; }

    [Required(ErrorMessage = "El identificador del cliente es requerido.")]
    [Range(1, int.MaxValue, ErrorMessage = "El IdCliente debe ser un entero mayor a cero.")]
    public int IdCliente { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 9999999.99, ErrorMessage = "El monto total no puede ser negativo.")]
    public decimal Total { get; set; }

    [Required(ErrorMessage = "Debe indicar el estado del pedido.")]
    [RegularExpression("^(PENDIENTE|PAGADO|CANCELADO|PROCESADO)$", ErrorMessage = "Valores permitidos para Estado: PENDIENTE, PAGADO, CANCELADO, PROCESADO.")]
    [StringLength(50)]
    public string Estado { get; set; } = "PENDIENTE";

    [ForeignKey(nameof(IdCliente))]
    public Cliente? Cliente { get; set; }

    public List<DetallePedido> Detalles { get; set; } = [];
}
