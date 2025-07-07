using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BackendRent2Go.Data;
using Rent2Go.API.IAM.Domain.Model;
using Rent2Go.API.IAM.Domain.Repositories;

namespace Rent2Go.API.IAM.Infrastructure.Persistence.EFC.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly ApplicationDbContext _context;

        public ClientRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Client?> GetByIdAsync(string id)
        {
            var dbClient = await _context.Clients
                .FirstOrDefaultAsync(c => c.Id == id);
            return dbClient != null ? MapToIamClient(dbClient) : null;
        }

        public async Task<Client?> GetByUserIdAsync(string userId)
        {
            var dbClient = await _context.Clients
                .FirstOrDefaultAsync(c => c.UserId == userId);
            return dbClient != null ? MapToIamClient(dbClient) : null;
        }

        public async Task<IEnumerable<Client>> GetAllAsync()
        {
            var dbClients = await _context.Clients.ToListAsync();
            return dbClients.Select(c => MapToIamClient(c));
        }

        public async Task<Client> AddAsync(Client client)
        {
            var dbClient = MapToDbClient(client);
            await _context.Clients.AddAsync(dbClient);
            await _context.SaveChangesAsync();
            return MapToIamClient(dbClient);
        }

        public async Task UpdateAsync(Client client)
        {
            var dbClient = await _context.Clients
                .FirstOrDefaultAsync(c => c.Id == client.Id);
            if (dbClient != null)
            {
                // Actualizar las propiedades manualmente
                dbClient.UserId = client.UserId;
                dbClient.Dni = client.Dni;
                dbClient.Phone = client.Phone;
                dbClient.UpdatedAt = client.UpdatedAt;
                
                _context.Clients.Update(dbClient);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(string id)
        {
            var dbClient = await _context.Clients
                .FirstOrDefaultAsync(c => c.Id == id);
            if (dbClient != null)
            {
                _context.Clients.Remove(dbClient);
                await _context.SaveChangesAsync();
            }
        }

        // Métodos auxiliares para mapear entre modelos
        private Client MapToIamClient(dynamic dbClient)
        {
            return new Client
            {
                Id = dbClient.Id,
                UserId = dbClient.UserId,
                Dni = dbClient.Dni,
                Phone = dbClient.Phone,
                CreatedAt = dbClient.CreatedAt,
                UpdatedAt = dbClient.UpdatedAt
                // La propiedad User se cargará por separado si es necesario
            };
        }

        private dynamic MapToDbClient(Client iamClient)
        {
            // Crear una instancia de la clase Client del DbContext
            var clientType = _context.Clients.GetType().GetGenericArguments()[0];
            var dbClient = System.Activator.CreateInstance(clientType);
            
            // Asignar propiedades usando reflexión
            clientType.GetProperty("Id").SetValue(dbClient, iamClient.Id);
            clientType.GetProperty("UserId").SetValue(dbClient, iamClient.UserId);
            clientType.GetProperty("Dni").SetValue(dbClient, iamClient.Dni);
            clientType.GetProperty("Phone").SetValue(dbClient, iamClient.Phone);
            clientType.GetProperty("CreatedAt").SetValue(dbClient, iamClient.CreatedAt);
            clientType.GetProperty("UpdatedAt").SetValue(dbClient, iamClient.UpdatedAt);
            
            return dbClient;
        }
    }
}
