using _0_Framework.Application;
using Microsoft.Extensions.Configuration;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ShopManagement.Application.Contracts.Order;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace PharmaShared.Web.Services
{
    public class InvoiceDocument : IDocument
    {
        public OrderViewModel Model { get; }
        private readonly IConfiguration _config;
        private readonly string _baseLogoPath;

        public InvoiceDocument(OrderViewModel model, IConfiguration config, string baseLogoPath)
        {
            Model = model;
            _config = config;
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

                page.Header().PaddingBottom(30).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        var basePath = _config["UploadSettings:BasePath"]
                            ?? throw new InvalidOperationException("Upload path is not configured.");

                        var logoPath = Path.Combine(basePath, "Pictures", _baseLogoPath);
                        if (File.Exists(logoPath))
                        {
                            col.Item().Width(65).Image(logoPath);
                        }

                        col.Item().PaddingTop(5).PaddingBottom(5).Text("داروخانه آنلاین فارماپلاس").FontSize(18).Bold().FontColor(Colors.Blue.Darken4);
                    });

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
                            txt.Span(Model.CreationDate.ToPersianNumber()).DirectionFromLeftToRight();
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

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Background(Colors.Grey.Lighten4).Border(1).BorderColor(Colors.Grey.Lighten3).Padding(15).Column(c =>
                    {
                        c.Item().PaddingBottom(10).Text("مشخصات خریدار").FontSize(14).Bold();

                        c.Item().Row(row =>
                        {
                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("نام و نام خانوادگی: ").Bold();
                                txt.Span(Model.ReceiverFullName);
                            });

                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("شماره موبایل: ").Bold();
                                txt.Span(Model.ReceiverMobile.ToPersianNumber()).DirectionFromLeftToRight();
                            });

                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("کد پستی: ").Bold();
                                txt.Span(Model.ReceiverZipCode.ToPersianNumber()).DirectionFromLeftToRight();
                            });
                        });

                        c.Item().PaddingVertical(8).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

                        c.Item().Text(txt =>
                        {
                            c.Item().Text(txt =>
                            {
                                txt.Span("آدرس پستی: ").Bold();
                                RenderBiDiText(txt, Model.ReceiverAddress.ToPersianNumber());
                            });
                        });
                    });

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
                            table.Cell().Element(c => CellStyle(c, bgColor)).Text(txt => RenderBiDiText(txt, item.ProductName.ToPersianNumber()));
                            table.Cell().Element(c => CellStyle(c, bgColor)).AlignCenter().Text(t => t.Span(item.Qty.ToString().ToPersianNumber()).DirectionFromLeftToRight());
                            table.Cell().Element(c => CellStyle(c, bgColor)).AlignCenter().Text(t => t.Span(item.UnitPrice.ToMoney().ToPersianNumber()).DirectionFromLeftToRight());
                            table.Cell().Element(c => CellStyle(c, bgColor)).AlignCenter().Text(t => t.Span(item.TotalDiscountAmount.ToMoney().ToPersianNumber()).DirectionFromLeftToRight());
                            table.Cell().Element(c => CellStyle(c, bgColor)).AlignCenter().Text(t => t.Span(item.TotalPriceWithDiscount.ToMoney().ToPersianNumber()).DirectionFromLeftToRight().Bold());

                            static IContainer CellStyle(IContainer c, string color) => c.Background(color).BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(8).PaddingHorizontal(5);
                        }
                    });

                    col.Item().PaddingTop(25).Row(row =>
                    {
                        row.RelativeItem();

                        row.ConstantItem(350).Background(Colors.Grey.Lighten5).Border(1).BorderColor(Colors.Grey.Lighten3).Padding(15).Column(c =>
                        {
                            c.Item().Table(table =>
                            {
                                table.ColumnsDefinition(cols => { cols.RelativeColumn(1.5f); cols.RelativeColumn(2.5f); });

                                table.Cell().PaddingBottom(5).Text("جمع کل اقلام:");
                                table.Cell().PaddingBottom(5).AlignLeft().Text(Model.TotalAmount.ToMoney().ToPersianNumber()).DirectionFromLeftToRight();

                                if (Model.DiscountAmount > 0)
                                {
                                    table.Cell().PaddingBottom(5).Text("تخفیف محصولات:");
                                    table.Cell().PaddingBottom(5).AlignLeft().Row(r =>
                                    {
                                        r.AutoItem().Text("- " + Model.DiscountAmount.ToMoney().ToPersianNumber())
                                         .DirectionFromLeftToRight()
                                         .FontColor(Colors.Red.Medium);
                                    });
                                }

                                if (Model.OrderCouponAmount > 0)
                                {
                                    table.Cell().PaddingBottom(5).Text(txt =>
                                    {
                                        var couponText = $"تخفیف کوپن {(string.IsNullOrEmpty(Model.CouponCode) ? "" : $"({Model.CouponCode})")}:";
                                        RenderBiDiText(txt, couponText);
                                    });

                                    table.Cell().PaddingBottom(5).AlignLeft().Row(r =>
                                    {
                                        r.AutoItem().Text("- " + Model.OrderCouponAmount.ToMoney().ToPersianNumber())
                                         .DirectionFromLeftToRight()
                                         .FontColor(Colors.Red.Medium);
                                    });
                                }

                                var totalFinalDiscount = Model.DiscountAmount + Model.OrderCouponAmount;
                                if (totalFinalDiscount > 0)
                                {
                                    table.Cell().PaddingVertical(5).BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2).Text("سود شما از این خرید:").Bold();
                                    table.Cell().PaddingVertical(5).BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2).AlignLeft()
                                         .Text(totalFinalDiscount.ToMoney().ToPersianNumber())
                                         .Bold()
                                         .FontColor(Colors.Green.Darken1)
                                         .DirectionFromLeftToRight();
                                }
                            });

                            c.Item().PaddingTop(10).BorderTop(1).BorderColor(Colors.Grey.Lighten3).PaddingTop(10).Row(r =>
                            {
                                r.RelativeItem().Text("مبلغ قابل پرداخت (تومان): ").Bold().FontSize(14);
                                r.RelativeItem().AlignLeft().Text(Model.PayAmount.ToMoney().ToPersianNumber()).Bold().FontSize(13).FontColor(Colors.Blue.Darken4).DirectionFromLeftToRight();
                            });
                        });
                    });
                });

                page.Footer().PaddingTop(30).Column(col =>
                {
                    col.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(10).AlignCenter().Text(x =>
                    {
                        x.Span("صفحه ").FontSize(9);
                        x.CurrentPageNumber().Format(num => num.ToString().ToPersianNumber());
                        x.Span(" از ").FontSize(9);
                        x.TotalPages().Format(num => num.ToString().ToPersianNumber());
                    });
                });
            });
        }

        static void RenderBiDiText(TextDescriptor txt, string content)
        {
            if (string.IsNullOrEmpty(content)) return;

            var regex = new System.Text.RegularExpressions.Regex(@"([a-zA-Z0-9۰-۹\+\-\.\/\\%\(\)]+(?:\s+[a-zA-Z0-9۰-۹\+\-\.\/\\%\(\)]+)*)");
            var parts = regex.Split(content);

            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part)) continue;
                if (regex.IsMatch(part))
                    txt.Span(part).DirectionFromLeftToRight();
                else
                    txt.Span(part);
            }
        }

    }
}