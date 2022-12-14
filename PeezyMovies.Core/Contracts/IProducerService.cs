namespace PeezyMovies.Core.Contracts
{
    using PeezyMovies.Core.Models;
    using PeezyMovies.Infrastructure.Data.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;


    public interface IProducerService
    {
        Task<bool> Exists(int id);
        Task<IEnumerable<ProducerViewModel>> GetAllAsync();

        Task AddProducerAsync(AddProducerViewModel model);

        Producer GetById(int producerId);

        AddProducerViewModel EditById(int producerId);  

        Task<ProducerViewModel> GetProducerDetails(int producerId);

        Task EditProducerDetailAsync(AddProducerViewModel model, int producerId);

        Task DeleteProducerAsync(int producerId);
    }
}
