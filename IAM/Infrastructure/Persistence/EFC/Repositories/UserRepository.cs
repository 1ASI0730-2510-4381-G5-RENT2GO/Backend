using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BackendRent2Go.Data;
using Rent2Go.API.IAM.Domain.Model.Aggregates;
using Rent2Go.API.IAM.Domain.Repositories;

namespace Rent2Go.API.IAM.Infrastructure.Persistence.EFC.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        public UserRepository(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<User>> ListAsync() =>
            await _context.Users.ToListAsync();

        public async Task<User> FindByIdAsync(string id) =>
            await _context.Users.FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new KeyNotFoundException($"User not found: {id}");

        public async Task<User?> FindByEmailAsync(string email) =>
            await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public void Update(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public void Remove(User user)
        {
            _context.Users.Remove(user);
            _context.SaveChanges();
        }
    }
}
