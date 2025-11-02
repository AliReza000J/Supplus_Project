using _0_Framework.Application;
using CommentManagement.Application.Contracts.Comment;
using CommentManagement.Domain.CommentAgg;

namespace CommentManagement.Application
{
    public class CommentApplication : ICommentApplication
    {
        private readonly ICommentRepository _commentRepository;

        public CommentApplication(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public OperationResult Add(AddComment command)
        {
            var op = new OperationResult();

            if (string.IsNullOrWhiteSpace(command.Username))
                return op.Failed("نام کاربر الزامی است.");

            if (command.Rate < 1 || command.Rate > 5)
                return op.Failed("امتیاز باید بین ۱ تا ۵ باشد.");

            var comment = new Comment(
                command.Username,
                command.Email,
                command.Message,
                command.OwnerRecordId,
                command.Type,
                //command.ParentId,
                command.Rate);

            if (string.IsNullOrWhiteSpace(comment.Message))
                comment.Confirm();

            _commentRepository.Create(comment);
            _commentRepository.SaveChanges();

            return op.Succeeded("کامنت ثبت شد و پس از بررسی نمایش داده خواهد شد.");
        }

        public OperationResult Edit(EditComment command)
        {
            var op = new OperationResult();
            var comment = _commentRepository.Get(command.Id);
            if (comment == null) return op.Failed("کامنت یافت نشد.");

            comment.Edit(command.Message, command.Rate);
            _commentRepository.SaveChanges();

            return op.Succeeded("ویرایش انجام شد.");
        }

        public OperationResult Delete(long id)
        {
            var op = new OperationResult();
            var comment = _commentRepository.Get(id);
            if (comment == null) return op.Failed("کامنت یافت نشد.");

            comment.Remove();
            _commentRepository.SaveChanges();

            return op.Succeeded("کامنت حذف شد.");
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
