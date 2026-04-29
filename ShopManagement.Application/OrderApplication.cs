using _0_Framework.Application;
using AccountManagement.Domain.AccountAgg;
using DiscountManagement.Application.Contracts.Discount;
using InventoryManagement.Application.Contracts.Inventory;
using InventoryManagement.Domain.InventoryAgg;
using Microsoft.Extensions.Configuration;
using ShopManagement.Application.Contracts.Cart;
using ShopManagement.Application.Contracts.Order;
using ShopManagement.Domain.CartAgg;
using ShopManagement.Domain.OrderAgg;
using ShopManagement.Domain.ProductAgg;

namespace ShopManagement.Application
{

    public class OrderApplication : IOrderApplication
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ICartRepository _cartRepository;
        private readonly ICartApplication _cartApplication;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
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
            IDiscountApplication discountApplication)
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
        }

        public async Task<OperationResult> ConfirmPaymentManuallyAsync(ConfirmPaymentManually command, CancellationToken cancellation)
        {
            var op = new OperationResult();

            var order = await _orderRepository.GetWithItemsAsync(command.OrderId, cancellation);
            if (order == null) return op.Failed("سفارش یافت نشد.");

            if (order.Status != OrderStatus.WaitingForManualVerification)
                return op.Failed("وضعیت سفارش برای تایید کارت به کارت نامعتبر است.");

            await ReduceInventory(order, cancellation);

            order.ConfirmManually();
            await _orderRepository.SaveChangesAsync(cancellation);

            return op.Succeeded("پرداخت کارت به کارت با موفقیت تایید شد.");
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
                $"[توسط]: Admin (Id: {command.AdminId}){Environment.NewLine}" +
                $"[شناسه کاربر]: {order.UserId.ToString()}{Environment.NewLine}" +
                $"[شناسه سفارش]: {order.Id.ToString()}{Environment.NewLine}" +
                $"[تاریخ]: {DateTime.Now.ToFarsiFull()}";

            order.Cancel(finalReason);
            await _orderRepository.SaveChangesAsync(cancellation);

            return op.Succeeded("سفارش با موفقیت لغو شد.");
        }

        public async Task<(PlaceOrderResult?, OperationResult)> PlaceOrderAsync(CreateOrderCommand command, CancellationToken cancellation)
        {
            var op = new OperationResult();
            if (command == null) return (null, op.Failed("سبد خرید خالی است."));

            var acc = await _accountRepository.GetAsync(command.UserId, cancellation);
            if (acc == null) return (null, op.Failed("کاربر یافت نشد."));

            if (!acc.IsCompleted()) return (null, op.Failed("لطفاً ابتدا پروفایل خود را تکمیل کنید."));

            if (string.IsNullOrWhiteSpace(command.ReceiverFullName))
            {
                if (!string.IsNullOrWhiteSpace(acc.Fullname))
                    command.ReceiverFullName = acc.Fullname;
                else
                    return (null, op.Failed("نام و نام خانوادگی گیرنده نمی‌تواند خالی باشد."));
            }

            if (string.IsNullOrWhiteSpace(command.ReceiverMobile))
                command.ReceiverMobile = acc.Mobile;

            command.ReceiverMobile = command.ReceiverMobile;

            var cartKey = $"user:{command.UserId}";
            var cart = await _cartRepository.GetOrCreateByCartKeyAsync(cartKey, cancellation);
            if (cart == null || !cart.Items.Any()) return (null, op.Failed("سبد خرید شما خالی است."));

            var discountModel = new DiscountCalculationModel { UserId = command.UserId, CouponCode = command.CouponCode };
            var enrichedItems = new List<(CartItem Item, Inventory Inventory, Product Product)>();

            foreach (var item in cart.Items)
            {
                var inv = await _inventoryRepository.GetBy(item.ProductId, cancellation);
                if (inv == null || inv.CalculateCurrentCount() < item.Quantity)
                    return (null, op.Failed($"موجودی محصول {item.ProductId} کافی نیست."));

                var product = await _productRepository.GetProductWithCategory(item.ProductId, cancellation);
                if (product == null) return (null, op.Failed($"محصول {item.ProductId} یافت نشد!"));
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

                var orderItem = new OrderItem(data.Item.ProductId, data.Item.Quantity,
                    data.Inventory.UnitPrice, discInfo.DiscountRate,
                    data.Product.Name, discInfo.DiscountedQty, discInfo.DiscountId, data.Product.Picture);

                orderItems.Add(orderItem);
            }

            var order = new Order(command.UserId, command.ReceiverFullName, command.ReceiverMobile,
                command.ReceiverAddress, command.ReceiverZipCode, command.PayMethod, orderItems);

            if (discountResult.AppliedCouponId > 0 && !string.IsNullOrEmpty(discountResult.CouponCode))
                order.ApplyOrderCoupon(discountResult.AppliedCouponId.Value, discountResult.CouponCode, discountResult.OrderCouponAmount);

            _orderRepository.Create(order);
            await _orderRepository.SaveChangesAsync(cancellation);

            var result = new PlaceOrderResult
            {
                OrderId = order.Id,
                DiscountMessage = discountResult.DiscountMessage
            };

            if (command.PayMethod == PaymentMethod.CardToCard)
            {
                if (string.IsNullOrWhiteSpace(command.CardOwnerName) || string.IsNullOrWhiteSpace(command.NationalCode) || command.PaymentReceiptPic == null)
                {
                    return (null, op.Failed("لطفاً اطلاعات کارت به کارت (نام کامل صاحب کارت، کد ملی صاحب کارت و تصویر رسید) را وارد کنید."));
                }

                var receiptPath = $"Orders/order_{order.Id}";
                var receiptPic = await fileUploader.UploadAsync(command.PaymentReceiptPic, receiptPath, cancellation);

                order.AddCardPaymentInfo(command.CardOwnerName, command.NationalCode, receiptPic);
                order.StartManualPayment();

                await _cartApplication.ClearCartItemsAsync(cartKey, cancellation);
                await _orderRepository.SaveChangesAsync(cancellation);

                result.ShouldRedirect = false;
                result.RedirectUrl = null;

                return (result,
                op.Succeeded("سفارش با موفقیت ثبت شد و در انتظار تایید است"));
                //op.Succeeded("سفارش با موفقیت ثبت شد و در انتظار تایید است " + discountResult.DiscountMessage));
            }
            else if (command.PayMethod == PaymentMethod.Gateway)
            {
                var (trackId, message) = await _zibalService.RequestPayment(order.PayAmount, order.Id.ToString());

                if (trackId > 0)
                {
                    order.StartGatewayPayment(trackId.ToString());
                    await _orderRepository.SaveChangesAsync(cancellation);

                    var paymentUrl = $"{_config["ZibalSettings:PaymentPageUrl"]}{trackId}";

                    result.ShouldRedirect = true;
                    result.RedirectUrl = paymentUrl;

                    return (result,
                        op.Succeeded("در حال انتقال به درگاه..."));
                }
                return (null, op.Failed($"خطا در اتصال به درگاه: {message}"));
            }

            return (null, op.Failed("روش پرداخت معتبر نیست."));
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

                    await ReduceInventory(order, cancellation);

                    await _cartApplication.ClearCartItemsAsync($"user:{order.UserId}", cancellation);
                    await _orderRepository.SaveChangesAsync(cancellation);

                }

                return op.Succeeded($"پرداخت با موفقیت تایید شد. شماره پیگیری: {refNumber}");
            }

            return op.Failed($"خطا در پرداخت: {message} (کد: {result})");
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

            // می‌توان Domain Event صادر کرد برای ارسال ایمیل و SMS
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

            // می‌توان Domain Event صادر کرد برای ارسال ایمیل و SMS
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
                new() { Id = OrderStatus.Cancelled, Name = OrderStatus.GetStatusTitleByCode(OrderStatus.Cancelled) }
            };
        }

        public async Task<OrderTrackingSmsDto> GetOrderForTrackingSms(long orderId, CancellationToken cancellation)
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

        private async Task ReduceInventory(Order order, CancellationToken cancellation)
        {
            var productIds = order.Items.Select(x => x.ProductId).ToList();
            var inventories = await _inventoryRepository.GetByProductIds(productIds, cancellation);

            foreach (var orderItem in order.Items)
            {
                var inventory = inventories.FirstOrDefault(x => x.ProductId == orderItem.ProductId);

                var reduceInv = new ReduceInventoryByOrder
                {
                    Count = orderItem.Quantity,
                    OrderId = order.Id,
                    Description = $"تایید نهایی سفارش #{order.Id.ToString()} - کاربر #{order.UserId.ToString()}",
                    InventoryId = inventory?.Id ?? 0,
                    ProductId = orderItem.ProductId,
                    UserId = order.UserId
                };

                await _inventoryApplication.ReduceByOrder(reduceInv, cancellation);

                if (orderItem.DiscountId > 0)
                {
                    await _discountApplication.IncreaseUsage(orderItem.DiscountId, order.UserId, cancellation);
                }
            }

            if (order.AppliedCouponId.HasValue)
            {
                await _discountApplication.IncreaseUsage(order.AppliedCouponId.Value, order.UserId, cancellation);
            }
        }

        public async Task<OrderViewModel?> GetDetails(long orderId, CancellationToken cancellation)
        {
            return await _orderRepository.GetDetails(orderId, cancellation);
        }

        public async Task<bool> UserOwnsOrder(long userId, long orderId, CancellationToken cancellation)
        {
            return await _orderRepository.UserOwnsOrder(userId, orderId, cancellation);
        }
    }
}
