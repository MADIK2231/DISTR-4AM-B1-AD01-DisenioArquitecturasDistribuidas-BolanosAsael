using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Catalogo.Api.Models;

[Table("Categoria")]
public class Categoria
{
    [Key]
    public int IdCategoria { get; set; }

    [Required(ErrorMessage = "Debe especificar el nombre de la categoría.")]
    [StringLength(100, ErrorMessage = "El nombre de la categoría admite hasta 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "La descripción admite hasta 250 caracteres.")]
    public string? Descripcion { get; set; }

    [JsonIgnore]
    public ICollection<Producto>? Productos { get; set; }
}
