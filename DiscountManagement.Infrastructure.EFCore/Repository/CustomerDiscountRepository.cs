using _0_Framework.Application;
using _0_Framework.Infrastructure;
using DiscountManagement.Application.Contracts.CustomerDiscount;
using DiscountManagement.Domain.CustomerDiscountAgg;
using Microsoft.EntityFrameworkCore;
using ShopManagement.Infrastructure.EFCore;
using System.Globalization;

namespace DiscountManagement.Infrastructure.EFCore.Repository
{
    public class CustomerDiscountRepository : BaseRepository<long, CustomerDiscount>, ICustomerDiscountRepository
    {
        private readonly DiscountContext _discountContext;
        private readonly ShopContext _shopContext;

        public CustomerDiscountRepository(DiscountContext context, ShopContext shopContext) : base(context)
        {
            _discountContext = context;
            _shopContext = shopContext;
        }

        public async Task<EditCustoemrDiscount> GetDetails(long id)
        {
            return await _discountContext.CustomerDiscounts.Select(x => new EditCustoemrDiscount
            {
                Id = x.Id,
                ProductId = x.ProductId,
                DiscountRate = x.DiscountRate,
                StartDate = x.StartDate.ToString(CultureInfo.InvariantCulture),
                EndDate = x.EndDate.ToString(CultureInfo.InvariantCulture),
                Reason = x.Reason,
                ProductCount = x.ProductCount
            }).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<CustomerDiscountViewModel>> Search(CustomerDiscountSearchModel searchModel)
        {
            var products = await _shopContext.Products
                .Select(x => new { x.Id, x.Name })
                .ToListAsync();

            var query = _discountContext.CustomerDiscounts.AsQueryable();

            if (searchModel.ProductId > 0)
                query = query.Where(x => x.ProductId == searchModel.ProductId);

            if (!string.IsNullOrWhiteSpace(searchModel.StartDate))
            {
                var start = searchModel.StartDate.ToGeorgianDateTime();
                query = query.Where(x => x.StartDate > start);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.EndDate))
            {
                var end = searchModel.EndDate.ToGeorgianDateTime();
                query = query.Where(x => x.EndDate < end);
            }

            var discounts = await query
                .OrderByDescending(x => x.Id)
                .Select(x => new CustomerDiscountViewModel
                {
                    Id = x.Id,
                    DiscountRate = x.DiscountRate,
                    EndDate = x.EndDate.ToFarsi().ToPersianNumber(),
                    EndDateGr = x.EndDate,
                    StartDate = x.StartDate.ToFarsi().ToPersianNumber(),
                    StartDateGr = x.StartDate,
                    ProductId = x.ProductId,
                    Reason = x.Reason,
                    ProductCount = x.ProductCount,
                    CreationDate = x.CreationDate.ToFarsi().ToPersianNumber()
                })
                .ToListAsync();

            discounts.ForEach(discount =>
                discount.Product = products.FirstOrDefault(x => x.Id == discount.ProductId)?.Name
            );

            return discounts;
        }

    }
}
