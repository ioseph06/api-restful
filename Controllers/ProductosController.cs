using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiPrimeraApi.DTOs;
using MiPrimeraApi.Services;
using Microsoft.AspNetCore.RateLimiting; // ✅ Nuevo using
using Asp.Versioning; // ✅ Nuevo using

namespace MiPrimeraApi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")] // ✅ Declaramos que este controlador es la v1.0
                        //[Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    [EnableRateLimiting("LimitePorIP")] // Aplica la política de rate limiting a este controlador
    public class ProductosController : ControllerBase
    {
        private readonly IProductoService _productoService;
        private readonly ILogger<ProductosController> _logger; // 🌟 Inyección del Logger
        public ProductosController(IProductoService productoService, ILogger<ProductosController> logger)
        {
            _productoService = productoService;
            _logger = logger;
        }


        // Controllers/ProductosController.cs
        //[HttpGet]
        //public async Task<ActionResult<PagedResult<ProductoDto>>> ObtenerPaginado([AsParameters] ProductoQueryParams queryParams)
        //{
        //    var resultado = await _productoService.ObtenerPaginadoAsync(queryParams);
        //    return Ok(resultado);
        //}

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> ObtenerTodos(
            [FromQuery] string? nombre,
            [FromQuery] bool? enStock)
        {
            var productos = await _productoService.ObtenerTodosAsync();

            if (!string.IsNullOrWhiteSpace(nombre))
                productos = productos.Where(p => p.Nombre.Contains(nombre, System.StringComparison.OrdinalIgnoreCase));

            if (enStock.HasValue)
                productos = productos.Where(p => p.EnStock == enStock.Value);

            return Ok(productos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductoDto>> ObtenerPorId(int id)
        {
            var producto = await _productoService.ObtenerPorIdAsync(id);
            if (producto == null) return NotFound();
            return Ok(producto);
        }

        [HttpPost]
        [Authorize] // Solo los usuarios con rol "Admin" pueden crear productos
        public async Task<ActionResult<ProductoDto>> Crear([FromBody] CrearProductoDto nuevoProducto)
        {
            _logger.LogInformation("Intentando crear producto: {ProductName} con precio {Price}",
                nuevoProducto.Nombre,
                nuevoProducto.Precio);

            try
            {
                var creado = await _productoService.CrearAsync(nuevoProducto);
                // 🌟 Log de éxito con datos estructurados (fácil de buscar después)
                _logger.LogInformation("Producto creado exitosamente con ID: {ProductId}", creado.Id);

                return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
            }
            catch (Exception ex)
            {
                // 🌟 Log de advertencia con la excepción adjunta
                _logger.LogWarning(ex, "Fallo al crear el producto {ProductName}", nuevoProducto.Nombre);
                throw; // Dejamos que el Middleware global lo maneje
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarProductoDto producto)
        {
            if (!await _productoService.ActualizarAsync(id, producto))
                return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Eliminar(int id)
        {
            if (!await _productoService.EliminarAsync(id))
                return NotFound();
            return NoContent();
        }



    }
}