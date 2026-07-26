using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Dashbord;
using AccountManagement.Infrastructure.EFCore;
using Microsoft.EntityFrameworkCore;
using ShopManagement.Domain.OrderAgg;
using ShopManagement.Infrastructure.EFCore;
using System.Globalization;

namespace _01_PharmaPlusQuery.Query
{
    public class DashboardQuery : IDashboardQuery
    {
        private readonly ShopContext _shopContext;
        private readonly AccountContext _accountContext;

        public DashboardQuery(ShopContext shopContext, AccountContext accountContext)
        {
            _shopContext = shopContext;
            _accountContext = accountContext;
        }

        public async Task<DashboardKpiDto> GetKpisAsync(CancellationToken cancellation)
        {
            // وضعیت‌های فروش موفق (پرداخت شده تا تحویل داده شده + درخواست مرجوعی که هنوز تایید/رد نشده)
            var validSalesStatuses = new[]
            {
            OrderStatus.Paid,
            OrderStatus.Processing,
            OrderStatus.Shipped,
            OrderStatus.Delivered,
            OrderStatus.ReturnRequested
        };

            return new DashboardKpiDto
            {
                TotalRevenue = await _shopContext.Orders
                    .Where(x => validSalesStatuses.Contains(x.Status))
                    .SumAsync(x => x.PayAmount, cancellation),

                TotalOrders = await _shopContext.Orders.CountAsync(cancellation),

                // تمام سفارشاتی که پرداختشان موفق بوده است (نه فقط وضعیت 4)
                PaidOrders = await _shopContext.Orders
                    .CountAsync(x => validSalesStatuses.Contains(x.Status), cancellation),

                ActiveUsers = await _accountContext.Accounts
                    .CountAsync(x => x.IsActive, cancellation),

                TotalProductViews = await _shopContext.Products
                    .SumAsync(x => x.ViewCount, cancellation)
            };
        }

        public async Task<List<ChartDto>> GetMonthlySalesLineChartAsync(
            int months = 6,
            CancellationToken cancellation = default)
        {
            var pc = new PersianCalendar();
            var now = DateTime.Now;

            int currentPersianYear = pc.GetYear(now);
            int currentPersianMonth = pc.GetMonth(now);

            int startPersianMonth = currentPersianMonth - (months - 1);
            int startPersianYear = currentPersianYear;

            if (startPersianMonth <= 0)
            {
                startPersianMonth += 12;
                startPersianYear--;
            }

            var fromDateLastYear = pc.ToDateTime(
                startPersianYear - 1,
                startPersianMonth,
                1,
                0, 0, 0, 0);

            var rawOrders = await _shopContext.Orders
                .AsNoTracking()
                .Where(x => x.CreationDate >= fromDateLastYear &&
                            ((x.Status >= OrderStatus.Paid && x.Status <= OrderStatus.Delivered) || x.Status == OrderStatus.ReturnRequested))
                .Select(x => new { x.CreationDate, x.PayAmount })
                .ToListAsync(cancellation);

            var salesByPersianMonth = rawOrders
                .GroupBy(x => new { Year = pc.GetYear(x.CreationDate), Month = pc.GetMonth(x.CreationDate) })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = (double)g.Sum(x => x.PayAmount) })
                .ToList();

            var currentYearData = new List<double>();
            var lastYearData = new List<double>();
            var labels = new List<string>();

            for (int i = months - 1; i >= 0; i--)
            {
                int month = currentPersianMonth - i;
                int year = currentPersianYear;

                if (month <= 0)
                {
                    month += 12;
                    year--;
                }

                labels.Add(GetPersianMonthName(month));

                var currentSale = salesByPersianMonth
                    .FirstOrDefault(x => x.Year == year && x.Month == month);

                var lastYearSale = salesByPersianMonth
                    .FirstOrDefault(x => x.Year == year - 1 && x.Month == month);

                currentYearData.Add(currentSale?.Total ?? 0);
                lastYearData.Add(lastYearSale?.Total ?? 0);
            }

            return new List<ChartDto>
        {
            new()
            {
                Label = "سال جاری",
                Data = currentYearData,
                labels = labels,
                BorderColor = "#4361ee",
                BackgroundColor = new List<string> { "rgba(67, 97, 238, 0.1)" }
            },
            new()
            {
                Label = "سال گذشته",
                Data = lastYearData,
                labels = labels,
                BorderColor = "#e63946",
                BackgroundColor = new List<string> { "rgba(230, 57, 70, 0.1)" }
            }
        };
        }

        public async Task<ChartDto> GetOrderStatusChartAsync(CancellationToken cancellation)
        {
            var data = await _shopContext.Orders
                .GroupBy(x => x.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(cancellation);

            // اضافه شدن وضعیت ۱۱ به لیست کامل وضعیت‌ها
            var allStatuses = new List<int>
        {
            OrderStatus.Pending, OrderStatus.WaitingForGateway, OrderStatus.WaitingForManualVerification,
            OrderStatus.Paid, OrderStatus.Processing, OrderStatus.Shipped, OrderStatus.Delivered,
            OrderStatus.NeedsRefund, OrderStatus.Cancelled, OrderStatus.Refunded, OrderStatus.ReturnRequested
        };

            var countsDict = data.ToDictionary(x => x.Status, x => (double)x.Count);

            return new ChartDto
            {
                Label = "تعداد سفارش‌ها بر اساس وضعیت",
                Data = allStatuses.Select(s => countsDict.ContainsKey(s) ? countsDict[s] : 0).ToList(),
                labels = allStatuses.Select(OrderStatus.GetStatusTitleByCode).ToList(),
                // ۱۱ رنگ جذاب و متمایز برای ۱۱ وضعیت
                BackgroundColor = new List<string>
            {
                "#f8d7da", // Pending 1
                "#fff3cd", // Gateway 2
                "#d1ecf1", // ManualVer 3
                "#d4edda", // Paid 4
                "#6c757d", // Processing 5
                "#17a2b8", // Shipped 6
                "#28a745", // Delivered 7
                "#fd7e14", // NeedsRefund 8
                "#dc3545", // Cancelled 9
                "#343a40", // Refunded 10
                "#007bff"  // ReturnRequested 11
            }
            };
        }

        public async Task<ChartDto> GetTopProductsDoughnutAsync(int take = 5, CancellationToken cancellation = default)
        {
            var data = await _shopContext.OrderItems
                .AsNoTracking()
                .Include(x => x.Order)
                .Where(x => (x.Order.Status >= OrderStatus.Paid && x.Order.Status <= OrderStatus.Delivered) || x.Order.Status == OrderStatus.ReturnRequested)
                .GroupBy(x => x.ProductName)
                .Select(g => new
                {
                    Product = g.Key,
                    Total = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Total)
                .Take(take)
                .ToListAsync(cancellation);

            return new ChartDto
            {
                Label = "سهم فروش محصولات",
                Data = data.Select(x => (double)x.Total).ToList(),
                labels = data.Select(x => x.Product).ToList(),
                BackgroundColor = new List<string> { "#4cc9f0", "#4895ef", "#4361ee", "#3f37c9", "#3a0ca3" }
            };
        }

        public async Task<ComparisonChartDto> GetSalesComparisonAsync(CancellationToken cancellation)
        {
            var pc = new PersianCalendar();
            var now = DateTime.Now;

            int currentPersianYear = pc.GetYear(now);
            int currentPersianMonth = pc.GetMonth(now);

            var currentMonthStart = pc.ToDateTime(currentPersianYear, currentPersianMonth, 1, 0, 0, 0, 0);

            int previousPersianMonth = currentPersianMonth - 1;
            int previousPersianYear = currentPersianYear;

            if (previousPersianMonth == 0)
            {
                previousPersianMonth = 12;
                previousPersianYear--;
            }

            var previousMonthStart = pc.ToDateTime(previousPersianYear, previousPersianMonth, 1, 0, 0, 0, 0);

            var currentSales = await _shopContext.Orders
                .Where(x => x.CreationDate >= currentMonthStart &&
                            ((x.Status >= OrderStatus.Paid && x.Status <= OrderStatus.Delivered) || x.Status == OrderStatus.ReturnRequested))
                .SumAsync(x => (double?)x.PayAmount, cancellation) ?? 0;

            var previousSales = await _shopContext.Orders
                .Where(x => x.CreationDate >= previousMonthStart &&
                            x.CreationDate < currentMonthStart &&
                            ((x.Status >= OrderStatus.Paid && x.Status <= OrderStatus.Delivered) || x.Status == OrderStatus.ReturnRequested))
                .SumAsync(x => (double?)x.PayAmount, cancellation) ?? 0;

            return new ComparisonChartDto
            {
                Label = "مقایسه فروش ماهانه",
                CurrentValue = currentSales,
                PreviousValue = previousSales,
                Labels = new List<string>
            {
                GetPersianMonthName(previousPersianMonth),
                GetPersianMonthName(currentPersianMonth)
            }
            };
        }

        public async Task<ChartDto> GetConversionRateAsync(CancellationToken cancellation)
        {
            var totalViews = await _shopContext.Products.SumAsync(x => x.ViewCount, cancellation);

            // نرخ تبدیل خرید موفق: تمام افرادی که تا مرحله پرداخت رفته‌اند
            var totalOrders = await _shopContext.Orders
                .CountAsync(x => (x.Status >= OrderStatus.Paid && x.Status <= OrderStatus.Delivered)
                                 || x.Status == OrderStatus.ReturnRequested
                                 || x.Status == OrderStatus.NeedsRefund
                                 || x.Status == OrderStatus.Refunded, cancellation);

            double conversionRate = totalViews > 0 ? Math.Round((double)totalOrders / totalViews * 100, 2) : 0;

            return new ChartDto
            {
                Label = "نرخ تبدیل",
                Data = new List<double> { conversionRate, 100 - conversionRate },
                labels = new List<string> { "تبدیل به خرید (فروش)", "فقط بازدید" },
                BackgroundColor = new List<string> { "#00b894", "#dfe6e9" }
            };
        }

        public async Task<ChartDto> GetTopBrandsChartAsync(CancellationToken cancellation)
        {
            var salesData = await _shopContext.OrderItems
                .Include(x => x.Order)
                .Where(x => (x.Order.Status >= OrderStatus.Paid && x.Order.Status <= OrderStatus.Delivered) || x.Order.Status == OrderStatus.ReturnRequested)
                .GroupBy(x => x.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalSales = g.Sum(x =>
                    (x.UnitPrice * x.Quantity) - ((x.DiscountedQty <= x.Quantity ? x.DiscountedQty : x.Quantity) * x.UnitPrice * x.DiscountRate / 100))
                })
                .ToListAsync(cancellation);

            var allBrands = salesData
                .Join(_shopContext.Products,
                      sale => sale.ProductId,
                      product => product.Id,
                      (sale, product) => new
                      {
                          Name = product.Brand ?? "بدون برند",
                          Amount = sale.TotalSales
                      })
                .GroupBy(x => x.Name)
                .Select(g => new { Name = g.Key, Total = g.Sum(x => x.Amount) })
                .OrderByDescending(x => x.Total)
                .ToList();

            var top5 = allBrands.Take(5).ToList();
            var others = allBrands.Skip(5).Sum(x => x.Total);

            var finalLabels = top5.Select(x => x.Name).ToList();
            var finalData = top5.Select(x => (double)x.Total).ToList();

            if (others > 0)
            {
                finalLabels.Add("سایر موارد");
                finalData.Add((double)others);
            }

            return new ChartDto
            {
                Label = "سهم فروش برندها",
                Data = finalData,
                labels = finalLabels,
                BackgroundColor = new List<string> { "#ff6384", "#36a2eb", "#ffce56", "#4bc0c0", "#9966ff", "#d2dae2" }
            };
        }

        public async Task<ChartDto> GetTopCategoriesChartAsync(CancellationToken cancellation)
        {
            var salesData = await _shopContext.OrderItems
                .Include(x => x.Order)
                .Where(x => (x.Order.Status >= OrderStatus.Paid && x.Order.Status <= OrderStatus.Delivered) || x.Order.Status == OrderStatus.ReturnRequested)
                .GroupBy(x => x.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .ToListAsync(cancellation);

            var allCategories = salesData
                .Join(_shopContext.Products,
                      sale => sale.ProductId,
                      product => product.Id,
                      (sale, product) => new { product.CategoryId, sale.TotalQuantity })
                .Join(_shopContext.ProductCategories,
                      p => p.CategoryId,
                      c => c.Id,
                      (p, c) => new
                      {
                          CategoryName = c.Name ?? "بدون دسته‌بندی",
                          Quantity = p.TotalQuantity
                      })
                .GroupBy(x => x.CategoryName)
                .Select(g => new { Name = g.Key, Total = g.Sum(x => x.Quantity) })
                .OrderByDescending(x => x.Total)
                .ToList();

            var top5 = allCategories.Take(5).ToList();
            var others = allCategories.Skip(5).Sum(x => x.Total);

            var finalLabels = top5.Select(x => x.Name).ToList();
            var finalData = top5.Select(x => (double)x.Total).ToList();

            if (others > 0)
            {
                finalLabels.Add("سایر موارد");
                finalData.Add((double)others);
            }

            return new ChartDto
            {
                Label = "محبوبیت دسته‌ها (تعداد فروش)",
                Data = finalData,
                labels = finalLabels,
                BackgroundColor = new List<string> { "#00d2d3", "#ff9f43", "#ee5253", "#10ac84", "#54a0ff", "#d2dae2" }
            };
        }

        public async Task<List<TopCustomerDto>> GetTopCustomersAsync(int take = 5, CancellationToken cancellation = default)
        {
            var topBuyers = await _shopContext.Orders
                .Where(x => (x.Status >= OrderStatus.Paid && x.Status <= OrderStatus.Delivered) || x.Status == OrderStatus.ReturnRequested)
                .GroupBy(x => x.UserId)
                .Select(g => new
                {
                    AccountId = g.Key,
                    TotalSpent = g.Sum(x => x.PayAmount),
                    OrderCount = g.Count()
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(take)
                .ToListAsync(cancellation);

            var accountIds = topBuyers.Select(x => x.AccountId).ToList();

            var accounts = await _accountContext.Accounts
                .Where(x => accountIds.Contains(x.Id))
                .Select(x => new { x.Id, x.Username })
                .ToListAsync(cancellation);

            return topBuyers.Select(buyer => new TopCustomerDto
            {
                AccountId = buyer.AccountId,
                TotalSpent = buyer.TotalSpent,
                OrderCount = buyer.OrderCount,
                Username = accounts.FirstOrDefault(a => a.Id == buyer.AccountId)?.Username ?? "کاربر ناشناس"
            }).ToList();
        }
        private static string GetPersianMonthName(int month)
        {
            string[] monthNames =
            {
                "فروردین", "اردیبهشت", "خرداد", "تیر",
                "مرداد", "شهریور", "مهر", "آبان",
                "آذر", "دی", "بهمن", "اسفند"
            };

            return monthNames[month - 1];
        }

        public async Task<List<RecentOrderDto>> GetRecentOrdersAsync(int take = 5, CancellationToken cancellation = default)
        {
            var recentOrders = await _shopContext.Orders
                .OrderByDescending(x => x.CreationDate)
                .Take(take)
                .Select(x => new RecentOrderDto
                {
                    Id = x.Id,
                    PayAmount = x.PayAmount,
                    StatusChangedAt = x.LastModifiedDate.ToFarsiFull(),
                    Status = x.Status,
                    AccountId = x.UserId
                })
                .ToListAsync(cancellation);

            var accountIds = recentOrders.Select(x => x.AccountId).Distinct().ToList();

            var accounts = await _accountContext.Accounts
                .Where(x => accountIds.Contains(x.Id))
                .Select(x => new { x.Id, x.Username })
                .ToListAsync(cancellation);

            foreach (var order in recentOrders)
            {
                order.CustomerUsername = accounts.FirstOrDefault(a => a.Id == order.AccountId)?.Username ?? "ناشناس";
            }
            return recentOrders;
        }
    }
}
