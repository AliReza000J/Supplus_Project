using _0_Framework.Application;
using AccountManagement.Domain.AccountAgg;
using BlogManagement.Domain.ArticleAgg;
using CommentManagement.Application.Contracts.Comment;
using CommentManagement.Domain.CommentAgg;
using ShopManagement.Domain.ProductAgg;

namespace CommentManagement.Application
{
    public class CommentApplication : ICommentApplication
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IProductRepository _productRepository;
        private readonly IArticleRepository _blogRepository;

        public CommentApplication(ICommentRepository commentRepository, IAccountRepository accountRepository,
            IProductRepository productRepository, IArticleRepository blogRepository)
        {
            _commentRepository = commentRepository;
            _accountRepository = accountRepository;
            _productRepository = productRepository;
            _blogRepository = blogRepository;
        }

        public async Task<OperationResult> AddAsync(AddComment command, string userIdentifier, string username)
        {
            var op = new OperationResult();

            if (string.IsNullOrWhiteSpace(username))
                return op.Failed("نام کاربر الزامی است.");

            if (command.Rate < 1 || command.Rate > 5)
                return op.Failed("امتیاز باید بین ۱ تا ۵ باشد.");

            if (command.Type == 1)
            {
                var product = await _productRepository.GetAsync(command.OwnerRecordId);

                if (product == null)
                    return op.Failed($"محصول {command.OwnerRecordId} یافت نشد.");
            }
            else if (command.Type == 2)
            {
                var article = await _blogRepository.GetAsync(command.OwnerRecordId);

                if (article == null)
                    return op.Failed($"مقاله {command.OwnerRecordId} یافت نشد.");
            }
            else
            {
                return op.Failed($"نوع {command.Type} یافت نشد.");
            }

            if (await _commentRepository.HasUserCommented
                (userIdentifier, command.Type, command.OwnerRecordId))
                return op.Failed($"شما قبلا نظر خود را ثبت کرده‌اید و امکان ثبت نظر مجدد در این صفحه برایتان وجود ندارد.");

            var comment = new Comment(
                username,
                userIdentifier,
                command.Message,
                command.OwnerRecordId,
                command.Type,
                command.Rate);

            _commentRepository.Create(comment);
            await _commentRepository.SaveChangesAsync();

            if (string.IsNullOrWhiteSpace(command.Message))
            {
                comment.Confirm();
                return op.Succeeded("امتیاز شما ثبت گردید.");
            }

            return op.Succeeded("کامنت ثبت شد و پس از بررسی نمایش داده خواهد شد.");
        }

        public async Task<OperationResult> EditAsync(EditComment command, string userIdentifier)
        {
            var op = new OperationResult();

            if (string.IsNullOrWhiteSpace(command.Message) && !command.Rate.HasValue)
                return op.Failed("امکان خالی بودن همزمان متن نظر و همچنین امتیاز وجود ندارد.");

            var comment = await _commentRepository.GetAsync(command.Id);
            if (comment == null || comment.IsCanceled) return op.Failed("کامنت یافت نشد.");

            if(await _commentRepository.OwnsComment(comment.Id, userIdentifier))
            {
                if(!string.IsNullOrWhiteSpace(command.Message))
                    comment.EditMessage(command.Message);
                
                if(command.Rate.HasValue && command.Rate.Value is >=1 and <=5)
                    comment.EditRate(command.Rate.Value);


                if (string.IsNullOrWhiteSpace(command.Message))
                {
                    comment.Confirm();
                    comment.ClearMessage();

                    await _commentRepository.SaveChangesAsync();

                    return op.Succeeded("امتیاز شما ثبت گردید.");
                }

                await _commentRepository.SaveChangesAsync();
                return op.Succeeded("ویرایش انجام شد. پس از بررسی و تایید در سایت نمایش داده می‌شود.");
            }
            
            return op.Failed("شما مجاز به انجام این کار نیستید.");
        }

        public async Task<OperationResult> DeleteAsync(long id, string userIdentifier)
        {
            var op = new OperationResult();
            var comment = await _commentRepository.GetAsync(id);
            if (comment == null) return op.Failed("کامنت یافت نشد.");

            if (await _commentRepository.OwnsComment(id, userIdentifier))
            {
                comment.Remove();
                await _commentRepository.SaveChangesAsync();
                return op.Succeeded("کامنت حذف شد.");
            }

            return op.Failed("شما مجاز به انجام این کار نیستید.");
        }

        public async Task<OperationResult> Confirm(long id)
        {
            var op = new OperationResult();
            var comment = await _commentRepository.GetAsync(id);
            if (comment == null) return op.Failed("کامنت یافت نشد.");

            comment.Confirm();
            await _commentRepository.SaveChangesAsync();

            return op.Succeeded("کامنت تأیید شد.");
        }

        public async Task<OperationResult> Cancel(long id)
        {
            var op = new OperationResult();
            var comment = await _commentRepository.GetAsync(id);
            if (comment == null) return op.Failed("کامنت یافت نشد.");

            comment.Cancel();
            await _commentRepository.SaveChangesAsync();

            return op.Succeeded("کامنت رد شد.");
        }

        public async Task<OperationResult> Reply(long id, string reply)
        {
            var op = new OperationResult();
            var comment = await _commentRepository.GetAsync(id);
            if (comment == null) return op.Failed("کامنت یافت نشد.");

            if (string.IsNullOrWhiteSpace(reply)) return op.Failed("متن پاسخ نمی‌تواند خالی باشد.");

            comment.AddReply(reply);
            await _commentRepository.SaveChangesAsync();

            return op.Succeeded("پاسخ ثبت شد.");
        }

        public async Task<OperationResult> LikeAsync(long id, string userIdentifier)
        {
            var op = new OperationResult();
            var comment = await _commentRepository.GetAsync(id);
            if (comment == null || comment.IsCanceled) return op.Failed("کامنت یافت نشد.");

            if (await _commentRepository.HasUserLiked(id, userIdentifier))
                return op.Failed("شما قبلاً لایک کرده‌اید.");

            var existing = await _commentRepository.GetUserReaction(id, userIdentifier);
            if (existing != null && existing.Reaction == ReactionType.Dislike)
            {
                // remove previous
                _commentRepository.RemoveReaction(existing);
                comment.RemoveReactionDislike();
            }

            // add reaction record
            var reaction = new CommentReaction(id, userIdentifier, ReactionType.Like);
            _commentRepository.AddReaction(reaction);

            // update aggregate count
            comment.AddLike();

            await _commentRepository.SaveChangesAsync();
            return op.Succeeded("لایک ثبت شد.");
        }

        public async Task<OperationResult> DislikeAsync(long id, string userIdentifier)
        {
            var op = new OperationResult();
            var comment = await _commentRepository.GetAsync(id);
            if (comment == null || comment.IsCanceled) return op.Failed("کامنت یافت نشد.");

            if (await _commentRepository.HasUserDisliked(id, userIdentifier))
                return op.Failed("شما قبلاً دیسلایک کرده‌اید.");

            var existing = await _commentRepository.GetUserReaction(id, userIdentifier);
            if (existing != null && existing.Reaction == ReactionType.Like)
            {
                _commentRepository.RemoveReaction(existing);
                comment.RemoveReactionLike();
            }

            var reaction = new CommentReaction(id, userIdentifier, ReactionType.Dislike);
            _commentRepository.AddReaction(reaction);

            comment.AddDislike();

            await _commentRepository.SaveChangesAsync();
            return op.Succeeded("دیسلایک ثبت شد.");
        }

        public async Task<CommentViewModel> GetDetailsAsync(long id)
        {
            return await _commentRepository.GetDetails(id);
        }

        public async Task<List<CommentViewModel>> SearchAsync(CommentSearchModel searchModel)
        {
            return await _commentRepository.Search(searchModel);
        }
    }
}
