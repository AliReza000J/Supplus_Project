using _0_Framework.Domain;
using CommentManagement.Application.Contracts.Comment;

namespace CommentManagement.Domain.CommentAgg
{
    public interface ICommentRepository : IRepository<long, Comment>
    {
        List<CommentViewModel> Search(CommentSearchModel searchModel);
        CommentViewModel GetDetails(long id);
        bool OwnsComment(long commentId, string userIdentifier);

        // reactions
        bool HasUserLiked(long commentId, string userIdentifier);
        bool HasUserDisliked(long commentId, string userIdentifier);

        // reaction add/remove helpers for repository-level operations
        void AddReaction(CommentReaction reaction);
        void RemoveReaction(CommentReaction reaction);
        CommentReaction? GetUserReaction(long commentId, string userIdentifier);

    }
}
