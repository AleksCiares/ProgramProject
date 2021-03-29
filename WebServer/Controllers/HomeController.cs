using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebServer.Models;
using Ninject;

namespace WebServer.Controllers
{
    public class HomeController : Controller
    {
        private Product[] products = {
            new Product {Name = "Kayak", Category = "Watersports", Price = 275M},
            new Product {Name = "Lifejacket", Category = "Watersports", Price = 48.95M},
            new Product {Name = "Soccer ball", Category = "Soccer", Price = 19.50M},
            new Product {Name = "Corner flag", Category = "Soccer", Price = 34.95M}
        };
        private IValueCalculator calculator;

        public HomeController(IValueCalculator calculator)
        {
            this.calculator = calculator;
        }

        //GET: Home
        public ActionResult Index()
        {
            ShoppingCart cart = new ShoppingCart(this.calculator) { Products = products };
            decimal totalValue = cart.CalculateProductTotal();
            return View(totalValue);
        }


    }
}