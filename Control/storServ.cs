using System.Threading.Tasks;
using Pandora.Models;
using Pandora.Repositories;

namespace Pandora.Services
{
    public interface IStorageManagementService
    {
        Task<StorageManagement> GetStorageManagementDetailsAsync(string managementClass);
    }

    public class StorageManagementService : IStorageManagementService
    {
        private readonly IStorageManagementRepository _repository;

        public StorageManagementService(IStorageManagementRepository repository)
        {
            _repository = repository;
        }

        public async Task<StorageManagement> GetStorageManagementDetailsAsync(string managementClass)
        {
            return await _repository.GetStorageManagementByClassAsync(managementClass);
        }
    }
}