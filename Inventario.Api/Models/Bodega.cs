using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Inventario.Api.Models;

[Table("Bodega")]
public class Bodega
{
    [Key]
    public int IdBodega { get; set; }

    [Required(ErrorMessage = "Debe ingresar el nombre de la bodega.")]
    [StringLength(100, ErrorMessage = "El nombre de la bodega admite hasta 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe ingresar la ciudad donde opera la bodega.")]
    [StringLength(100, ErrorMessage = "La ciudad admite hasta 100 caracteres.")]
    public string Ciudad { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe ingresar la dirección física de la bodega.")]
    [StringLength(200, ErrorMessage = "La dirección admite hasta 200 caracteres.")]
    public string Direccion { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<Existencia>? Existencias { get; set; }

    [JsonIgnore]
    public ICollection<MovimientoInventario>? Movimientos { get; set; }

    [JsonIgnore]
    public ICollection<ReservaStock>? Reservas { get; set; }
}
