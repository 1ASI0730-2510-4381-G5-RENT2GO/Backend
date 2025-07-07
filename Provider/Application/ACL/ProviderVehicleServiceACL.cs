using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rent2Go.API.Provider.Domain.Model;
using Rent2Go.API.Provider.Domain.Repositories;
using Rent2Go.API.Provider.Domain.Services;
using Rent2Go.API.Provider.Interfaces.ACL;

namespace Rent2Go.API.Provider.Application.ACL
{
    /// <summary>
    /// Implementación de la capa ACL para servicios de vehículos
    /// Actúa como traductor entre el contexto de Provider y otros contextos como Client
    /// </summary>
    public class ProviderVehicleServiceACL : IProviderVehicleService
    {
        private readonly IVehicleService _vehicleService;
        private readonly IVehicleRepository _vehicleRepository;

        public ProviderVehicleServiceACL(
            IVehicleService vehicleService,
            IVehicleRepository vehicleRepository)
        {
            _vehicleService = vehicleService;
            _vehicleRepository = vehicleRepository;
        }

        public async Task<IEnumerable<Vehicle>> FindByStatusAsync(string status)
        {
            // Adaptamos la llamada al repositorio subyacente
            var vehicles = await _vehicleRepository.ListByOwnerAsync(null);
            
            // Filtramos por estado y devolvemos solo los que coinciden
            return vehicles.Where(v => v.Status.ToLower() == status.ToLower());
        }

        public async Task<Vehicle> GetVehicleByIdAsync(string id)
        {
            // Simplemente delegamos al servicio subyacente
            return await _vehicleService.GetVehicleByIdAsync(id);
        }

        public async Task<IEnumerable<Vehicle>> GetVehiclesByOwnerIdAsync(string ownerId)
        {
            // Delegamos al servicio subyacente
            return await _vehicleService.GetVehiclesByOwnerIdAsync(ownerId);
        }

        public async Task<Vehicle> UpdateVehicleStatusAsync(string id, string status)
        {
            // Delegamos al servicio subyacente
            return await _vehicleService.UpdateVehicleStatusAsync(id, status);
        }
    }
}
