using System;

namespace BackendRent2Go.Shared.Domain.Services
{
    /// <summary>
    /// Interfaz que proporciona la fecha y hora actual para facilitar pruebas unitarias
    /// </summary>
    public interface IDateTimeProvider
    {
        DateTime Now { get; }
    }
}
