using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace StateOfOhioLibrary.Models
{
    public class ForgotPasswordModel
    {
        [Required(ErrorMessageResourceType = typeof(ErrorMessageContainer), ErrorMessageResourceName = "EmailRequired")]
        [RegularExpression(@"^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$", ErrorMessageResourceType = typeof(ErrorMessageContainer), ErrorMessageResourceName = "EmailInvalid")]

        public string Email { get; set; }

        public string PasswordStatus { get; set; }

    }

    public class ResetPasswordViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Token { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorMessageContainer), ErrorMessageResourceName = "NewPasswordRequired")]
        [StringLength(20, ErrorMessageResourceType = typeof(ErrorMessageContainer), ErrorMessageResourceName = "PasswordLength", MinimumLength = 6)]
        [DataType(DataType.Password)]

        public string NewPassword { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorMessageContainer), ErrorMessageResourceName = "ConfirmPasswordRequired")]
        [System.ComponentModel.DataAnnotations.CompareAttribute("NewPassword", ErrorMessageResourceType = typeof(ErrorMessageContainer), ErrorMessageResourceName = "PasswordMatch")]
        public string ConfirmPassword { get; set; }

    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessageResourceType = typeof(ErrorMessageContainer), ErrorMessageResourceName = "OldPasswordRequired")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorMessageContainer), ErrorMessageResourceName = "NewPasswordRequired")]
        [StringLength(20, ErrorMessageResourceType = typeof(ErrorMessageContainer), ErrorMessageResourceName = "PasswordLength", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorMessageContainer), ErrorMessageResourceName = "ConfirmPasswordRequired")]
        [System.ComponentModel.DataAnnotations.CompareAttribute("NewPassword", ErrorMessageResourceType = typeof(ErrorMessageContainer), ErrorMessageResourceName = "ChangePasswordMatch")]
        public string ConfirmPassword { get; set; }

        public string PreviousPageUrl { get; set; }

        public int CurrentUserId { get; set; }

        public int MemberId { get; set; }
    }
}