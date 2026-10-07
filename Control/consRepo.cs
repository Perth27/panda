using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pandora.Models;

namespace Pandora.Repositories
{
    public interface IConsolidatedBackupsRepository
    {
        IQueryable<ConsolidatedValidationsData> GetAllAsNoTracking();
        IQueryable<ConsolidatedValidationsData> GetAll();
        Task<List<ConsolidatedValidationsData>> GetAllListAsync();
        Task<ConsolidatedValidationsData> GetByIdAsync(int? id);
        Task<ConsolidatedValidationsData> FindByIdAsync(int? id);
        Task UpdateAsync(ConsolidatedValidationsData record);
        bool RecordExists(int id);
    }

    public class ConsolidatedBackupsRepository : IConsolidatedBackupsRepository
    {
        private readonly PandoraDBContext _context;

        public ConsolidatedBackupsRepository(PandoraDBContext context)
        {
            _context = context;
        }

        public IQueryable<ConsolidatedValidationsData> GetAllAsNoTracking()
        {
            return _context.ConsolidatedValidationsData.AsNoTracking();
        }

        public IQueryable<ConsolidatedValidationsData> GetAll()
        {
            return _context.ConsolidatedValidationsData;
        }

        public async Task<List<ConsolidatedValidationsData>> GetAllListAsync()
        {
            return await _context.ConsolidatedValidationsData.ToListAsync();
        }

        public async Task<ConsolidatedValidationsData> GetByIdAsync(int? id)
        {
            return await _context.ConsolidatedValidationsData.Where(m => m.Id == id).FirstOrDefaultAsync();
        }

        public async Task<ConsolidatedValidationsData> FindByIdAsync(int? id)
        {
            return await _context.ConsolidatedValidationsData.FindAsync(id);
        }

        public async Task UpdateAsync(ConsolidatedValidationsData record)
        {
            _context.ConsolidatedValidationsData.Update(record);
            await _context.SaveChangesAsync();
        }

        public bool RecordExists(int id)
        {
            return _context.ConsolidatedValidationsData.Any(e => e.Id == id);
        }
    }
}