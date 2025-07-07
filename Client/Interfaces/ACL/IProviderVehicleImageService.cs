using System.Collections.Generic;
using System.Threading.Tasks;
using DM = BackendRent2Go.Client.Domain.Model.ACL;

namespace BackendRent2Go.Client.Interfaces.ACL
{
    /// <summary>
    /// Interfaz ACL para acceder a servicios de imágenes de vehículos del bounded context Provider desde Client
    /// </summary>
    public interface IProviderVehicleImageService
    {
        /// <summary>
        /// Obtiene todas las imágenes asociadas a un vehículo específico
        /// </summary>
        Task<IEnumerable<DM.ProviderVehicleImageDto>> FindByVehicleIdAsync(string vehicleId);
    }
}
