using _01_PharmaPlusQuery.Contracts.Comment;

namespace _01_PharmaPlusQuery.Contracts.Article
{
    public class ArticleQueryModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string Body { get; set; }
        public string Picture { get; set; }
        public string PictureAlt { get; set; }
        public string PictureTitle { get; set; }
        public string PublishDate { get; set; }
        public string Slug { get; set; }
        public string Keywords { get; set; }
        public List<string> KeywordList { get; set; }
        public string MetaDescription { get; set; }
        public string CanonicalAddress { get; set; }
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategorySlug { get; set; }
        public List<CommentQueryModel> Comments { get; set; }
        public float AvgRate { get; set; }
        public string? AvgRateStr { get; set; }
        public int CommentCount { get; set; }
        public string? CommentCountStr { get; set; }
        public int RateCount { get; set; }
        public string? RateCountStr { get; set; }
        public int ViewCount { get; set; }
        public string? ViewsLabel { get; set; }
    }
}