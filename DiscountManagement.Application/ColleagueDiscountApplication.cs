using _0_Framework.Application;
using DiscountManagement.Application.Contracts.ColleagueDiscount;
using DiscountManagement.Domain.ColleagueDiscountAgg;

namespace DiscountManagement.Application
{
    public class ColleagueDiscountApplication : IColleagueDiscountApplication
    {
        private readonly IColleagueDiscountRepository _colleagueDiscountRepository;

        public ColleagueDiscountApplication(IColleagueDiscountRepository colleagueDiscountRepository)
        {
            _colleagueDiscountRepository = colleagueDiscountRepository;
        }

        public async Task<OperationResult> Define(DefineColleagueDiscount command, CancellationToken cancellation)
        {
            var operation = new OperationResult();
            if (await _colleagueDiscountRepository.ExistsAsync
                (x => x.ProductId == command.ProductId && x.DiscountRate == command.DiscountRate, cancellation))
                return operation.Failed(ApplicationMessages.DuplicatedRecord);

            var colleagueDiscount = new ColleagueDiscount(command.ProductId, command.DiscountRate);

            _colleagueDiscountRepository.Create(colleagueDiscount);
            await _colleagueDiscountRepository.SaveChangesAsync(cancellation);
            return operation.Succeeded();
        }

        public async Task<OperationResult> Edit(EditColleagueDiscount command, CancellationToken cancellation)
        {
            var operation = new OperationResult();
            var colleagueDiscount = await _colleagueDiscountRepository.GetAsync(command.Id, cancellation);
            if (colleagueDiscount == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            if (await _colleagueDiscountRepository.ExistsAsync(x => x.ProductId == command.ProductId && x.DiscountRate == command.DiscountRate && x.Id != command.Id, cancellation))
                return operation.Failed(ApplicationMessages.DuplicatedRecord);

            colleagueDiscount.Edit(command.ProductId, command.DiscountRate);

            await _colleagueDiscountRepository.SaveChangesAsync(cancellation);
            return operation.Succeeded();
        }

        public async Task<EditColleagueDiscount?> GetDetails(long id, CancellationToken cancellation)
        {
            return await _colleagueDiscountRepository.GetDetails(id, cancellation);
        }

        public async Task<OperationResult> Remove(long id, CancellationToken cancellation)
        {
            var operation = new OperationResult();
            var colleagueDiscount = await _colleagueDiscountRepository.GetAsync(id, cancellation);
            if (colleagueDiscount == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            colleagueDiscount.Remove();

            await _colleagueDiscountRepository.SaveChangesAsync(cancellation);
            return operation.Succeeded();
        }

        public async Task<OperationResult> Restore(long id, CancellationToken cancellation)
        {
            var operation = new OperationResult();
            var colleagueDiscount = await _colleagueDiscountRepository.GetAsync(id, cancellation);
            if (colleagueDiscount == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            colleagueDiscount.Restore();

            await _colleagueDiscountRepository.SaveChangesAsync(cancellation);
            return operation.Succeeded();
        }

        public async Task<List<ColleagueDiscountViewModel>> Search(ColleagueDiscountSearchModel searchModel, CancellationToken cancellation)
        {
            return await _colleagueDiscountRepository.Search(searchModel, cancellation);
        }
    }
}
