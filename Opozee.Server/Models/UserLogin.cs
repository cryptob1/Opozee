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

        public string UserName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Password required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string Token { get; set; }
        public string ImageURL { get; set; }

        public int BalanceToken { get; set; }
        //[Display(Name = "Remember Me")]
        //public bool RememberMe { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string ReferralCode { get; set; }
        public int TotalReferred { get; set; }
        public bool IsSocialLogin { get; set; }
        public bool IsVerificationLogin { get; set; }

        public int Followers { get; set; }
        public int Followings { get; set; }

        public string DeviceType { get; set; }
        public string DeviceToken { get; set; }

        public AuthToken AuthToken { get; set; }
    }

    public class AuthToken
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
    }
}