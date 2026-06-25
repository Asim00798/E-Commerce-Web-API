using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Api.Controllers.v1.Orders
{
    public class OrdersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
