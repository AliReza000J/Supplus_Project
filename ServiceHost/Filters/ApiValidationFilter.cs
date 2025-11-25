using _0_Framework.Application;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace ServiceHost.Filters
{
    public class ApiValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        k => k.Key,
                        v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                context.Result = new BadRequestObjectResult(
                    ApiResponse<object>.Fail("خطا در اعتبارسنجی داده‌ها.", errors));
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }

}
