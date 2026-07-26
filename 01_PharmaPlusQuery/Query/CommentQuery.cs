using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Comment;
using CommentManagement.Domain.CommentAgg;
using CommnetManagement.Infrastructure.EFCore;
using Microsoft.EntityFrameworkCore;
using ShopManagement.Infrastructure.EFCore;

namespace _01_PharmaPlusQuery.Query
{
    public class CommentQuery : ICommentQuery
    {
        private readonly CommentContext _commentContext;
        private readonly ShopContext _shopContext;

        public CommentQuery(CommentContext commentContext, ShopContext shopContext)
        {
            _commentContext = commentContext;
            _shopContext = shopContext;
        }

        public async Task<List<UserCommentQueryModel>?> GetUserComments(long userId, CancellationToken cancellation)
        {
            var comments = await _commentContext.Comments
                .Where(c => c.UserId == userId && c.Type == (int)CommentType.Product)
                .OrderByDescending(c => c.Id)
                .Select(c => new UserCommentQueryModel
                {
                    RecordId = c.RecordId,
                    Message = c.Message,
                    CreationDate = c.CreationDate.ToFarsi(),
                    Rate = c.Rate,
                    LikeCount = c.LikeCount,
                    DislikeCount = c.DislikeCount,
                    Reply = c.AdminReply,
                    ReplyDate = c.AdminReplyDate.ToFarsi()
                })
                .ToListAsync(cancellation);

            if (comments == null || !comments.Any())
                return comments;

            var productIds = comments.Select(c => c.RecordId).Distinct().ToList();

            var productSlugsDict = await _shopContext.Products
                .Where(p => productIds.Contains(p.Id))
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Slug,
                    CategoryName = p.Category.Name,
                    CategoryFullSlug = p.Category.FullSlug
                })
                .ToDictionaryAsync(p => p.Id, cancellation);

            foreach (var comment in comments)
            {
                if (productSlugsDict.TryGetValue(comment.RecordId, out var slugInfo))
                {
                    comment.ProductName = slugInfo.Name;
                    comment.ProductSlug = slugInfo.Slug;
                    comment.CategoryName = slugInfo.CategoryName;
                    comment.CategoryFullSlug = slugInfo.CategoryFullSlug;
                }
            }

            return comments;
        }


    }
}
