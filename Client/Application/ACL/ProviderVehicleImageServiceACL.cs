using System.Collections.Generic;
using System.Threading.Tasks;
using ClientInterfaces = BackendRent2Go.Client.Interfaces.ACL;
using ClientDomainModel = BackendRent2Go.Client.Domain.Model.ACL;
using ProviderACL = Rent2Go.API.Provider.Interfaces.ACL;

namespace BackendRent2Go.Client.Application.ACL
{
    /// <summary>
    /// Implementación de la capa ACL para servicios de imágenes de vehículos
    /// Traduce entre el contexto de Provider y Client
    /// </summary>
    public class ProviderVehicleImageServiceAcl : ClientInterfaces.IProviderVehicleImageService
    {
        private readonly ProviderACL.IProviderVehicleImageService _providerImageService;

        public ProviderVehicleImageServiceAcl(ProviderACL.IProviderVehicleImageService providerImageService)
        {
            _providerImageService = providerImageService;
        }

        public async Task<IEnumerable<ClientDomainModel.ProviderVehicleImageDto>> FindByVehicleIdAsync(string vehicleId)
        {
            var images = await _providerImageService.FindByVehicleIdAsync(vehicleId);
            var result = new List<ClientDomainModel.ProviderVehicleImageDto>();

            foreach (var image in images)
            {
                result.Add(new ClientDomainModel.ProviderVehicleImageDto
                {
                    Id = image.Id,
                    VehicleId = image.VehicleId,
                    ImageUrl = image.ImageUrl
                });
            }

            return result;
        }
    }
}
