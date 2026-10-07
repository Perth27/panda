using System.Collections.Generic;
using System.Threading.Tasks;
using Pandora.Models;
using Pandora.Repositories;

namespace Pandora.Services
{
    public interface ITapeDSService
    {
        Task<List<TapeDS>> GetTapeDSAsync();
        Task<TapeDS> GetTapeDSDetailsAsync(string id);
        Task CreateTapeDSAsync(TapeDS tapeDS);
        Task<TapeDS> GetTapeDSForEditAsync(string id);
        Task UpdateTapeDSAsync(TapeDS tapeDS);
        Task<TapeDS> GetTapeDSForDeleteAsync(string id);
        Task DeleteTapeDSAsync(string id);
        bool TapeDSExists(string id);
    }

    public class TapeDSService : ITapeDSService
    {
        private readonly ITapeDSRepository _repository;

        public TapeDSService(ITapeDSRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<TapeDS>> GetTapeDSAsync()
        {
            return await _repository.GetAllTapeDSAsync();
        }

        public async Task<TapeDS> GetTapeDSDetailsAsync(string id)
        {
            return await _repository.GetTapeDSByIdAsync(id);
        }

        public async Task CreateTapeDSAsync(TapeDS tapeDS)
        {
            await _repository.AddTapeDSAsync(tapeDS);
        }

        public async Task<TapeDS> GetTapeDSForEditAsync(string id)
        {
            return await _repository.FindTapeDSByIdAsync(id);
        }

        public async Task UpdateTapeDSAsync(TapeDS tapeDS)
        {
            await _repository.UpdateTapeDSAsync(tapeDS);
        }

        public async Task<TapeDS> GetTapeDSForDeleteAsync(string id)
        {
            return await _repository.GetTapeDSByIdAsync(id);
        }

        public async Task DeleteTapeDSAsync(string id)
        {
            var tapeDS = await _repository.FindTapeDSByIdAsync(id);
            if (tapeDS != null)
            {
                await _repository.RemoveTapeDSAsync(tapeDS);
            }
        }

        public bool TapeDSExists(string id)
        {
            return _repository.TapeDSExists(id);
        }
    }
}