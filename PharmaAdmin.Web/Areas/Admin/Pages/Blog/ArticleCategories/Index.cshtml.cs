using BlogManagement.Application.Contracts.ArticleCategory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PharmaAdmin.Web.Areas.Admin.Pages.Blog.ArticleCategories
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

        public async Task OnGet(ArticleCategorySearchModel searchModel, CancellationToken cancellation)
        {
            ArticleCategories = await _articleCategoryApplication.Search(searchModel, cancellation);
        }

        public IActionResult OnGetCreate()
        {
            return Partial("./Create", new CreateArticleCategory());
        }

        public async Task<JsonResult> OnPostCreate(CreateArticleCategory command, CancellationToken cancellation)
        {
            var result = await _articleCategoryApplication.Create(command, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnGetEdit(long id, CancellationToken cancellation)
        {
            var productCategory = await _articleCategoryApplication.GetDetails(id, cancellation);
            return Partial("Edit", productCategory);
        }

        public async Task<JsonResult> OnPostEdit(EditArticleCategory command, CancellationToken cancellation)
        {
            var result = await _articleCategoryApplication.Edit(command, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return new JsonResult(result);
        }
    }
}