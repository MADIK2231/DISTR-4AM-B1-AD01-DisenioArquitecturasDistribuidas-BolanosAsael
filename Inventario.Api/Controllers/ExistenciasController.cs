using Inventario.Api.Data;
using Inventario.Api.Models;
using Inventario.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExistenciasController(InventarioDbContext db, CatalogoClient servicioCatalogo) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Existencia>>> GetExistencias()
    {
        var lista = await db.Existencias
            .Include(e => e.Bodega)
            .AsNoTracking()
            .ToListAsync();

        return Ok(lista);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Existencia>> GetExistencia(int id)
    {
        var registro = await db.Existencias
            .Include(e => e.Bodega)
            .FirstOrDefaultAsync(e => e.IdExistencia == id);

        if (registro is null)
        {
            return NotFound(new { error = $"No existe el registro de existencia con ID {id}." });
        }

        return Ok(registro);
    }

    [HttpGet("producto/{idProducto:int}")]
    public async Task<ActionResult<IEnumerable<Existencia>>> GetExistenciasPorProducto(int idProducto)
    {
        var disponibles = await db.Existencias
            .Include(e => e.Bodega)
            .Where(e => e.IdProducto == idProducto)
            .AsNoTracking()
            .ToListAsync();

        return Ok(disponibles);
    }

    [HttpPost]
    public async Task<ActionResult<Existencia>> PostExistencia([FromBody] Existencia modelo)
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

        if (await db.Existencias.AnyAsync(e => e.IdProducto == modelo.IdProducto && e.IdBodega == modelo.IdBodega))
        {
            return BadRequest(new { error = "Ya existe un registro de stock para ese producto en la bodega indicada." });
        }

        db.Existencias.Add(modelo);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetExistencia), new { id = modelo.IdExistencia }, modelo);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutExistencia(int id, [FromBody] Existencia modelo)
    {
        if (id != modelo.IdExistencia)
        {
            return BadRequest(new { error = "El ID de la URL no coincide con el cuerpo de la solicitud." });
        }

        if (!await db.Existencias.AnyAsync(e => e.IdExistencia == id))
        {
            return NotFound(new { error = $"No existe el registro de existencia con ID {id}." });
        }

        if (!await db.Bodegas.AnyAsync(b => b.IdBodega == modelo.IdBodega))
        {
            return BadRequest(new { error = $"FK Física inválida: la bodega con ID {modelo.IdBodega} no existe." });
        }

        if (!await servicioCatalogo.ValidarProductoEnCatalogoAsync(modelo.IdProducto))
        {
            return BadRequest(new { error = $"FK Lógica inválida: el producto con ID {modelo.IdProducto} no existe o está inactivo en Catalogo.Api." });
        }

        if (await db.Existencias.AnyAsync(e => e.IdProducto == modelo.IdProducto && e.IdBodega == modelo.IdBodega && e.IdExistencia != id))
        {
            return BadRequest(new { error = "Otro registro ya asocia este producto con la bodega seleccionada." });
        }

        db.Entry(modelo).State = EntityState.Modified;
        await db.SaveChangesAsync();

        return Ok(modelo);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteExistencia(int id)
    {
        var registro = await db.Existencias.FindAsync(id);
        if (registro is null)
        {
            return NotFound(new { error = $"No existe el registro de existencia con ID {id}." });
        }

        db.Existencias.Remove(registro);
        await db.SaveChangesAsync();

        return Ok(new { resultado = "Existencia eliminada exitosamente." });
    }
}
