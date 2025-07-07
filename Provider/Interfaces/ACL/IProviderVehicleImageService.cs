using System.Collections.Generic;
using System.Threading.Tasks;
using Rent2Go.API.Provider.Domain.Model;

namespace Rent2Go.API.Provider.Interfaces.ACL
{
    /// <summary>
    /// Interfaz ACL que expone operaciones sobre imágenes de vehículos
    /// desde el contexto de Provider a otros contextos como Client.
    /// </summary>
    public interface IProviderVehicleImageService
    {
        /// <summary>
        /// Obtiene todas las imágenes asociadas a un vehículo específico
        /// </summary>
        Task<IEnumerable<VehicleImage>> FindByVehicleIdAsync(string vehicleId);
        
        /// <summary>
        /// Obtiene una imagen específica por su identificador
        /// </summary>
        Task<VehicleImage> FindByIdAsync(string id);
    }
}
