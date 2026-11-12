using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Pedidos.Api.Models;

[Table("Cliente")]
public class Cliente
{
    [Key]
    public int IdCliente { get; set; }

    [Required(ErrorMessage = "Debe ingresar el nombre completo del cliente.")]
    [StringLength(150, ErrorMessage = "El nombre admite hasta 150 caracteres.")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe ingresar el número de cédula o RUC.")]
    [StringLength(20, MinimumLength = 10, ErrorMessage = "La identificación debe contener entre 10 y 20 caracteres.")]
    public string Cedula { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe ingresar el correo electrónico del cliente.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no posee un formato válido.")]
    [StringLength(150)]
    public string Correo { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<Pedido>? Pedidos { get; set; }
}
