using _0_Framework.Application;

namespace DiscountManagement.Application.Contracts.ColleagueDiscount
{
    public interface IColleagueDiscountApplication
    {
        Task<OperationResult> Define(DefineColleagueDiscount command, CancellationToken cancellation);
        Task<OperationResult> Edit(EditColleagueDiscount command, CancellationToken cancellation);
        Task<OperationResult> Remove(long id, CancellationToken cancellation);
        Task<OperationResult> Restore(long id, CancellationToken cancellation);
        Task<EditColleagueDiscount?> GetDetails(long id, CancellationToken cancellation);
        Task<List<ColleagueDiscountViewModel>> Search(ColleagueDiscountSearchModel searchModel, CancellationToken cancellation);
    }
}
