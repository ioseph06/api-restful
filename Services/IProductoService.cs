using MiPrimeraApi.DTOs;
using MiPrimeraApi.Common;

namespace MiPrimeraApi.Services
{
    public interface IProductoService
    {
         Task<PagedResult<ProductoDto>> ObtenerPaginadoAsync(ProductoQueryParams queryParams);         
        Task<ProductoDto?> ObtenerPorIdAsync(int id);
        Task<ProductoDto> CrearAsync(CrearProductoDto dto);
        Task<bool> ActualizarAsync(int id, ActualizarProductoDto dto);
        Task<bool> EliminarAsync(int id);
        
    }
}
