using _0_Framework.Application;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ShopManagement.Application.Contracts.Order;
using SiteManagement.Application;

namespace ServiceHost.Services
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
                            txt.Span(Model.Id.ToString().ToPersianNumber()).DirectionFromLeftToRight();
                        });

                        col.Item().Text(txt =>
                        {
                            txt.Span("تاریخ ثبت: ").SemiBold();
                            txt.Span(Model.CreationDate?.ToPersianNumber()).DirectionFromLeftToRight();
                        });

                        if (!string.IsNullOrEmpty(Model.RefNumber))
                        {
                            col.Item().PaddingTop(5).PaddingBottom(5).Text(txt =>
                            {
                                txt.Span("کد پیگیری پرداخت: ").FontSize(10);
                                txt.Span(Model.RefNumber.ToPersianNumber()).FontSize(10).DirectionFromLeftToRight();
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
                                txt.Span(Model.ReceiverMobile?.ToPersianNumber()).DirectionFromLeftToRight();
                            });

                            // ستون چپ: کد پستی
                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("کد پستی: ").Bold();
                                txt.Span(Model.ReceiverZipCode?.ToPersianNumber()).DirectionFromLeftToRight();
                            });
                        });

                        // خط جداکننده نازک
                        c.Item().PaddingVertical(8).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

                        // ردیف دوم: آدرس (تمام عرض) - انتقال به انتها طبق درخواست
                        c.Item().Text(txt =>
                        {
                            txt.Span("آدرس پستی: ").Bold();
                            txt.Span(Model.ReceiverAddress?.ToPersianNumber());
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

                            table.Cell().Element(c => CellStyle(c, bgColor)).AlignCenter().Text((i + 1).ToString().ToPersianNumber()).DirectionFromLeftToRight();
                            table.Cell().Element(c => CellStyle(c, bgColor)).Text(item.ProductName);
                            table.Cell().Element(c => CellStyle(c, bgColor)).AlignCenter().Text(t => t.Span(item.Qty.ToString().ToPersianNumber()).DirectionFromLeftToRight());
                            table.Cell().Element(c => CellStyle(c, bgColor)).AlignCenter().Text(t => t.Span(item.UnitPrice.ToMoney().ToPersianNumber()).DirectionFromLeftToRight());
                            table.Cell().Element(c => CellStyle(c, bgColor)).AlignCenter().Text(t => t.Span(item.TotalDiscountAmount.ToMoney().ToPersianNumber()).DirectionFromLeftToRight());
                            table.Cell().Element(c => CellStyle(c, bgColor)).AlignCenter().Text(t => t.Span(item.TotalPriceWithDiscount.ToMoney().ToPersianNumber()).DirectionFromLeftToRight().Bold());

                            static IContainer CellStyle(IContainer c, string color) => c.Background(color).BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(8).PaddingHorizontal(5);
                        }
                    });

                    // --- فوتر محاسبات (اصلاح شده) ---
                    col.Item().PaddingTop(25).Row(row =>
                    {
                        row.RelativeItem(1); // فضای خالی سمت راست

                        // افزایش عرض باکس محاسبات به 350 برای جلوگیری از شکستن خطوط
                        row.ConstantItem(350).Background(Colors.Grey.Lighten5).Border(1).BorderColor(Colors.Grey.Lighten3).Padding(15).Column(c =>
                        {
                            c.Item().Table(table =>
                            {
                                table.ColumnsDefinition(cols => { cols.RelativeColumn(1.5f); cols.RelativeColumn(2.5f); });

                                table.Cell().PaddingBottom(5).Text("جمع کل اقلام:");
                                table.Cell().PaddingBottom(5).AlignLeft().Text(t => t.Span($"{Model.TotalAmount.ToMoney().ToPersianNumber()}").DirectionFromLeftToRight());

                                table.Cell().PaddingBottom(5).Text("سود شما از این خرید:");
                                table.Cell().PaddingBottom(5).AlignLeft().Text(t => t.Span($"{Model.DiscountAmount.ToMoney().ToPersianNumber()}").DirectionFromLeftToRight().FontColor(Colors.Red.Medium));
                            });

                            c.Item().PaddingTop(10).BorderTop(1).BorderColor(Colors.Grey.Lighten3).PaddingTop(10).Row(r =>
                            {
                                r.RelativeItem().Text("مبلغ قابل پرداخت به تومان: ").Bold().FontSize(14);
                                r.RelativeItem().AlignLeft().Text(t => t.Span($"{Model.PayAmount.ToMoney().ToPersianNumber()}").DirectionFromLeftToRight().Bold().FontSize(13).FontColor(Colors.Blue.Darken4));
                            });
                        });
                    });
                });

                // --- فوتر صفحه ---
                page.Footer().PaddingTop(30).Column(col =>
                {
                    col.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(10).AlignCenter().Text(x =>
                    {
                        x.Span("از اعتماد شما به داروخانه آنلاین فارماپلاس سپاسگزاریم | ").FontSize(9).FontColor(Colors.Grey.Darken1);

                        x.Span("صفحه ").FontSize(9);

                        // اصلاح شماره صفحه فعلی
                        x.CurrentPageNumber().Format(num => num.ToString().ToPersianNumber());

                        x.Span(" از ").FontSize(9);

                        // اصلاح تعداد کل صفحات
                        x.TotalPages().Format(num => num.ToString().ToPersianNumber());
                    });
                });
            });
        }
    }
}