using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace tTask.Models.Forms
{
    public class SignInForm
    {
        [Display(Name = "Email")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} is required field!")]
        [EmailAddress(ErrorMessage = "This is not an email address.")]
        public string Email { get; set; }
        [Display(Name = "Password")]
        [StringLength(60, MinimumLength = 8, ErrorMessage = "Minimum length is 8 letters.")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} is required field!")]
        public string Password { get; set; }
    }
}
