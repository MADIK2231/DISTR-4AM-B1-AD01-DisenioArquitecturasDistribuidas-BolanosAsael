using Catalogo.Api.Data;
using Catalogo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController(CatalogoDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Categoria>>> GetCategorias()
    {
        var lista = await db.Categorias.AsNoTracking().ToListAsync();
        return Ok(lista);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Categoria>> GetCategoria(int id)
    {
        var registro = await db.Categorias.FindAsync(id);
        if (registro is null)
        {
            return NotFound(new { error = $"No se encontró la categoría con ID {id}." });
        }

        return Ok(registro);
    }

    [HttpPost]
    public async Task<ActionResult<Categoria>> PostCategoria([FromBody] Categoria modelo)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        db.Categorias.Add(modelo);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategoria), new { id = modelo.IdCategoria }, modelo);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutCategoria(int id, [FromBody] Categoria modelo)
    {
        if (id != modelo.IdCategoria)
        {
            return BadRequest(new { error = "El identificador de la ruta no coincide con el cuerpo de la petición." });
        }

        var existeCategoria = await db.Categorias.AnyAsync(x => x.IdCategoria == id);
        if (!existeCategoria)
        {
            return NotFound(new { error = $"No se encontró la categoría con ID {id} para actualizar." });
        }

        db.Entry(modelo).State = EntityState.Modified;
        await db.SaveChangesAsync();

        return Ok(modelo);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategoria(int id)
    {
        var registro = await db.Categorias.FindAsync(id);
        if (registro is null)
        {
            return NotFound(new { error = $"No se encontró la categoría con ID {id}." });
        }

        var enUso = await db.Productos.AnyAsync(p => p.IdCategoria == id);
        if (enUso)
        {
            return BadRequest(new { error = "Restricción de llave foránea física: existen productos vinculados a esta categoría." });
        }

        db.Categorias.Remove(registro);
        await db.SaveChangesAsync();

        return Ok(new { resultado = "Categoría eliminada exitosamente." });
    }
}
