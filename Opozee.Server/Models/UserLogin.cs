﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Opozee.Models
{
    public class UserLogin
    {
        [Display(Name = "Email ID")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email ID required")]
        public string EmailID { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Password required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
    public class UserLoginWeb
    {
       
        public int Id { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email required")]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Password required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string Token { get; set; }
        public string ImageURL { get; set; }

        public int BalanceToken { get; set; }
        //[Display(Name = "Remember Me")]
        //public bool RememberMe { get; set; }
    }
}