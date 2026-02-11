using _0_Framework.Domain;
using DiscountManagement.Application.Contracts.ColleagueDiscount;

namespace DiscountManagement.Domain.ColleagueDiscountAgg
{
    public interface IColleagueDiscountRepository : IRepository<long, ColleagueDiscount>
    {
        Task<EditColleagueDiscount?> GetDetails(long id, CancellationToken cancellation);
        Task<List<ColleagueDiscountViewModel>> Search(ColleagueDiscountSearchModel searchModel, CancellationToken cancellation);
    }
}
