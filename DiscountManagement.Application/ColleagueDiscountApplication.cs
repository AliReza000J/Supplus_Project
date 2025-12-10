using _0_Framework.Application;
using DiscountManagement.Application.Contracts.ColleagueDiscount;
using DiscountManagement.Domain.ColleagueDiscountAgg;
using System;
using System.Collections.Generic;

namespace DiscountManagement.Application
{
    public class ColleagueDiscountApplication : IColleagueDiscountApplication
    {
        private readonly IColleagueDiscountRepository _colleagueDiscountRepository;

        public ColleagueDiscountApplication(IColleagueDiscountRepository colleagueDiscountRepository)
        {
            _colleagueDiscountRepository = colleagueDiscountRepository;
        }

        public async Task<OperationResult> Define(DefineColleagueDiscount command)
        {
            var operation = new OperationResult();
            if (await _colleagueDiscountRepository.ExistsAsync
                (x => x.ProductId == command.ProductId && x.DiscountRate == command.DiscountRate))
                return operation.Failed(ApplicationMessages.DuplicatedRecord);

            var colleagueDiscount = new ColleagueDiscount(command.ProductId, command.DiscountRate);

            _colleagueDiscountRepository.Create(colleagueDiscount);
            await _colleagueDiscountRepository.SaveChangesAsync();
            return operation.Succeeded();
        }

        public async Task<OperationResult> Edit(EditColleagueDiscount command)
        {
            var operation = new OperationResult();
            var colleagueDiscount = await _colleagueDiscountRepository.GetAsync(command.Id);
            if (colleagueDiscount == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            if (await _colleagueDiscountRepository.ExistsAsync(x => x.ProductId == command.ProductId && x.DiscountRate == command.DiscountRate && x.Id != command.Id))
                return operation.Failed(ApplicationMessages.DuplicatedRecord);

            colleagueDiscount.Edit(command.ProductId, command.DiscountRate);

            await _colleagueDiscountRepository.SaveChangesAsync();
            return operation.Succeeded();
        }

        public async Task<EditColleagueDiscount> GetDetails(long id)
        {
            return await _colleagueDiscountRepository.GetDetails(id);
        }

        public async Task<OperationResult> Remove(long id)
        {
            var operation = new OperationResult();
            var colleagueDiscount = await _colleagueDiscountRepository.GetAsync(id);
            if (colleagueDiscount == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            colleagueDiscount.Remove();

            await _colleagueDiscountRepository.SaveChangesAsync();
            return operation.Succeeded();
        }

        public async Task<OperationResult> Restore(long id)
        {
            var operation = new OperationResult();
            var colleagueDiscount = await _colleagueDiscountRepository.GetAsync(id);
            if (colleagueDiscount == null)
                return operation.Failed(ApplicationMessages.RecordNotFound);

            colleagueDiscount.Restore();

            await _colleagueDiscountRepository.SaveChangesAsync();
            return operation.Succeeded();
        }

        public async Task<List<ColleagueDiscountViewModel>> Search(ColleagueDiscountSearchModel searchModel)
        {
            return await _colleagueDiscountRepository.Search(searchModel);
        }
    }
}
