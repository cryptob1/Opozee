﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Opozee.Models
{
    public class UserModel
    {
    }

    public class UserModelProfileEditWeb
    {
        [Display(Name = "Email ID")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "First name required")]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Last Name required")]
        [DataType(DataType.Password)]
        public string LastName { get; set; }

        [Display(Name = "user name required")]
        public string UserName { get; set; }

        [Display(Name = "Email required")]
        public string Email { get; set; }


        [Display(Name = "User Info")]
        public string UserInfo { get; set; }


        [Display(Name = "Password required")]
        public string Password { get; set; }
        public string ImageURL { get; set; }

        public int UserId { get; set; }

        public bool IsSocialLogin { get; set; }

    }

    public class PostAnswerWeb
    {
   
        public int QuestId { get; set; }
        public string Comment { get; set; }
        public int CommentedUserId { get; set; }
        public string CreationDate { get; set; }
        public string ModifiedDate { get; set; }
        public int Likes { get; set; }
        public bool OpinionAgreeStatus { get; set; }
        public int Dislikes { get; set; }
    }


}