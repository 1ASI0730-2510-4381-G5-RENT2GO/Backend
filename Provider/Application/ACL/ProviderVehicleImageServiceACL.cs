using System.Collections.Generic;
using System.Threading.Tasks;
using Rent2Go.API.Provider.Domain.Model;
using Rent2Go.API.Provider.Domain.Repositories;
using Rent2Go.API.Provider.Interfaces.ACL;

namespace Rent2Go.API.Provider.Application.ACL
{
    /// <summary>
    /// Implementación de la capa ACL para servicios de imágenes de vehículos
    /// Actúa como traductor entre el contexto de Provider y otros contextos como Client
    /// </summary>
    public class ProviderVehicleImageServiceACL : IProviderVehicleImageService
    {
        private readonly IVehicleImageRepository _vehicleImageRepository;

        public ProviderVehicleImageServiceACL(IVehicleImageRepository vehicleImageRepository)
        {
            _vehicleImageRepository = vehicleImageRepository;
        }

        public async Task<VehicleImage> FindByIdAsync(string id)
        {
            return await _vehicleImageRepository.FindByIdAsync(id);
        }

        public async Task<IEnumerable<VehicleImage>> FindByVehicleIdAsync(string vehicleId)
        {
            return await _vehicleImageRepository.GetByVehicleIdAsync(vehicleId);
        }
    }
}
