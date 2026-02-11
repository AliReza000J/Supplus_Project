using _0_Framework.Application;
using AccountManagement.Domain.AccountAgg;
using BlogManagement.Domain.ArticleAgg;
using CommentManagement.Application.Contracts.Comment;
using CommentManagement.Domain.CommentAgg;
using ShopManagement.Domain.OrderAgg;
using ShopManagement.Domain.ProductAgg;

namespace CommentManagement.Application
{
    public class CommentApplication : ICommentApplication
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IProductRepository _productRepository;
        private readonly IArticleRepository _blogRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IAccountRepository _accountRepository;

        public CommentApplication(ICommentRepository commentRepository,
            IProductRepository productRepository, IArticleRepository blogRepository,
            IOrderRepository orderRepository, IAccountRepository accountRepository)
        {
            _commentRepository = commentRepository;
            _productRepository = productRepository;
            _blogRepository = blogRepository;
            _orderRepository = orderRepository;
            _accountRepository = accountRepository;
        }

        public async Task<OperationResult> AddAsync(AddComment command, long userId, string username, CancellationToken cancellation)
        {
            var op = new OperationResult();

            if (string.IsNullOrWhiteSpace(username))
                return op.Failed("نام کاربر الزامی است.");

            if (command.Rate < 1 || command.Rate > 5)
                return op.Failed("امتیاز باید بین ۱ تا ۵ باشد.");

            var acc = await _accountRepository.GetAsync(userId, cancellation);
            if (acc == null)
                return op.Failed("کاربر یافت نشد!");
            if (!acc.IsCompleted())
                op.Failed("برای ثبت نظر باید پروفایل خود را تکمیل نمایید.");

            if (command.Type == 1)
            {
                var product = await _productRepository.GetAsync(command.RecordId, cancellation);

                if (product == null)
                    return op.Failed($"محصول {command.RecordId} یافت نشد.");
            }
            else if (command.Type == 2)
            {
                var article = await _blogRepository.GetAsync(command.RecordId, cancellation);

                if (article == null)
                    return op.Failed($"مقاله {command.RecordId} یافت نشد.");
            }
            else
            {
                return op.Failed($"نوع {command.Type} یافت نشد.");
            }

            if (await _commentRepository.HasUserCommented
                (userId, command.Type, command.RecordId, cancellation))
                return op.Failed($"شما قبلا نظر خود را ثبت کرده‌اید و امکان ثبت نظر مجدد در این صفحه برایتان وجود ندارد.");

            var comment = new Comment(
                username,
                userId,
                command.Message,
                command.RecordId,
                command.Type,
                command.Rate);

            if (command.Type == (int)CommentType.Product &&
                await _orderRepository
                .HasThisUserBoughtProduct(userId, command.RecordId, cancellation))
            {
                comment.BuyerComment();
            }

            _commentRepository.Create(comment);
            await _commentRepository.SaveChangesAsync(cancellation);

            if (string.IsNullOrWhiteSpace(command.Message))
            {
                comment.Confirm();
                return op.Succeeded("امتیاز شما ثبت گردید.");
            }

            return op.Succeeded("کامنت ثبت شد و پس از بررسی نمایش داده خواهد شد.");
        }

        public async Task<OperationResult> EditAsync(EditComment command, long userId, CancellationToken cancellation)
        {
            var op = new OperationResult();

            if (string.IsNullOrWhiteSpace(command.Message) && !command.Rate.HasValue)
                return op.Failed("امکان خالی بودن همزمان متن نظر و همچنین امتیاز وجود ندارد.");

            var comment = await _commentRepository.GetAsync(command.Id, cancellation);
            if (comment == null || comment.IsCanceled) return op.Failed("کامنت یافت نشد.");

            if (command.Message == comment.Message &&
                command.Rate == comment.Rate)
                return op.Failed(ApplicationMessages.DuplicatedRecord);

            if (await _commentRepository.OwnsComment(comment.Id, userId, cancellation))
            {
                if (command.Rate.HasValue && command.Rate.Value is >= 1 and <= 5)
                    comment.EditRate(command.Rate.Value);

                if (!string.IsNullOrWhiteSpace(command.Message))
                    comment.EditMessage(command.Message);

                if (string.IsNullOrWhiteSpace(command.Message))
                {
                    comment.Confirm();
                    comment.ClearMessage();

                    await _commentRepository.SaveChangesAsync(cancellation);

                    return op.Succeeded("امتیاز شما ثبت گردید.");
                }

                await _commentRepository.SaveChangesAsync(cancellation);
                return op.Succeeded("ویرایش انجام شد. پس از بررسی و تایید در سایت نمایش داده می‌شود.");
            }

            return op.Failed("شما مجاز به انجام این کار نیستید.");
        }

        public async Task<OperationResult> DeleteAsync(long id, long userId, CancellationToken cancellation)
        {
            var op = new OperationResult();
            var comment = await _commentRepository.GetAsync(id, cancellation);
            if (comment == null) return op.Failed("کامنت یافت نشد.");

            if (await _commentRepository.OwnsComment(id, userId, cancellation))
            {
                comment.Remove();
                await _commentRepository.SaveChangesAsync(cancellation);
                return op.Succeeded("کامنت حذف شد.");
            }

            return op.Failed("شما مجاز به انجام این کار نیستید.");
        }

        public async Task<OperationResult> Confirm(long id, CancellationToken cancellation)
        {
            var op = new OperationResult();
            var comment = await _commentRepository.GetAsync(id, cancellation);
            if (comment == null) return op.Failed("کامنت یافت نشد.");

            comment.Confirm();
            await _commentRepository.SaveChangesAsync(cancellation);

            return op.Succeeded("کامنت تأیید شد.");
        }

        public async Task<OperationResult> Cancel(long id, CancellationToken cancellation)
        {
            var op = new OperationResult();
            var comment = await _commentRepository.GetAsync(id, cancellation);
            if (comment == null) return op.Failed("کامنت یافت نشد.");

            comment.Cancel();
            await _commentRepository.SaveChangesAsync(cancellation);

            return op.Succeeded("کامنت رد شد.");
        }

        public async Task<OperationResult> Reply(long id, string reply, CancellationToken cancellation)
        {
            var op = new OperationResult();
            var comment = await _commentRepository.GetAsync(id, cancellation);
            if (comment == null) return op.Failed("کامنت یافت نشد.");

            if (string.IsNullOrWhiteSpace(reply)) return op.Failed("متن پاسخ نمی‌تواند خالی باشد.");

            comment.AddReply(reply);
            await _commentRepository.SaveChangesAsync(cancellation);

            return op.Succeeded("پاسخ ثبت شد.");
        }

        public async Task<OperationResult> LikeAsync(long id, long userId, CancellationToken cancellation)
        {
            var op = new OperationResult();
            var comment = await _commentRepository.GetAsync(id, cancellation);
            if (comment == null || comment.IsCanceled) return op.Failed("کامنت یافت نشد.");

            if (await _commentRepository.HasUserLiked(id, userId, cancellation))
                return op.Failed("شما قبلاً لایک کرده‌اید.");

            var existing = await _commentRepository.GetUserReaction(id, userId, cancellation);
            if (existing != null && existing.Reaction == ReactionType.Dislike)
            {
                // remove previous
                _commentRepository.RemoveReaction(existing);
                comment.RemoveReactionDislike();
            }

            // add reaction record
            var reaction = new CommentReaction(id, userId, ReactionType.Like);
            _commentRepository.AddReaction(reaction);

            // update aggregate count
            comment.AddLike();

            await _commentRepository.SaveChangesAsync(cancellation);
            return op.Succeeded("لایک ثبت شد.");
        }

        public async Task<OperationResult> DislikeAsync(long id, long userId, CancellationToken cancellation)
        {
            var op = new OperationResult();
            var comment = await _commentRepository.GetAsync(id, cancellation);
            if (comment == null || comment.IsCanceled) return op.Failed("کامنت یافت نشد.");

            if (await _commentRepository.HasUserDisliked(id, userId, cancellation))
                return op.Failed("شما قبلاً دیسلایک کرده‌اید.");

            var existing = await _commentRepository.GetUserReaction(id, userId, cancellation);
            if (existing != null && existing.Reaction == ReactionType.Like)
            {
                _commentRepository.RemoveReaction(existing);
                comment.RemoveReactionLike();
            }

            var reaction = new CommentReaction(id, userId, ReactionType.Dislike);
            _commentRepository.AddReaction(reaction);

            comment.AddDislike();

            await _commentRepository.SaveChangesAsync(cancellation);
            return op.Succeeded("دیسلایک ثبت شد.");
        }

        public async Task<CommentViewModel> GetDetailsAsync(long id, CancellationToken cancellation)
        {
            return await _commentRepository.GetDetails(id, cancellation);
        }

        public async Task<List<CommentViewModel>> SearchAsync(CommentSearchModel searchModel, CancellationToken cancellation)
        {
            return await _commentRepository.Search(searchModel, cancellation);
        }
    }
}
