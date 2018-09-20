using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace StateOfOhioLibrary.Models
{
    public class LoginModel
    {
        public string ReturnUrl { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorMessageContainer), ErrorMessageResourceName = "UsernameRequired")]
        [RegularExpression(@"^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$", ErrorMessageResourceType = typeof(ErrorMessageContainer), ErrorMessageResourceName = "EmailInvalid")]

        public string UserName { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorMessageContainer), ErrorMessageResourceName = "PasswordRequired")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
        public string LoginStatus { get; set; }

    }
}