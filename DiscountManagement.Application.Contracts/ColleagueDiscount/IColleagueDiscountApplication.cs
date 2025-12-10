using _0_Framework.Application;

namespace DiscountManagement.Application.Contracts.ColleagueDiscount
{
    public interface IColleagueDiscountApplication
    {
        Task<OperationResult> Define(DefineColleagueDiscount command);
        Task<OperationResult> Edit(EditColleagueDiscount command);
        Task<OperationResult> Remove(long id);
        Task<OperationResult> Restore(long id);
        Task<EditColleagueDiscount> GetDetails(long id);
        Task<List<ColleagueDiscountViewModel>> Search(ColleagueDiscountSearchModel searchModel);
    }
}
