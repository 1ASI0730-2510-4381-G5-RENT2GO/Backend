using System.Collections.Generic;
using System.Threading.Tasks;
using Rent2Go.API.Provider.Domain.Model;

namespace Rent2Go.API.Provider.Interfaces.ACL
{
    /// <summary>
    /// Interfaz ACL (Anti-Corruption Layer) que expone servicios de vehículos del contexto Provider
    /// a otros contextos delimitados como Client, manteniendo la integridad del modelo de dominio.
    /// </summary>
    public interface IProviderVehicleService
    {
        /// <summary>
        /// Obtiene vehículos disponibles con un determinado estado
        /// </summary>
        Task<IEnumerable<Vehicle>> FindByStatusAsync(string status);
        
        /// <summary>
        /// Obtiene un vehículo por su identificador
        /// </summary>
        Task<Vehicle> GetVehicleByIdAsync(string id);
        
        /// <summary>
        /// Obtiene todos los vehículos de un propietario específico
        /// </summary>
        Task<IEnumerable<Vehicle>> GetVehiclesByOwnerIdAsync(string ownerId);
        
        /// <summary>
        /// Actualiza el estado de un vehículo
        /// </summary>
        Task<Vehicle> UpdateVehicleStatusAsync(string id, string status);
    }
}
