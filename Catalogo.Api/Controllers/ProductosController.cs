using Catalogo.Api.Data;
using Catalogo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController(CatalogoDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
    {
        var listado = await db.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .AsNoTracking()
            .ToListAsync();

        return Ok(listado);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Producto>> GetProducto(int id)
    {
        var item = await db.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .FirstOrDefaultAsync(p => p.IdProducto == id);

        if (item is null)
        {
            return NotFound(new { error = $"El producto con ID {id} no está registrado en CatalogoDB." });
        }

        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<Producto>> PostProducto([FromBody] Producto modelo)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!await db.Categorias.AnyAsync(c => c.IdCategoria == modelo.IdCategoria))
        {
            return BadRequest(new { error = $"FK Física inválida: no existe la categoría con ID {modelo.IdCategoria}." });
        }

        if (!await db.Marcas.AnyAsync(m => m.IdMarca == modelo.IdMarca))
        {
            return BadRequest(new { error = $"FK Física inválida: no existe la marca con ID {modelo.IdMarca}." });
        }

        if (await db.Productos.AnyAsync(p => p.SKU == modelo.SKU))
        {
            return BadRequest(new { error = $"El código SKU '{modelo.SKU}' ya se encuentra asignado a otro producto." });
        }

        db.Productos.Add(modelo);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProducto), new { id = modelo.IdProducto }, modelo);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutProducto(int id, [FromBody] Producto modelo)
    {
        if (id != modelo.IdProducto)
        {
            return BadRequest(new { error = "El ID de la URL no corresponde al ID del producto enviado." });
        }

        if (!await db.Productos.AnyAsync(p => p.IdProducto == id))
        {
            return NotFound(new { error = $"El producto con ID {id} no existe." });
        }

        if (!await db.Categorias.AnyAsync(c => c.IdCategoria == modelo.IdCategoria))
        {
            return BadRequest(new { error = $"FK Física inválida: no existe la categoría con ID {modelo.IdCategoria}." });
        }

        if (!await db.Marcas.AnyAsync(m => m.IdMarca == modelo.IdMarca))
        {
            return BadRequest(new { error = $"FK Física inválida: no existe la marca con ID {modelo.IdMarca}." });
        }

        if (await db.Productos.AnyAsync(p => p.SKU == modelo.SKU && p.IdProducto != id))
        {
            return BadRequest(new { error = $"El código SKU '{modelo.SKU}' ya pertenece a otro registro." });
        }

        db.Entry(modelo).State = EntityState.Modified;
        await db.SaveChangesAsync();

        return Ok(modelo);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProducto(int id)
    {
        var item = await db.Productos.FindAsync(id);
        if (item is null)
        {
            return NotFound(new { error = $"El producto con ID {id} no existe." });
        }

        db.Productos.Remove(item);
        await db.SaveChangesAsync();

        return Ok(new { resultado = "Producto eliminado exitosamente." });
    }
}
