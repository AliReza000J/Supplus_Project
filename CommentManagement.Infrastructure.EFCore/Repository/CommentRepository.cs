using _0_Framework.Application;
using _0_Framework.Infrastructure;
using BlogManagement.Infrastructure.EFCore;
using CommentManagement.Application.Contracts.Comment;
using CommentManagement.Domain.CommentAgg;
using CommnetManagement.Infrastructure.EFCore;
using Microsoft.EntityFrameworkCore;
using ShopManagement.Infrastructure.EFCore;

namespace CommentManagement.Infrastructure.EFCore.Repository
{
    public class CommentRepository : BaseRepository<long, Comment>, ICommentRepository
    {
        private readonly CommentContext _context;
        private readonly ShopContext _shopContext;
        private readonly BlogContext _blogContext;
        public CommentRepository(CommentContext context, ShopManagement.Infrastructure.EFCore.ShopContext shopContext, BlogContext blogContext) : base(context)
        {
            _context = context;
            _shopContext = shopContext;
            _blogContext = blogContext;
        }

        public List<CommentViewModel> Search(CommentSearchModel searchModel)
        {
            var query = _context.Comments.AsNoTracking()
                .Select(c => new CommentViewModel
                {
                    Id = c.Id,
                    Username = c.Username,
                    Email = c.Email,
                    Message = c.Message,
                    OwnerRecordId = c.OwnerRecordId,
                    OwnerName = "", // fill later
                    Type = c.Type,
                    IsConfirmed = c.IsConfirmed,
                    IsCanceled = c.IsCanceled,
                    CommentDate = c.LastModified.HasValue ? c.LastModified.Value.ToFarsi() : DateTime.Now.ToFarsi(),
                    Rate = c.Rate,
                    LikeCount = c.LikeCount,
                    DislikeCount = c.DislikeCount,
                    Reply = c.AdminReply,
                    ReplyDate = c.AdminReplyDate.HasValue ? c.AdminReplyDate.Value.ToFarsi() : null
                });

            if (searchModel != null)
            {
                if (searchModel.Type.HasValue)
                    query = query.Where(x => x.Type == searchModel.Type.Value);

                if (!string.IsNullOrWhiteSpace(searchModel.Username))
                    query = query.Where(x => x.Username.Contains(searchModel.Username));

                if (searchModel.OwnerRecordId.HasValue)
                    query = query.Where(x => x.OwnerRecordId == searchModel.OwnerRecordId.Value);

                if (searchModel.IsConfirmed.HasValue)
                    query = query.Where(x => x.IsConfirmed == searchModel.IsConfirmed.Value);

                if (searchModel.IsCanceled.HasValue)
                    query = query.Where(x => x.IsCanceled == searchModel.IsCanceled.Value);

                if (searchModel.Rate.HasValue)
                    query = query.Where(x => x.Rate == searchModel.Rate.Value);
            }

            var list = query.OrderByDescending(x => x.Id).ToList();

            // Resolve OwnerName for product comments (optional)
            var productIds = list.Where(l => l.Type == (int)CommentType.Product).Select(l => l.OwnerRecordId).Distinct().ToList();
            if (productIds.Any())
            {
                var names = _shopContext.Products
                    .Where(p => productIds.Contains(p.Id))
                    .Select(p => new { p.Id, p.Name })
                    .ToDictionary(p => p.Id, p => p.Name);

                foreach (var item in list.Where(x => x.Type == (int)CommentType.Product))
                {
                    if (names.TryGetValue(item.OwnerRecordId, out var name))
                        item.OwnerName = name;
                }
            }

            var articleIds = list.Where(l => l.Type == (int)CommentType.Article).Select(l => l.OwnerRecordId).Distinct().ToList();
            if (articleIds.Any())
            {
                var names = _blogContext.Articles
                    .Where(p => articleIds.Contains(p.Id))
                    .Select(p => new { p.Id, p.Title })
                    .ToDictionary(p => p.Id, p => p.Title);

                foreach (var item in list.Where(x => x.Type == (int)CommentType.Article))
                {
                    if (names.TryGetValue(item.OwnerRecordId, out var name))
                        item.OwnerName = name;
                }
            }

            return list;
        }

        public CommentViewModel GetDetails(long id)
        {
            var c = _context.Comments
                .AsNoTracking()
                .FirstOrDefault(x => x.Id == id);

            if (c == null) return null!;

            var vm = new CommentViewModel
            {
                Id = c.Id,
                Username = c.Username,
                Email = c.Email,
                Message = c.Message,
                OwnerRecordId = c.OwnerRecordId,
                OwnerName = "", // fill below
                Type = c.Type,
                IsConfirmed = c.IsConfirmed,
                IsCanceled = c.IsCanceled,
                CommentDate = c.LastModified.HasValue ? c.LastModified.Value.ToFarsi() : DateTime.Now.ToFarsi(),
                Rate = c.Rate,
                LikeCount = c.LikeCount,
                DislikeCount = c.DislikeCount,
                Reply = c.AdminReply,
                ReplyDate = c.AdminReplyDate.HasValue ? c.AdminReplyDate.Value.ToFarsi() : null
            };

            if (c.Type == (int)CommentType.Product)
            {
                var product = _shopContext.Products.Select(p => new { p.Id, p.Name }).FirstOrDefault(p => p.Id == vm.OwnerRecordId);
                vm.OwnerName = product?.Name ?? "";
            }

            if (c.Type == (int)CommentType.Article)
            {
                var product = _blogContext.Articles.Select(p => new { p.Id, p.Title }).FirstOrDefault(p => p.Id == vm.OwnerRecordId);
                vm.OwnerName = product?.Title ?? "";
            }

            return vm;
        }

        // reactions
        public bool HasUserLiked(long commentId, string userIdentifier)
        {
            return _context.CommentReactions
                .AsNoTracking()
                .Any(r => r.CommentId == commentId && r.UserIdentifier == userIdentifier && r.Reaction == ReactionType.Like);
        }

        public bool HasUserDisliked(long commentId, string userIdentifier)
        {
            return _context.CommentReactions
                .AsNoTracking()
                .Any(r => r.CommentId == commentId && r.UserIdentifier == userIdentifier && r.Reaction == ReactionType.Dislike);
        }

        public void AddReaction(CommentReaction reaction)
        {
            _context.CommentReactions.Add(reaction);
        }

        public void RemoveReaction(CommentReaction reaction)
        {
            _context.CommentReactions.Remove(reaction);
        }

        public CommentReaction? GetUserReaction(long commentId, string userIdentifier)
        {
            return _context.CommentReactions.FirstOrDefault(r => r.CommentId == commentId && r.UserIdentifier == userIdentifier);
        }
    }
}
