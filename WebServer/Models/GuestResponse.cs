using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace WebServer.Models
{
    public class GuestResponse
    {
        [Required(ErrorMessage = "Please enter your name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter your email")]
        [RegularExpression(@".+\@.+\..+", ErrorMessage = "Please enter valid email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter your phone")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Please precify wheter you`ll attend")]
        public bool? WillAttend { get; set; }
    }
}