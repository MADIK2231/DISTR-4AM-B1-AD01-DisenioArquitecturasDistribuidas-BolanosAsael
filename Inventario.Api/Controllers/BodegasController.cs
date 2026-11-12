using Inventario.Api.Data;
using Inventario.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BodegasController(InventarioDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Bodega>>> GetBodegas()
    {
        return Ok(await db.Bodegas.AsNoTracking().ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Bodega>> GetBodega(int id)
    {
        var bodega = await db.Bodegas.FindAsync(id);
        if (bodega is null)
        {
            return NotFound(new { error = $"No se encontró la bodega con ID {id}." });
        }

        return Ok(bodega);
    }

    [HttpPost]
    public async Task<ActionResult<Bodega>> PostBodega([FromBody] Bodega modelo)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        db.Bodegas.Add(modelo);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBodega), new { id = modelo.IdBodega }, modelo);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutBodega(int id, [FromBody] Bodega modelo)
    {
        if (id != modelo.IdBodega)
        {
            return BadRequest(new { error = "El ID de la URL no coincide con el ID de la bodega." });
        }

        if (!await db.Bodegas.AnyAsync(b => b.IdBodega == id))
        {
            return NotFound(new { error = $"No se encontró la bodega con ID {id}." });
        }

        db.Entry(modelo).State = EntityState.Modified;
        await db.SaveChangesAsync();

        return Ok(modelo);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteBodega(int id)
    {
        var bodega = await db.Bodegas.FindAsync(id);
        if (bodega is null)
        {
            return NotFound(new { error = $"No se encontró la bodega con ID {id}." });
        }

        var poseeDependencias = await db.Existencias.AnyAsync(e => e.IdBodega == id)
            || await db.MovimientosInventario.AnyAsync(m => m.IdBodega == id)
            || await db.ReservasStock.AnyAsync(r => r.IdBodega == id);

        if (poseeDependencias)
        {
            return BadRequest(new { error = "Restricción de llave foránea física: la bodega registra existencias, movimientos o reservas." });
        }

        db.Bodegas.Remove(bodega);
        await db.SaveChangesAsync();

        return Ok(new { resultado = "Bodega eliminada exitosamente." });
    }
}
