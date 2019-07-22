using Opozee.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Opozee.Models.API
{
    public class QuestionDetail
    {
        public PostQuestionDetail PostQuestionDetail { get; set; }
        public List<Comments> Comments { get; set; }

    }
    public class BookMarkQuestion
    {
        public BookMarkQuestionDetail PostQuestionDetail { get; set; }
        public List<Comments> Comments { get; set; }

    }


    public class BookMarkQuestionMobile
    {
        public BookMarkQuestionDetailMobile PostQuestionDetail { get; set; }
        public List<Comments> Comments { get; set; }

    }


    public class AllUserQuestions
    {

        public List<PostQuestionDetail> PostQuestionDetail { get; set; }
        //public List<Comments> Comments { get; set; }

    }
    public class AllUserQuestionsMobile
    {

        public List<PostQuestionDetailMobile> PostQuestionDetail { get; set; }
        //public List<Comments> Comments { get; set; }

    }
    public class PostQuestionDetail
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public int OwnerUserID { get; set; }
        [Display(Name = "UserName")]
        public string OwnerUserName { get; set; }
        public string Name { get; set; }
        public string UserImage { get; set; }
        public string HashTags { get; set; }
        public int? TotalLikes { get; set; }
        public int YesCount { get; set; }
        public int NoCount { get; set; }
        public int? TotalDisLikes { get; set; }
        public DateTime? CreationDate { get; set; }
        public Comments MostYesLiked { get; set; }
        public Comments MostNoLiked { get; set; }
        public bool? IsSlider { get; set; }
        public List<Comments> Comments { get; set; }
    }

    public class PostQuestionDetailMobile
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public int OwnerUserID { get; set; }
        [Display(Name = "UserName")]
        public string OwnerUserName { get; set; }
        public string Name { get; set; }
        public string UserImage { get; set; }
        public string HashTags { get; set; }
        public int? TotalLikes { get; set; }
        public int YesCount { get; set; }
        public int NoCount { get; set; }
        public int? TotalDisLikes { get; set; }
        public DateTime? CreationDate { get; set; }
        public Comments MostYesLiked { get; set; }
        public Comments MostNoLiked { get; set; }
        public bool? IsSlider { get; set; }

        public List<Comments> Comments { get; set; }
        public DateTime? LastActivityTime { get; set; }
        public int? ReactionSum { get; set; }
    }

    public class BookMarkQuestionDetail
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public int OwnerUserID { get; set; }
        [Display(Name = "UserName")]
        public string OwnerUserName { get; set; }
        public string UserImage { get; set; }
        public string Name { get; set; }
        public string HashTags { get; set; }
        public int? TotalLikes { get; set; }
        public int? TotalDisLikes { get; set; }
        public int YesCount { get; set; }
        public int NoCount { get; set; }
        public DateTime? CreationDate { get; set; }
        public bool? IsBookmark { get; set; }
        public int BookmarkId { get; set; }
        public bool IsUserPosted { get; set; }
        public IQueryable TaggedUsers { get; set; }
        public bool? IsSlider { get; set; }
    }


    public class BookMarkQuestionDetailMobile
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public int OwnerUserID { get; set; }
        [Display(Name = "UserName")]
        public string OwnerUserName { get; set; }
        public string UserImage { get; set; }
        public string Name { get; set; }
        public string HashTags { get; set; }
        public int? TotalLikes { get; set; }
        public int? TotalDisLikes { get; set; }
        public int YesCount { get; set; }
        public int NoCount { get; set; }
        public DateTime? CreationDate { get; set; }
        public bool? IsBookmark { get; set; }
        public int BookmarkId { get; set; }
        public bool IsUserPosted { get; set; }
        public IQueryable TaggedUsers { get; set; }
        public bool? IsSlider { get; set; }

        public DateTime? LastActivityTime { get; set; }
        public int? ReactionSum { get; set; }
    }


    public class Comments
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public string LongForm { get; set; }
        public int CommentedUserId { get; set; }
        public string CommentedUserName { get; set; }
        public int LikesCount { get; set; }
        public string Name { get; set; }
        public string UserImage { get; set; }
        public int DislikesCount { get; set; }
        public DateTime? CreationDate { get; set; }
        public bool Likes { get; set; }
        public bool DisLikes { get; set; }
        public bool? IsAgree { get; set; }

        public int LikesThoughtfulCount { get; set; }
        public int LikesFactualCount { get; set; }
        public int LikesFunnyCount { get; set; }
        public int DislikesIrrationalCount { get; set; }
        public int DislikesFakeNewsCount { get; set; }
        public int DislikesOffTopicCount { get; set; }

        public int SubReaction { get; set; }
        public string BeliefImage { get; set; }
    }


    public class Belief
    {
        public int Id { get; set; }
        public int questionId { get; set; }
        public string beliefText { get; set; }
        public string LongForm { get; set; }
        public int userId { get; set; }
        public string userName { get; set; }
        public string UserFullName { get; set; }
        public string UserImage { get; set; }
        public int LikesCount { get; set; }
        public int DislikesCount { get; set; }
        public DateTime? CreationDate { get; set; }
        public bool? IsAgree { get; set; }
        public String questionText { get; set; }
        public string BeliefImage { get; set; }



    }



}