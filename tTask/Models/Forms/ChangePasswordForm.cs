using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace tTask.Models.Forms
{
    public class ChangePasswordForm
    {
        [Display(Name = "Current Password")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} is required field!")]
        public string CurrentPassword { get; set; }
        [Display(Name = "New Password")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} is required field!")]
        [StringLength(60, MinimumLength = 8, ErrorMessage = "Minimum length is 8 letters.")]
        public string NewPassword { get; set; }

        [Display(Name = "Confirm Password")]
        [Compare("NewPassword", ErrorMessage = "Password and Confirm Password fields do not match.")]
        public string ConfirmNewPassword { get; set; }
    }
}
