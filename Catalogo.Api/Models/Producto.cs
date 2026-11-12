using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Catalogo.Api.Models;

[Table("Producto")]
public class Producto
{
    [Key]
    public int IdProducto { get; set; }

    [Required(ErrorMessage = "El identificador de categoría es requerido.")]
    [Range(1, int.MaxValue, ErrorMessage = "El IdCategoria debe ser un número positivo válido.")]
    public int IdCategoria { get; set; }

    [Required(ErrorMessage = "El identificador de marca es requerido.")]
    [Range(1, int.MaxValue, ErrorMessage = "El IdMarca debe ser un número positivo válido.")]
    public int IdMarca { get; set; }

    [Required(ErrorMessage = "Debe ingresar el nombre comercial del producto.")]
    [StringLength(150, ErrorMessage = "El nombre del producto admite hasta 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe ingresar el código SKU del producto.")]
    [StringLength(50, ErrorMessage = "El código SKU admite hasta 50 caracteres.")]
    public string SKU { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe indicar el precio unitario del producto.")]
    [Range(0.01, 999999.99, ErrorMessage = "El precio debe ser mayor que cero.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Precio { get; set; }

    public bool Estado { get; set; } = true;

    [ForeignKey(nameof(IdCategoria))]
    public Categoria? Categoria { get; set; }

    [ForeignKey(nameof(IdMarca))]
    public Marca? Marca { get; set; }
}
