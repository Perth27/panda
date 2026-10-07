using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pandora.Models;

namespace Pandora.Repositories
{
    public interface IMigratesRepository
    {
        Task<List<Migrate>> GetAllMigratesAsync();
        Task<Migrate> GetMigrateByIdAsync(string id);
        Task<Migrate> FindMigrateByIdAsync(string id);
        Task AddMigrateAsync(Migrate migrate);
        Task UpdateMigrateAsync(Migrate migrate);
        Task RemoveMigrateAsync(Migrate migrate);
        bool MigrateExists(string id);
    }

    public class MigratesRepository : IMigratesRepository
    {
        private readonly PandoraDBContext _context;

        public MigratesRepository(PandoraDBContext context)
        {
            _context = context;
        }

        public async Task<List<Migrate>> GetAllMigratesAsync()
        {
            return await _context.Migrate.ToListAsync();
        }

        public async Task<Migrate> GetMigrateByIdAsync(string id)
        {
            return await _context.Migrate.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Migrate> FindMigrateByIdAsync(string id)
        {
            return await _context.Migrate.FindAsync(id);
        }

        public async Task AddMigrateAsync(Migrate migrate)
        {
            _context.Add(migrate);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateMigrateAsync(Migrate migrate)
        {
            _context.Update(migrate);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveMigrateAsync(Migrate migrate)
        {
            _context.Migrate.Remove(migrate);
            await _context.SaveChangesAsync();
        }

        public bool MigrateExists(string id)
        {
            return _context.Migrate.Any(e => e.Id == id);
        }
    }
}