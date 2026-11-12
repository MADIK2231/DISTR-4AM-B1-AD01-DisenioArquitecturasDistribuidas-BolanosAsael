using Inventario.Api.Data;
using Inventario.Api.Models;
using Inventario.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovimientosInventarioController(InventarioDbContext db, CatalogoClient servicioCatalogo) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MovimientoInventario>>> GetMovimientos()
    {
        var historial = await db.MovimientosInventario
            .Include(m => m.Bodega)
            .OrderByDescending(m => m.Fecha)
            .AsNoTracking()
            .ToListAsync();

        return Ok(historial);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MovimientoInventario>> GetMovimiento(int id)
    {
        var registro = await db.MovimientosInventario
            .Include(m => m.Bodega)
            .FirstOrDefaultAsync(m => m.IdMovimiento == id);

        if (registro is null)
        {
            return NotFound(new { error = $"No existe el movimiento de inventario con ID {id}." });
        }

        return Ok(registro);
    }

    [HttpPost]
    public async Task<ActionResult<MovimientoInventario>> PostMovimiento([FromBody] MovimientoInventario modelo)
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

        var existenciaActual = await db.Existencias
            .FirstOrDefaultAsync(e => e.IdProducto == modelo.IdProducto && e.IdBodega == modelo.IdBodega);

        switch (modelo.Tipo)
        {
            case "ENTRADA":
                if (existenciaActual is null)
                {
                    db.Existencias.Add(new Existencia
                    {
                        IdProducto = modelo.IdProducto,
                        IdBodega = modelo.IdBodega,
                        Stock = modelo.Cantidad
                    });
                }
                else
                {
                    existenciaActual.Stock += modelo.Cantidad;
                }
                break;

            case "SALIDA":
                if (existenciaActual is null || existenciaActual.Stock < modelo.Cantidad)
                {
                    return BadRequest(new
                    {
                        error = $"Stock insuficiente en bodega {modelo.IdBodega} para el producto {modelo.IdProducto}. Disponible: {existenciaActual?.Stock ?? 0}."
                    });
                }
                existenciaActual.Stock -= modelo.Cantidad;
                break;

            case "AJUSTE":
                if (existenciaActual is null)
                {
                    db.Existencias.Add(new Existencia
                    {
                        IdProducto = modelo.IdProducto,
                        IdBodega = modelo.IdBodega,
                        Stock = modelo.Cantidad
                    });
                }
                else
                {
                    existenciaActual.Stock = modelo.Cantidad;
                }
                break;
        }

        modelo.Fecha = DateTime.Now;
        db.MovimientosInventario.Add(modelo);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMovimiento), new { id = modelo.IdMovimiento }, modelo);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutMovimiento(int id, [FromBody] MovimientoInventario modelo)
    {
        if (id != modelo.IdMovimiento)
        {
            return BadRequest(new { error = "El ID de la URL no coincide con el ID del movimiento." });
        }

        if (!await db.MovimientosInventario.AnyAsync(m => m.IdMovimiento == id))
        {
            return NotFound(new { error = $"No existe el movimiento con ID {id}." });
        }

        if (!await db.Bodegas.AnyAsync(b => b.IdBodega == modelo.IdBodega))
        {
            return BadRequest(new { error = $"FK Física inválida: la bodega con ID {modelo.IdBodega} no existe." });
        }

        if (!await servicioCatalogo.ValidarProductoEnCatalogoAsync(modelo.IdProducto))
        {
            return BadRequest(new { error = $"FK Lógica inválida: el producto con ID {modelo.IdProducto} no existe en Catalogo.Api." });
        }

        db.Entry(modelo).State = EntityState.Modified;
        await db.SaveChangesAsync();

        return Ok(modelo);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteMovimiento(int id)
    {
        var registro = await db.MovimientosInventario.FindAsync(id);
        if (registro is null)
        {
            return NotFound(new { error = $"No existe el movimiento con ID {id}." });
        }

        db.MovimientosInventario.Remove(registro);
        await db.SaveChangesAsync();

        return Ok(new { resultado = "Movimiento eliminado exitosamente." });
    }
}
