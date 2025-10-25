using _01_PharmaPlusQuery.Contracts.ProductCategory;
using Microsoft.AspNetCore.Mvc;

namespace ServiceHost.Controllers.Shop
{
    [Route("shop/[controller]")]
    [ApiController]
    public class ProductCategoryController : ControllerBase
    {
        private readonly IProductCategoryQuery _productCategoryQuery;
        private readonly ILogger<ProductCategoryController> _logger;

        public ProductCategoryController(
            IProductCategoryQuery productCategoryQuery,
            ILogger<ProductCategoryController> logger)
        {
            _productCategoryQuery = productCategoryQuery;
            _logger = logger;
        }

        //[HttpGet]
        //public async Task<IActionResult> GetProductCategories() //full
        //{
        //    try
        //    {
        //        var productCategories = await _productCategoryQuery.GetProductCategories();

        //        if (productCategories == null || !productCategories.Any())
        //            return NotFound("هیچ دسته‌بندی محصولی یافت نشد.");

        //        return Ok(productCategories);
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        _logger.LogWarning(ex, "ورودی نامعتبر در دریافت دسته‌بندی‌ها");
        //        return BadRequest(new
        //        {
        //            error = "خطای ورودی",
        //            message = "داده‌های ارسال‌شده معتبر نیستند."
        //        });
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        _logger.LogWarning(ex, "خطای منطقی در دریافت دسته‌بندی‌ها");
        //        return StatusCode(StatusCodes.Status409Conflict, new
        //        {
        //            error = "تداخل در پردازش",
        //            message = "عملیات مورد نظر با مشکل مواجه شد."
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "خطای غیرمنتظره در دریافت دسته‌بندی‌ها");
        //        return Problem(
        //            title: "خطای سرور",
        //            detail: "در هنگام پردازش درخواست خطایی رخ داده است. لطفاً مجدداً تلاش کنید.",
        //            statusCode: StatusCodes.Status500InternalServerError);
        //    }
        //}


        //[HttpGet("leaf/{slug}")]
        //public async Task<IActionResult> GetLeafProductCategoryWithProductsBySlug(string slug) //specific leaf
        //{
            //try
            //{
            //    var productCategories = await _productCategoryQuery.GetLeafProductCategoryWithProductsBy(slug);

            //    if (productCategories == null)
            //        return NotFound($"هیچ دسته‌بندی محصولی برگ با {slug} یافت نشد.");

            //    return Ok(productCategories);
            //}
            //catch (ArgumentException ex)
            //{
            //    _logger.LogWarning(ex, "ورودی نامعتبر در دریافت دسته‌بندی‌ها");
            //    return BadRequest(new
            //    {
            //        error = "خطای ورودی",
            //        message = "داده‌های ارسال‌شده معتبر نیستند."
            //    });
            //}
            //catch (InvalidOperationException ex)
            //{
            //    _logger.LogWarning(ex, "خطای منطقی در دریافت دسته‌بندی‌ها");
            //    return StatusCode(StatusCodes.Status409Conflict, new
            //    {
            //        error = "تداخل در پردازش",
            //        message = "عملیات مورد نظر با مشکل مواجه شد."
            //    });
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "خطای غیرمنتظره در دریافت دسته‌بندی‌ها");
            //    return Problem(
            //        title: "خطای سرور",
            //        detail: "در هنگام پردازش درخواست خطایی رخ داده است. لطفاً مجدداً تلاش کنید.",
            //        statusCode: StatusCodes.Status500InternalServerError);
            //}
        //}

        //[HttpGet("with-products")]
        //public async Task<IActionResult> GetProductCategoriesWithProducts()
        //{
        //    try
        //    {
        //        var productCategories = await _productCategoryQuery.GetProductCategoriesWithProducts();

        //        if (productCategories == null || !productCategories.Any())
        //            return NotFound("هیچ دسته‌بندی محصولی یافت نشد.");

        //        return Ok(productCategories);
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        _logger.LogWarning(ex, "ورودی نامعتبر در دریافت دسته‌بندی‌ها");
        //        return BadRequest(new
        //        {
        //            error = "خطای ورودی",
        //            message = "داده‌های ارسال‌شده معتبر نیستند."
        //        });
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        _logger.LogWarning(ex, "خطای منطقی در دریافت دسته‌بندی‌ها");
        //        return StatusCode(StatusCodes.Status409Conflict, new
        //        {
        //            error = "تداخل در پردازش",
        //            message = "عملیات مورد نظر با مشکل مواجه شد."
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "خطای غیرمنتظره در دریافت دسته‌بندی‌ها");
        //        return Problem(
        //            title: "خطای سرور",
        //            detail: "در هنگام پردازش درخواست خطایی رخ داده است. لطفاً مجدداً تلاش کنید.",
        //            statusCode: StatusCodes.Status500InternalServerError);
        //    }
        //}

        [HttpGet]
        public async Task<IActionResult> GetAllCategories() //full menu
        {
            try
            {
                var result = await _productCategoryQuery.GetAll();

                if (result == null || !result.Any())
                    return NotFound("هیچ دسته‌بندی محصولی یافت نشد.");

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "ورودی نامعتبر در دریافت دسته‌بندی‌ها");
                return BadRequest(new
                {
                    error = "خطای ورودی",
                    message = "داده‌های ارسال‌شده معتبر نیستند."
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "خطای منطقی در دریافت دسته‌بندی‌ها");
                return StatusCode(StatusCodes.Status409Conflict, new
                {
                    error = "تداخل در پردازش",
                    message = "عملیات مورد نظر با مشکل مواجه شد."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطای غیرمنتظره در دریافت دسته‌بندی‌ها");
                return Problem(
                    title: "خطای سرور",
                    detail: "در هنگام پردازش درخواست خطایی رخ داده است. لطفاً مجدداً تلاش کنید.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
            
        }

        //[HttpGet("leaf")]
        //public async Task<IActionResult> GetLeafProductCategories() // all leaves
        //{
        //    try
        //    {
        //        var result = await _productCategoryQuery.GetLeafProductCategories();

        //        if (result == null || !result.Any())
        //            return NotFound("هیچ دسته‌بندی محصولی یافت نشد.");

        //        return Ok(result);
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        _logger.LogWarning(ex, "ورودی نامعتبر در دریافت دسته‌بندی‌ها");
        //        return BadRequest(new
        //        {
        //            error = "خطای ورودی",
        //            message = "داده‌های ارسال‌شده معتبر نیستند."
        //        });
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        _logger.LogWarning(ex, "خطای منطقی در دریافت دسته‌بندی‌ها");
        //        return StatusCode(StatusCodes.Status409Conflict, new
        //        {
        //            error = "تداخل در پردازش",
        //            message = "عملیات مورد نظر با مشکل مواجه شد."
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "خطای غیرمنتظره در دریافت دسته‌بندی‌ها");
        //        return Problem(
        //            title: "خطای سرور",
        //            detail: "در هنگام پردازش درخواست خطایی رخ داده است. لطفاً مجدداً تلاش کنید.",
        //            statusCode: StatusCodes.Status500InternalServerError);
        //    }
        //}
        //////////////////

        [HttpGet("{*path}")]
        public async Task<IActionResult> GetCategoryByPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return BadRequest("مسیر نامعتبر.");

            try
            {
                var productCategories = await _productCategoryQuery.GetCategoryByFullSlug(path);

                if (productCategories == null)
                    return NotFound($"دسته‌بندی محصول {path} یافت نشد.");

                return Ok(productCategories);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "ورودی نامعتبر در دریافت دسته‌بندی‌ها");
                return BadRequest(new
                {
                    error = "خطای ورودی",
                    message = "داده‌های ارسال‌شده معتبر نیستند."
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "خطای منطقی در دریافت دسته‌بندی‌ها");
                return StatusCode(StatusCodes.Status409Conflict, new
                {
                    error = "تداخل در پردازش",
                    message = "عملیات مورد نظر با مشکل مواجه شد."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطای غیرمنتظره در دریافت دسته‌بندی‌ها");
                return Problem(
                    title: "خطای سرور",
                    detail: "در هنگام پردازش درخواست خطایی رخ داده است. لطفاً مجدداً تلاش کنید.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
            
        }
    }
}
