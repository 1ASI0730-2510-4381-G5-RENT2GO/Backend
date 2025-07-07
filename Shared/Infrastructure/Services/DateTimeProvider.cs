using System;
using BackendRent2Go.Shared.Domain.Services;

namespace BackendRent2Go.Shared.Infrastructure.Services;

/// <summary>
/// Implementación concreta del proveedor de fecha y hora
/// </summary>
public class DateTimeProvider : IDateTimeProvider
{
    /// <summary>
    /// Obtiene la fecha y hora actual del sistema
    /// </summary>
    public DateTime Now => DateTime.UtcNow;
}
