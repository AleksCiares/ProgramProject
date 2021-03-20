using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebServer.Models
{
    public class ShoppingCart
    {
        private IValueCalculator calc;
        public IEnumerable<Product> Products { get; set; }

        public ShoppingCart(IValueCalculator calcParam)
        {
            calc = calcParam;
        }

        public decimal CalculateProductTotal()
        {
            return calc.ValueProducts(Products);
        }
    }
}