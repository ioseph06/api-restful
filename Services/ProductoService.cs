using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed; // ✅ Nuevo using
using MiPrimeraApi.Data;
using MiPrimeraApi.DTOs;
using MiPrimeraApi.Models;
using System.Collections.Generic;
using System.Text.Json; // ✅ Nuevo using
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // Para ver si funciona

namespace MiPrimeraApi.Services
{
    public class ProductoService : IProductoService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache; // ✅ Inyectado
        private readonly ILogger<ProductoService> _logger;
        private const string CacheKeyTodos = "productos_todos";
        public ProductoService(AppDbContext context, IMapper mapper, IDistributedCache cache, ILogger<ProductoService> logger)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductoDto>> ObtenerTodosAsync()
        {
            // 1. INTENTAR OBTENER DE LA CACHÉ
            var cachedData = await _cache.GetStringAsync(CacheKeyTodos);
            
            if (!string.IsNullOrEmpty(cachedData))
            {
                _logger.LogInformation("✅ Datos obtenidos de REDIS (Caché)");
             
                var deserializedData = JsonSerializer.Deserialize<IEnumerable<ProductoDto>>(cachedData);
                return deserializedData ?? Enumerable.Empty<ProductoDto>();

            }

            // 2. SI NO ESTÁ EN CACHÉ, IR A LA BASE DE DATOS
            _logger.LogInformation("⚠️ Datos NO encontrados en caché. Consultando Base de Datos...");
            var productos = await _context.Productos.ToListAsync();
            var productosDto = _mapper.Map<IEnumerable<ProductoDto>>(productos);

            // 3. GUARDAR EN CACHÉ PARA LA PRÓXIMA VEZ
            var cacheOptions = new DistributedCacheEntryOptions
            {
                // La caché expirará 5 minutos después del último acceso (Sliding)
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };

            var serializedData = JsonSerializer.Serialize(productosDto);
            await _cache.SetStringAsync(CacheKeyTodos, serializedData, cacheOptions);

            return productosDto;
        }

        public async Task<ProductoDto?> ObtenerPorIdAsync(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return null;
            
            return _mapper.Map<ProductoDto>(producto);
        }

        public async Task<ProductoDto> CrearAsync(CrearProductoDto dto)
        {
            var producto = _mapper.Map<Producto>(dto);
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
            
            // 🚨 4. INVALIDAR LA CACHÉ (¡MUY IMPORTANTE!)
            // Si creamos un producto, la lista "todos" ya no es válida. La borramos.
            await _cache.RemoveAsync(CacheKeyTodos);
            _logger.LogInformation("🗑️ Caché de 'todos los productos' invalidada por creación.");

            return _mapper.Map<ProductoDto>(producto);
        }

        public async Task<bool> ActualizarAsync(int id, ActualizarProductoDto dto)
        {
            if (id != dto.Id) return false;

            var existente = await _context.Productos.FindAsync(id);
            if (existente == null) return false;

            // Mapeamos los campos del DTO a la entidad existente
            _mapper.Map(dto, existente);
            
            await _context.SaveChangesAsync();
            
            await _cache.RemoveAsync(CacheKeyTodos);
            return true;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return false;

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
            await _cache.RemoveAsync(CacheKeyTodos);
            return true;
        }
    }
}