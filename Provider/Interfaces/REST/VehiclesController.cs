using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rent2Go.API.Provider.Domain.Model;
using Rent2Go.API.Provider.Domain.Services;
using Rent2Go.API.IAM.Domain.Repositories;

namespace Rent2Go.API.Provider.Interfaces.REST
{
    [ApiController]
    [Route("api/provider/vehicles")]
    [Authorize(Roles = "provider")]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;
        private readonly ILogger<VehiclesController> _logger;
        private readonly IProviderRepository _providerRepository;

        public VehiclesController(
            IVehicleService vehicleService,
            ILogger<VehiclesController> logger,
            IProviderRepository providerRepository)
        {
            _vehicleService = vehicleService;
            _logger = logger;
            _providerRepository = providerRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetVehicles()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                // Buscar el ID del proveedor
                var provider = await _providerRepository.GetByUserIdAsync(userId);
                
                if (provider == null)
                {
                    return BadRequest("No se encontró un perfil de proveedor para su cuenta");
                }
                
                _logger.LogInformation($"Obteniendo vehículos para proveedor: {provider.Id}");
                var vehicles = await _vehicleService.GetVehiclesByOwnerIdAsync(provider.Id);
                return Ok(vehicles);
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error al obtener vehículos: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Vehicle>> GetVehicle(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                // Buscar el ID del proveedor
                var provider = await _providerRepository.GetByUserIdAsync(userId);
                
                if (provider == null)
                {
                    return BadRequest("No se encontró un perfil de proveedor para su cuenta");
                }
                
                _logger.LogInformation($"Obteniendo vehículo: {id} para proveedor: {provider.Id}");
                var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
                
                // Verificar que el vehículo pertenezca al usuario autenticado
                if (vehicle.OwnerId != provider.Id)
                {
                    _logger.LogWarning($"Intento de acceso no autorizado a vehículo: {id} por proveedor: {provider.Id}");
                    return Forbid();
                }
                
                return Ok(vehicle);
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error al obtener vehículo: {ex.Message}");
                return NotFound($"Vehículo con ID {id} no encontrado");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Vehicle>> CreateVehicle([FromBody] VehicleRequest vehicleRequest)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                // Buscar el ID del proveedor basado en el ID del usuario autenticado
                var provider = await _providerRepository.GetByUserIdAsync(userId);
                
                if (provider == null)
                {
                    _logger.LogWarning($"No se encontró un registro de proveedor para el usuario: {userId}");
                    return BadRequest("No se encontró un perfil de proveedor para su cuenta. Por favor, complete su perfil de proveedor primero.");
                }
                
                _logger.LogInformation($"Creando nuevo vehículo para proveedor: {provider.Id} (Usuario: {userId})");
                
                // Crear un nuevo objeto Vehicle a partir de la solicitud
                var vehicle = new Vehicle
                {
                    OwnerId = provider.Id, // Usar el ID del proveedor, no el ID del usuario
                    Brand = vehicleRequest.Brand,
                    Model = vehicleRequest.Model,
                    Year = vehicleRequest.Year,
                    Type = vehicleRequest.Type,
                    DailyRate = vehicleRequest.DailyRate,
                    Description = vehicleRequest.Description,
                    Location = vehicleRequest.Location,
                    Status = vehicleRequest.Status,
                    Doors = vehicleRequest.Doors,
                    Seats = vehicleRequest.Seats,
                    Transmission = vehicleRequest.Transmission,
                    FuelType = vehicleRequest.FuelType,
                    AirConditioner = vehicleRequest.AirConditioner
                };
                
                var newVehicle = await _vehicleService.RegisterVehicleAsync(vehicle);
                
                return CreatedAtAction(nameof(GetVehicle), new { id = newVehicle.Id }, newVehicle);
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error al crear vehículo: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Vehicle>> UpdateVehicle(string id, [FromBody] VehicleRequest vehicleData)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                // Buscar el ID del proveedor
                var provider = await _providerRepository.GetByUserIdAsync(userId);
                
                if (provider == null)
                {
                    return BadRequest("No se encontró un perfil de proveedor para su cuenta");
                }
                
                // Verificar que el vehículo pertenece al usuario autenticado
                var existingVehicle = await _vehicleService.GetVehicleByIdAsync(id);
                
                if (existingVehicle.OwnerId != provider.Id)
                {
                    _logger.LogWarning($"Intento de modificación no autorizada de vehículo: {id} por proveedor: {provider.Id}");
                    return Forbid();
                }
                
                // Crear un objeto Vehicle a partir de la solicitud
                var vehicle = new Vehicle
                {
                    Id = id, // Mantener el mismo ID
                    OwnerId = provider.Id, // Mantener el mismo propietario
                    Brand = vehicleData.Brand,
                    Model = vehicleData.Model,
                    Year = vehicleData.Year,
                    Type = vehicleData.Type,
                    DailyRate = vehicleData.DailyRate,
                    Description = vehicleData.Description,
                    Location = vehicleData.Location,
                    Status = vehicleData.Status,
                    Doors = vehicleData.Doors,
                    Seats = vehicleData.Seats,
                    Transmission = vehicleData.Transmission,
                    FuelType = vehicleData.FuelType,
                    AirConditioner = vehicleData.AirConditioner
                };
                
                _logger.LogInformation($"Actualizando vehículo: {id} para proveedor: {provider.Id}");
                var updatedVehicle = await _vehicleService.UpdateVehicleAsync(id, vehicle);
                
                return Ok(updatedVehicle);
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error al actualizar vehículo: {ex.Message}");
                
                if (ex.Message.Contains("no encontrado"))
                {
                    return NotFound(ex.Message);
                }
                
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteVehicle(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                // Buscar el ID del proveedor
                var provider = await _providerRepository.GetByUserIdAsync(userId);
                
                if (provider == null)
                {
                    return BadRequest("No se encontró un perfil de proveedor para su cuenta");
                }
                
                // Verificar que el vehículo pertenece al usuario autenticado
                var existingVehicle = await _vehicleService.GetVehicleByIdAsync(id);
                
                if (existingVehicle.OwnerId != provider.Id)
                {
                    _logger.LogWarning($"Intento de eliminación no autorizada de vehículo: {id} por proveedor: {provider.Id}");
                    return Forbid();
                }
                
                _logger.LogInformation($"Eliminando vehículo: {id} para proveedor: {provider.Id}");
                var result = await _vehicleService.DeleteVehicleAsync(id);
                
                if (result)
                {
                    return NoContent();
                }
                
                return BadRequest("No se pudo eliminar el vehículo");
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error al eliminar vehículo: {ex.Message}");
                
                if (ex.Message.Contains("no encontrado"))
                {
                    return NotFound(ex.Message);
                }
                
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<ActionResult<Vehicle>> UpdateVehicleStatus(string id, [FromBody] UpdateStatusRequest request)
        {
            if (string.IsNullOrEmpty(request.Status))
            {
                return BadRequest("El estado es requerido");
            }
            
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                // Buscar el ID del proveedor
                var provider = await _providerRepository.GetByUserIdAsync(userId);
                
                if (provider == null)
                {
                    return BadRequest("No se encontró un perfil de proveedor para su cuenta");
                }
                
                // Verificar que el vehículo pertenece al usuario autenticado
                var existingVehicle = await _vehicleService.GetVehicleByIdAsync(id);
                
                if (existingVehicle.OwnerId != provider.Id)
                {
                    _logger.LogWarning($"Intento de cambio de estado no autorizado para vehículo: {id} por proveedor: {provider.Id}");
                    return Forbid();
                }
                
                _logger.LogInformation($"Actualizando estado de vehículo: {id} a: {request.Status} para proveedor: {provider.Id}");
                var updatedVehicle = await _vehicleService.UpdateVehicleStatusAsync(id, request.Status);
                
                return Ok(updatedVehicle);
            }
            catch (System.ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error al actualizar estado de vehículo: {ex.Message}");
                
                if (ex.Message.Contains("no encontrado"))
                {
                    return NotFound(ex.Message);
                }
                
                return BadRequest(ex.Message);
            }
        }
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; }
    }

    // Clase para recibir datos del vehículo desde el frontend
    public class VehicleRequest
    {
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Type { get; set; }
        public decimal DailyRate { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Status { get; set; } = "available";
        public int Doors { get; set; }
        public int Seats { get; set; }
        public string Transmission { get; set; }
        public string FuelType { get; set; }
        public bool AirConditioner { get; set; }
    }
}
