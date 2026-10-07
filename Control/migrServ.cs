using System.Collections.Generic;
using System.Threading.Tasks;
using Pandora.Models;
using Pandora.Repositories;

namespace Pandora.Services
{
    public interface IMigratesService
    {
        Task<List<Migrate>> GetMigratesAsync();
        Task<Migrate> GetMigrateDetailsAsync(string id);
        Task CreateMigrateAsync(Migrate migrate);
        Task<Migrate> GetMigrateForEditAsync(string id);
        Task UpdateMigrateAsync(Migrate migrate);
        Task<Migrate> GetMigrateForDeleteAsync(string id);
        Task DeleteMigrateAsync(string id);
        bool MigrateExists(string id);
    }

    public class MigratesService : IMigratesService
    {
        private readonly IMigratesRepository _repository;

        public MigratesService(IMigratesRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Migrate>> GetMigratesAsync()
        {
            return await _repository.GetAllMigratesAsync();
        }

        public async Task<Migrate> GetMigrateDetailsAsync(string id)
        {
            return await _repository.GetMigrateByIdAsync(id);
        }

        public async Task CreateMigrateAsync(Migrate migrate)
        {
            await _repository.AddMigrateAsync(migrate);
        }

        public async Task<Migrate> GetMigrateForEditAsync(string id)
        {
            return await _repository.FindMigrateByIdAsync(id);
        }

        public async Task UpdateMigrateAsync(Migrate migrate)
        {
            await _repository.UpdateMigrateAsync(migrate);
        }

        public async Task<Migrate> GetMigrateForDeleteAsync(string id)
        {
            return await _repository.GetMigrateByIdAsync(id);
        }

        public async Task DeleteMigrateAsync(string id)
        {
            var migrate = await _repository.FindMigrateByIdAsync(id);
            if (migrate != null)
            {
                await _repository.RemoveMigrateAsync(migrate);
            }
        }

        public bool MigrateExists(string id)
        {
            return _repository.MigrateExists(id);
        }
    }
}