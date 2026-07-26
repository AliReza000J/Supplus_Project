using _0_Framework.Application;
using AccountManagement.Domain.AccountAgg;
using AccountManagement.Domain.ShippingInfoAgg;
using DiscountManagement.Application.Contracts.Discount;
using DiscountManagement.Domain.DiscountAgg;
using InventoryManagement.Application.Contracts.Inventory;
using InventoryManagement.Domain.InventoryAgg;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShopManagement.Application.Contracts.Cart;
using ShopManagement.Application.Contracts.Order;
using ShopManagement.Domain.CartAgg;
using ShopManagement.Domain.OrderAgg;
using ShopManagement.Domain.ProductAgg;
using System.Transactions;

namespace ShopManagement.Application
{

    public class OrderApplication : IOrderApplication
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IShippingInfoRepository _shippingInfoRepository;
        private readonly ICartApplication _cartApplication;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IDiscountRepository _discountRepository;
        private readonly IFileUploader fileUploader;
        private readonly IZibalService _zibalService;
        private readonly IConfiguration _config;
        private readonly IInventoryApplication _inventoryApplication;
        private readonly IDiscountApplication _discountApplication;

        public OrderApplication(IAccountRepository accountRepository,
            ICartRepository cartRepository,
            IInventoryRepository inventoryRepository,
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            ICartApplication cartApplication,
            IFileUploader fileUploader,
            IZibalService zibalService,
            IConfiguration config,
            IInventoryApplication inventoryApplication,
            IDiscountApplication discountApplication,
            IDiscountRepository discountRepository,
            IShippingInfoRepository shippingInfoRepository)
        {
            _accountRepository = accountRepository;
            _cartRepository = cartRepository;
            _inventoryRepository = inventoryRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _cartApplication = cartApplication;
            this.fileUploader = fileUploader;
            _zibalService = zibalService;
            _config = config;
            _inventoryApplication = inventoryApplication;
            _discountApplication = discountApplication;
            _discountRepository = discountRepository;
            _shippingInfoRepository = shippingInfoRepository;
        }

        public async Task<OperationResult> ConfirmPaymentManuallyAsync(ConfirmPaymentManually command, CancellationToken cancellation)
        {
            var op = new OperationResult();

            var order = await _orderRepository.GetWithItemsAsync(command.OrderId, cancellation);
            if (order == null) return op.Failed("سفارش یافت نشد.");

            if (order.Status != OrderStatus.WaitingForManualVerification)
                return op.Failed("وضعیت سفارش برای تایید کارت به کارت نامعتبر است.");

            try
            {
                var (finalizeResult, finalizeResultType) = await FinalizeOrderProcessingAsync(order, cancellation);

                if (finalizeResult.IsSucceeded)
                {
                    order.ConfirmManually();
                    await _orderRepository.SaveChangesAsync(cancellation);

                    return op.Succeeded("پرداخت کارت به کارت با موفقیت تایید شد.");
                }
                else if (finalizeResultType == OperationResultType.ConcurrencyError)
                {
                    var finalReason =
                        $"[کد]: [EXCEPTION - موجودی یا تخفیف تمام شده]{Environment.NewLine}" +
                        $"[توضیحات]: {finalizeResult.Message}{Environment.NewLine}" +
                        $"[شناسه کاربر]: {order.UserId.ToString().ToPersianNumber()}{Environment.NewLine}" +
                        $"[شناسه سفارش]: {order.Id.ToString().ToPersianNumber()}{Environment.NewLine}" +
                        $"[تاریخ]: {DateTime.Now.ToFarsiFull().ToPersianNumber()}";

                    order.Cancel(finalReason);
                    await _orderRepository.SaveChangesAsync(cancellation);

                    return op.Failed($"امکان تایید وجود ندارد!\n{finalizeResult.Message}\n\nسفارش لغو شد و به کاربر اطلاع‌رسانی خواهد شد.");
                }
                else
                {
                    var finalReason =
                        $"[کد]: [EXCEPTION - خطا در تایید سفارش]{Environment.NewLine}" +
                        $"[توضیحات]: {finalizeResult.Message}{Environment.NewLine}" +
                        $"[شناسه کاربر]: {order.UserId.ToString().ToPersianNumber()}{Environment.NewLine}" +
                        $"[شناسه سفارش]: {order.Id.ToString().ToPersianNumber()}{Environment.NewLine}" +
                        $"[تاریخ]: {DateTime.Now.ToFarsiFull().ToPersianNumber()}";

                    order.Cancel(finalReason);
                    await _orderRepository.SaveChangesAsync(cancellation);

                    return op.Failed($"خطا در تایید: {finalizeResult.Message}\n\nسفارش لغو شد و به کاربر اطلاع‌رسانی خواهد شد.");
                }
            }
            catch (Exception ex)
            {
                var finalReason =
                        $"[کد]: [EXCEPTION - خطای غیرمنتظره]{Environment.NewLine}" +
                        $"[توضیحات]: {ex.Message}{Environment.NewLine}" +
                        $"[شناسه کاربر]: {order.UserId.ToString().ToPersianNumber()}{Environment.NewLine}" +
                        $"[شناسه سفارش]: {order.Id.ToString().ToPersianNumber()}{Environment.NewLine}" +
                        $"[تاریخ]: {DateTime.Now.ToFarsiFull().ToPersianNumber()}";

                order.Cancel(finalReason);
                await _orderRepository.SaveChangesAsync(cancellation);

                return op.Failed($"خطای غیرمنتظره: {ex.Message}\n\nسفارش لغو شد و به کاربر اطلاع‌رسانی خواهد شد.");
            }
        }

        public async Task<OperationResult> RejectPaymentManuallyAsync(RejectPaymentManually command, CancellationToken cancellation)
        {
            var op = new OperationResult();

            var order = await _orderRepository.GetAsync(command.OrderId, cancellation);
            if (order == null)
                return op.Failed("سفارش یافت نشد");

            var finalReason =
                $"[کد]: {command.SelectedReason}{Environment.NewLine}" +
                $"[توضیحات]: {command.ReasonDescription}{Environment.NewLine}" +
                $"[توسط]: Admin (Id: {command.AdminId.ToString().ToPersianNumber()}){Environment.NewLine}" +
                $"[شناسه کاربر]: {order.UserId.ToString().ToPersianNumber()}{Environment.NewLine}" +
                $"[شناسه سفارش]: {order.Id.ToString().ToPersianNumber()}{Environment.NewLine}" +
                $"[تاریخ]: {DateTime.Now.ToFarsiFull().ToPersianNumber()}";

            order.Cancel(finalReason);
            await _orderRepository.SaveChangesAsync(cancellation);

            return op.Succeeded("سفارش با موفقیت لغو شد.");
        }

        public async Task<List<OrderViewModel>> SearchAsync(OrderSearchModel searchModel, CancellationToken cancellation)
        {
            return await _orderRepository.Search(searchModel, cancellation);
        }

        public async Task<OperationResult> VerifyGatewayPaymentAsync(VerifyGatewayPaymentCommand command, CancellationToken cancellation)
        {
            var op = new OperationResult();

            var order = await _orderRepository.GetWithItemsAsync(command.OrderId, cancellation);
            if (order == null) return op.Failed("سفارش یافت نشد.");

            if (order.Status == OrderStatus.Paid)
                return op.Succeeded("این سفارش قبلاً با موفقیت پرداخت و تایید شده است.");

            if (order.Status != OrderStatus.WaitingForGateway)
                return op.Failed("این سفارش قبلاً پردازش شده است.");

            var (result, paidAmount, refNumber, message) = await _zibalService.VerifyPayment(long.Parse(command.TrackId));

            if (result == 100 || result == 201)
            {
                if (result == 100)
                {
                    if (Math.Abs(paidAmount - order.PayAmount) > 0.1)
                        return op.Failed("مبلغ پرداخت شده با مبلغ سفارش مطابقت ندارد.");

                    order.MarkAsPaid(refNumber.ToString());

                    try
                    {
                        var (finalizeResult, finalizeResultType) = await FinalizeOrderProcessingAsync(order, cancellation);

                        if (finalizeResult.IsSucceeded)
                        {
                            await _cartApplication.ClearCartItemsAsync($"user:{order.UserId}", cancellation);
                        }
                        else if (finalizeResultType == OperationResultType.ConcurrencyError)
                        {
                            var finalReason =
                                $"[کد]: [EXCEPTION - خطای اتمام موجودی کالا]{Environment.NewLine}" +
                                $"[توضیحات]: [CONCURRENCY - موجودی کالاها پس از پرداخت تمام شده است. مبلغ بازگشت داده خواهد شد]{Environment.NewLine}" +
                                $"[شناسه کاربر]: {order.UserId.ToString().ToPersianNumber()}{Environment.NewLine}" +
                                $"[شناسه سفارش]: {order.Id.ToString().ToPersianNumber()}{Environment.NewLine}" +
                                $"[تاریخ]: {DateTime.Now.ToFarsiFull().ToPersianNumber()}";

                            order.MarkAsNeedsRefund(finalReason);
                            await _orderRepository.SaveChangesAsync(cancellation);

                            return op.Failed("متأسفانه موجودی کالاها تمام شده است. مبلغ پرداختی طی ۲۴ ساعت بازگشت داده خواهد شد.");
                        }
                        else
                        {
                            var finalReason =
                                $"[کد]: [EXCEPTION - خطا در نهایی‌سازی]{Environment.NewLine}" +
                                $"[توضیحات]: {finalizeResult.Message} - مبلغ بازگشت داده خواهد شد{Environment.NewLine}" +
                                $"[شناسه کاربر]: {order.UserId.ToString().ToPersianNumber()}{Environment.NewLine}" +
                                $"[شناسه سفارش]: {order.Id.ToString().ToPersianNumber()}{Environment.NewLine}" +
                                $"[تاریخ]: {DateTime.Now.ToFarsiFull().ToPersianNumber()}";

                            order.MarkAsNeedsRefund(finalReason);
                            await _orderRepository.SaveChangesAsync(cancellation);

                            return op.Failed($"خطا در پردازش سفارش {order.Id.ToString().ToPersianNumber()}#: {finalizeResult.Message}. مبلغ بازگشت داده خواهد شد.");
                        }
                    }
                    catch (Exception ex)
                    {
                        var finalReason =
                            $"[کد]: [EXCEPTION - خطای غیرمنتظره]{Environment.NewLine}" +
                            $"[توضیحات]: {ex.Message} - مبلغ بازگشت داده خواهد شد{Environment.NewLine}" +
                            $"[شناسه کاربر]: {order.UserId.ToString().ToPersianNumber()}{Environment.NewLine}" +
                            $"[شناسه سفارش]: {order.Id.ToString().ToPersianNumber()}{Environment.NewLine}" +
                            $"[تاریخ]: {DateTime.Now.ToFarsiFull().ToPersianNumber()}";

                        order.MarkAsNeedsRefund(finalReason);
                        await _orderRepository.SaveChangesAsync(cancellation);

                        return op.Failed("خطای غیرمنتظره رخ داده است. مبلغ پرداختی بازگشت داده خواهد شد.");
                    }
                }

                return op.Succeeded($"پرداخت با موفقیت تایید شد. شماره پیگیری: {refNumber}");
            }

            return op.Failed($"خطا در پرداخت: {message} (کد: {result})");
        }

        public async Task<OperationResult> ChangeStatusToCancelledAndRefunded(long orderId, CancellationToken cancellation)
        {
            var op = new OperationResult();

            var order = await _orderRepository.GetAsync(orderId, cancellation);
            if (order == null) return op.Failed("سفارش یافت نشد.");

            if (order.Status != OrderStatus.NeedsRefund)
                return op.Failed("وضعیت سفارش نامناسب برای این عمل است.");

            order.RefundAndCancel();
            await _orderRepository.SaveChangesAsync(cancellation);

            return op.Succeeded("محصول در حالت لغو و عودت موفق مبلغ به کاربر قرار گرفت.");
        }

        public async Task<OperationResult> ChangeStatusToProcessing(long orderId, CancellationToken cancellation)
        {
            var op = new OperationResult();

            var order = await _orderRepository.GetAsync(orderId, cancellation);
            if (order == null) return op.Failed("سفارش یافت نشد.");

            if (order.Status != OrderStatus.Paid)
                return op.Failed("وضعیت سفارش برای پردازش نامناسب است.");

            order.ProcessOrder();
            await _orderRepository.SaveChangesAsync(cancellation);

            return op.Succeeded("محصول در حالت آماده‌سازی قرار گرفت.");
        }

        public async Task<OperationResult> ChangeStatusToShipped(ShipCommand command, CancellationToken cancellation)
        {
            var op = new OperationResult();

            if (command == null)
                return op.Failed("ورودی نمی‌تواند خالی باشد.");

            if (string.IsNullOrWhiteSpace(command.PostTrackingNo))
                return op.Failed("کد رهگیری پست نمی‌تواند خالی باشد.");

            var order = await _orderRepository.GetAsync(command.OrderId, cancellation);
            if (order == null) return op.Failed("سفارش یافت نشد.");

            if (order.Status != OrderStatus.Processing)
                return op.Failed("وضعیت سفارش برای ارسال نامناسب است.");

            order.Ship(command.PostTrackingNo);
            await _orderRepository.SaveChangesAsync(cancellation);

            return op.Succeeded("محصول در حالت ارسال شده قرار گرفت.");
        }

        public async Task<OperationResult> ChangeStatusToDelivered(long orderId, CancellationToken cancellation)
        {
            var op = new OperationResult();

            var order = await _orderRepository.GetAsync(orderId, cancellation);
            if (order == null) return op.Failed("سفارش یافت نشد.");

            if (order.Status != OrderStatus.Shipped)
                return op.Failed("وضعیت سفارش برای تایید تحویل نامناسب است.");

            order.MarkAsDeliverd();
            await _orderRepository.SaveChangesAsync(cancellation);

            return op.Succeeded("محصول در حالت تحویل داده شده قرار گرفت.");
        }

        public async Task<List<OrderItemViewModel>> GetItems(long orderId, CancellationToken cancellation)
        {
            return await _orderRepository.GetItems(orderId, cancellation);
        }

        public List<OrderStatusDto> GetStatuses()
        {
            return new List<OrderStatusDto>
            {
                new() { Id = OrderStatus.Pending, Name = OrderStatus.GetStatusTitleByCode(OrderStatus.Pending) },
                new() { Id = OrderStatus.WaitingForGateway, Name = OrderStatus.GetStatusTitleByCode(OrderStatus.WaitingForGateway) },
                new() { Id = OrderStatus.WaitingForManualVerification, Name = OrderStatus.GetStatusTitleByCode(OrderStatus.WaitingForManualVerification) },
                new() { Id = OrderStatus.Paid, Name = OrderStatus.GetStatusTitleByCode(OrderStatus.Paid) },
                new() { Id = OrderStatus.Processing, Name = OrderStatus.GetStatusTitleByCode(OrderStatus.Processing) },
                new() { Id = OrderStatus.Shipped, Name = OrderStatus.GetStatusTitleByCode(OrderStatus.Shipped) },
                new() { Id = OrderStatus.Delivered, Name = OrderStatus.GetStatusTitleByCode(OrderStatus.Delivered) },
                new() { Id = OrderStatus.NeedsRefund, Name = OrderStatus.GetStatusTitleByCode(OrderStatus.NeedsRefund) },
                new() { Id = OrderStatus.Cancelled, Name = OrderStatus.GetStatusTitleByCode(OrderStatus.Cancelled) },
                new() { Id = OrderStatus.Refunded, Name = OrderStatus.GetStatusTitleByCode(OrderStatus.Refunded) }
            };
        }

        public async Task<OrderTrackingSmsDto?> GetOrderForTrackingSms(long orderId, CancellationToken cancellation)
        {
            var order = await _orderRepository.GetAsync(orderId, cancellation);

            if (order == null)
                return null;

            return new OrderTrackingSmsDto
            {
                OrderId = orderId,
                Mobile = order.ReceiverMobile,
                PostTrackingNo = order.ShipTrackingNo ?? ""
            };
        }

        public async Task<OrderViewModel?> GetDetails(long orderId, CancellationToken cancellation)
        {
            return await _orderRepository.GetDetails(orderId, cancellation);
        }

        public async Task<bool> UserOwnsOrder(long userId, long orderId, CancellationToken cancellation)
        {
            return await _orderRepository.UserOwnsOrder(userId, orderId, cancellation);
        }

        public async Task<(CheckoutPreviewDto?, OperationResult)> GetCheckoutPreviewAsync(long userId, string? couponCode, CancellationToken cancellation)
        {
            var op = new OperationResult();

            var acc = await _accountRepository.GetAsync(userId, cancellation);
            if (acc == null) return (null, op.Failed("کاربر یافت نشد."));
            //if (!acc.IsCompleted()) return (null, op.Failed("لطفاً ابتدا پروفایل خود را تکمیل کنید."));

            var cartKey = $"user:{userId}";
            var cart = await _cartRepository.GetOrCreateByCartKeyAsync(cartKey, cancellation);
            if (cart == null || !cart.Items.Any()) return (null, op.Failed("سبد خرید شما خالی است."));

            var discountModel = new DiscountCalculationModel { UserId = userId, CouponCode = couponCode };

            var productIds = cart.Items.Select(x => x.ProductId).Distinct().ToList();

            var inventoryDict = (await _inventoryRepository.GetInvListByIdsAsync(productIds, cancellation))
                .ToDictionary(x => x.ProductId);

            var productInfoDict = (await _productRepository.GetProductDetailsByIdsAsync(productIds, cancellation))
                .ToDictionary(x => x.ProductId);

            foreach (var item in cart.Items)
            {
                if (!inventoryDict.TryGetValue(item.ProductId, out var inv) || inv.CalculateCurrentCount() < item.Quantity)
                    return (null, op.Failed($"موجودی محصول {item.ProductId} کافی نیست."));

                if (!productInfoDict.TryGetValue(item.ProductId, out var product))
                    return (null, op.Failed($"محصول {item.ProductId} یافت نشد!"));

                discountModel.Items.Add(new CartItemForDiscount
                {
                    ProductId = item.ProductId,
                    UnitPrice = inv.UnitPrice,
                    Quantity = item.Quantity,
                    CategoryId = product.CategoryId,
                    Brand = product.Brand
                });
            }

            var discountResult = await _discountApplication.CalculateDiscountsAsync(discountModel, cancellation);

            double shippingCost = 100000;

            double finalPayAmount = discountResult.PayAmount + shippingCost;

            if (string.IsNullOrWhiteSpace(couponCode))
            {
                return (new CheckoutPreviewDto
                {
                    IsCouponHasValue = false,
                    IsCouponApplied = false,
                    DiscountMessage = null,
                    TotalAmount = discountResult.TotalAmount,
                    TotalDiscountAmount = discountResult.TotalDiscountAmount,
                    ShippingCost = shippingCost,
                    FinalPayAmount = finalPayAmount
                }, op.Succeeded());
            }

            return (new CheckoutPreviewDto
            {
                IsCouponHasValue = true,
                IsCouponApplied = discountResult.AppliedCouponId.HasValue && discountResult.AppliedCouponId > 0,
                DiscountMessage = discountResult.DiscountMessage ?? null,
                TotalAmount = discountResult.TotalAmount,
                TotalDiscountAmount = discountResult.TotalDiscountAmount,
                ShippingCost = shippingCost,
                FinalPayAmount = finalPayAmount
            }, op.Succeeded());
        }

        public async Task<(CreateOrderResult?, OperationResult)> CreateOrderAsync(CreateOrderCommand command, CancellationToken cancellation)
        {
            var op = new OperationResult();

            var acc = await _accountRepository.GetAsync(command.UserId, cancellation);
            if (acc == null) return (null, op.Failed("کاربر یافت نشد."));
            //if (!acc.IsCompleted()) return (null, op.Failed("لطفاً ابتدا پروفایل خود را تکمیل کنید."));

            var shippingInfo = await _shippingInfoRepository.GetAsync(command.ShippingInfoId, cancellation);
            if (shippingInfo == null || shippingInfo.UserId != command.UserId)
                return (null, op.Failed("آدرس انتخاب شده نامعتبر است."));

            var cartKey = $"user:{command.UserId}";
            var cart = await _cartRepository.GetOrCreateByCartKeyAsync(cartKey, cancellation);
            if (cart == null || !cart.Items.Any())
                return (null, op.Failed("سبد خرید شما خالی است."));

            var productIds = cart.Items.Select(x => x.ProductId).Distinct().ToList();
            var inventoryDict = (await _inventoryRepository.GetInvListByIdsAsync(productIds, cancellation)).ToDictionary(x => x.ProductId);
            var productDict = (await _productRepository.GetProductsWithCategoryByIdsAsync(productIds, cancellation)).ToDictionary(x => x.Id);

            var discountModel = new DiscountCalculationModel { UserId = command.UserId, CouponCode = command.CouponCode };
            var enrichedItems = new List<(CartItem Item, Inventory Inventory, Product Product)>();

            foreach (var item in cart.Items)
            {
                if (!inventoryDict.TryGetValue(item.ProductId, out var inv) || inv.CalculateCurrentCount() < item.Quantity)
                    return (null, op.Failed($"موجودی محصول {item.ProductId} کافی نیست."));

                if (!productDict.TryGetValue(item.ProductId, out var product))
                    return (null, op.Failed($"محصول {item.ProductId} یافت نشد!"));

                enrichedItems.Add((item, inv, product));
                discountModel.Items.Add(new CartItemForDiscount
                {
                    ProductId = item.ProductId,
                    UnitPrice = inv.UnitPrice,
                    Quantity = item.Quantity,
                    CategoryId = product.CategoryId,
                    Brand = product.Brand
                });
            }

            var discountResult = await _discountApplication.CalculateDiscountsAsync(discountModel, cancellation);

            var orderItems = new List<OrderItem>();
            foreach (var data in enrichedItems)
            {
                var discInfo = discountResult.Items.First(x => x.ProductId == data.Item.ProductId);

                var orderItem = new OrderItem(
                    data.Item.ProductId,
                    data.Item.Quantity,
                    data.Inventory.UnitPrice,
                    discInfo.DiscountRate,
                    data.Product.Name,
                    discInfo.DiscountedQty,
                    discInfo.DiscountId,
                    data.Product.Picture);

                orderItems.Add(orderItem);
            }

            var order = new Order(
                command.UserId,
                shippingInfo.ReceiverFullName,
                shippingInfo.ReceiverMobile,
                shippingInfo.ReceiverAddress,
                shippingInfo.ReceiverZipCode,
                command.PayMethod,
                orderItems);

            if (discountResult.AppliedCouponId.HasValue && discountResult.AppliedCouponId > 0 && !string.IsNullOrEmpty(command.CouponCode))
            {
                order.ApplyOrderCoupon(discountResult.AppliedCouponId.Value, command.CouponCode, discountResult.OrderCouponAmount);
            }

            double shippingCost = 100000;
            order.SetShippingCost(shippingCost);

            var result = new CreateOrderResult();

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                _orderRepository.Create(order);

                if (command.PayMethod == PaymentMethod.Gateway)
                {
                    await _orderRepository.SaveChangesAsync(cancellation);
                    result.OrderId = order.Id;

                    var (trackId, message) = await _zibalService.RequestPayment(order.PayAmount, order.Id.ToString());

                    if (trackId > 0)
                    {
                        order.StartGatewayPayment(trackId.ToString());
                        result.ShouldRedirect = true;
                        result.RedirectUrl = $"{_config["ZibalSettings:PaymentPageUrl"]}{trackId}";
                    }
                    else
                    {
                        return (null, op.Failed($"خطا در اتصال به درگاه: {message}"));
                    }
                }
                else if (command.PayMethod == PaymentMethod.CardToCard)
                {
                    if (string.IsNullOrWhiteSpace(command.CardOwnerName) || string.IsNullOrWhiteSpace(command.NationalCode) || command.PaymentReceiptPic == null)
                    {
                        return (null, op.Failed("اطلاعات کارت به کارت ناقص است."));
                    }

                    var folderName = "Orders";

                    var uploadedReceiptName = await fileUploader.UploadAsync(command.PaymentReceiptPic, folderName, cancellation);

                    order.AddCardPaymentInfo(command.CardOwnerName, command.NationalCode, uploadedReceiptName);
                    order.StartManualPayment();

                    await _cartApplication.ClearCartItemsAsync(cartKey, cancellation);

                    result.ShouldRedirect = false;
                }
                else
                {
                    return (null, op.Failed("روش پرداخت معتبر نیست."));
                }

                await _orderRepository.SaveChangesAsync(cancellation);

                scope.Complete();
                result.OrderId = order.Id;
            }

            var successMessage = command.PayMethod == PaymentMethod.Gateway
                ? "در حال انتقال به درگاه..."
                : "سفارش با موفقیت ثبت شد و در انتظار تایید است";

            return (result, op.Succeeded(successMessage));
        }

        private async Task<(OperationResult, OperationResultType)> FinalizeOrderProcessingAsync(Order order, CancellationToken cancellation)
        {
            var op = new OperationResult();

            using var scope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.ReadCommitted,
                    Timeout = TimeSpan.FromMinutes(2)
                },
                TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                var productIds = order.Items.Select(x => x.ProductId).ToList();
                var discountIds = order.Items.Where(x => x.DiscountId > 0)
                                       .Select(x => x.DiscountId)
                                       .Union(order.AppliedCouponId.HasValue ?
                                             new[] { order.AppliedCouponId.Value } :
                                             Array.Empty<long>())
                                       .ToList();

                var inventories = await _inventoryRepository.GetByProductIds(productIds, cancellation);

                var discounts = discountIds.Any() ?
                    await _discountRepository.GetByIds(discountIds, cancellation) :
                    new List<Discount>();

                foreach (var orderItem in order.Items)
                {
                    var inventory = inventories.FirstOrDefault(x => x.ProductId == orderItem.ProductId);
                    if (inventory == null || inventory.CalculateCurrentCount() < orderItem.Quantity)
                    {
                        throw new InsufficientInventoryException(
                            $"موجودی کافی برای محصول {orderItem.ProductName.ToPersianNumber()} وجود ندارد. " +
                            $"درخواستی: {orderItem.Quantity.ToString().ToPersianNumber()}, " +
                            $"موجود: {(inventory?.CalculateCurrentCount() ?? 0).ToString().ToPersianNumber()}");
                    }
                }

                foreach (var discount in discounts)
                {
                    if (!discount.IsUsable(order.UserId, DateTime.Now))
                    {
                        throw new DiscountCapacityExceededException(
                            $"ظرفیت کد تخفیف '{discount.Title}' به پایان رسیده است و یا منقضی شده است.");
                    }
                }

                foreach (var orderItem in order.Items)
                {
                    var reduceInv = new ReduceInventoryByOrder
                    {
                        Count = orderItem.Quantity,
                        OrderId = order.Id,
                        Description = $"تایید نهایی سفارش {order.Id.ToString().ToPersianNumber()}# - کاربر {order.UserId.ToString().ToPersianNumber()}#",
                        InventoryId = inventories.First(x => x.ProductId == orderItem.ProductId).Id,
                        ProductId = orderItem.ProductId,
                        UserId = order.UserId
                    };

                    await _inventoryApplication.ReduceByOrder(reduceInv, cancellation);
                }

                foreach (var orderItem in order.Items.Where(x => x.DiscountId > 0))
                {
                    await _discountApplication.IncreaseUsage(orderItem.DiscountId, order.UserId, cancellation);
                }

                if (order.AppliedCouponId.HasValue)
                {
                    await _discountApplication.IncreaseUsage(order.AppliedCouponId.Value, order.UserId, cancellation);
                }

                order.ProcessOrder();
                await _orderRepository.SaveChangesAsync(cancellation);

                scope.Complete();

                return (op.Succeeded(), OperationResultType.Succeeded);
            }
            catch (InsufficientInventoryException ex)
            {
                return (op.Failed(ex.Message), OperationResultType.Failed);
            }
            catch (DiscountCapacityExceededException ex)
            {
                return (op.Failed(ex.Message), OperationResultType.Failed);
            }
            catch (DbUpdateConcurrencyException)
            {
                return (op.Failed("خطا در به‌روزرسانی! لطفاً مجدداً تلاش کنید."),
                    OperationResultType.ConcurrencyError);
            }
            catch (Exception ex)
            {
                return (op.Failed($"خطای غیرمنتظره: {ex.Message}"), OperationResultType.Failed);
            }
        }

        public class InsufficientInventoryException : Exception
        {
            public InsufficientInventoryException(string message) : base(message) { }
        }

        public class DiscountCapacityExceededException : Exception
        {
            public DiscountCapacityExceededException(string message) : base(message) { }
        }

        public enum OperationResultType
        {
            Succeeded,
            Failed,
            ConcurrencyError,
            NeedsRefund
        }

        public async Task<OperationResult> RequestReturnAsync(RequestReturnCommand command, CancellationToken cancellation)
        {
            var op = new OperationResult();
            var order = await _orderRepository.GetAsync(command.OrderId, cancellation);

            if (order == null)
                return op.Failed("سفارش یافت نشد.");

            if (order.UserId != command.UserId)
                return op.Failed("کاربر یافت نشد.");

            if ((order.Status != OrderStatus.Delivered) || (DateTime.Now - order.LastModifiedDate).TotalDays > 7)
                return op.Failed("این سفارش در وضعیتی نیست که بتوان آن را مرجوع کرد.");

            var returnReason =
                $"[کد]: USER_RETURN_REQUEST{Environment.NewLine}" +
                $"[توضیحات کاربر]: {command.Reason}{Environment.NewLine}" +
                $"[شناسه کاربر]: {order.UserId.ToString().ToPersianNumber()}{Environment.NewLine}" +
                $"[تاریخ درخواست]: {DateTime.Now.ToFarsiFull().ToPersianNumber()}";

            order.RequestReturn(returnReason);
            await _orderRepository.SaveChangesAsync(cancellation);

            return op.Succeeded("درخواست مرجوعی شما با موفقیت ثبت شد. نتیجه از طریق پنل اطلاع‌رسانی خواهد شد.");
        }

        public async Task<OperationResult> RejectReturnAsync(RejectReturnCommand command, CancellationToken cancellation)
        {
            var op = new OperationResult();
            var order = await _orderRepository.GetAsync(command.OrderId, cancellation);

            if (order == null) return op.Failed("سفارش یافت نشد.");

            if (order.Status != OrderStatus.ReturnRequested)
                return op.Failed("این سفارش در وضعیت درخواست مرجوعی قرار ندارد.");

            var finalReason =
                $"{order.CancellationReason}{Environment.NewLine}" +
                $"[عملیات]: رد مرجوعی{Environment.NewLine}" +
                $"[دلیل رد]: {command.RejectionReason}{Environment.NewLine}" +
                $"[توسط]: Admin (Id: {command.AdminId.ToString().ToPersianNumber()}){Environment.NewLine}" +
                $"[تاریخ رد]: {DateTime.Now.ToFarsiFull().ToPersianNumber()}";

            order.RejectReturn(finalReason);
            await _orderRepository.SaveChangesAsync(cancellation);

            return op.Succeeded("درخواست مرجوعی کاربر رد شد و سفارش به وضعیت 'تحویل داده شده' بازگشت.");
        }

        public async Task<OperationResult> ApproveReturnAsync(ApproveReturnCommand command, CancellationToken cancellation)
        {
            var op = new OperationResult();
            var order = await _orderRepository.GetAsync(command.OrderId, cancellation);

            if (order == null) return op.Failed("سفارش یافت نشد.");

            if (order.Status != OrderStatus.ReturnRequested)
                return op.Failed("این سفارش در وضعیت درخواست مرجوعی قرار ندارد.");

            var finalReason =
                $"{order.CancellationReason}{Environment.NewLine}" +
                $"[عملیات]: تایید مرجوعی{Environment.NewLine}" +
                $"[توضیحات ادمین]: {command.Notes}{Environment.NewLine}" +
                $"[توسط]: Admin (Id: {command.AdminId.ToString().ToPersianNumber()}){Environment.NewLine}" +
                $"[تاریخ تایید]: {DateTime.Now.ToFarsiFull().ToPersianNumber()}";

            order.MarkAsNeedsRefund(finalReason);

            await _orderRepository.SaveChangesAsync(cancellation);
            return op.Succeeded("درخواست مرجوعی با موفقیت تایید و سفارش به وضعیت مرجوع‌شده تغییر یافت.");
        }
    }
}
