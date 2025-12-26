using _0_Framework.Application;
using AccountManagement.Domain.AccountAgg;
using DiscountManagement.Domain.CustomerDiscountAgg;
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
        private readonly ICustomerDiscountRepository _discountRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IFileUploader fileUploader;
        private readonly IZibalService _zibalService;
        private readonly IConfiguration _config;
        private readonly IInventoryApplication _inventoryApplication;

        public OrderApplication(IAccountRepository accountRepository,
            ICartRepository cartRepository,
            IInventoryRepository inventoryRepository,
            ICustomerDiscountRepository discountRepository,
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            ICartApplication cartApplication,
            IFileUploader fileUploader,
            IZibalService zibalService,
            IConfiguration config,
            IInventoryApplication inventoryApplication)
        {
            _accountRepository = accountRepository;
            _cartRepository = cartRepository;
            _inventoryRepository = inventoryRepository;
            _discountRepository = discountRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _cartApplication = cartApplication;
            this.fileUploader = fileUploader;
            _zibalService = zibalService;
            _config = config;
            _inventoryApplication = inventoryApplication;
        }

        public async Task<OperationResult> ConfirmPaymentManuallyAsync(ConfirmPaymentManually command)
        {
            var op = new OperationResult();

            var order = await _orderRepository.GetWithItemsAsync(command.OrderId);
            if (order == null) return op.Failed("سفارش یافت نشد.");

            if (order.Status != OrderStatus.WaitingForManualVerification)
                return op.Failed("وضعیت سفارش برای تایید کارت به کارت نامعتبر است.");

            foreach (var orderItem in order.Items)
            {
                var invId = (await _inventoryRepository.GetBy(orderItem.ProductId)).Id;
                var reduceInv = new ReduceInventoryByOrder
                {
                    Count = orderItem.Quantity,
                    OrderId = order.Id,
                    Description = $"خرید توسط کاربر {order.AccountId} با شماره سفارش {order.Id} در تاریخ {order.CreationDate.ToFarsiFull().ToPersianNumber()}",
                    InventoryId = invId,
                    ProductId = orderItem.ProductId,
                    AccountId = order.AccountId
                };
                await _inventoryApplication.ReduceByOrder(reduceInv);

                if (orderItem.DiscountRate > 0)
                {
                    var disc = await _discountRepository.GetByProductId(orderItem.ProductId);
                    disc?.ReduceQty(orderItem.DiscountedQty);
                    await _discountRepository.SaveChangesAsync();
                }
            }

            order.ConfirmManually();
            await _orderRepository.SaveChangesAsync();

            // می‌توان Domain Event صادر کرد برای ارسال ایمیل و SMS
            return op.Succeeded("پرداخت کارت به کارت با موفقیت تایید شد.");
        }

        public async Task<OperationResult> RejectPaymentManuallyAsync(RejectPaymentManually command)
        {
            var op = new OperationResult();

            var order = await _orderRepository.GetAsync(command.OrderId);
            if (order == null)
                return op.Failed("سفارش یافت نشد");

            var finalReason =
                $"[ReasonCode]: {command.SelectedReason}{Environment.NewLine}" +
                $"[Description]: {command.ReasonDescription}{Environment.NewLine}" +
                $"[ActionBy]: Admin (Id: {command.AdminId}){Environment.NewLine}" +
                $"[AccountId]: {order.AccountId}{Environment.NewLine}" +
                $"[OrderId]: {order.Id}{Environment.NewLine}" +
                $"[ActionDate]: {DateTime.Now.ToFarsiFull()}";

            order.Cancel(finalReason);
            await _orderRepository.SaveChangesAsync();

            return op.Succeeded("سفارش با موفقیت لغو شد.");
        }

        public async Task<(PlaceOrderResult?, OperationResult)> PlaceOrderAsync(CreateOrderCommand command)
        {
            var op = new OperationResult();

            if (command == null)
                return (null, op.Failed("سبد خرید خالی است."));

            var acc = await _accountRepository.GetAsync(command.AccountId);
            if (acc == null)
                return (null, op.Failed($"کاربر با آیدی {command.AccountId} یافت نشد."));

            var cartKey = $"user:{command.AccountId}";
            var cart = await _cartRepository.GetByCartKeyAsync(cartKey);
            if (cart == null || !cart.Items.Any())
                return (null, op.Failed("سبد خریدی برای این کاربر یافت نشد."));

            var orderItems = new List<OrderItem>();
            foreach (var cartItem in cart.Items)
            {
                var inv = await _inventoryRepository.GetBy(cartItem.ProductId);
                if (inv == null)
                    return (null, op.Failed("این محصول در انبار وجود ندارد."));

                var disc = await _discountRepository.GetByProductId(cartItem.ProductId);
                var discRate = disc != null ? disc.DiscountRate : 0;
                var discQty = disc != null ? disc.ProductCount : 0;

                var productName = await _productRepository.GetProductName(cartItem.ProductId) ?? "";
                var productPic = await _productRepository.GetProductPic(cartItem.ProductId) ?? "";

                var orderItem = new OrderItem(cartItem.ProductId,
                    cartItem.Quantity, inv.UnitPrice, discRate,
                    productName, discQty, productPic);

                orderItems.Add(orderItem);
            }

            if (string.IsNullOrWhiteSpace(command.RecieverFullName))
                command.RecieverFullName = acc.Fullname;

            if (string.IsNullOrWhiteSpace(command.RecieverMobile))
                command.RecieverMobile = acc.Mobile;

            var order = new Order(command.AccountId, command.RecieverFullName,
                    command.RecieverMobile, command.RecieverAddress,
                    command.RecieverZipCode, command.PayMethod, orderItems);

            _orderRepository.Create(order);
            await _orderRepository.SaveChangesAsync();

            if (command.PayMethod == PaymentMethod.CardToCard)
            {
                if (string.IsNullOrWhiteSpace(command.CardOwnerName) || string.IsNullOrWhiteSpace(command.NationalCode) || command.PaymentReceiptPic == null)
                {
                    return (null, op.Failed("لطفاً اطلاعات کارت به کارت (نام کامل صاحب کارت، کد ملی صاحب کارت و تصویر رسید) را وارد کنید."));
                }

                var receiptPath = $"orders/order_{order.Id}";
                var receiptPic = fileUploader.Upload(command.PaymentReceiptPic, receiptPath);

                order.AddCardPaymentInfo(command.CardOwnerName, command.NationalCode, receiptPic);
                order.StartManualPayment();

                await _cartApplication.ClearCartItemsAsync(cartKey);
                await _orderRepository.SaveChangesAsync();

                return (new PlaceOrderResult { OrderId = order.Id, ShouldRedirect = false, RedirectUrl = null },
                op.Succeeded("سفارش با موفقیت ثبت شد و در انتظار تایید است"));
            }
            else if (command.PayMethod == PaymentMethod.Gateway)
            {
                var (trackId, message) = await _zibalService.RequestPayment(order.PayAmount, order.Id.ToString());

                if (trackId > 0)
                {
                    order.StartGatewayPayment(trackId.ToString());
                    await _orderRepository.SaveChangesAsync();

                    var paymentUrl = $"{_config["ZibalSettings:PaymentPageUrl"]}{trackId}";

                    return (new PlaceOrderResult
                    {
                        OrderId = order.Id,
                        ShouldRedirect = true,
                        RedirectUrl = paymentUrl
                    }, op.Succeeded("در حال انتقال به درگاه پرداخت..."));
                }
                return (null, op.Failed($"خطا در اتصال به درگاه: {message}"));
            }

            return (null, op.Failed("روش پرداخت معتبر نیست."));
        }

        public async Task<List<OrderViewModel>> SearchAsync(OrderSearchModel searchModel)
        {
            return await _orderRepository.Search(searchModel);
        }

        public async Task<OperationResult> VerifyGatewayPaymentAsync(VerifyGatewayPaymentCommand command)
        {
            var op = new OperationResult();

            var order = await _orderRepository.GetWithItemsAsync(command.OrderId);
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

                    foreach (var orderItem in order.Items)
                    {
                        var invId = (await _inventoryRepository.GetBy(orderItem.ProductId)).Id;
                        var reduceInv = new ReduceInventoryByOrder
                        {
                            Count = orderItem.Quantity,
                            OrderId = order.Id,
                            Description = $"خرید توسط کاربر {order.AccountId} با شماره سفارش {order.Id} در تاریخ {order.CreationDate.ToFarsiFull().ToPersianNumber()}",
                            InventoryId = invId,
                            ProductId = orderItem.ProductId,
                            AccountId = order.AccountId
                        };
                        await _inventoryApplication.ReduceByOrder(reduceInv);

                        if (orderItem.DiscountRate > 0)
                        {
                            var disc = await _discountRepository.GetByProductId(orderItem.ProductId);
                            disc?.ReduceQty(orderItem.DiscountedQty);
                            await _discountRepository.SaveChangesAsync();
                        }
                    }

                    await _cartApplication.ClearCartItemsAsync($"user:{order.AccountId}");
                    await _orderRepository.SaveChangesAsync();

                }

                return op.Succeeded($"پرداخت با موفقیت تایید شد. شماره پیگیری: {refNumber}");
            }

            return op.Failed($"خطا در پرداخت: {message} (کد: {result})");
        }

        public async Task<OperationResult> ChangeStatusToProcessing(long orderId)
        {
            var op = new OperationResult();

            var order = await _orderRepository.GetAsync(orderId);
            if (order == null) return op.Failed("سفارش یافت نشد.");

            if (order.Status != OrderStatus.Paid)
                return op.Failed("وضعیت سفارش برای پردازش نامناسب است.");

            order.ProcessOrder();
            await _orderRepository.SaveChangesAsync();

            // می‌توان Domain Event صادر کرد برای ارسال ایمیل و SMS
            return op.Succeeded("محصول در حالت آماده‌سازی قرار گرفت.");
        }

        public async Task<OperationResult> ChangeStatusToShipped(ShipCommand command)
        {
            var op = new OperationResult();

            if (command == null)
                return op.Failed("ورودی نمی‌تواند خالی باشد.");

            if (string.IsNullOrWhiteSpace(command.PostTrackingNo))
                return op.Failed("کد رهگیری پست نمی‌تواند خالی باشد.");

            var order = await _orderRepository.GetAsync(command.OrderId);
            if (order == null) return op.Failed("سفارش یافت نشد.");

            if (order.Status != OrderStatus.Processing)
                return op.Failed("وضعیت سفارش برای ارسال نامناسب است.");

            order.Ship(command.PostTrackingNo);
            await _orderRepository.SaveChangesAsync();

            return op.Succeeded("محصول در حالت ارسال شده قرار گرفت.");
        }

        public async Task<OperationResult> ChangeStatusToDelivered(long orderId)
        {
            var op = new OperationResult();

            var order = await _orderRepository.GetAsync(orderId);
            if (order == null) return op.Failed("سفارش یافت نشد.");

            if (order.Status != OrderStatus.Shipped)
                return op.Failed("وضعیت سفارش برای تایید تحویل نامناسب است.");

            order.MarkAsDeliverd();
            await _orderRepository.SaveChangesAsync();

            // می‌توان Domain Event صادر کرد برای ارسال ایمیل و SMS
            return op.Succeeded("محصول در حالت تحویل داده شده قرار گرفت.");
        }

        public async Task<List<OrderItemViewModel>> GetItems(long orderId)
        {
            return await _orderRepository.GetItems(orderId);
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

        public async Task<OrderTrackingSmsDto> GetOrderForTrackingSms(long orderId)
        {
            var order = await _orderRepository.GetAsync(orderId);

            if (order == null)
                return null;

            return new OrderTrackingSmsDto
            {
                OrderId = orderId,
                Mobile = order.RecieverMobile,
                PostTrackingNo = order.ShipTrackingNo ?? ""
            };
        }

        public async Task<OrderViewModel?> GetDetails(long orderId)
        {
            return await _orderRepository.GetDetails(orderId);
        }

        public async Task<bool> UserOwnsOrder(string userId, long orderId)
        {
            return await _orderRepository.UserOwnsOrder(userId, orderId);
        }
    }
}
