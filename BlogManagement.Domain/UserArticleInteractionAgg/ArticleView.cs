using _0_Framework.Domain;

namespace BlogManagement.Domain.UserArticleInteractionAgg
{
    public class ArticleView : BaseEntity
    {
        public long ArticleId { get; set; }
        public string? UserId { get; set; }

        protected ArticleView()
        {
            
        }
        public ArticleView(long articleId, string? userId)
        {
            ArticleId = articleId;
            if(string.IsNullOrWhiteSpace(userId))
                UserId = "-";
            else
                UserId = userId;
        }
    }
}
