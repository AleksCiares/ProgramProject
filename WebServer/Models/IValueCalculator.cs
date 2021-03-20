using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebServer.Models
{
    public interface IValueCalculator
    {
        decimal ValueProducts(IEnumerable<Product> products);
    }
}