using Microsoft.AspNetCore.Mvc;

namespace TechExchangeApp.Controllers
{
    public class ErrorsController : Controller
    {
        public IActionResult NotFoundPage()
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound");
        }

        public IActionResult StatusCodePage(int statusCode)
        {
            Response.StatusCode = statusCode;

            if (statusCode == StatusCodes.Status404NotFound)
            {
                return View("NotFound");
            }

            ViewData["StatusCode"] = statusCode;
            return View("NotFound");
        }
    }
}
