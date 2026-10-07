using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pandora.Models;

namespace Pandora.Repositories
{
    public interface ITapeDSRepository
    {
        Task<List<TapeDS>> GetAllTapeDSAsync();
        Task<TapeDS> GetTapeDSByIdAsync(string id);
        Task<TapeDS> FindTapeDSByIdAsync(string id);
        Task AddTapeDSAsync(TapeDS tapeDS);
        Task UpdateTapeDSAsync(TapeDS tapeDS);
        Task RemoveTapeDSAsync(TapeDS tapeDS);
        bool TapeDSExists(string id);
    }

    public class TapeDSRepository : ITapeDSRepository
    {
        private readonly PandoraDBContext _context;

        public TapeDSRepository(PandoraDBContext context)
        {
            _context = context;
        }

        public async Task<List<TapeDS>> GetAllTapeDSAsync()
        {
            return await _context.TapeDS.ToListAsync();
        }

        public async Task<TapeDS> GetTapeDSByIdAsync(string id)
        {
            return await _context.TapeDS.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<TapeDS> FindTapeDSByIdAsync(string id)
        {
            return await _context.TapeDS.FindAsync(id);
        }

        public async Task AddTapeDSAsync(TapeDS tapeDS)
        {
            _context.Add(tapeDS);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTapeDSAsync(TapeDS tapeDS)
        {
            _context.Update(tapeDS);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveTapeDSAsync(TapeDS tapeDS)
        {
            _context.TapeDS.Remove(tapeDS);
            await _context.SaveChangesAsync();
        }

        public bool TapeDSExists(string id)
        {
            return _context.TapeDS.Any(e => e.Id == id);
        }
    }
}