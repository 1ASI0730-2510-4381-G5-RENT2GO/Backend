using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rent2Go.API.IAM.Domain.Repositories;
using Rent2Go.API.Provider.Domain.Services;

namespace Rent2Go.API.Provider.Interfaces.REST
{
    [ApiController]
    [Route("api/provider/vehicles")]
    [Authorize(Roles = "provider")]
    public class VehicleImagesController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IVehicleService _vehicleService;
        private readonly IProviderRepository _providerRepository;
        private readonly ILogger<VehicleImagesController> _logger;

        public VehicleImagesController(
            IWebHostEnvironment environment,
            IVehicleService vehicleService,
            IProviderRepository providerRepository,
            ILogger<VehicleImagesController> logger)
        {
            _environment = environment;
            _vehicleService = vehicleService;
            _providerRepository = providerRepository;
            _logger = logger;
        }

        [HttpPost("{id}/images")]
        public async Task<IActionResult> UploadImages(string id, [FromForm] IFormFileCollection files)
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

                // Verificar que el vehículo existe y pertenece al proveedor
                var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
                
                if (vehicle == null)
                {
                    return NotFound($"Vehículo con ID {id} no encontrado");
                }
                
                if (vehicle.OwnerId != provider.Id)
                {
                    _logger.LogWarning($"Intento de carga de imágenes no autorizado para vehículo: {id} por proveedor: {provider.Id}");
                    return Forbid();
                }

                // Validar los archivos
                if (files == null || files.Count == 0)
                {
                    return BadRequest("No se ha proporcionado ninguna imagen");
                }
                
                if (files.Count > 5)
                {
                    return BadRequest("Se permite un máximo de 5 imágenes por vehículo");
                }

                var imageUrls = new List<string>();
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "vehicles");
                
                // Crear el directorio si no existe
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                foreach (var file in files)
                {
                    // Validar el tipo de archivo
                    if (!IsValidImageFile(file))
                    {
                        return BadRequest("Solo se permiten imágenes de tipo PNG, JPG o JPEG");
                    }

                    // Validar el tamaño del archivo (5MB máximo)
                    if (file.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest("El tamaño máximo de archivo es 5MB");
                    }

                    // Generar un nombre único para la imagen
                    var fileName = $"{vehicle.Id}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Guardar el archivo
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Agregar la URL de la imagen
                    var imageUrl = $"/images/vehicles/{fileName}";
                    imageUrls.Add(imageUrl);
                }

                // Actualizar las imágenes del vehículo
                vehicle.Images.AddRange(imageUrls);
                
                // Guardar el vehículo actualizado
                await _vehicleService.UpdateVehicleImagesAsync(id, vehicle.Images);

                return Ok(new { message = "Imágenes cargadas correctamente", images = vehicle.Images });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al cargar imágenes para vehículo {id}: {ex.Message}");
                return StatusCode(500, "Error interno del servidor al cargar las imágenes");
            }
        }

        [HttpGet("{id}/images")]
        public async Task<IActionResult> GetImages(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var provider = await _providerRepository.GetByUserIdAsync(userId);
            if (provider == null) return BadRequest("No se encontró un perfil de proveedor para su cuenta");
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
            if (vehicle == null || vehicle.OwnerId != provider.Id) return Forbid();
            return Ok(vehicle.Images);
        }

        [HttpDelete("{id}/images")]
        public async Task<IActionResult> DeleteImages(string id, [FromBody] DeleteImagesRequest request)
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

                // Verificar que el vehículo existe y pertenece al proveedor
                var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
                
                if (vehicle == null)
                {
                    return NotFound($"Vehículo con ID {id} no encontrado");
                }
                
                if (vehicle.OwnerId != provider.Id)
                {
                    _logger.LogWarning($"Intento de eliminación de imágenes no autorizado para vehículo: {id} por proveedor: {provider.Id}");
                    return Forbid();
                }

                // Validar la solicitud
                if (request == null || request.ImageUrls == null || request.ImageUrls.Count == 0)
                {
                    return BadRequest("No se han especificado imágenes para eliminar");
                }

                // Eliminar los archivos físicos
                var webRootPath = _environment.WebRootPath;

                foreach (var imageUrl in request.ImageUrls)
                {
                    // Extraer la ruta relativa desde la URL
                    var imagePath = imageUrl.TrimStart('/');
                    var fullPath = Path.Combine(webRootPath, imagePath);

                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }

                // Actualizar las imágenes del vehículo
                var remainingImages = vehicle.Images.Where(img => !request.ImageUrls.Contains(img)).ToList();
                await _vehicleService.UpdateVehicleImagesAsync(id, remainingImages);

                return Ok(new { message = "Imágenes eliminadas correctamente", images = remainingImages });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar imágenes para vehículo {id}: {ex.Message}");
                return StatusCode(500, "Error interno del servidor al eliminar las imágenes");
            }
        }

        private bool IsValidImageFile(IFormFile file)
        {
            // Verificar la extensión del archivo
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            // Verificar el tipo MIME
            var allowedMimeTypes = new[] { "image/jpeg", "image/png" };
            
            return allowedExtensions.Contains(fileExtension) && allowedMimeTypes.Contains(file.ContentType);
        }
    }

    public class DeleteImagesRequest
    {
        public List<string> ImageUrls { get; set; }
    }
}
