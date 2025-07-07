using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClientInterfaces = BackendRent2Go.Client.Interfaces.ACL;
using ClientDomainModel = BackendRent2Go.Client.Domain.Model.ACL;
using ProviderACL = Rent2Go.API.Provider.Interfaces.ACL;

namespace BackendRent2Go.Client.Application.ACL
{
    /// <summary>
    /// Implementación de la capa ACL para servicios de vehículos
    /// Traduce entre el contexto de Provider y Client
    /// </summary>
    public class ProviderVehicleServiceAcl : ClientInterfaces.IProviderVehicleService
    {
        private readonly ProviderACL.IProviderVehicleService _providerService;

        public ProviderVehicleServiceAcl(ProviderACL.IProviderVehicleService providerService)
        {
            _providerService = providerService;
        }

        public async Task<IEnumerable<ClientDomainModel.ProviderVehicleDto>> FindByStatusAsync(string status)
        {
            try
            {
                Console.WriteLine($"ProviderVehicleServiceAcl: Buscando vehículos con estado: {status}");
                
                var vehicles = await _providerService.FindByStatusAsync(status);
                
                if (vehicles == null)
                {
                    Console.WriteLine("ProviderVehicleServiceAcl: El servicio del proveedor devolvió null");
                    return new List<ClientDomainModel.ProviderVehicleDto>(); // Devolver lista vacía en lugar de null
                }
                
                Console.WriteLine($"ProviderVehicleServiceAcl: Se encontraron {vehicles.Count()} vehículos");
                
                var result = new List<ClientDomainModel.ProviderVehicleDto>();

                foreach (var vehicle in vehicles)
                {
                    if (vehicle == null)
                    {
                        Console.WriteLine("ProviderVehicleServiceAcl: Se encontró un vehículo null en la lista");
                        continue; // Saltar este vehículo y continuar con el siguiente
                    }
                    
                    Console.WriteLine($"ProviderVehicleServiceAcl: Procesando vehículo ID={vehicle.Id}, Marca={vehicle.Brand}, Modelo={vehicle.Model}, Estado={vehicle.Status}");
                    
                    // Verificar si el vehículo tiene ID válido
                    if (string.IsNullOrEmpty(vehicle.Id))
                    {
                        Console.WriteLine("ProviderVehicleServiceAcl: Se encontró un vehículo sin ID válido");
                        continue; // Saltar este vehículo y continuar con el siguiente
                    }
                    
                    // Intentar obtener los datos de las especificaciones del vehículo
                    // Valores por defecto
                    int seats = 5;
                    string transmission = "automatic";
                    
                    try
                    {
                        // Verificar si podemos acceder a las propiedades de especificaciones
                        var specs = vehicle.GetType().GetProperty("Specifications")?.GetValue(vehicle);
                        if (specs != null)
                        {
                            // Si tenemos especificaciones, intentar obtener los asientos
                            var seatsProperty = specs.GetType().GetProperty("Seats");
                            if (seatsProperty != null)
                            {
                                var seatsValue = seatsProperty.GetValue(specs);
                                if (seatsValue != null)
                                {
                                    seats = Convert.ToInt32(seatsValue);
                                }
                            }

                            // Intentar obtener la transmisión
                            var transmissionProperty = specs.GetType().GetProperty("Transmission");
                            if (transmissionProperty != null)
                            {
                                var transmissionValue = transmissionProperty.GetValue(specs);
                                if (transmissionValue != null)
                                {
                                    transmission = transmissionValue.ToString();
                                }
                            }

                            Console.WriteLine($"ProviderVehicleServiceAcl: Usando especificaciones - Asientos: {seats}, Transmisión: {transmission}");
                        }
                        else
                        {
                            // Intentar obtener propiedades directamente del vehículo
                            var seatsProperty = vehicle.GetType().GetProperty("Seats");
                            if (seatsProperty != null)
                            {
                                var seatsValue = seatsProperty.GetValue(vehicle);
                                if (seatsValue != null)
                                {
                                    seats = Convert.ToInt32(seatsValue);
                                }
                            }

                            var transmissionProperty = vehicle.GetType().GetProperty("Transmission");
                            if (transmissionProperty != null)
                            {
                                var transmissionValue = transmissionProperty.GetValue(vehicle);
                                if (transmissionValue != null && !string.IsNullOrEmpty(transmissionValue.ToString()))
                                {
                                    transmission = transmissionValue.ToString();
                                }
                            }

                            Console.WriteLine($"ProviderVehicleServiceAcl: Usando propiedades directas - Asientos: {seats}, Transmisión: {transmission}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ProviderVehicleServiceAcl: Error al obtener especificaciones: {ex.Message}");
                        // Usar valores por defecto si hay algún error
                    }
                    
                    try
                    {
                        result.Add(new ClientDomainModel.ProviderVehicleDto
                        {
                            Id = vehicle.Id,
                            Brand = vehicle.Brand ?? "Sin marca",
                            Model = vehicle.Model ?? "Sin modelo",
                            Year = vehicle.Year,
                            Type = vehicle.Type ?? "Sin categoría",
                            DailyRate = vehicle.DailyRate,
                            Seats = seats,
                            LuggageCapacity = 3, // Valor por defecto
                            Transmission = transmission,
                            Rating = 4.5m, // Valor decimal literal con el sufijo 'm'
                            ReviewCount = 0, // Valor por defecto para número de reseñas
                            Status = vehicle.Status ?? "unknown"
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ProviderVehicleServiceAcl: Error al crear DTO para vehículo {vehicle.Id}: {ex.Message}");
                        // Continuar con el siguiente vehículo
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ProviderVehicleServiceAcl: Error general en FindByStatusAsync: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                // En lugar de propagar la excepción, devolvemos una lista vacía
                return new List<ClientDomainModel.ProviderVehicleDto>();
            }
        }

        public async Task<ClientDomainModel.ProviderVehicleDto> GetVehicleByIdAsync(string id)
        {
            try
            {
                Console.WriteLine($"ProviderVehicleServiceAcl: Buscando vehículo con ID: {id}");
                
                if (string.IsNullOrEmpty(id))
                {
                    Console.WriteLine("ProviderVehicleServiceAcl: ID de vehículo nulo o vacío");
                    return null!;
                }
                
                var vehicle = await _providerService.GetVehicleByIdAsync(id);
                if (vehicle == null)
                {
                    Console.WriteLine($"ProviderVehicleServiceAcl: No se encontró vehículo con ID: {id}");
                    return null!;
                }

                Console.WriteLine($"ProviderVehicleServiceAcl: Vehículo encontrado - ID={vehicle.Id}, Marca={vehicle.Brand}, Modelo={vehicle.Model}");
                
                // Intentar obtener los datos de las especificaciones del vehículo
                // Valores por defecto
                int seats = 5;
                string transmission = "automatic";
                
                try
                {
                    // Verificar si podemos acceder a las propiedades de especificaciones
                    var specs = vehicle.GetType().GetProperty("Specifications")?.GetValue(vehicle);
                    if (specs != null)
                    {
                        // Si tenemos especificaciones, intentar obtener los asientos
                        var seatsProperty = specs.GetType().GetProperty("Seats");
                        if (seatsProperty != null)
                        {
                            var seatsValue = seatsProperty.GetValue(specs);
                            if (seatsValue != null)
                            {
                                seats = Convert.ToInt32(seatsValue);
                            }
                        }

                        // Intentar obtener la transmisión
                        var transmissionProperty = specs.GetType().GetProperty("Transmission");
                        if (transmissionProperty != null)
                        {
                            var transmissionValue = transmissionProperty.GetValue(specs);
                            if (transmissionValue != null)
                            {
                                transmission = transmissionValue.ToString();
                            }
                        }
                    }
                    else
                    {
                        // Intentar obtener propiedades directamente del vehículo
                        var seatsProperty = vehicle.GetType().GetProperty("Seats");
                        if (seatsProperty != null)
                        {
                            var seatsValue = seatsProperty.GetValue(vehicle);
                            if (seatsValue != null)
                            {
                                seats = Convert.ToInt32(seatsValue);
                            }
                        }

                        var transmissionProperty = vehicle.GetType().GetProperty("Transmission");
                        if (transmissionProperty != null)
                        {
                            var transmissionValue = transmissionProperty.GetValue(vehicle);
                            if (transmissionValue != null && !string.IsNullOrEmpty(transmissionValue.ToString()))
                            {
                                transmission = transmissionValue.ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ProviderVehicleServiceAcl: Error al obtener especificaciones para ID {id}: {ex.Message}");
                    // Usar valores por defecto si hay algún error
                }

                return new ClientDomainModel.ProviderVehicleDto
                {
                    Id = vehicle.Id,
                    Brand = vehicle.Brand ?? "Sin marca",
                    Model = vehicle.Model ?? "Sin modelo",
                    Year = vehicle.Year,
                    Type = vehicle.Type ?? "Sin categoría",
                    DailyRate = vehicle.DailyRate,
                    Seats = seats,
                    LuggageCapacity = 3, // Valor por defecto
                    Transmission = transmission,
                    Rating = 4.5m, // Valor decimal literal con el sufijo 'm'
                    ReviewCount = 0, // Valor por defecto para número de reseñas
                    Status = vehicle.Status ?? "unknown"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ProviderVehicleServiceAcl: Error general en GetVehicleByIdAsync para ID {id}: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return null!;
            }
        }
    }
}
