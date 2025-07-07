using System.Collections.Generic;
using System.Threading.Tasks;
using DM = BackendRent2Go.Client.Domain.Model.ACL;

namespace BackendRent2Go.Client.Interfaces.ACL
{
    /// <summary>
    /// Interfaz ACL para acceder a servicios de vehículos del bounded context Provider desde Client
    /// </summary>
    public interface IProviderVehicleService
    {
        /// <summary>
        /// Obtiene vehículos con un estado específico
        /// </summary>
        Task<IEnumerable<DM.ProviderVehicleDto>> FindByStatusAsync(string status);
        
        /// <summary>
        /// Obtiene un vehículo por su ID
        /// </summary>
        Task<DM.ProviderVehicleDto> GetVehicleByIdAsync(string id);
    }
}
