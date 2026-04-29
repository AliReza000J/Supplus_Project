using _0_Framework.Application;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ShopManagement.Application.Contracts.Order;

namespace PharmaShared.Web.Services
{
    public class InvoiceDocument : IDocument
    {
        public OrderViewModel Model { get; }
        private readonly string _webRootPath;
        private readonly string _baseLogoPath;

        public InvoiceDocument(OrderViewModel model, string webRootPath, string baseLogoPath)
        {
            Model = model;
            _webRootPath = webRootPath;
            _baseLogoPath = baseLogoPath;
        }

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.DefaultTextStyle(x => x.FontFamily("XB Niloofar").FontSize(11));
                page.Margin(1.5f, Unit.Centimetre);
                page.ContentFromRightToLeft();
                page.PageColor(Colors.White);

                // --- هدر فاکتور ---
                page.Header().PaddingBottom(30).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Row(row =>
                {
                    // بخش راست: لوگو و نام
                    row.RelativeItem().Column(col =>
                    {
                        var logoPath = Path.Combine(_webRootPath, "Pictures", _baseLogoPath);
                        if (File.Exists(logoPath))
                        {
                            // تغییر سایز: عرض روی 65 تنظیم شد (قبلا 80 بود)
                            col.Item().Width(65).Image(logoPath);
                        }

                        col.Item().PaddingTop(5).PaddingBottom(5).Text("داروخانه آنلاین فارماپلاس").FontSize(18).Bold().FontColor(Colors.Blue.Darken4);
                    });

                    // بخش چپ: اطلاعات فاکتور
                    row.RelativeItem().AlignLeft().Column(col =>
                    {
                        col.Item().Text("فاکتور فروش کالا/خدمات").FontSize(14).SemiBold();

                        col.Item().PaddingTop(10).Text(txt =>
                        {
                            txt.Span("شماره سفارش: ").SemiBold();
                            txt.Span(Model.Id.ToString()).DirectionFromLeftToRight();
                        });

                        col.Item().Text(txt =>
                        {
                            txt.Span("تاریخ ثبت: ").SemiBold();
                            txt.Span(Model.CreationDate).DirectionFromLeftToRight();
                        });

                        if (!string.IsNullOrEmpty(Model.RefNumber))
                        {
                            col.Item().PaddingTop(5).PaddingBottom(5).Text(txt =>
                            {
                                txt.Span("کد پیگیری پرداخت: ").FontSize(10);
                                txt.Span(Model.RefNumber).FontSize(10).DirectionFromLeftToRight();
                            });
                        }
                    });
                });

                // --- محتوای اصلی ---
                page.Content().PaddingVertical(10).Column(col =>
                {
                    // --- بخش مشخصات خریدار (اصلاح شده) ---
                    col.Item().Background(Colors.Grey.Lighten4).Border(1).BorderColor(Colors.Grey.Lighten3).Padding(15).Column(c =>
                    {
                        c.Item().PaddingBottom(10).Text("مشخصات خریدار").FontSize(14).Bold();

                        // ردیف اول: نام، موبایل و کد پستی
                        c.Item().Row(row =>
                        {
                            // ستون راست: نام
                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("نام و نام خانوادگی: ").Bold();
                                txt.Span(Model.ReceiverFullName);
                            });

                            // ستون وسط: موبایل
                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("شماره موبایل: ").Bold();
                                txt.Span(Model.ReceiverMobile).DirectionFromLeftToRight();
                            });

                            // ستون چپ: کد پستی
                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("کد پستی: ").Bold();
                                txt.Span(Model.ReceiverZipCode).DirectionFromLeftToRight();
                            });
                        });

                        // خط جداکننده نازک
                        c.Item().PaddingVertical(8).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

                        // ردیف دوم: آدرس (تمام عرض) - انتقال به انتها طبق درخواست
                        c.Item().Text(txt =>
                        {
                            txt.Span("آدرس پستی: ").Bold();
                            txt.Span(Model.ReceiverAddress);
                        });
                    });

                    // --- جدول اقلام سفارش ---
                    col.Item().PaddingTop(20).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30); // ردیف
                            columns.RelativeColumn(6);  // شرح کالا
                            columns.RelativeColumn(1);  // تعداد
                            columns.RelativeColumn(2);  // قیمت واحد
                            columns.RelativeColumn(2);  // تخفیف
                            columns.RelativeColumn(2);  // مبلغ کل
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderStyle).Text("#");
                            header.Cell().Element(HeaderStyle).Text("شرح کالا / خدمات");
                            header.Cell().Element(HeaderStyle).Text("تعداد");
                            header.Cell().Element(HeaderStyle).Text("قیمت واحد");
                            header.Cell().Element(HeaderStyle).Text("تخفیف");
                            header.Cell().Element(HeaderStyle).Text("مبلغ کل");

                            static IContainer HeaderStyle(IContainer c)
                            {
                                return c.Background(Colors.Blue.Lighten4)
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Blue.Lighten2)
                                        .Padding(8)
                                        .AlignCenter()
                                        .DefaultTextStyle(x => x.Bold());
                            }
                        });

                        for (int i = 0; i < Model.Items.Count; i++)
                        {
                            var item = Model.Items[i];
                            var bgColor = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten5;

                            table.Cell().Element(c => CellStyle(c, bgColor)).AlignCenter().Text((i + 1).ToString()).DirectionFromLeftToRight();
                            //table.Cell().Element(c => CellStyle(c, bgColor)).Text(item.ProductName);
                            table.Cell().Element(c => CellStyle(c, bgColor)).Text(txt =>
                            {
                                // 1. ابتدا جهت کلی متن را راست‌به‌چپ نگه می‌داریم
                                // 2. متن را به کلمات تقسیم می‌کنیم
                                var parts = item.ProductName.Split(' ');

                                foreach (var part in parts)
                                {
                                    // اگر کلمه شامل حروف انگلیسی یا عدد بود، آن را ایزوله می‌کنیم
                                    if (System.Text.RegularExpressions.Regex.IsMatch(part, @"[a-zA-Z0-9\+]"))
                                    {
                                        txt.Span("\u200E" + " " + part); // اجبار به نمایش چپ‌به‌راست برای این کلمه خاص
                                    }
                                    else
                                    {
                                        txt.Span(part + " "); // نمایش عادی برای کلمات فارسی
                                    }
                                }
                            });
                            table.Cell().Element(c => CellStyle(c, bgColor)).AlignCenter().Text(t => t.Span(item.Qty.ToString()).DirectionFromLeftToRight());
                            table.Cell().Element(c => CellStyle(c, bgColor)).AlignCenter().Text(t => t.Span(item.UnitPrice.ToMoney()).DirectionFromLeftToRight());
                            table.Cell().Element(c => CellStyle(c, bgColor)).AlignCenter().Text(t => t.Span(item.TotalDiscountAmount.ToMoney()).DirectionFromLeftToRight());
                            table.Cell().Element(c => CellStyle(c, bgColor)).AlignCenter().Text(t => t.Span(item.TotalPriceWithDiscount.ToMoney()).DirectionFromLeftToRight().Bold());

                            static IContainer CellStyle(IContainer c, string color) => c.Background(color).BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(8).PaddingHorizontal(5);
                        }
                    });

                    // --- فوتر محاسبات (اصلاح شده برای رفع مشکل آینه‌ای شدن) ---
                    col.Item().PaddingTop(25).Row(row =>
                    {
                        row.RelativeItem();

                        row.ConstantItem(350).Background(Colors.Grey.Lighten5).Border(1).BorderColor(Colors.Grey.Lighten3).Padding(15).Column(c =>
                        {
                            c.Item().Table(table =>
                            {
                                table.ColumnsDefinition(cols => { cols.RelativeColumn(1.5f); cols.RelativeColumn(2.5f); });

                                // ۱. جمع کل اقلام
                                table.Cell().PaddingBottom(5).Text("جمع کل اقلام:");
                                table.Cell().PaddingBottom(5).AlignLeft().Text(Model.TotalAmount.ToMoney()).DirectionFromLeftToRight();

                                // ۲. مجموع تخفیف‌های محصولات
                                if (Model.DiscountAmount > 0)
                                {
                                    table.Cell().PaddingBottom(5).Text("تخفیف محصولات:");
                                    table.Cell().PaddingBottom(5).AlignLeft().Row(r =>
                                    {
                                        //r.AutoItem().Text("- ").FontColor(Colors.Red.Medium); // علامت منفی جدا
                                        r.AutoItem().Text("- " + Model.DiscountAmount.ToMoney()).DirectionFromLeftToRight();
                                    });
                                }

                                // ۳. تخفیف کوپن
                                if (Model.OrderCouponAmount > 0)
                                {
                                    table.Cell().PaddingBottom(5).Text($"تخفیف کوپن {(string.IsNullOrEmpty(Model.CouponCode) ? "" : $"({Model.CouponCode})")}:");
                                    table.Cell().PaddingBottom(5).AlignLeft().Row(r =>
                                    {
                                        //r.AutoItem().Text("- ").FontColor(Colors.Red.Medium);
                                        r.AutoItem().Text("- " + Model.OrderCouponAmount.ToMoney()).DirectionFromLeftToRight();
                                    });
                                }

                                // ۴. سود نهایی (جمع کل تخفیفات)
                                var totalFinalDiscount = Model.DiscountAmount + Model.OrderCouponAmount;
                                if (totalFinalDiscount > 0)
                                {
                                    table.Cell().PaddingVertical(5).BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2).Text("سود شما از این خرید:").Bold();
                                    table.Cell().PaddingVertical(5).BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2).AlignLeft().Text(totalFinalDiscount.ToMoney()).Bold().FontColor(Colors.Green.Medium).DirectionFromLeftToRight();
                                }
                            });

                            // ۵. مبلغ نهایی قابل پرداخت
                            c.Item().PaddingTop(10).BorderTop(1).BorderColor(Colors.Grey.Lighten3).PaddingTop(10).Row(r =>
                            {
                                r.RelativeItem().Text("مبلغ قابل پرداخت (تومان): ").Bold().FontSize(14);
                                r.RelativeItem().AlignLeft().Text(Model.PayAmount.ToMoney()).Bold().FontSize(13).FontColor(Colors.Blue.Darken4).DirectionFromLeftToRight();
                            });
                        });
                    });
                });

                // --- فوتر صفحه (شماره صفحه) ---
                page.Footer().PaddingTop(30).Column(col =>
                {
                    col.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(10).AlignCenter().Text(x =>
                    {
                        x.Span("صفحه ").FontSize(9);
                        x.CurrentPageNumber().Format(num => num.ToString());
                        x.Span(" از ").FontSize(9);
                        x.TotalPages().Format(num => num.ToString());
                    });
                });
            });
        }
    }
}