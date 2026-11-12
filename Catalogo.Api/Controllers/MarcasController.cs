using Catalogo.Api.Data;
using Catalogo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarcasController(CatalogoDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Marca>>> GetMarcas()
    {
        var lista = await db.Marcas.AsNoTracking().ToListAsync();
        return Ok(lista);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Marca>> GetMarca(int id)
    {
        var registro = await db.Marcas.FindAsync(id);
        if (registro is null)
        {
            return NotFound(new { error = $"No se encontró la marca con ID {id}." });
        }

        return Ok(registro);
    }

    [HttpPost]
    public async Task<ActionResult<Marca>> PostMarca([FromBody] Marca modelo)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        db.Marcas.Add(modelo);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMarca), new { id = modelo.IdMarca }, modelo);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutMarca(int id, [FromBody] Marca modelo)
    {
        if (id != modelo.IdMarca)
        {
            return BadRequest(new { error = "El identificador de la ruta no coincide con el cuerpo de la petición." });
        }

        var existeMarca = await db.Marcas.AnyAsync(x => x.IdMarca == id);
        if (!existeMarca)
        {
            return NotFound(new { error = $"No se encontró la marca con ID {id} para actualizar." });
        }

        db.Entry(modelo).State = EntityState.Modified;
        await db.SaveChangesAsync();

        return Ok(modelo);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteMarca(int id)
    {
        var registro = await db.Marcas.FindAsync(id);
        if (registro is null)
        {
            return NotFound(new { error = $"No se encontró la marca con ID {id}." });
        }

        var enUso = await db.Productos.AnyAsync(p => p.IdMarca == id);
        if (enUso)
        {
            return BadRequest(new { error = "Restricción de llave foránea física: existen productos vinculados a esta marca." });
        }

        db.Marcas.Remove(registro);
        await db.SaveChangesAsync();

        return Ok(new { resultado = "Marca eliminada exitosamente." });
    }
}
