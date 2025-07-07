using Rent2Go.API.Provider.Domain.Model;
using Rent2Go.API.Provider.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Rent2Go.API.Provider.Domain.Services
{
    public interface IVehicleService
    {
        Task<IEnumerable<Vehicle>> GetVehiclesByOwnerIdAsync(string ownerId);
        Task<Vehicle> GetVehicleByIdAsync(string id);
        Task<Vehicle> RegisterVehicleAsync(Vehicle vehicle);
        Task<Vehicle> UpdateVehicleAsync(string id, Vehicle vehicle);
        Task<bool> DeleteVehicleAsync(string id);
        Task<Vehicle> UpdateVehicleStatusAsync(string id, string status);
        Task<Vehicle> UpdateVehicleImagesAsync(string id, List<string> imageUrls);
    }
    
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IVehicleSpecificationRepository _specificationRepository;
        private readonly IVehicleImageRepository _vehicleImageRepository;

        public VehicleService(
            IVehicleRepository vehicleRepository, 
            IVehicleSpecificationRepository specificationRepository,
            IVehicleImageRepository vehicleImageRepository)
        {
            _vehicleRepository = vehicleRepository;
            _specificationRepository = specificationRepository;
            _vehicleImageRepository = vehicleImageRepository;
        }

        public async Task<IEnumerable<Vehicle>> GetVehiclesByOwnerIdAsync(string ownerId)
        {
            var vehicles = await _vehicleRepository.ListByOwnerAsync(ownerId);
            var vehiclesList = vehicles.ToList(); // Convertir a lista para evitar múltiples enumeraciones
            
            // Cargar las especificaciones e imágenes para cada vehículo
            foreach (var vehicle in vehiclesList)
            {
                vehicle.Specifications = await _specificationRepository.FindByVehicleIdAsync(vehicle.Id);
                
                // Copiar las propiedades para compatibilidad
                if (vehicle.Specifications != null)
                {
                    vehicle.Doors = vehicle.Specifications.Doors;
                    vehicle.Seats = vehicle.Specifications.Seats;
                    vehicle.Transmission = vehicle.Specifications.Transmission;
                    vehicle.FuelType = vehicle.Specifications.FuelType;
                    vehicle.AirConditioner = vehicle.Specifications.AirConditioner;
                }
                
                // Cargar las imágenes del vehículo desde la base de datos
                try
                {
                    // Obtener imágenes de la base de datos
                    var vehicleImages = await _vehicleImageRepository.GetByVehicleIdAsync(vehicle.Id);
                    vehicle.Images = vehicleImages.Select(i => i.ImageUrl).ToList();
                    
                    // Si no hay imágenes en la base de datos, intentar buscarlas en el sistema de archivos (para compatibilidad)
                    if (!vehicle.Images.Any())
                    {
                        string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "vehicles");
                        if (Directory.Exists(uploadFolder))
                        {
                            // Buscar archivos que comiencen con el ID del vehículo
                            string[] files = Directory.GetFiles(uploadFolder, $"{vehicle.Id}_*");
                            
                            // Convertir rutas de archivo a URLs relativas
                            vehicle.Images = files.Select(f => $"/images/vehicles/{Path.GetFileName(f)}").ToList();
                            
                            // Si se encontraron imágenes en el sistema de archivos pero no en la base de datos,
                            // guardarlas en la base de datos
                            if (vehicle.Images.Any())
                            {
                                Console.WriteLine($"Migración de imágenes: Se encontraron {vehicle.Images.Count} imágenes en el sistema de archivos para el vehículo {vehicle.Id}");
                                
                                // Crear registros en la base de datos para cada imagen
                                bool isFirst = true;
                                foreach (var imageUrl in vehicle.Images)
                                {
                                    var vehicleImage = new VehicleImage
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        VehicleId = vehicle.Id,
                                        ImageUrl = imageUrl,
                                        IsPrimary = isFirst // La primera imagen es la principal
                                    };
                                    
                                    await _vehicleImageRepository.AddAsync(vehicleImage);
                                    isFirst = false;
                                }
                                
                                await _vehicleImageRepository.SaveChangesAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al cargar imágenes para el vehículo {vehicle.Id}: {ex.Message}");
                    // No interrumpir el flujo si hay un error al cargar imágenes
                    vehicle.Images = new List<string>();
                }
            }
            
            return vehiclesList;
        }

        public async Task<Vehicle> GetVehicleByIdAsync(string id)
        {
            var vehicle = await _vehicleRepository.FindByIdAsync(id);
            
            if (vehicle == null)
            {
                throw new Exception($"Vehículo con ID {id} no encontrado");
            }
            
            // Cargar las especificaciones
            vehicle.Specifications = await _specificationRepository.FindByVehicleIdAsync(id);
            
            // Copiar las propiedades para compatibilidad
            if (vehicle.Specifications != null)
            {
                vehicle.Doors = vehicle.Specifications.Doors;
                vehicle.Seats = vehicle.Specifications.Seats;
                vehicle.Transmission = vehicle.Specifications.Transmission;
                vehicle.FuelType = vehicle.Specifications.FuelType;
                vehicle.AirConditioner = vehicle.Specifications.AirConditioner;
            }
            
            // Cargar las imágenes del vehículo desde la base de datos
            try
            {
                // Obtener imágenes de la base de datos
                var vehicleImages = await _vehicleImageRepository.GetByVehicleIdAsync(id);
                vehicle.Images = vehicleImages.Select(i => i.ImageUrl).ToList();
                
                Console.WriteLine($"Cargadas {vehicle.Images.Count} imágenes para el vehículo {id} desde la base de datos");
                
                // Si no hay imágenes en la base de datos, intentar buscarlas en el sistema de archivos (para compatibilidad)
                if (!vehicle.Images.Any())
                {
                    string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "vehicles");
                    if (Directory.Exists(uploadFolder))
                    {
                        // Buscar archivos que comiencen con el ID del vehículo
                        string[] files = Directory.GetFiles(uploadFolder, $"{id}_*");
                        
                        // Convertir rutas de archivo a URLs relativas
                        vehicle.Images = files.Select(f => $"/images/vehicles/{Path.GetFileName(f)}").ToList();
                        
                        // Si se encontraron imágenes en el sistema de archivos pero no en la base de datos,
                        // guardarlas en la base de datos
                        if (vehicle.Images.Any())
                        {
                            Console.WriteLine($"Migración de imágenes: Se encontraron {vehicle.Images.Count} imágenes en el sistema de archivos para el vehículo {id}");
                            await UpdateVehicleImagesAsync(id, vehicle.Images);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar imágenes para el vehículo {id}: {ex.Message}");
                // No interrumpir el flujo si hay un error al cargar imágenes
                vehicle.Images = new List<string>();
            }
            
            return vehicle;
        }

        public async Task<Vehicle> RegisterVehicleAsync(Vehicle vehicle)
        {
            // Generar nuevo ID para el vehículo
            vehicle.Id = Guid.NewGuid().ToString();
            vehicle.CreatedAt = DateTime.UtcNow;
            vehicle.UpdatedAt = DateTime.UtcNow;
            vehicle.Status = "available";
            
            // Crear las especificaciones del vehículo
            var specifications = new VehicleSpecification
            {
                VehicleId = vehicle.Id,
                Doors = vehicle.Doors,
                Seats = vehicle.Seats,
                Transmission = vehicle.Transmission ?? "manual",
                FuelType = vehicle.FuelType ?? "gasoline",
                AirConditioner = vehicle.AirConditioner
            };
            
            // Guardar el vehículo
            await _vehicleRepository.AddAsync(vehicle);
            
            // Guardar las especificaciones
            await _specificationRepository.AddAsync(specifications);
            
            await _vehicleRepository.SaveChangesAsync();
            
            // Asignar las especificaciones al vehículo
            vehicle.Specifications = specifications;
            
            return vehicle;
        }

        public async Task<Vehicle> UpdateVehicleAsync(string id, Vehicle vehicleData)
        {
            var existingVehicle = await GetVehicleByIdAsync(id);
            
            // Actualizar propiedades básicas del vehículo
            existingVehicle.Brand = vehicleData.Brand;
            existingVehicle.Model = vehicleData.Model;
            existingVehicle.Year = vehicleData.Year;
            existingVehicle.Type = vehicleData.Type;
            existingVehicle.DailyRate = vehicleData.DailyRate;
            existingVehicle.Description = vehicleData.Description;
            existingVehicle.Location = vehicleData.Location;
            existingVehicle.Status = vehicleData.Status; // Mantener el estado
            
            // Importante: Asegurarse de que las fechas estén en UTC
            existingVehicle.UpdatedAt = DateTime.UtcNow;
            // Asegurarse de que CreatedAt sea UTC si viene del cliente
            if (existingVehicle.CreatedAt.Kind != DateTimeKind.Utc)
            {
                existingVehicle.CreatedAt = DateTime.SpecifyKind(existingVehicle.CreatedAt, DateTimeKind.Utc);
            }
            
            // Actualizar las especificaciones
            if (existingVehicle.Specifications != null)
            {
                existingVehicle.Specifications.Doors = vehicleData.Doors;
                existingVehicle.Specifications.Seats = vehicleData.Seats;
                existingVehicle.Specifications.Transmission = vehicleData.Transmission ?? "manual";
                existingVehicle.Specifications.FuelType = vehicleData.FuelType ?? "gasoline";
                existingVehicle.Specifications.AirConditioner = vehicleData.AirConditioner;
                
                _specificationRepository.Update(existingVehicle.Specifications);
            }
            else
            {
                // Crear nuevas especificaciones si no existen
                var specifications = new VehicleSpecification
                {
                    VehicleId = existingVehicle.Id,
                    Doors = vehicleData.Doors,
                    Seats = vehicleData.Seats,
                    Transmission = vehicleData.Transmission ?? "manual",
                    FuelType = vehicleData.FuelType ?? "gasoline",
                    AirConditioner = vehicleData.AirConditioner
                };
                
                await _specificationRepository.AddAsync(specifications);
                existingVehicle.Specifications = specifications;
            }
            
            // Actualizar el vehículo
            _vehicleRepository.Update(existingVehicle);
            await _vehicleRepository.SaveChangesAsync();
            
            // Actualizar propiedades para compatibilidad
            existingVehicle.Doors = existingVehicle.Specifications.Doors;
            existingVehicle.Seats = existingVehicle.Specifications.Seats;
            existingVehicle.Transmission = existingVehicle.Specifications.Transmission;
            existingVehicle.FuelType = existingVehicle.Specifications.FuelType;
            existingVehicle.AirConditioner = existingVehicle.Specifications.AirConditioner;
            
            return existingVehicle;
        }

        public async Task<bool> DeleteVehicleAsync(string id)
        {
            var vehicle = await _vehicleRepository.FindByIdAsync(id);
            
            if (vehicle == null)
            {
                throw new Exception($"Vehículo con ID {id} no encontrado");
            }
            
            // Obtener las especificaciones
            var specifications = await _specificationRepository.FindByVehicleIdAsync(id);
            
            // Eliminar las especificaciones si existen
            if (specifications != null)
            {
                _specificationRepository.Remove(specifications);
            }
            
            // Eliminar el vehículo
            _vehicleRepository.Remove(vehicle);
            var result = await _vehicleRepository.SaveChangesAsync();
            
            return result > 0;
        }

        public async Task<Vehicle> UpdateVehicleStatusAsync(string id, string status)
        {
            var vehicle = await GetVehicleByIdAsync(id);
            
            if (!IsValidStatus(status))
            {
                throw new ArgumentException("Estado no válido. Los estados permitidos son: available, rented, maintenance, inactive");
            }
            
            vehicle.Status = status;
            vehicle.UpdatedAt = DateTime.UtcNow;
            
            // Asegurarse de que CreatedAt sea UTC si viene del cliente
            if (vehicle.CreatedAt.Kind != DateTimeKind.Utc)
            {
                vehicle.CreatedAt = DateTime.SpecifyKind(vehicle.CreatedAt, DateTimeKind.Utc);
            }
            
            _vehicleRepository.Update(vehicle);
            await _vehicleRepository.SaveChangesAsync();
            
            return vehicle;
        }
        
        public async Task<Vehicle> UpdateVehicleImagesAsync(string id, List<string> imageUrls)
        {
            var vehicle = await GetVehicleByIdAsync(id);
            
            // Eliminar todas las imágenes existentes de la base de datos
            await _vehicleImageRepository.DeleteAllByVehicleIdAsync(id);
            
            // Agregar las nuevas imágenes a la base de datos
            if (imageUrls != null && imageUrls.Count > 0)
            {
                bool isFirst = true;
                foreach (var imageUrl in imageUrls)
                {
                    var vehicleImage = new VehicleImage
                    {
                        Id = Guid.NewGuid().ToString(),
                        VehicleId = id,
                        ImageUrl = imageUrl,
                        IsPrimary = isFirst // La primera imagen es la principal
                    };
                    
                    await _vehicleImageRepository.AddAsync(vehicleImage);
                    isFirst = false;
                }
                
                // Guardar explícitamente los cambios en la base de datos para las imágenes
                await _vehicleImageRepository.SaveChangesAsync();
                
                // Añadir log para verificar que se guardaron las imágenes
                Console.WriteLine($"Se guardaron {imageUrls.Count} imágenes en la base de datos para el vehículo {id}");
            }
            
            // Actualizar la propiedad Images del vehículo para compatibilidad
            vehicle.Images = imageUrls ?? new List<string>();
            vehicle.UpdatedAt = DateTime.UtcNow;
            
            // Asegurarse de que CreatedAt sea UTC si viene del cliente
            if (vehicle.CreatedAt.Kind != DateTimeKind.Utc)
            {
                vehicle.CreatedAt = DateTime.SpecifyKind(vehicle.CreatedAt, DateTimeKind.Utc);
            }
            
            _vehicleRepository.Update(vehicle);
            await _vehicleRepository.SaveChangesAsync();
            
            return vehicle;
        }
        
        private bool IsValidStatus(string status)
        {
            return status == "available" || status == "rented" || status == "maintenance" || status == "inactive";
        }
    }
}

