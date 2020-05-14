using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tTask.Models.Forms;

namespace tTask.ViewModels
{
    public class IndexViewModel
    {
        public string Domain { get; set; }
        public bool SignUpSelected { get; set; }

        public SignInForm SignIn { get; set; }
        public SignUpForm SignUp { get; set; }
    }
}
