using BlogManagement.Application.Contracts.ArticleCategory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ServiceHost.Areas.Admin.Pages.Blog.ArticleCategories
{
    public class IndexModel : PageModel
    {
        public ArticleCategorySearchModel SearchModel;
        public List<ArticleCategoryViewModel> ArticleCategories;

        private readonly IArticleCategoryApplication _articleCategoryApplication;

        public IndexModel(IArticleCategoryApplication articleCategoryApplication)
        {
            _articleCategoryApplication = articleCategoryApplication;
        }

        public async Task OnGet(ArticleCategorySearchModel searchModel)
        {
            ArticleCategories = await _articleCategoryApplication.Search(searchModel);
        }

        public IActionResult OnGetCreate()
        {
            return Partial("./Create", new CreateArticleCategory());
        }

        public async Task<JsonResult> OnPostCreate(CreateArticleCategory command)
        {
            var result = await _articleCategoryApplication.Create(command);
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnGetEdit(long id)
        {
            var productCategory = await _articleCategoryApplication.GetDetails(id);
            return Partial("Edit", productCategory);
        }

        public async Task<JsonResult> OnPostEdit(EditArticleCategory command)
        {
            var result = await _articleCategoryApplication.Edit(command);
            return new JsonResult(result);
        }
    }
}