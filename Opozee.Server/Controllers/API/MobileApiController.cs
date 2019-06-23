﻿using opozee.Enums;
using Opozee.Models.API;
using Opozee.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Configuration;
using System.Web.Http;
using OpozeeLibrary.API;
using static OpozeeLibrary.Utilities.ResizeImage;
using OpozeeLibrary.Utilities;
using System.Data.Entity;
using System.Data.SqlClient;
using Opozee.Models;
using System.Web;
using MvcPaging;
using OpozeeLibrary.PushNotfication;
using System.Configuration;
using RestSharp;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Dynamic;
using Opozee.Server.Services;
using System.IO;
using Opozee.Server.Models.API;
using Opozee.Server.Models;

namespace opozee.Controllers.API
{
    [RoutePrefix("opozee")]
    public class MobileApiController : ApiController
    {
        OpozeeDbEntities db = new OpozeeDbEntities();
        public static string con = ConfigurationManager.ConnectionStrings["OpozeeDbEntitiesSp"].ToString();
        int _Imagethumbsize = 0;
        int _imageSize = 0;
        // GET: api/MobileApi
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/MobileApi/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/MobileApi
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/MobileApi/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/MobileApi/5
        public void Delete(int id)
        {
        }

        #region "Socail Login" 
        [HttpPost]
        [Route("api/MobileApi/signinthirdparty")]
        public HttpResponseMessage SigninThirdParty(InputSignInWithThirdParty input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }

                User entity = null;
                if (input.ThirdPartyType == ThirdPartyType.Facebook)
                {
                    entity = db.Users.Where(p => p.SocialID == input.ThirdPartyId
                                        && p.RecordStatus != RecordStatus.Deleted.ToString()).FirstOrDefault();
                }
                else if (input.ThirdPartyType == ThirdPartyType.Twitter)
                {
                    entity = db.Users.Where(p => p.SocialID == input.ThirdPartyId
                                        && p.RecordStatus != RecordStatus.Deleted.ToString()).FirstOrDefault();
                }
                else if (input.ThirdPartyType == ThirdPartyType.GooglePlus)
                {
                    entity = db.Users.Where(p => p.SocialID == input.ThirdPartyId
                                        && p.RecordStatus != RecordStatus.Deleted.ToString()).FirstOrDefault();
                }
                string strThumbnailURLfordb = null;
                string strIamgeURLfordb = null;
                string _SiteRoot = WebConfigurationManager.AppSettings["SiteImgPath"];
                string _SiteURL = WebConfigurationManager.AppSettings["SiteImgURL"];

                string strThumbnailImage = input.ImageURL;
                if (entity != null)
                {

                    if (entity.RecordStatus != RecordStatus.Active.ToString())
                    {
                        //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "User is not Active"));
                    }

                    entity.UserName = input.UserName != null && input.UserName != "" ? input.UserName : entity.UserName;
                    if (!string.IsNullOrEmpty(input.Password))
                    {
                        entity.Password = AesCryptography.Encrypt(input.Password);
                    }
                    entity.DeviceType = input.DeviceType != null && input.DeviceType != "" ? input.DeviceType : entity.DeviceType;
                    entity.DeviceToken = input.DeviceToken != null && input.DeviceToken != "" ? input.DeviceToken : entity.DeviceToken;
                    entity.ImageURL = entity.ImageURL;
                    //if (input.ImageURL != null && input.ImageURL != "")
                    //{
                    //    try
                    //    {
                    //        string strTempImageSave = OpozeeLibrary.Utilities.ResizeImage.Download_Image(input.ImageURL);
                    //        string profileFilePath = _SiteURL + "/ProfileImage/" + strTempImageSave;
                    //        strIamgeURLfordb = profileFilePath;
                    //        entity.ImageURL = profileFilePath;
                    //    }
                    //    catch (Exception ex)
                    //    {

                    //    }
                    //}
                    //else
                    //{
                    //    entity.ImageURL = _SiteURL + "/ProfileImage/opozee-profile.png";
                    //}
                    db.Entry(entity).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    int userID = entity.UserID;
                    entity = db.Users.Find(userID);
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, entity, "UserData"));
                }
                else
                {
                    entity = new User();
                    Token token = new Token();
                    entity.UserName = input.UserName;
                    entity.FirstName = input.FirstName;
                    entity.LastName = input.LastName;
                    entity.Email = input.Email;

                    bool Email = false;
                    Email = OpozeeLibrary.Utilities.Helper.IsValidEmail(input.Email);
                    if (!string.IsNullOrEmpty(input.Password))
                    {
                        entity.Password = AesCryptography.Encrypt(input.Password);
                    }

                    entity.DeviceType = input.DeviceType;
                    entity.DeviceToken = input.DeviceToken;
                    entity.CreatedDate = DateTime.Now;
                    entity.RecordStatus = RecordStatus.Active.ToString();
                    entity.SocialID = input.ThirdPartyId;
                    if (input.ThirdPartyType == ThirdPartyType.Facebook)
                    {
                        entity.SocialType = ThirdPartyType.Facebook.ToString();
                    }
                    else if (input.ThirdPartyType == ThirdPartyType.GooglePlus)
                    {
                        entity.SocialType = ThirdPartyType.GooglePlus.ToString();
                    }
                    else if (input.ThirdPartyType == ThirdPartyType.Twitter)
                    {
                        entity.SocialType = ThirdPartyType.Twitter.ToString();
                    }

                    //if (input.ImageURL != null && input.ImageURL != "")
                    //{
                    //    try
                    //    {
                    //        string strTempImageSave = OpozeeLibrary.Utilities.ResizeImage.Download_Image(input.ImageURL);
                    //        string profileFilePath = _SiteURL + "/ProfileImage/" + strTempImageSave;
                    //        strIamgeURLfordb = profileFilePath;
                    //        entity.ImageURL = profileFilePath;
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        strThumbnailURLfordb = strThumbnailImage;
                    //        strIamgeURLfordb = strThumbnailImage;
                    //    }
                    //}
                    //else
                    //{

                    //    entity.ImageURL = _SiteURL + "/ProfileImage/opozee-profile.png";
                    //    strIamgeURLfordb = entity.ImageURL;
                    //}

                    entity.ImageURL = "https://opozee.com:81/Content/Upload/ProfileImage/opozee-profile.png";
                    entity.ImageURL = strIamgeURLfordb;
                    db.Users.Add(entity);
                    db.SaveChanges();

                    int userID = entity.UserID;
                    token.TotalToken = 500;
                    token.BalanceToken = 500;
                    token.UserId = userID;
                    db.Tokens.Add(token);
                    db.SaveChanges();
                    entity = db.Users.Find(userID);

                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, entity, "UserData"));
                }
            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "UserData"));
            }
        }
        #endregion

        #region "Post Question" 
        [HttpPost]
        [Route("api/MobileApi/PostQuestion")]
        public HttpResponseMessage PostQuestion([FromBody] Question postQuestion)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }
                Question quest = null;
                PushNotifications pushNotifications = new PushNotifications();
                quest = db.Questions.Where(p => p.Id == postQuestion.Id
                                       ).FirstOrDefault();
                if (quest != null)
                {
                    quest.PostQuestion = postQuestion.PostQuestion;
                    quest.OwnerUserID = postQuestion.OwnerUserID;
                    quest.HashTags = postQuestion.HashTags;
                    quest.TaggedUser = postQuestion.TaggedUser;
                    quest.IsDeleted = false;
                    quest.ModifiedDate = DateTime.Now;
                    quest.IsSlider = false;
                    db.Entry(quest).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    int questID = quest.Id;
                    quest = db.Questions.Find(questID);
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, quest, "Question"));
                }
                else
                {
                    quest = new Question();
                    Token token = new Token();
                    quest.PostQuestion = postQuestion.PostQuestion;
                    quest.OwnerUserID = postQuestion.OwnerUserID;
                    quest.HashTags = postQuestion.HashTags;
                    quest.TaggedUser = postQuestion.TaggedUser;
                    quest.IsDeleted = false;
                    quest.IsSlider = false;
                    quest.CreationDate = DateTime.Now;
                    db.Questions.Add(quest);
                    db.SaveChanges();
                    //token = db.Tokens.Where(p => p.UserId == postQuestion.OwnerUserID).FirstOrDefault();
                    //token.BalanceToken = token.BalanceToken - 1;

                    //db.Entry(token).State = System.Data.Entity.EntityState.Modified;
                    //db.SaveChanges();
                    User questionOwner = db.Users.Find(quest.OwnerUserID);
                    int questID = quest.Id;
                    quest = db.Questions.Find(questID);
                    string taggedUser = postQuestion.TaggedUser;

                    if (!string.IsNullOrEmpty(taggedUser))
                    {
                        var roleIds = taggedUser.Split(',').Select(s => int.Parse(s));
                        foreach (int items in roleIds)
                        {
                            User data = db.Users.Find(items);
                            if (data != null)
                            {
                                string finalMessage = questionOwner.UserName + " has tagged you in question";

                                pushNotifications.SendNotification_Android(data.DeviceToken, finalMessage, "QD", questID.ToString());
                            }
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, quest, "Question"));
                }
            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "Question"));
            }
        }
        #endregion

        #region "Post Answer" 
        [HttpPost]
        [Route("api/MobileApi/PostAnswer")]
        public HttpResponseMessage PostAnswer([FromBody] PostAnswer postAnswer)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }
                Opinion opinion = null;
                Notification notification = null;
                opinion = db.Opinions.Where(p => p.Id == postAnswer.Id).FirstOrDefault();
                if (opinion != null)
                {
                    opinion.Comment = postAnswer.Comment;
                    opinion.QuestId = postAnswer.QuestId;
                    opinion.CommentedUserId = postAnswer.CommentedUserId;
                    opinion.ModifiedDate = DateTime.Now;
                    db.Entry(opinion).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    int opinionID = opinion.Id;
                    opinion = db.Opinions.Find(opinionID);
                    notification = db.Notifications.Where(p => p.CommentedUserId == postAnswer.CommentedUserId && p.CommentId == opinionID).FirstOrDefault();
                    if (notification != null)
                    {
                        notification.CommentedUserId = postAnswer.CommentedUserId;
                        notification.CommentId = opinionID;
                        notification.questId = postAnswer.QuestId;
                        notification.ModifiedDate = DateTime.Now;
                        db.Entry(notification).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        notification = new Notification();
                        notification.CommentedUserId = postAnswer.CommentedUserId;
                        notification.CommentId = opinionID;
                        notification.questId = postAnswer.QuestId;
                        notification.CreationDate = DateTime.Now;
                        db.Notifications.Add(notification);
                        db.SaveChanges();
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, opinion, "Opinion"));
                }
                else
                {
                    PushNotifications pushNotifications = new PushNotifications();
                    opinion = new Opinion();
                    Token token = new Token();
                    opinion.Comment = postAnswer.Comment;
                    opinion.QuestId = postAnswer.QuestId;
                    opinion.CommentedUserId = postAnswer.CommentedUserId;
                    opinion.IsAgree = postAnswer.OpinionAgreeStatus;

                    opinion.CreationDate = DateTime.Now;
                    db.Opinions.Add(opinion);
                    db.SaveChanges();
                    //token = db.Tokens.Where(p => p.UserId == postAnswer.CommentedUserId).FirstOrDefault();
                    //token.BalanceToken = token.BalanceToken - 1;

                    //db.Entry(token).State = System.Data.Entity.EntityState.Modified;
                    //db.SaveChanges();
                    int opinionID = opinion.Id;
                    opinion = db.Opinions.Find(opinionID);
                    notification = db.Notifications.Where(p => p.CommentedUserId == postAnswer.CommentedUserId && p.CommentId == opinionID).FirstOrDefault();
                    Question quest = db.Questions.Find(postAnswer.QuestId);
                    User questOwner = db.Users.Where(u => u.UserID == quest.OwnerUserID).FirstOrDefault();
                    User user = db.Users.Where(u => u.UserID == postAnswer.CommentedUserId).FirstOrDefault();
                    if (questOwner != null)
                    {
                        if (quest.OwnerUserID != postAnswer.CommentedUserId)
                        {
                            //***** Notification to question owner
                            string finalMessage = GenerateTagsForQuestion(false, false, true, user.UserName);

                            pushNotifications.SendNotification_Android(questOwner.DeviceToken, finalMessage, "QD", postAnswer.QuestId.ToString());
                            //***** Notification to Tagged Users
                            string taggedUser = quest.TaggedUser;

                            if (!string.IsNullOrEmpty(taggedUser))
                            {
                                var roleIds = taggedUser.Split(',').Select(s => int.Parse(s));
                                foreach (int items in roleIds)
                                {
                                    if (postAnswer.CommentedUserId != items)
                                    {
                                        User data = db.Users.Find(items);
                                        if (data != null)
                                        {
                                            string finalMessage1 = user.UserName + " has given opinion on question in which you're tagged in.";

                                            pushNotifications.SendNotification_Android(data.DeviceToken, finalMessage1, "QD", postAnswer.QuestId.ToString());
                                        }
                                    }
                                }
                            }
                        }
                        else if (quest.OwnerUserID == postAnswer.CommentedUserId)
                        {
                            //in this block notification will send to tagged users
                            string taggedUser = quest.TaggedUser;

                            if (!string.IsNullOrEmpty(taggedUser))
                            {
                                var roleIds = taggedUser.Split(',').Select(s => int.Parse(s));
                                foreach (int items in roleIds)
                                {
                                    User data = db.Users.Find(items);
                                    if (data != null)
                                    {
                                        string finalMessage = user.UserName + " has given opinion on question in which you're tagged in.";

                                        pushNotifications.SendNotification_Android(data.DeviceToken, finalMessage, "QD", postAnswer.QuestId.ToString());
                                    }
                                }
                            }
                        }
                    }

                    if (notification != null)
                    {
                        notification.CommentedUserId = postAnswer.CommentedUserId;
                        notification.CommentId = opinionID;
                        notification.questId = postAnswer.QuestId;
                        notification.ModifiedDate = DateTime.Now;
                        db.Entry(notification).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        notification = new Notification();
                        notification.CommentedUserId = postAnswer.CommentedUserId;
                        notification.CommentId = opinionID;
                        notification.Comment = true;
                        notification.questId = postAnswer.QuestId;
                        notification.CreationDate = DateTime.Now;
                        db.Notifications.Add(notification);
                        db.SaveChanges();
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, opinion, "Opinion"));
                }
            }

            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "Opinion"));
            }
        }

        #endregion

        public string GenerateTagsForQuestion(bool? like, bool? dislike, bool? comment, string UserName)
        {
            string Tag = "";
            if (like == true && dislike == false && comment == false)
            {
                Tag = UserName + " has liked your question's opinion.";
            }
            else if (dislike == true && like == false && comment == false)
            {
                Tag = UserName + " has disliked your question's opinion.";
            }
            else if (comment == true && like == false && dislike == false)
            {
                Tag = UserName + " has given opinion on your question.";
            }

            return Tag;
        }
        public string GenerateTagsForOpinion(bool? like, bool? dislike, bool? comment, string UserName)
        {
            string Tag = "";
            if (like == true && dislike == false && comment == false)
            {
                Tag = UserName + " has liked your opinion.";
            }
            else if (dislike == true && like == false && comment == false)
            {
                Tag = UserName + " has disliked your opinion.";
            }
            else if (comment == true && like == false && dislike == false)
            {
                Tag = UserName + " has given opinion on your question.";
            }

            return Tag;
        }

        public string GenerateTagsForTaggedUsers(bool? like, bool? dislike, bool? comment, string ActionUserName)
        {
            string Tag = "";
            if (like == true && dislike == false && comment == false)
            {
                Tag = ActionUserName + " has liked question's opinion in which you're tagged in";
            }
            else if (dislike == true && like == false && comment == false)
            {
                Tag = ActionUserName + " has disliked question's opinion in which you're tagged in";
            }
            else if (comment == true && like == false && dislike == false)
            {
                Tag = ActionUserName + " has given opinion on question in which you're tagged in";
            }
            //else if (like == true && dislike == false && comment == true)
            //{
            //    Tag = UserName + " Has Liked and given opinion on your Question.";
            //}
            //else if (dislike == true && like == false && comment == true)
            //{
            //    Tag = UserName + " Has Disliked and given opinion on your Question.";
            //}

            return Tag;
        }

        #region "Get All Opinion by question Id" 
        [HttpGet]
        [Route("api/MobileApi/GetAllOpinion")]
        public HttpResponseMessage GetAllOpinion(string questId, string userid, int Pageindex, int Pagesize)
        {
            try
            {
                using (OpozeeDbEntities db = new OpozeeDbEntities())
                {
                    if (!ModelState.IsValid)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                    }
                    BookMarkQuestionMobile questionDetail = new BookMarkQuestionMobile();
                    int id = Convert.ToInt32(questId);
                    int userId = Convert.ToInt32(userid);
                    //                  title == null ?
                    //items.Where(x => x.Title == null) : items.Where(x => x.Title == title);
                    string taggedUser = db.Questions.Find(id).TaggedUser;
                    IEnumerable<int> roleIds = null;
                    if (!string.IsNullOrEmpty(taggedUser))
                    {
                        roleIds = db.Questions.Find(id).TaggedUser.Split(',').Select(s => int.Parse(s));
                    }
                    else
                    {
                        roleIds = new int[] { };
                    }
                    questionDetail.PostQuestionDetail = (from q in db.Questions
                                                         join u in db.Users on q.OwnerUserID equals u.UserID
                                                         where q.Id == id && q.IsDeleted == false
                                                         select new BookMarkQuestionDetailMobile
                                                         {
                                                             Id = q.Id,
                                                             Question = q.PostQuestion,
                                                             OwnerUserID = q.OwnerUserID,
                                                             OwnerUserName = u.UserName,
                                                             UserImage = string.IsNullOrEmpty(u.ImageURL) ? "" : u.ImageURL,
                                                             HashTags = q.HashTags,
                                                             Name = u.FirstName + " " + u.LastName,
                                                             IsBookmark = db.BookMarks.Where(b => b.UserId == userId && b.QuestionId == id).Select(b => b.IsBookmark.HasValue ? b.IsBookmark.Value : false).FirstOrDefault(),
                                                             IsSlider = q.IsSlider,
                                                             IsUserPosted = db.Opinions.Any(cus => cus.CommentedUserId == userId && cus.QuestId == id),
                                                             TotalLikes = db.Notifications.Where(o => o.questId == q.Id && o.Like == true).Count(),
                                                             TotalDisLikes = db.Notifications.Where(o => o.questId == q.Id && o.Dislike == true).Count(),
                                                             TaggedUsers = db.Users.Where(k => roleIds.Contains(k.UserID)).Select(p => p.FirstName + " " + p.LastName).AsQueryable(),
                                                             YesCount = db.Opinions.Where(o => o.QuestId == q.Id && o.IsAgree == true).Count(),
                                                             NoCount = db.Opinions.Where(o => o.QuestId == q.Id && o.IsAgree == false).Count(),
                                                             CreationDate = q.CreationDate,
                                                             ReactionSum = (db.Opinions.Where(o => o.QuestId == q.Id && o.IsAgree == true).Count()
                                                                           + db.Opinions.Where(o => o.QuestId == q.Id && o.IsAgree == false).Count()
                                                                           + db.Notifications.Where(o => o.questId == q.Id && o.Like == true).Count()
                                                                           + db.Notifications.Where(o => o.questId == q.Id && o.Dislike == true).Count()),
                                                             LastActivityTime = (DateTime?)(db.Notifications.Where(o => o.questId == q.Id).Max(b => b.CreationDate)),
                                                         }).FirstOrDefault();

                    questionDetail.Comments = (from e in db.Opinions
                                               join t in db.Users on e.CommentedUserId equals t.UserID
                                               where e.QuestId == id
                                               select new Comments
                                               {
                                                   Id = e.Id,
                                                   Comment = e.Comment,
                                                   CommentedUserId = t.UserID,
                                                   Name = t.FirstName + " " + t.LastName,
                                                   UserImage = string.IsNullOrEmpty(t.ImageURL) ? "" : t.ImageURL,
                                                   LikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Like == true).Count(),
                                                   DislikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Dislike == true).Count(),
                                                   Likes = db.Notifications.Where(p => p.CommentedUserId == userId && p.CommentId == e.Id).Select(b => b.Like.HasValue ? b.Like.Value : false).FirstOrDefault(),
                                                   DisLikes = db.Notifications.Where(p => p.CommentedUserId == userId && p.CommentId == e.Id).Select(b => b.Dislike.HasValue ? b.Dislike.Value : false).FirstOrDefault(),
                                                   CommentedUserName = t.UserName,
                                                   IsAgree = e.IsAgree,
                                                   CreationDate = e.CreationDate,

                                                   LikesThoughtfulCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Like == true && (p.ReactionType == 1 || p.ReactionType == null)).Count(),
                                                   LikesFactualCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Like == true && p.ReactionType == 2).Count(),
                                                   LikesFunnyCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Like == true && p.ReactionType == 3).Count(),
                                                   DislikesIrrationalCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Dislike == true && (p.ReactionType == 4 || p.ReactionType == null)).Count(),
                                                   DislikesFakeNewsCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Dislike == true && p.ReactionType == 5).Count(),
                                                   DislikesOffTopicCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Dislike == true && p.ReactionType == 6).Count(),
                                                   SubReaction = db.Notifications.Where(p => p.CommentedUserId == userId && p.CommentId == e.Id).FirstOrDefault() == null ? 0 : db.Notifications.Where(p => p.CommentedUserId == userId && p.CommentId == e.Id).FirstOrDefault().ReactionType ?? 0
                                               }).OrderByDescending(p => p.CreationDate).ToPagedList(Pageindex - 1, Pagesize).ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, questionDetail, "AllOpinion"));
                }
            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "AllOpinion"));
            }
        }
        #endregion

        #region "Get All Opinion by User Id" 
        [HttpGet]
        [Route("api/MobileApi/GetAllPostsByUserId")]
        public HttpResponseMessage GetAllPostsByUserId(string UserID, int Pageindex, int Pagesize)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }
                AllUserQuestions questionDetail = new AllUserQuestions();
                int id = Convert.ToInt32(UserID);

                questionDetail.PostQuestionDetail = (from q in db.Questions
                                                     join u in db.Users on q.OwnerUserID equals u.UserID
                                                     where u.UserID == id && q.IsDeleted == false
                                                     select new PostQuestionDetail
                                                     {
                                                         Id = q.Id,
                                                         Question = q.PostQuestion,
                                                         OwnerUserID = q.OwnerUserID,
                                                         OwnerUserName = u.UserName,
                                                         Name = u.FirstName + " " + u.LastName,
                                                         UserImage = string.IsNullOrEmpty(u.ImageURL) ? "" : u.ImageURL,
                                                         HashTags = q.HashTags,
                                                         TotalLikes = db.Notifications.Where(o => o.questId == q.Id && o.Like == true).Count(),
                                                         TotalDisLikes = db.Notifications.Where(o => o.questId == q.Id && o.Dislike == true).Count(),
                                                         YesCount = db.Opinions.Where(o => o.QuestId == q.Id && o.IsAgree == true).Count(),
                                                         NoCount = db.Opinions.Where(o => o.QuestId == q.Id && o.IsAgree == false).Count(),
                                                         CreationDate = q.CreationDate,
                                                         IsSlider = q.IsSlider

                                                     }).OrderByDescending(p => p.CreationDate).ToPagedList(Pageindex - 1, Pagesize).ToList();


                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, questionDetail, "AllUserQuestions"));
            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "AllUserQuestions"));
            }
        }

        #endregion





        #region "Get All Posts" 
        [HttpGet]
        [Route("api/MobileApi/GetAllPosts")]
        public HttpResponseMessage GetAllPosts(int Pageindex, int Pagesize)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }
                AllUserQuestions questionDetail = new AllUserQuestions();
                //int id = Convert.ToInt32(UserID);

                questionDetail.PostQuestionDetail = (from q in db.Questions
                                                     join u in db.Users on q.OwnerUserID equals u.UserID
                                                     where q.IsDeleted == false
                                                     select new PostQuestionDetail
                                                     {
                                                         Id = q.Id,
                                                         Question = q.PostQuestion,
                                                         OwnerUserID = q.OwnerUserID,
                                                         OwnerUserName = u.UserName,
                                                         Name = u.FirstName + " " + u.LastName,
                                                         UserImage = string.IsNullOrEmpty(u.ImageURL) ? "" : u.ImageURL,
                                                         HashTags = q.HashTags,
                                                         CreationDate = q.CreationDate,
                                                         IsSlider = q.IsSlider,
                                                         YesCount = db.Opinions.Where(o => o.QuestId == q.Id && o.IsAgree == true).Count(),
                                                         NoCount = db.Opinions.Where(o => o.QuestId == q.Id && o.IsAgree == false).Count(),
                                                         TotalLikes = db.Notifications.Where(o => o.questId == q.Id && o.Like == true).Count(),
                                                         TotalDisLikes = db.Notifications.Where(o => o.questId == q.Id && o.Dislike == true).Count(),
                                                     }).OrderByDescending(p => p.CreationDate).ToPagedList(Pageindex - 1, Pagesize).ToList();


                foreach (var data in questionDetail.PostQuestionDetail)
                {
                    var opinionList = db.Opinions.Where(p => p.QuestId == data.Id).ToList();
                    if (opinionList.Count > 0)
                    {

                        int? maxYesLike = opinionList.Where(p => p.IsAgree == true).Max(i => i.Likes);
                        int? maxNoLike = opinionList.Where(p => p.IsAgree == false).Max(i => i.Likes);
                        //int? maxDislike = opinionList.Max(i => i.Dislikes);
                        if (maxYesLike != null && maxYesLike > 0)
                        {
                            data.MostYesLiked = (from e in db.Opinions
                                                 join t in db.Users on e.CommentedUserId equals t.UserID
                                                 join n in db.Notifications on e.QuestId equals n.questId
                                                 where e.IsAgree == true && e.QuestId == data.Id && n.Like == true
                                                 select new Comments
                                                 {
                                                     Id = e.Id,
                                                     Comment = e.Comment,
                                                     CommentedUserId = t.UserID,
                                                     Name = t.FirstName + " " + t.LastName,
                                                     UserImage = string.IsNullOrEmpty(t.ImageURL) ? "" : t.ImageURL,
                                                     LikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Like == true).Count(),
                                                     DislikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Dislike == true).Count(),
                                                     CommentedUserName = t.UserName,
                                                     CreationDate = e.CreationDate
                                                 }).OrderByDescending(s => s.LikesCount).ThenByDescending(s => s.CreationDate).First();

                        }
                        if (maxNoLike != null && maxNoLike > 0)
                        {
                            data.MostNoLiked = (from e in db.Opinions
                                                join t in db.Users on e.CommentedUserId equals t.UserID
                                                join n in db.Notifications on e.QuestId equals n.questId
                                                where e.IsAgree == false && e.QuestId == data.Id && n.Like == true
                                                select new Comments
                                                {
                                                    Id = e.Id,
                                                    Comment = e.Comment,
                                                    CommentedUserId = t.UserID,
                                                    Name = t.FirstName + " " + t.LastName,
                                                    UserImage = string.IsNullOrEmpty(t.ImageURL) ? "" : t.ImageURL,
                                                    LikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Like == true).Count(),
                                                    DislikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Dislike == true).Count(),
                                                    CommentedUserName = t.UserName,
                                                    CreationDate = e.CreationDate
                                                }).OrderByDescending(s => s.LikesCount).ThenByDescending(s => s.CreationDate).First();
                        }
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, questionDetail, "AllUserQuestions"));
            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "AllUserQuestions"));
            }
        }
        #endregion



        [HttpGet]
        [Route("api/MobileApi/GetAllPostsMobile")]
        public HttpResponseMessage GetAllPostsMobile(int userId, int Pageindex, int Pagesize, int Sort = 1, string Search = "")
        {
            //List<PostQuestionDetailWebModel> questionDetail = new List<PostQuestionDetailWebModel>();
            AllUserQuestionsMobile questionDetail = new AllUserQuestionsMobile();
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }

                Search = string.IsNullOrEmpty(Search) ? "" : Search.ToLower() == "all" ? "" : Search;

                questionDetail.PostQuestionDetail = (from q in db.Questions
                                                     join u in db.Users on q.OwnerUserID equals u.UserID
                                                     where q.IsDeleted == false && q.HashTags.Contains(Search)
                                                     select new PostQuestionDetailMobile
                                                     {
                                                         Id = q.Id,
                                                         Question = q.PostQuestion,
                                                         OwnerUserID = q.OwnerUserID,
                                                         OwnerUserName = u.UserName,
                                                         Name = u.FirstName + " " + u.LastName,
                                                         UserImage = string.IsNullOrEmpty(u.ImageURL) ? "" : u.ImageURL,
                                                         HashTags = q.HashTags,
                                                         CreationDate = q.CreationDate,
                                                         IsSlider = q.IsSlider,
                                                         YesCount = db.Opinions.Where(o => o.QuestId == q.Id && o.IsAgree == true).Count(),
                                                         NoCount = db.Opinions.Where(o => o.QuestId == q.Id && o.IsAgree == false).Count(),
                                                         TotalLikes = db.Notifications.Where(o => o.questId == q.Id && o.Like == true).Count(),
                                                         TotalDisLikes = db.Notifications.Where(o => o.questId == q.Id && o.Dislike == true).Count(),

                                                         ReactionSum = (db.Opinions.Where(o => o.QuestId == q.Id && o.IsAgree == true).Count()
                                                                 + db.Opinions.Where(o => o.QuestId == q.Id && o.IsAgree == false).Count()
                                                                 + db.Notifications.Where(o => o.questId == q.Id && o.Like == true).Count()
                                                                 + db.Notifications.Where(o => o.questId == q.Id && o.Dislike == true).Count()),

                                                         //TotalRecordcount = 1,
                                                         LastActivityTime = (DateTime?)(db.Notifications.Where(o => o.questId == q.Id).Max(b => b.CreationDate)),

                                                     }).ToList(); //.OrderByDescending(p => p.LastActivityTime).Skip(skip).Take(pageSize).ToList();



                if (Sort == 0) //sort by last reaction time
                {
                    questionDetail.PostQuestionDetail = questionDetail.PostQuestionDetail.OrderByDescending(p => p.LastActivityTime).ToPagedList(Pageindex - 1, Pagesize).ToList();
                }
                else if (Sort == 1) //sort by most reactions
                {

                    questionDetail.PostQuestionDetail = questionDetail.PostQuestionDetail.OrderByDescending(p => p.ReactionSum).ToPagedList(Pageindex - 1, Pagesize).ToList();
                }

                else if (Sort == 2)// sort by least reactions
                {
                    questionDetail.PostQuestionDetail = questionDetail.PostQuestionDetail.OrderBy(p => p.ReactionSum).ToPagedList(Pageindex - 1, Pagesize).ToList();

                }

                foreach (var data in questionDetail.PostQuestionDetail)
                {
                    var opinionList = db.Opinions.Where(p => p.QuestId == data.Id).ToList();
                    if (opinionList.Count > 0)
                    {

                        int? maxYesLike = opinionList.Where(p => p.IsAgree == true).Max(i => i.Likes);
                        int? maxNoLike = opinionList.Where(p => p.IsAgree == false).Max(i => i.Likes);
                        //int? maxDislike = opinionList.Max(i => i.Dislikes);
                        if (maxYesLike != null && maxYesLike > 0)
                        {
                            data.MostYesLiked = (from e in db.Opinions
                                                 join t in db.Users on e.CommentedUserId equals t.UserID
                                                 join n in db.Notifications on e.QuestId equals n.questId
                                                 where e.IsAgree == true && e.QuestId == data.Id && n.Like == true
                                                 select new Comments
                                                 {
                                                     Id = e.Id,
                                                     Comment = e.Comment,
                                                     CommentedUserId = t.UserID,
                                                     Name = t.FirstName + " " + t.LastName,
                                                     UserImage = string.IsNullOrEmpty(t.ImageURL) ? "" : t.ImageURL,
                                                     IsAgree = e.IsAgree,
                                                     LikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Like == true).Count(),
                                                     DislikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Dislike == true).Count(),
                                                     CommentedUserName = t.UserName,
                                                     CreationDate = e.CreationDate
                                                 }).OrderByDescending(s => s.LikesCount).ThenByDescending(s => s.CreationDate).FirstOrDefault();

                        }
                        else if (maxYesLike != null)
                        {
                            // get the latest opinion with no votes
                            data.MostYesLiked = (from e in db.Opinions
                                                 join t in db.Users on e.CommentedUserId equals t.UserID
                                                 join n in db.Notifications on e.QuestId equals n.questId
                                                 where e.IsAgree == true && e.QuestId == data.Id
                                                 select new Comments
                                                 {
                                                     Id = e.Id,
                                                     Comment = e.Comment,
                                                     CommentedUserId = t.UserID,
                                                     Name = t.FirstName + " " + t.LastName,
                                                     UserImage = string.IsNullOrEmpty(t.ImageURL) ? "" : t.ImageURL,
                                                     IsAgree = e.IsAgree,
                                                     LikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Like == true).Count(),
                                                     DislikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Dislike == true).Count(),
                                                     CommentedUserName = t.UserName,
                                                     CreationDate = e.CreationDate
                                                 }).OrderByDescending(s => s.CreationDate).FirstOrDefault();

                        }

                        if (maxNoLike != null && maxNoLike > 0)
                        {
                            data.MostNoLiked = (from e in db.Opinions
                                                join t in db.Users on e.CommentedUserId equals t.UserID
                                                join n in db.Notifications on e.QuestId equals n.questId
                                                where e.IsAgree == false && e.QuestId == data.Id && n.Like == true
                                                select new Comments
                                                {
                                                    Id = e.Id,
                                                    Comment = e.Comment,
                                                    CommentedUserId = t.UserID,
                                                    Name = t.FirstName + " " + t.LastName,
                                                    UserImage = string.IsNullOrEmpty(t.ImageURL) ? "" : t.ImageURL,
                                                    IsAgree = e.IsAgree,
                                                    LikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Like == true).Count(),
                                                    DislikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Dislike == true).Count(),
                                                    CommentedUserName = t.UserName,
                                                    CreationDate = e.CreationDate
                                                }).OrderByDescending(s => s.LikesCount).ThenByDescending(s => s.CreationDate).FirstOrDefault();
                        }
                        else if (maxNoLike != null)
                        {
                            data.MostNoLiked = (from e in db.Opinions
                                                join t in db.Users on e.CommentedUserId equals t.UserID
                                                join n in db.Notifications on e.QuestId equals n.questId
                                                where e.IsAgree == false && e.QuestId == data.Id
                                                select new Comments
                                                {
                                                    Id = e.Id,
                                                    Comment = e.Comment,
                                                    CommentedUserId = t.UserID,
                                                    Name = t.FirstName + " " + t.LastName,
                                                    UserImage = string.IsNullOrEmpty(t.ImageURL) ? "" : t.ImageURL,
                                                    IsAgree = e.IsAgree,
                                                    LikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Like == true).Count(),
                                                    DislikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Dislike == true).Count(),
                                                    CommentedUserName = t.UserName,
                                                    CreationDate = e.CreationDate
                                                }).OrderByDescending(s => s.CreationDate).FirstOrDefault();
                        }
                    }
                }
                var data1 = questionDetail;

                // return questionDetail; //.OrderByDescending(p=>p.LastActivityTime);
                return Request.CreateResponse(JsonResponse.GetResponse(ResponseCode.Success, questionDetail, "AllUserQuestions"));
            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "AllUserQuestions"));
                //return questionDetail;
            }
        }


        #region "Like Dislike Opinion" 
        [HttpPost]
        [Route("api/MobileApi/LikeDislikeOpinion")]
        public HttpResponseMessage LikeDislikeOpinion([FromBody] LikeDislike likeDislike)
        {
            try
            {
                int token_score = 0;
                db.Configuration.LazyLoadingEnabled = false;
                string action = "";
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }
                Notification notification = null;
                PushNotifications pNoty = new PushNotifications();
                notification = db.Notifications.Where(p => p.CommentedUserId == likeDislike.CommentedUserId && p.CommentId == likeDislike.CommentId).FirstOrDefault();
                if (notification != null)
                {
                    if (likeDislike.CommentStatus == CommentStatus.DisLike)
                    {
                        notification.Dislike = true;
                        notification.Like = false;
                        action = "dislike";
                        token_score = -1;
                    }
                    else if (likeDislike.CommentStatus == CommentStatus.Like)
                    {
                        notification.Like = true;
                        notification.Dislike = false;
                        action = "like";
                        token_score = 1;
                    }
                    if (likeDislike.CommentStatus == CommentStatus.RemoveLike)
                    {
                        notification.Like = false;
                        action = "remove like";
                        token_score = -1;
                    }
                    else if (likeDislike.CommentStatus == CommentStatus.RemoveDisLike)
                    {
                        notification.Dislike = false;
                        action = "remove dislike";
                        token_score = 1;
                    }
                    notification.CommentedUserId = likeDislike.CommentedUserId;
                    notification.CommentId = likeDislike.CommentId;
                    notification.questId = likeDislike.questId;
                    notification.ReactionType = likeDislike.ReactionType;
                    notification.ModifiedDate = DateTime.UtcNow;
                    db.Entry(notification).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();


                    List<Opinion> opinion = db.Opinions.Where(p => p.Id == likeDislike.CommentId).ToList();
                    int questId = opinion[0].QuestId;
                    Question ques = db.Questions.Where(p => p.Id == questId).FirstOrDefault();
                    User questOwner = db.Users.Where(u => u.UserID == ques.OwnerUserID).FirstOrDefault();
                    User user = db.Users.Where(u => u.UserID == notification.CommentedUserId).FirstOrDefault();
                    int OpinionUserID = opinion[0].CommentedUserId;
                    User commentOwner = db.Users.Where(u => u.UserID == OpinionUserID).FirstOrDefault();
                    if (questOwner != null && (!action.Contains("remove")))
                    {
                        if (ques.OwnerUserID != notification.CommentedUserId)
                        {
                            //***** Notification to question owner
                            string finalMessage = GenerateTagsForQuestion(notification.Like, notification.Dislike, false, user.UserName);

                            pNoty.SendNotification_Android(questOwner.DeviceToken, finalMessage, "QD", questId.ToString());

                            //***** Notification to Tagged Users
                            string taggedUser = ques.TaggedUser;

                            if (!string.IsNullOrEmpty(taggedUser))
                            {
                                var roleIds = taggedUser.Split(',').Select(s => int.Parse(s));
                                foreach (int items in roleIds)
                                {
                                    if (notification.CommentedUserId != items)
                                    {
                                        User data = db.Users.Find(items);
                                        if (data != null)
                                        {
                                            string finalMessage1 = user.UserName + " has " + action + " question in which you're tagged in.";

                                            pNoty.SendNotification_Android(data.DeviceToken, finalMessage1, "QD", questId.ToString());
                                        }
                                    }
                                }
                            }
                        }
                        else if (ques.OwnerUserID == notification.CommentedUserId)
                        {
                            //in this block notification will send to tagged users
                            string taggedUser = ques.TaggedUser;

                            if (!string.IsNullOrEmpty(taggedUser))
                            {
                                var roleIds = taggedUser.Split(',').Select(s => int.Parse(s));
                                foreach (int items in roleIds)
                                {
                                    User data = db.Users.Find(items);
                                    if (data != null)
                                    {
                                        string finalMessage = user.UserName + " has " + action + " question in which you're tagged in.";

                                        pNoty.SendNotification_Android(data.DeviceToken, finalMessage, "QD", questId.ToString());
                                    }
                                }
                            }
                        }
                        if (commentOwner.UserID != notification.CommentedUserId)
                        {
                            //***** Notification to question owner
                            string finalMessage = GenerateTagsForOpinion(notification.Like, notification.Dislike, false, user.UserName);

                            pNoty.SendNotification_Android(commentOwner.DeviceToken, finalMessage, "QD", questId.ToString());
                        }
                    }
                    foreach (Opinion orderID in opinion)
                    {
                        orderID.Likes = db.Notifications.Where(p => p.CommentId == orderID.Id && p.Like == true).Count();
                        orderID.Dislikes = db.Notifications.Where(p => p.CommentId == orderID.Id && p.Dislike == true).Count();
                        db.Entry(orderID).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        //give or take tokens 
                        Token userToken = db.Tokens.Where(x => x.UserId == orderID.CommentedUserId).FirstOrDefault();
                        userToken.BalanceToken = userToken.BalanceToken + token_score;
                        db.Entry(userToken).State = System.Data.Entity.EntityState.Modified;
                    }


                    int questID = notification.Id;
                    notification = db.Notifications.Find(questID);
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, notification, "LikeDislikeOpinion"));
                }
                else
                {
                    notification = new Notification();

                    PushNotifications pushNotifications = new PushNotifications();

                    if (likeDislike.CommentStatus == CommentStatus.DisLike)
                    {
                        notification.Dislike = true;
                        notification.Like = false;
                        action = "dislike";
                        token_score = -1;
                    }
                    else if (likeDislike.CommentStatus == CommentStatus.Like)
                    {
                        notification.Like = true;
                        notification.Dislike = false;
                        action = "like";
                        token_score = 1;
                    }
                    if (likeDislike.CommentStatus == CommentStatus.RemoveLike)
                    {
                        notification.Like = false;
                        action = "remove like";
                        token_score = -1;
                    }
                    else if (likeDislike.CommentStatus == CommentStatus.RemoveDisLike)
                    {
                        notification.Dislike = false;
                        action = "remove disLike";
                        token_score = +1;
                    }
                    notification.CommentedUserId = likeDislike.CommentedUserId;
                    notification.CommentId = likeDislike.CommentId;
                    notification.questId = likeDislike.questId;
                    notification.CreationDate = DateTime.UtcNow;
                    notification.ReactionType = likeDislike.ReactionType;
                    db.Notifications.Add(notification);
                    db.SaveChanges();
                    int questID = notification.Id;
                    notification = db.Notifications.Find(questID);

                    List<Opinion> op = db.Opinions.Where(p => p.Id == likeDislike.CommentId).ToList();
                    int questId = op[0].QuestId;
                    Question ques = db.Questions.Where(p => p.Id == questId).FirstOrDefault();
                    User questOwner = db.Users.Where(u => u.UserID == ques.OwnerUserID).FirstOrDefault();
                    int OpinionUserID = op[0].CommentedUserId;
                    User commentOwner = db.Users.Where(u => u.UserID == OpinionUserID).FirstOrDefault();

                    User user = db.Users.Where(u => u.UserID == notification.CommentedUserId).FirstOrDefault();
                    //if (questOwner != null)
                    //{
                    //    string finalMessage = GenerateTagsForQuestion(notification.Like, notification.Dislike, false, user.FirstName + " " + user.LastName);

                    //    pNoty.SendNotification_Android(questOwner.DeviceToken, finalMessage, "QD", questId.ToString());
                    //    //updateCount = true;
                    //}
                    if (questOwner != null && (!action.Contains("remove")))
                    {
                        if (ques.OwnerUserID != notification.CommentedUserId)
                        {
                            //***** Notification to question owner
                            string finalMessage = GenerateTagsForQuestion(notification.Like, notification.Dislike, false, user.UserName);

                            pushNotifications.SendNotification_Android(questOwner.DeviceToken, finalMessage, "QD", questId.ToString());
                            //***** Notification to Tagged Users
                            string taggedUser = ques.TaggedUser;

                            if (!string.IsNullOrEmpty(taggedUser))
                            {
                                var roleIds = taggedUser.Split(',').Select(s => int.Parse(s));
                                foreach (int items in roleIds)
                                {
                                    if (notification.CommentedUserId != items)
                                    {
                                        User data = db.Users.Find(items);
                                        if (data != null)
                                        {
                                            string finalMessage1 = user.UserName + " has " + action + " question in which you're tagged in.";

                                            pushNotifications.SendNotification_Android(data.DeviceToken, finalMessage1, "QD", questId.ToString());
                                        }
                                    }
                                }
                            }
                        }
                        else if (ques.OwnerUserID == notification.CommentedUserId)
                        {
                            //in this block notification will send to tagged users
                            string taggedUser = ques.TaggedUser;

                            if (!string.IsNullOrEmpty(taggedUser))
                            {
                                var roleIds = taggedUser.Split(',').Select(s => int.Parse(s));
                                foreach (int items in roleIds)
                                {
                                    User data = db.Users.Find(items);
                                    if (data != null)
                                    {
                                        string finalMessage = user.UserName + " has " + action + " question in which you're tagged in.";

                                        pushNotifications.SendNotification_Android(data.DeviceToken, finalMessage, "QD", questId.ToString());
                                    }
                                }
                            }
                        }
                        if (commentOwner.UserID != notification.CommentedUserId)
                        {
                            //***** Notification to question owner
                            string finalMessage = GenerateTagsForOpinion(notification.Like, notification.Dislike, false, user.UserName);

                            pushNotifications.SendNotification_Android(commentOwner.DeviceToken, finalMessage, "QD", questId.ToString());
                        }
                    }
                    foreach (Opinion orderID in op)
                    {
                        orderID.Likes = db.Notifications.Where(p => p.CommentId == orderID.Id && p.Like == true).Count();
                        orderID.Dislikes = db.Notifications.Where(p => p.CommentId == orderID.Id && p.Dislike == true).Count();
                        db.Entry(orderID).State = System.Data.Entity.EntityState.Modified;


                        //give or take tokens 
                        Token userToken = db.Tokens.Where(x => x.UserId == orderID.CommentedUserId).FirstOrDefault();
                        userToken.BalanceToken = userToken.BalanceToken + token_score;
                        db.Entry(userToken).State = System.Data.Entity.EntityState.Modified;

                        db.SaveChanges();
                    }





                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, notification, "LikeDislikeOpinion"));
                }
            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "LikeDislikeOpinion"));
            }
        }

        #endregion



        #region "Search by hashtag" 
        [HttpGet]
        [Route("api/MobileApi/SearchByHashTagOrString")]
        public HttpResponseMessage SearchByHashTagOrString(string search, int Pageindex, int Pagesize)
        {
            try
            {

                if (search.Contains("~"))
                {
                    search = search.Replace("~", "#").ToString();
                }
                var result = (from q in db.Questions
                              join u in db.Users on q.OwnerUserID equals u.UserID
                              where q.IsDeleted == false
                              select new PostQuestionDetail
                              {
                                  Id = q.Id,
                                  Question = q.PostQuestion,
                                  OwnerUserID = q.OwnerUserID,
                                  OwnerUserName = u.UserName,
                                  Name = u.FirstName + " " + u.LastName,
                                  UserImage = string.IsNullOrEmpty(u.ImageURL) ? "" : u.ImageURL,
                                  HashTags = q.HashTags,
                                  IsSlider = q.IsSlider,
                                  CreationDate = q.CreationDate,
                                  TotalLikes = db.Notifications.Where(o => o.questId == q.Id && o.Like == true).Count(),
                                  TotalDisLikes = db.Notifications.Where(o => o.questId == q.Id && o.Dislike == true).Count(),
                                  YesCount = db.Opinions.Where(o => o.QuestId == q.Id && o.IsAgree == true).Count(),
                                  NoCount = db.Opinions.Where(o => o.QuestId == q.Id && o.IsAgree == false).Count()
                              }).Where(s => s.HashTags.Contains(search) || s.Question.Contains(search)).OrderByDescending(p => p.CreationDate).ToPagedList(Pageindex - 1, Pagesize).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, result, "SearchQuestion"));
            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "SearchQuestion"));
            }
        }
        #endregion

        #region "BookMark Question" 
        [HttpPost]
        [Route("api/MobileApi/BookMarkQuestion")]
        public HttpResponseMessage BookMarkQuestion([FromBody] QuestionBookmark questionBookmark)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }
                BookMark quest = null;

                quest = db.BookMarks.Where(p => p.QuestionId == questionBookmark.QuestionId).FirstOrDefault();
                if (quest != null)
                {
                    quest.IsBookmark = questionBookmark.IsBookmark;
                    quest.UserId = questionBookmark.UserId;
                    quest.QuestionId = questionBookmark.QuestionId;
                    quest.ModifiedDate = DateTime.Now;
                    db.Entry(quest).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    int questID = quest.Id;
                    quest = db.BookMarks.Find(questID);
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, quest, "UserData"));
                }
                else
                {
                    quest = new BookMark();

                    quest.IsBookmark = questionBookmark.IsBookmark;
                    quest.UserId = questionBookmark.UserId;
                    quest.QuestionId = questionBookmark.QuestionId;
                    quest.CreationDate = DateTime.Now;
                    db.BookMarks.Add(quest);
                    db.SaveChanges();
                    int questID = quest.Id;
                    quest = db.BookMarks.Find(questID);
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, quest, "Question"));
                }
            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "Question"));
            }
        }
        #endregion

        #region "Get All Notification by user Id" 
        [HttpGet]
        [Route("api/MobileApi/GetAllNotificationByUser")]
        public HttpResponseMessage GetAllNotificationByUser(string userId, int Pageindex, int Pagesize)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }
                UserNotifications userNotifications = new UserNotifications();
                int id = Convert.ToInt32(userId);

                var userNotifications1 = (from q in db.Questions
                                          join o in db.Opinions on q.Id equals o.QuestId
                                          join n in db.Notifications on o.Id equals n.CommentId
                                          join u in db.Users on o.CommentedUserId equals u.UserID
                                         // where q.OwnerUserID == id && q.IsDeleted == false
                                         // && n.CommentedUserId != id


                                          where ((q.OwnerUserID == id && o.CommentedUserId != id && n.Comment == true) || //someone left a comment
                                       (o.CommentedUserId == id && n.Comment != true)) && q.IsDeleted == false  //someone left a vote

                                          select new UserNotifications
                                          {
                                              QuestionId = q.Id,
                                              Question = q.PostQuestion,
                                              HashTags = q.HashTags,
                                              OpinionId = o.Id,
                                              Opinion = o.Comment,
                                              Image = u.ImageURL,
                                              Name = u.FirstName + " " + u.LastName,
                                              CommentedUserId = o.CommentedUserId,
                                              UserName = db.Users.Where(x => x.UserID == n.CommentedUserId).FirstOrDefault().UserName,
                                              Like = ((n.Like ?? false) ? true : false),
                                              Dislike = ((n.Dislike ?? false) ? true : false),
                                              Comment = ((n.Comment ?? false) ? true : false),
                                              CreationDate = n.CreationDate,
                                              ModifiedDate = n.ModifiedDate

                                          }).OrderByDescending(p => p.CreationDate).ToPagedList(Pageindex - 1, Pagesize).ToList();

                foreach (var data in userNotifications1)
                {
                    data.Message = GenerateTags(data.Like, data.Dislike, data.Comment, data.UserName);
                    data.Tag = (data.Like == true) ? "Like" : (data.Dislike == true) ? "Dislike" : (data.Comment == true) ? "Comment" : "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, userNotifications1, "AllOpinion"));
            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "AllOpinion"));
            }
        }

        public string GenerateTags(bool? like, bool? dislike, bool? comment, string UserName)
        {
            string Tag = "";
            if (like == true && dislike == false && comment == false)
            {
                Tag = UserName + " Has Liked your opinion.";
            }
            else if (dislike == true && like == false && comment == false)
            {
                Tag = UserName + " Has Disliked your opinion.";
            }
            else if (comment == true && like == false && dislike == false)
            {
                Tag = UserName + " Has given opinion on your Question.";
            }
            else if (like == true && dislike == false && comment == true)
            {
                Tag = UserName + " Has Liked and given opinion on your Question.";
            }
            else if (dislike == true && like == false && comment == true)
            {
                Tag = UserName + " Has Disliked and given opinion on your Question.";
            }
            else if (dislike == false && like == false && comment == false)
            {
                Tag = UserName + " Has removed his/her Vote.";
            }

            return Tag;
        }
        #endregion

        #region "Get User Profile" 
        [HttpGet]
        [Route("api/MobileApi/GetUserProfile")]
        public HttpResponseMessage GetUserProfile(int userId, int viewUserId)
        {
            try
            {
                bool _hasFollowBack = false;
                try
                {
                    var follow = db.Followers.Where(x => x.UserId == userId && x.FollowedId == viewUserId && x.IsFollowing == true).FirstOrDefault();
                    _hasFollowBack = follow == null ? false : true;
                }
                catch { }

                db.Configuration.LazyLoadingEnabled = false;
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }
                UserProfile UserProfile = new UserProfile();
                //int id = Convert.ToInt32(UserID);

                UserProfile = (from t in db.Tokens
                               join u in db.Users on t.UserId equals u.UserID
                               where u.UserID == viewUserId
                               select new UserProfile
                               {
                                   UserID = u.UserID,
                                   UserName = u.UserName,
                                   FirstName = u.FirstName,
                                   LastName = u.LastName,
                                   Email = u.Email,
                                   ImageURL = u.ImageURL,
                                   BalanceToken = t.BalanceToken,
                                   TotalPostedQuestion = db.Questions.Where(p => p.OwnerUserID == viewUserId && p.IsDeleted == false).Count(),
                                   TotalLikes = (from q in db.Questions
                                                 join o in db.Opinions on q.Id equals o.QuestId
                                                 where q.OwnerUserID == viewUserId && q.IsDeleted == false
                                                 select o.Likes).Sum(),
                                   TotalDislikes = (from q in db.Questions
                                                    join o in db.Opinions on q.Id equals o.QuestId
                                                    where q.OwnerUserID == viewUserId && q.IsDeleted == false
                                                    select o.Dislikes).Sum(),
                                   Followers = db.Followers.Where(y => y.FollowedId == u.UserID && y.IsFollowing == true).ToList().Count(),
                                   Followings = db.Followers.Where(z => z.UserId == u.UserID && z.IsFollowing == true).ToList().Count(),
                                   HasFollowed = _hasFollowBack
                               }).FirstOrDefault();


                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, UserProfile, "UserProfile"));
            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "UserProfile"));
            }
        }
        #endregion


        #region "Update User Profile" 
        [HttpPost]
        [Route("api/MobileApi/UpdateUserProfile")]
        public HttpResponseMessage UpdateUserProfile()
        {
            try
            {
                var httpContext = (HttpContextWrapper)Request.Properties["MS_HttpContext"];
                int userid = 0;
                string firstname = "";
                string lastname = "";
                string username = "";

                if (httpContext.Request.Form["UserID"] == "")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "provide UserID"));
                }
                else
                {
                    userid = Convert.ToInt32(httpContext.Request.Form["UserID"].ToString());
                }
                if (httpContext.Request.Form["FirstName"] != "" && httpContext.Request.Form["FirstName"] != null)
                {
                    firstname = httpContext.Request.Form["FirstName"].ToString();

                }
                if (httpContext.Request.Form["LastName"] != "" && httpContext.Request.Form["LastName"] != null)
                {
                    lastname = httpContext.Request.Form["LastName"].ToString();
                }

                //ViewModelUser User = new ViewModelUser();
                string strIamgeURLfordb = null;
                string _SiteRoot = WebConfigurationManager.AppSettings["SiteImgPath"];
                string _SiteURL = WebConfigurationManager.AppSettings["SiteImgURL"];
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }
                User entity = null;
                entity = db.Users.Find(userid);

                if (entity != null)
                {
                    if (httpContext.Request.Form["UserName"] != "")
                    {
                        username = httpContext.Request.Form["UserName"].ToString();
                        var userExist = db.Database
                        .SqlQuery<User>("SELECT * FROM [Users] WHERE [UserName] = @username and UserID !=@UserID", new SqlParameter("@username", username), new SqlParameter("@UserID", userid))
                        .Count();

                        if (userExist > 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, "User Already Exsist.", "UpdateUserProfile"));
                        }
                        //if(userExist)
                    }

                    //entity.ImageURL = "";
                    if (!string.IsNullOrEmpty(firstname))
                        entity.FirstName = firstname;
                    else
                        entity.FirstName = "";

                    if (!string.IsNullOrEmpty(lastname))
                        entity.LastName = lastname;
                    else
                        entity.LastName = "";
                    entity.UserName = username;
                    entity.ModifiedDate = DateTime.Now;
                    if (HttpContext.Current.Request.Files.Count > 0)
                    {
                        ImageResponse imgResponse = MultipartFiles.GetMultipartImage(HttpContext.Current.Request.Files, "Image", "profileimage", _Imagethumbsize, _Imagethumbsize, _imageSize, _imageSize, true, true, true, "profileimage");

                        if (imgResponse.IsSuccess == true)
                        {
                            //entity.ThumbnailURL = imgResponse.ThumbnailURL;
                            entity.ImageURL = imgResponse.ImageURL;
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, imgResponse.ResponseMessage));
                        }
                        // _UsersRepo.Repository.Update(entity);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(entity.ImageURL))
                        {
                            entity.ImageURL = _SiteURL + "/ProfileImage/opozee-profile.png";
                        }
                        //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Info, "Image file is not provided", "UserData"));
                    }
                    //if (User.ImageURL != null && User.ImageURL != "")
                    //{
                    //    //try
                    //    //{
                    //    //    string strTempImageSave = OpozeeLibrary.Utilities.ResizeImage.Download_Image(User.ImageURL);
                    //    //    string profileFilePath = _SiteURL + "/ProfileImage/" + strTempImageSave;
                    //    //    strIamgeURLfordb = profileFilePath;
                    //    //    entity.ImageURL = profileFilePath;
                    //    //}
                    //    //catch (Exception ex)
                    //    //{

                    //    //}
                    //}
                    //else
                    //{
                    //    entity.ImageURL = _SiteURL + "/ProfileImage/opozee-profile.png";
                    //}
                    db.Entry(entity).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    int userID = entity.UserID;
                    entity = db.Users.Find(userID);
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, entity, "UpdateUserProfile"));
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, "User Not Found", "UpdateUserProfile"));
                }

            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "UpdateUserProfile"));
            }
        }
        #endregion

        #region "Delete Question" 
        [HttpPost]
        [Route("api/MobileApi/DeleteQuestion")]
        public HttpResponseMessage DeleteQuestion(int questId)
        {
            try
            {
                Question question = db.Questions.Find(questId);

                if (question != null)
                {
                    //db.Questions.Remove(question);
                    //db.SaveChanges();
                    question.IsDeleted = true;
                    db.Entry(question).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, question, "DeleteQuestion"));
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, "Question Not Found", "DeleteQuestion"));
                }

            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "DeleteQuestion"));
            }
        }
        #endregion


        #region "Get All BookMark By Id" 
        [HttpGet]
        [Route("api/MobileApi/GetAllBookMarkById")]
        public HttpResponseMessage GetAllBookMarkById(int userId, int Pageindex, int Pagesize)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }
                AllUserQuestions questionDetail = new AllUserQuestions();
                int id = Convert.ToInt32(userId);
                questionDetail.PostQuestionDetail = (from q in db.Questions
                                                     join b in db.BookMarks on q.Id equals b.QuestionId
                                                     join u in db.Users on q.OwnerUserID equals u.UserID
                                                     where q.IsDeleted == false && b.UserId == userId && b.IsBookmark == true
                                                     select new PostQuestionDetail
                                                     {
                                                         Id = q.Id,
                                                         Question = q.PostQuestion,
                                                         OwnerUserID = q.OwnerUserID,
                                                         OwnerUserName = u.UserName,
                                                         UserImage = string.IsNullOrEmpty(u.ImageURL) ? "" : u.ImageURL,
                                                         HashTags = q.HashTags,
                                                         Name = u.FirstName + " " + u.LastName,
                                                         TotalLikes = db.Notifications.Where(o => o.questId == q.Id && o.Like == true).Count(),
                                                         TotalDisLikes = db.Notifications.Where(o => o.questId == q.Id && o.Dislike == true).Count(),
                                                         CreationDate = q.CreationDate,
                                                         YesCount = db.Opinions.Where(o => o.QuestId == q.Id && o.IsAgree == true).Count(),
                                                         NoCount = db.Opinions.Where(o => o.QuestId == q.Id && o.IsAgree == false).Count()
                                                     }).OrderByDescending(p => p.CreationDate).ToPagedList(Pageindex - 1, Pagesize).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, questionDetail, "GetBookmarkQuestion"));
            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "GetBookmarkQuestion"));
            }
        }
        #endregion


        #region "Get All Users" 
        [HttpGet]
        [Route("api/MobileApi/GetAllUsers")]
        public HttpResponseMessage GetAllUsers(int Pageindex, int Pagesize)
        {
            try
            {
                List<ViewModelUser> user = new List<ViewModelUser>();

                user = (from u in db.Users
                        where u.IsAdmin != true
                        select new ViewModelUser
                        {
                            UserID = u.UserID,
                            UserName = u.UserName,
                            Name = u.FirstName + " " + u.LastName,
                            ImageURL = string.IsNullOrEmpty(u.ImageURL) ? "" : u.ImageURL,
                            CreatedDate = u.CreatedDate
                        }).OrderByDescending(p => p.CreatedDate).ToPagedList(Pageindex - 1, Pagesize).ToList();


                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, user, "GetAllUsers"));
            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "GetAllUsers"));
            }
        }
        #endregion


        #region "Get All Search Users" 
        [HttpGet]
        [Route("api/MobileApi/GetAllSearchUsers")]
        public HttpResponseMessage GetAllSearchUsers(string search)
        {
            try
            {
                List<ViewModelUser> user = new List<ViewModelUser>();

                user = (from u in db.Users
                        where u.IsAdmin != true
                        select new ViewModelUser
                        {
                            UserID = u.UserID,
                            UserName = u.UserName,
                            Name = u.FirstName + " " + u.LastName,
                            ImageURL = string.IsNullOrEmpty(u.ImageURL) ? "" : u.ImageURL,
                            CreatedDate = u.CreatedDate
                        }).Where(s => s.UserName.Contains(search) || s.Name.Contains(search)).ToList();


                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, user, "GetAllUsers"));
            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "GetAllUsers"));
            }
        }
        #endregion

        //#region "Get All Search Users" 
        //[HttpGet]
        //[Route("api/MobileApi/GetAllTaggedUsers")]
        //public HttpResponseMessage GetAllTaggedUsers(string questId)
        //{
        //    try
        //    {
        //        List<ViewTaggedUser> user = new List<ViewTaggedUser>();
        //        int id = Convert.ToInt32(questId);
        //        Question quest = db.Questions.Find(id);
        //        user = (from u in db.Users
        //                where quest.TaggedUser.Contains(u.UserID.ToString())
        //                select new ViewTaggedUser
        //                {
        //                    UserID = u.UserID.ToString(),
        //                    UserName = u.UserName,
        //                    Name = u.FirstName + " " + u.LastName,
        //                    FirstName = u.FirstName,
        //                    LastName = u.LastName,
        //                    TaggedUser = quest.TaggedUser,
        //                    ImageURL = string.IsNullOrEmpty(u.ImageURL) ? "" : u.ImageURL,
        //                    CreatedDate = u.CreatedDate
        //                }).ToList();
        //        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, user, "GetAllUsers"));
        //    }
        //    catch (Exception ex)
        //    {
        //        OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
        //        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "GetAllUsers"));
        //    }
        //}
        //#endregion
        #region "Get All Search Users" 
        [HttpGet]
        [Route("api/MobileApi/GetAllTaggedUsers")]
        public HttpResponseMessage GetAllTaggedUsers(int questId)
        {
            try
            {
                List<ViewTaggedUser> user = new List<ViewTaggedUser>();
                int id = Convert.ToInt32(questId);

                Question questTags = db.Questions.Where(x => x.Id == questId).FirstOrDefault();

                if (!string.IsNullOrEmpty(questTags.TaggedUser))
                {
                    string TagIds = questTags.TaggedUser.ToString();
                    var stringsarray = TagIds.Split(',');
                    string stringFinal = "";
                    foreach (var item in stringsarray)
                    {
                        stringFinal += "'" + item + "',";
                    }

                    stringFinal = stringFinal.Substring(0, stringFinal.Length - 1);
                    string query = "select * from users where CAST(userid as int)  in (" + stringFinal + ")";

                    //List<ViewTaggedUser> user1 = new List<ViewTaggedUser>();
                    SqlConnection connection = new SqlConnection(con);
                    var command1 = new SqlCommand(query, connection);
                    command1.CommandType = System.Data.CommandType.Text;
                    connection.Open();
                    SqlDataReader reader = command1.ExecuteReader();
                    ViewTaggedUser objitem = null;
                    while (reader.Read())
                    {
                        objitem = new ViewTaggedUser();
                        objitem.UserID = reader["UserID"].ToString();
                        objitem.UserName = reader["UserName"].ToString();
                        objitem.FirstName = reader["FirstName"].ToString();
                        objitem.LastName = reader["LastName"].ToString();
                        objitem.TaggedUser = TagIds;
                        objitem.UserID = reader["UserID"].ToString();
                        objitem.ImageURL = string.IsNullOrEmpty(reader["ImageURL"].ToString()) ? "" : reader["ImageURL"].ToString();
                        objitem.Name = objitem.FirstName + objitem.LastName;
                        user.Add(objitem);
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, user, "GetAllUsers"));
            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "GetAllUsers"));
            }
        }
        #endregion

        #region "Get Other User Profile" 
        [HttpGet]
        [Route("api/MobileApi/GetOtherUserProfile")]
        public HttpResponseMessage GetOtherUserProfile(int userid, int Pageindex, int Pagesize)
        {
            try
            {
                OtherUserProfile UserProfile = new OtherUserProfile();
                UserProfile.UserProfile = (from q in db.Questions
                                           join u in db.Users on q.OwnerUserID equals u.UserID
                                           where u.UserID == userid
                                           select new OtherUsers
                                           {
                                               UserID = u.UserID,
                                               UserName = u.UserName,
                                               FirstName = u.FirstName,
                                               LastName = u.LastName,
                                               Name = u.FirstName + " " + u.LastName,
                                               Email = u.Email,
                                               ImageURL = u.ImageURL,
                                               TotalPostedQuestion = db.Questions.Where(p => p.OwnerUserID == userid && p.IsDeleted == false).Count()
                                           }).FirstOrDefault();


                UserProfile.Question = (from q in db.Questions
                                        join u in db.Users on q.OwnerUserID equals u.UserID
                                        where u.UserID == userid
                                        select new PostQuestionDetail
                                        {
                                            Id = q.Id,
                                            Question = q.PostQuestion,
                                            TotalLikes = (from q in db.Questions
                                                          join o in db.Opinions on q.Id equals o.QuestId
                                                          where q.OwnerUserID == userid && q.IsDeleted == false
                                                          select o.Likes).Sum(),
                                            TotalDisLikes = (from q in db.Questions
                                                             join o in db.Opinions on q.Id equals o.QuestId
                                                             where q.OwnerUserID == userid && q.IsDeleted == false
                                                             select o.Dislikes).Sum(),
                                            CreationDate = q.CreationDate
                                        }).OrderByDescending(p => p.CreationDate).ToPagedList(Pageindex - 1, Pagesize).ToList();


                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, UserProfile, "OtherUserProfile"));
            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "OtherUserProfile"));
            }
        }
        #endregion


        #region "Get Beliefs by User Id"
        [Route("api/MobileApi/GetUserBeliefs")]
        [HttpGet]
        public HttpResponseMessage getUserBeliefs(int userId)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }



                List<Belief> beliefList = (from belief in db.Opinions
                                           join user in db.Users on belief.CommentedUserId equals user.UserID
                                           where user.UserID == userId
                                           select new Belief
                                           {
                                               Id = belief.Id,
                                               questionId = belief.QuestId,
                                               beliefText = belief.Comment,
                                               userId = user.UserID,
                                               UserFullName = user.FirstName + " " + user.LastName,
                                               UserImage = string.IsNullOrEmpty(user.ImageURL) ? "" : user.ImageURL,
                                               LikesCount = db.Notifications.Where(p => p.CommentId == belief.Id && p.Like == true).Count(),
                                               DislikesCount = db.Notifications.Where(p => p.CommentId == belief.Id && p.Dislike == true).Count(),
                                               userName = user.UserName,
                                               IsAgree = belief.IsAgree,
                                               CreationDate = belief.CreationDate,
                                               questionText = db.Questions.Where(question => question.Id == belief.QuestId).FirstOrDefault().PostQuestion
                                           }).OrderByDescending(p => p.CreationDate).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, beliefList);
            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "GetUserBeliefs"));
            }

        }
        #endregion

        ///
        [HttpPost]
        [Route("api/MobileApi/Login")]
        public dynamic Login(UserLoginWeb login)
        {
            dynamic _response = new ExpandoObject();
            UserLoginWeb ObjLogin = new UserLoginWeb();
            using (OpozeeDbEntities db = new OpozeeDbEntities())
            {

                var v = db.Users.Where(a => a.Email == login.Email && (a.IsAdmin ?? false) == false).FirstOrDefault();
                if (v != null)
                {
                    if (v.EmailConfirmed == true)
                    {
                        ObjLogin.Token = AesCryptography.Encrypt(login.Password);
                        ObjLogin.Token = AesCryptography.Decrypt(ObjLogin.Token);
                        if (string.Compare(AesCryptography.Encrypt(login.Password), v.Password) == 0)
                        {
                            try
                            {
                                //string _apiURL = "http://localhost:61545/";
                                string _apiURL = WebConfigurationManager.AppSettings["WebPath"];

                                var client = new RestClient($"{_apiURL}OpozeeGrantResourceOwnerCredentialSecret");
                                var request = new RestRequest(Method.POST);
                                request.AddHeader("content-type", "application/x-www-form-urlencoded");

                                request.AddParameter("application/x-www-form-urlencoded",
                                    $"username={v.Email}&password={v.Password}&grant_type=password",
                                    ParameterType.RequestBody);

                                IRestResponse response = client.Execute(request);

                                AuthToken _authToken = JsonConvert.DeserializeObject<AuthToken>(response.Content);
                                ObjLogin.AuthToken = _authToken;
                            }
                            catch (Exception ex)
                            {
                                _response.success = false;
                                _response.data = null;
                                _response.message = "Please check user name or Password!";
                                return _response;
                            }

                            ObjLogin.Id = v.UserID;
                            ObjLogin.Email = v.Email;
                            ObjLogin.ImageURL = v.ImageURL;

                            ObjLogin.BalanceToken = db.Tokens.Where(x => x.UserId == v.UserID).FirstOrDefault() == null
                                ? 0 : db.Tokens.Where(x => x.UserId == v.UserID).FirstOrDefault().BalanceToken ?? 0;

                            var totalRef = db.Referrals.Where(x => x.ReferralUserId == v.UserID).ToList();
                            ObjLogin.TotalReferred = totalRef == null ? 0 : totalRef.Count;

                            try
                            {
                                ObjLogin.Followers = db.Followers.Where(x => x.FollowedId == v.UserID && x.IsFollowing == true).ToList() == null
                                    ? 0 : db.Followers.Where(x => x.FollowedId == v.UserID && x.IsFollowing == true).ToList().Count;

                                ObjLogin.Followings = db.Followers.Where(x => x.UserId == v.UserID && x.IsFollowing == true).ToList() == null
                                    ? 0 : db.Followers.Where(x => x.UserId == v.UserID && x.IsFollowing == true).ToList().Count;

                                //update once logged-in
                                v.DeviceType = !string.IsNullOrEmpty(login.DeviceType) ? login.DeviceType : v.DeviceType;
                                v.DeviceToken = !string.IsNullOrEmpty(login.DeviceToken) ? login.DeviceToken : v.DeviceToken;
                                v.ModifiedDate = DateTime.Now.ToUniversalTime();
                                db.Entry(v).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            catch { }

                            ObjLogin.LastLoginDate = v.ModifiedDate;
                            ObjLogin.UserName = v.UserName;
                            ObjLogin.ReferralCode = v.ReferralCode;
                            ObjLogin.IsSocialLogin = false;
                            ObjLogin.DeviceType = v.DeviceType;
                            ObjLogin.DeviceToken = v.DeviceToken;

                            _response.success = HttpStatusCode.OK;
                            _response.message = "Login successful!";
                            _response.data = ObjLogin;
                            return _response;
                        }
                        else
                        {
                            _response.data = null;
                            _response.success = HttpStatusCode.BadRequest;
                            _response.message = "Please check user name or Password!";
                            return _response;
                        }
                    }
                    else
                    {
                        _response.data = null;
                        _response.success = HttpStatusCode.BadRequest;
                        _response.message = "Please confirm your email address.";
                        return _response;
                    }
                }
                else
                {
                    _response.data = null;
                    _response.success = HttpStatusCode.BadRequest;
                    _response.message = "Please check user name or Password!";
                    return _response;
                }
            }

        }


        [HttpPost]
        [Route("api/MobileApi/Following")]
        public HttpResponseMessage SetFollowing(FollowerVM follow)
        {
            //dynamic _response = new ExpandoObject();
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }

                Follower follower = db.Followers.Where(x => x.FollowedId == follow.Following && x.UserId == follow.UserId).FirstOrDefault();

                if (follower == null)
                {
                    follower = new Follower();
                    follower.CreationDate = DateTime.UtcNow;
                    follower.FollowedId = follow.Following;
                    follower.UserId = follow.UserId;
                    follower.IsFollowing = true;
                    db.Followers.Add(follower);
                    db.SaveChanges();
                }
                else
                {
                    follower.CreationDate = DateTime.UtcNow;
                    follower.FollowedId = follow.Following;
                    follower.UserId = follow.UserId;
                    follower.IsFollowing = true;
                    db.SaveChanges();
                }
                //_response.success = true;
                //_response.data = follower;
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, follower, "followerData"));
                //return _response;

                //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, follower, "followerData"));
            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
               return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "followerData"));

                //_response.success = false;
                //return _response;
            }
        }

        [HttpPost]
        [Route("api/MobileApi/UnfollowUser")]
        public HttpResponseMessage UnfollowUser(FollowerVM follow)
        {
           // dynamic _response = new ExpandoObject();
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }

                Follower follower = db.Followers.Where(x => x.FollowedId == follow.Following && x.UserId == follow.UserId).FirstOrDefault();

                if (follower != null)
                {
                    db.Followers.Remove(follower);
                    db.SaveChanges();
                }
                //_response.success = true;
                //return _response;

                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, null, "UnfollowUser"));
            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "UnfollowUser"));

                //_response.success = false;
                //return _response;
            }
        }

        [HttpPost]
        [Route("api/MobileApi/GetMyFollowers")]
        public HttpResponseMessage GetMyFollowers(PagingModel Model)
        {
            //dynamic _response = new ExpandoObject();
            var followers = new List<FollowerUsers>();
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
            }

            try
            {
                int pageSize = Model.PageSize > 0 ? Model.PageSize : 10; // set your page size, which is number of records per page
                int page = Model.PageNumber;
                int skip = pageSize * (page - 1);

                followers = (from f in db.Followers
                                 //join u in db.Users on f.FollowedId equals u.UserID
                             where f.FollowedId == Model.UserId && f.IsFollowing == true
                             select new FollowerUsers
                             {
                                 UserID = f.FollowedId, //my userid
                                 FollowerId = f.UserId,
                                 UserName = db.Users.Where(u => u.UserID == f.UserId).FirstOrDefault().UserName,
                                 ImageURL = db.Users.Where(u => u.UserID == f.UserId).FirstOrDefault().ImageURL,
                                 HasFollowBack = db.Followers.Where(x => x.UserId == f.FollowedId && x.FollowedId == f.UserId && x.IsFollowing == true).FirstOrDefault() == null ? false : true,
                                 IsFollowing = f.IsFollowing,
                                 CreationDate = f.CreationDate
                             }).OrderByDescending(x => x.CreationDate).Skip(skip).Take(pageSize).ToList();
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "GetMyFollowers"));
            }
            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, followers, "GetMyFollowers"));
            //return followers;
        }
        [HttpPost]
        [Route("api/MobileApi/GetMyFollowing")]
        public HttpResponseMessage GetMyFollowing(PagingModel Model)
        {
            //dynamic _response = new ExpandoObject();
            var following = new List<FollowerUsers>();
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
            }

            try
            {
                int pageSize = Model.PageSize > 0 ? Model.PageSize : 10; // set your page size, which is number of records per page
                int page = Model.PageNumber;
                int skip = pageSize * (page - 1);

                following = (from f in db.Followers
                                 //join u in db.Users on f.UserId equals u.UserID
                             where f.UserId == Model.UserId && f.IsFollowing == true
                             select new FollowerUsers
                             {
                                 UserID = f.UserId, //my userid
                                 FollowerId = f.FollowedId,
                                 UserName = db.Users.Where(u => u.UserID == f.FollowedId).FirstOrDefault().UserName,
                                 ImageURL = db.Users.Where(u => u.UserID == f.FollowedId).FirstOrDefault().ImageURL,
                                 HasFollowBack = db.Followers.Where(x => x.FollowedId == f.UserId && x.UserId == f.FollowedId && x.IsFollowing == true).FirstOrDefault() == null ? false : true,
                                 IsFollowing = f.IsFollowing,
                                 CreationDate = f.CreationDate
                             }).OrderByDescending(x => x.CreationDate).Skip(skip).Take(pageSize).ToList();
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "GetMyFollowing"));
            }
            return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, following, "GetMyFollowing"));
        }


        [HttpPost]
        [Route("api/MobileApi/CrashEMail")]
        public async Task<dynamic> CrashEMail(CrashEMailVM model)
        {
            dynamic _response = new ExpandoObject();
            _response.success = false;
            try
            {
                string recepientName = "Paras";
                string recepientEmail = string.IsNullOrEmpty(model.Email) ? "test.ducktale@gmail.com" : model.Email;
                string subject = "Crash Exception Info | Opozee apk testing";

                var _user = db.Users.Where(a => a.UserID == model.UserId).FirstOrDefault();
                var _name = _user == null ? recepientName : "UserId: " + _user.UserID + "<br/>Username: " + _user.UserName + "<br/>Email: " + _user.Email;
                _name += string.IsNullOrEmpty(model.Info) ? "" : "<br/><b>Info: </b>" + model.Info;

                bool isHtml = true;
                string pathHTMLFile = HttpContext.Current.Server.MapPath("~/Content/mail-template/ContactMailTemplate.html");
                string TEMPLATE = File.ReadAllText(pathHTMLFile);
                TEMPLATE = TEMPLATE.Replace("##MESSAGE##", model.Exception);
                TEMPLATE = TEMPLATE.Replace("##NAME##", _name);
                TEMPLATE = TEMPLATE.Replace("##PHONE##", "");
                TEMPLATE = TEMPLATE.Replace("##EMAIL##", "");

                string body = TEMPLATE.Replace("Thanks & Regards", "User Info:").Replace("Message:", "<b>Crash Exception:</b>").Replace("Opozee Team", "Opozee Tester");

                (bool success, string errorMsg) = await EmailSender.SendEmailAsync(recepientName, recepientEmail, subject, body, isHtml);

                _response.success = success;

                if (!success)
                {
                    _response.message = errorMsg;
                    return _response;
                }
                _response.message = "EMail has been sent";
                return _response;
            }
            catch (Exception ex)
            {
                _response.message = ex.Message;
                return _response;
            }

        }


        private bool CheckEmailExist(string Email)
        {
            try
            {
                return db.Database
                    .SqlQuery<int>("SELECT COUNT(*) FROM [Users] WHERE [Email] = @email", new SqlParameter("@email", Email))
                    .FirstOrDefault() > 0 ? true : false;
            }
            catch (Exception ex)
            {
            }
            return false;
        }


        private (bool IsValidCode, int? ReferralUserId) CheckIfValidReferralCode(string referralCode)
        {
            using (OpozeeDbEntities db = new OpozeeDbEntities())
            {
                try
                {
                    var ReferralUser = db.Database
                           .SqlQuery<User>("SELECT * FROM [Users] WHERE [ReferralCode] = @referralCode",
                                new SqlParameter("@referralCode", referralCode))
                           .FirstOrDefault();

                    if (ReferralUser == null)
                        return (false, null);

                    return (true, ReferralUser.UserID);
                }
                catch { }
            }
            return (false, null);
        }

        private (bool isExist, User user) CheckUserNameExist(string UserName)
        {
            try
            {
                var user = this.GetByUserName(UserName);
                return user == null ? (false, user) : (true, user);
            }
            catch (Exception ex)
            {
            }
            return (false, null);
        }

        private User GetByUserName(string UserName)
        {
            try
            {
                return db.Database
                    .SqlQuery<User>("SELECT * FROM [Users] WHERE [UserName] = @username", new SqlParameter("@username", UserName))
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        #region Get Popular Hashtags

        public class PopularTag
        {
            public string HashTag { get; set; }
            //public int QuestionId { get; set; }
            public int Count { get; set; }
        }

        [HttpGet]
        [Route("api/MobileApi/GetPopularHashTags")]
        public dynamic GetPopularHashTags()
        {
            dynamic _response = new ExpandoObject();
            List<PopularTag> TopPopularHashTags = new List<PopularTag>();
            TopPopularHashTags.Add(new PopularTag { HashTag = "All", Count = 99999 });

            try
            {
                //db.Configuration.LazyLoadingEnabled = false;
                //DateTime from_date = DateTime.UtcNow.AddDays(-30).Date; //last 30 days hashtag

                //var PopularHashTagsList = (from q in db.Questions //join u in db.Users on q.OwnerUserID equals u.UserID
                //                           where q.IsDeleted == false && q.HashTags != "" //&& q.CreationDate > from_date
                //                           select new
                //                           {
                //                               HashTag = q.HashTags,
                //                               QuestionId = q.Id
                //                           }).ToList();


                //List<String> tags = new List<String>();
                string[] tags = { "isThisTrue", "Career", "Sports", "Crypto", "JEE" };

                foreach (var tag in tags)
                {
                    PopularTag _hashtag = new PopularTag();
                    _hashtag.HashTag = tag;// Helper.FirstCharToUpper(tag).Replace("#","");
                    _hashtag.Count = 1;
                    TopPopularHashTags.Add(_hashtag);

 
                }

                //foreach (var item in PopularHashTagsList)
                //{
                //    var splitHastags = tags.ToList();//item.HashTag.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                //    //splitHastags.Remove("All");

                //    foreach (var tag in splitHastags)
                //    {
                //        PopularTag _hashtag = new PopularTag();
                //        _hashtag.HashTag = tag;// Helper.FirstCharToUpper(tag).Replace("#","");

                //        var existingTag = TopPopularHashTags.Where(x => x.HashTag == _hashtag.HashTag).FirstOrDefault();
                //        if (existingTag == null)
                //        {
                //            _hashtag.Count = 1;
                //            TopPopularHashTags.Add(_hashtag);
                //        }
                //        else
                //            existingTag.Count += 1;
                //    }
                //}

                //TopPopularHashTags = TopPopularHashTags.OrderByDescending(x => x.Count).Distinct().ToList();
                //if (TopPopularHashTags.Count >= 6)
                //{
                //    TopPopularHashTags = TopPopularHashTags.Take(6).ToList();
                //}




            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
            }

            //string[] tags = { "crypto", "health", "sports", "uspolitics", "india" };

            //foreach (var tag in TopPopularHashTags)
            //{
            //    PopularTag _hashtag = new PopularTag();
            //    _hashtag.HashTag = tag;

            //    TopPopularHashTags.Add(_hashtag);
            //}

            _response.success = HttpStatusCode.OK;
            _response.data = TopPopularHashTags;
            return _response;
        }
        #endregion

    }
}
