using Inventario.Api.Data;
using Inventario.Api.Models;
using Inventario.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservasStockController(InventarioDbContext db, CatalogoClient servicioCatalogo) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReservaStock>>> GetReservas()
    {
        var reservas = await db.ReservasStock
            .Include(r => r.Bodega)
            .AsNoTracking()
            .ToListAsync();

        return Ok(reservas);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReservaStock>> GetReserva(int id)
    {
        var reserva = await db.ReservasStock
            .Include(r => r.Bodega)
            .FirstOrDefaultAsync(r => r.IdReserva == id);

        if (reserva is null)
        {
            return NotFound(new { error = $"No se encontró la reserva con ID {id}." });
        }

        return Ok(reserva);
    }

    [HttpPost]
    public async Task<ActionResult<ReservaStock>> PostReserva([FromBody] ReservaStock modelo)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!await db.Bodegas.AnyAsync(b => b.IdBodega == modelo.IdBodega))
        {
            return BadRequest(new { error = $"FK Física inválida: la bodega con ID {modelo.IdBodega} no existe." });
        }

        if (!await servicioCatalogo.ValidarProductoEnCatalogoAsync(modelo.IdProducto))
        {
            return BadRequest(new { error = $"FK Lógica inválida: el producto con ID {modelo.IdProducto} no existe o está inactivo en Catalogo.Api." });
        }

        var existencia = await db.Existencias
            .FirstOrDefaultAsync(e => e.IdProducto == modelo.IdProducto && e.IdBodega == modelo.IdBodega);

        if (existencia is null || existencia.Stock < modelo.Cantidad)
        {
            return BadRequest(new
            {
                error = $"Stock insuficiente para reservar en la bodega {modelo.IdBodega}. Disponible: {existencia?.Stock ?? 0}, Solicitado: {modelo.Cantidad}."
            });
        }

        existencia.Stock -= modelo.Cantidad;

        db.MovimientosInventario.Add(new MovimientoInventario
        {
            IdProducto = modelo.IdProducto,
            IdBodega = modelo.IdBodega,
            Tipo = "SALIDA",
            Cantidad = modelo.Cantidad,
            Fecha = DateTime.Now
        });

        db.ReservasStock.Add(modelo);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetReserva), new { id = modelo.IdReserva }, modelo);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutReserva(int id, [FromBody] ReservaStock modelo)
    {
        if (id != modelo.IdReserva)
        {
            return BadRequest(new { error = "El ID de la URL no coincide con el ID de la reserva." });
        }

        var actual = await db.ReservasStock.FindAsync(id);
        if (actual is null)
        {
            return NotFound(new { error = $"No se encontró la reserva con ID {id}." });
        }

        if (!await db.Bodegas.AnyAsync(b => b.IdBodega == modelo.IdBodega))
        {
            return BadRequest(new { error = $"FK Física inválida: la bodega con ID {modelo.IdBodega} no existe." });
        }

        if (!await servicioCatalogo.ValidarProductoEnCatalogoAsync(modelo.IdProducto))
        {
            return BadRequest(new { error = $"FK Lógica inválida: el producto con ID {modelo.IdProducto} no existe en Catalogo.Api." });
        }

        if (actual.Estado != "CANCELADA" && modelo.Estado == "CANCELADA")
        {
            var existencia = await db.Existencias
                .FirstOrDefaultAsync(e => e.IdProducto == actual.IdProducto && e.IdBodega == actual.IdBodega);

            if (existencia is not null)
            {
                existencia.Stock += actual.Cantidad;
            }
        }

        actual.IdProducto = modelo.IdProducto;
        actual.IdBodega = modelo.IdBodega;
        actual.Cantidad = modelo.Cantidad;
        actual.Estado = modelo.Estado;

        await db.SaveChangesAsync();

        return Ok(actual);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteReserva(int id)
    {
        var reserva = await db.ReservasStock.FindAsync(id);
        if (reserva is null)
        {
            return NotFound(new { error = $"No se encontró la reserva con ID {id}." });
        }

        db.ReservasStock.Remove(reserva);
        await db.SaveChangesAsync();

        return Ok(new { resultado = "Reserva de stock eliminada exitosamente." });
    }
}
