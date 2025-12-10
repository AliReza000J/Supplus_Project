using _0_Framework.Application;
using _0_Framework.Infrastructure;
using DiscountManagement.Application.Contracts.ColleagueDiscount;
using DiscountManagement.Domain.ColleagueDiscountAgg;
using Microsoft.EntityFrameworkCore;
using ShopManagement.Infrastructure.EFCore;

namespace DiscountManagement.Infrastructure.EFCore.Repository
{
    public class ColleagueDiscountRepository : BaseRepository<long, ColleagueDiscount>, IColleagueDiscountRepository
    {
        private readonly DiscountContext _discountContext;
        private readonly ShopContext _shopContext;

        public ColleagueDiscountRepository(DiscountContext context, ShopContext shopContext) : base(context)
        {
            _discountContext = context;
            _shopContext = shopContext;
        }

        public async Task<EditColleagueDiscount> GetDetails(long id)
        {
            return await _discountContext.ColleagueDiscounts.Select(x => new EditColleagueDiscount
            {
                Id = x.Id,
                DiscountRate = x.DiscountRate,
                ProductId = x.ProductId
            }).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<ColleagueDiscountViewModel>> Search(ColleagueDiscountSearchModel searchModel)
        {
            var products = await _shopContext.Products
                .Select(x => new { x.Id, x.Name })
                .ToListAsync();

            var query = _discountContext.ColleagueDiscounts
                .Select(x => new ColleagueDiscountViewModel
                {
                    Id = x.Id,
                    CreationDate = x.CreationDate.ToFarsi().ToPersianNumber(),
                    DiscountRate = x.DiscountRate,
                    ProductId = x.ProductId,
                    IsRemoved = x.IsRemoved
                });

            if (searchModel.ProductId > 0)
                query = query.Where(x => x.ProductId == searchModel.ProductId);

            var discounts = await query
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            discounts.ForEach(d =>
                d.Product = products.FirstOrDefault(p => p.Id == d.ProductId)?.Name
            );

            return discounts;
        }

    }
}
