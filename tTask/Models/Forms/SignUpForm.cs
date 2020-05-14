using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace tTask.Models.Forms
{
    public class SignUpForm
    {
        [Display(Name = "Domain")]
        [StringLength(100)]
        public string TenantDomain { get; set; }

        [Display(Name = "Company name")]
        [StringLength(100)]
        public string TenantName { get; set; }

        [Display(Name = "Email")]
        [StringLength(100)]
        [EmailAddress]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} is required field!")]
        public string Email { get; set; }

        [Display(Name = "First Name")]
        [StringLength(100)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} is required field!")]
        public string FirstName { get; set; }

        [Display(Name = "Surname")]
        [StringLength(100)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} is required field!")]
        public string Surname { get; set; }

        [Display(Name = "PhoneNumber")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} is required field!")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Password")]
        [StringLength(60, MinimumLength = 8, ErrorMessage = "Minimum length is 8 letters.")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} is required field!")]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Password and Confirm Password fields do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
