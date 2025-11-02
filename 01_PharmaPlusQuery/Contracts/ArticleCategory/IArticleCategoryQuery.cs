using System.Collections.Generic;

namespace _01_PharmaPlusQuery.Contracts.ArticleCategory
{
    public interface IArticleCategoryQuery
    {
        Task<ArticleCategoryQueryModel> GetArticleCategory(string slug);
        Task<List<ArticleCategoryQueryModel>> GetArticleCategories();
    }
}
