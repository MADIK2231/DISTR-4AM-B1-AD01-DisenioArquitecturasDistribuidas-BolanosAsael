using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Catalogo.Api.Models;

[Table("Marca")]
public class Marca
{
    [Key]
    public int IdMarca { get; set; }

    [Required(ErrorMessage = "Debe especificar el nombre de la marca.")]
    [StringLength(100, ErrorMessage = "El nombre de la marca admite hasta 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "El país de origen admite hasta 100 caracteres.")]
    public string? PaisOrigen { get; set; }

    [JsonIgnore]
    public ICollection<Producto>? Productos { get; set; }
}
