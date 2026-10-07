using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pandora.Models;

namespace Pandora.Repositories
{
    public interface IStorageManagementRepository
    {
        Task<StorageManagement> GetStorageManagementByClassAsync(string managementClass);
    }

    public class StorageManagementRepository : IStorageManagementRepository
    {
        private readonly PandoraDBContext _context;

        public StorageManagementRepository(PandoraDBContext context)
        {
            _context = context;
        }

        public async Task<StorageManagement> GetStorageManagementByClassAsync(string managementClass)
        {
            return await _context.StorageManagement
                .FirstOrDefaultAsync(m => m.ManagementClass == managementClass.Trim());
        }
    }
}