using Opozee.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Opozee.Models.API
{
    public class UserNotifications
    {
        public int? UserId { get; set; }
        public int? QuestionId { get; set; }
        public string Question { get; set; }
        public string HashTags { get; set; }
        public int? OpinionId { get; set; }
        public string Opinion { get; set; }
        public int? CommentedUserId { get; set; }
        public int? NotificationUserId { get; set; }
         
        public string UserName { get; set; }
        public string Name { get; set; }
        public bool? Like { get; set; }
        public string Image { get; set; }
        public bool? Dislike { get; set; }
        public bool? Comment { get; set; }
        public string Tag { get; set; }
        public string Message { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? TotalRecordcount { get; set; }
        public int? NotificationId { get; set; }
        public bool? IsAgree { get; set; }
        public List<string> OpinionList { get; set; }
        public DateTime? QOCreationDate { get; set; }
        public bool IsValidToDelete { get; set; }
        public bool RefferalStatus { get; set; }
    }
    public class UserProfile
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ImageURL { get; set; }
        public int? BalanceToken { get; set; }
        public int TotalPostedQuestion { get; set; }
        public int? TotalLikes { get; set; }
        public int? TotalDislikes { get; set; }
        public int? TotalPostedBeliefs { get; set; }
 

        public bool IsSocialLogin { get; set; }
        public DateTime? LastLoginDate { get; set; }

        public string ReferralCode { get; set; }
        public int TotalReferred { get; set; }
    }
    public class OtherUsers
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string ImageURL { get; set; }
        public int TotalPostedQuestion { get; set; }

    }

    public class OtherUserProfile
    {
        public OtherUsers UserProfile { get; set; }
        public List<PostQuestionDetail> Question { get; set; }
    }
}