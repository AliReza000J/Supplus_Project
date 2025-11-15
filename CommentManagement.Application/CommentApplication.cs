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

        public CommentApplication(ICommentRepository commentRepository, IAccountRepository accountRepository, IProductRepository productRepository, IArticleRepository blogRepository)
        {
            _commentRepository = commentRepository;
            _accountRepository = accountRepository;
            _productRepository = productRepository;
            _blogRepository = blogRepository;
        }

        public OperationResult Add(AddComment command, string userIdentifier, string username)
        {
            var op = new OperationResult();

            if (string.IsNullOrWhiteSpace(username))
                return op.Failed("نام کاربر الزامی است.");

            if (command.Rate < 1 || command.Rate > 5)
                return op.Failed("امتیاز باید بین ۱ تا ۵ باشد.");

            if (command.Type == 1)
            {
                var product = _productRepository.Get(command.OwnerRecordId);

                if (product == null)
                    return op.Failed($"محصول {command.OwnerRecordId} یافت نشد.");
            }
            else if (command.Type == 2)
            {
                var article = _blogRepository.Get(command.OwnerRecordId);

                if (article == null)
                    return op.Failed($"مقاله {command.OwnerRecordId} یافت نشد.");
            }
            else
            {
                return op.Failed($"نوع {command.Type} یافت نشد.");
            }

            var comment = new Comment(
                username,
                userIdentifier,
                command.Message,
                command.OwnerRecordId,
                command.Type,
                command.Rate);

            if (string.IsNullOrWhiteSpace(comment.Message))
                comment.Confirm();

            _commentRepository.Create(comment);
            _commentRepository.SaveChanges();

            return op.Succeeded("کامنت ثبت شد و پس از بررسی نمایش داده خواهد شد.");
        }

        public OperationResult Edit(EditComment command, string userIdentifier)
        {
            var op = new OperationResult();
            var comment = _commentRepository.Get(command.Id);
            if (comment == null) return op.Failed("کامنت یافت نشد.");

            if(_commentRepository.OwnsComment(comment.Id, userIdentifier))
            {
                comment.Edit(command.Message, command.Rate);
                _commentRepository.SaveChanges();
                return op.Succeeded("ویرایش انجام شد.");
            }
            
            return op.Failed("شما مجاز به انجام این کار نیستید.");
        }

        public OperationResult Delete(long id, string userIdentifier)
        {
            var op = new OperationResult();
            var comment = _commentRepository.Get(id);
            if (comment == null) return op.Failed("کامنت یافت نشد.");

            if (_commentRepository.OwnsComment(id, userIdentifier))
            {
                comment.Remove();
                _commentRepository.SaveChanges();
                return op.Succeeded("کامنت حذف شد.");
            }

            return op.Failed("شما مجاز به انجام این کار نیستید.");
        }

        public OperationResult Confirm(long id)
        {
            var op = new OperationResult();
            var comment = _commentRepository.Get(id);
            if (comment == null) return op.Failed("کامنت یافت نشد.");

            comment.Confirm();
            _commentRepository.SaveChanges();

            return op.Succeeded("کامنت تأیید شد.");
        }

        public OperationResult Cancel(long id)
        {
            var op = new OperationResult();
            var comment = _commentRepository.Get(id);
            if (comment == null) return op.Failed("کامنت یافت نشد.");

            comment.Cancel();
            _commentRepository.SaveChanges();

            return op.Succeeded("کامنت رد شد.");
        }

        public OperationResult Reply(long id, string reply)
        {
            var op = new OperationResult();
            var comment = _commentRepository.Get(id);
            if (comment == null) return op.Failed("کامنت یافت نشد.");

            if (string.IsNullOrWhiteSpace(reply)) return op.Failed("متن پاسخ نمی‌تواند خالی باشد.");

            comment.AddReply(reply);
            _commentRepository.SaveChanges();

            return op.Succeeded("پاسخ ثبت شد.");
        }

        public OperationResult Like(long id, string userIdentifier)
        {
            var op = new OperationResult();
            var comment = _commentRepository.Get(id);
            if (comment == null) return op.Failed("کامنت یافت نشد.");

            if (_commentRepository.HasUserLiked(id, userIdentifier))
                return op.Failed("شما قبلاً لایک کرده‌اید.");

            var existing = _commentRepository.GetUserReaction(id, userIdentifier);
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

            _commentRepository.SaveChanges();
            return op.Succeeded("لایک ثبت شد.");
        }

        public OperationResult Dislike(long id, string userIdentifier)
        {
            var op = new OperationResult();
            var comment = _commentRepository.Get(id);
            if (comment == null) return op.Failed("کامنت یافت نشد.");

            if (_commentRepository.HasUserDisliked(id, userIdentifier))
                return op.Failed("شما قبلاً دیسلایک کرده‌اید.");

            var existing = _commentRepository.GetUserReaction(id, userIdentifier);
            if (existing != null && existing.Reaction == ReactionType.Like)
            {
                _commentRepository.RemoveReaction(existing);
                comment.RemoveReactionLike();
            }

            var reaction = new CommentReaction(id, userIdentifier, ReactionType.Dislike);
            _commentRepository.AddReaction(reaction);

            comment.AddDislike();

            _commentRepository.SaveChanges();
            return op.Succeeded("دیسلایک ثبت شد.");
        }

        public CommentViewModel GetDetails(long id)
        {
            return _commentRepository.GetDetails(id);
        }

        public List<CommentViewModel> Search(CommentSearchModel searchModel)
        {
            return _commentRepository.Search(searchModel);
        }
    }
}
