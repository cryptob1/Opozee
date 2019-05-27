using opozee.Enums;
using Opozee.Models;
using Opozee.Models.API;
using Opozee.Models.Models;
using Opozee.Server.Models.API;
using Opozee.Server.Services;
using OpozeeLibrary.API;
using OpozeeLibrary.PushNotfication;
using OpozeeLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Cors;

namespace opozee.Controllers.API
{
    [RoutePrefix("opozee")]
    [EnableCors(origins: "*", headers: "*", methods: "*", SupportsCredentials = true)]
    public class WebApiController : ApiController
    {
        public static string con = ConfigurationManager.ConnectionStrings["OpozeeDbEntitiesSp"].ToString();
        OpozeeDbEntities db = new OpozeeDbEntities();
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }


        [HttpPost]
        [Route("api/WebApi/RegisterUser")]
        public HttpResponseMessage RegisterUser(User users)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }

                User entity = null;
                entity = db.Users.Find(users.UserID);

                string strThumbnailURLfordb = null;
                string strIamgeURLfordb = null;
                string _SiteRoot = WebConfigurationManager.AppSettings["SiteImgPath"];
                string _SiteURL = WebConfigurationManager.AppSettings["SiteImgURL"];

                string strThumbnailImage = users.ImageURL;
                if (entity != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, entity, "User Exists"));
                }
                else
                {
                    entity = new User();
                    Token token = new Token();
                    entity.UserName = users.UserName;
                    entity.FirstName = "FirstName";
                    entity.LastName = "Lastname";
                    entity.Email = users.Email;
                    entity.IsAdmin = false;
                    bool Email = false;
                    Email = OpozeeLibrary.Utilities.Helper.IsValidEmail(users.Email);
                    if (!string.IsNullOrEmpty(users.Password))
                    {
                        entity.Password = AesCryptography.Encrypt(users.Password);
                    }

                    entity.DeviceType = "Web";
                    entity.DeviceToken = users.DeviceToken;
                    entity.CreatedDate = DateTime.Now.ToUniversalTime();
                    entity.RecordStatus = RecordStatus.Active.ToString();
                    // entity.SocialID = users.ThirdPartyId;
                    //if (input.ThirdPartyType == ThirdPartyType.Facebook)
                    //{
                    //    entity.SocialType = ThirdPartyType.Facebook.ToString();
                    //}
                    //else if (input.ThirdPartyType == ThirdPartyType.GooglePlus)
                    //{
                    //    entity.SocialType = ThirdPartyType.GooglePlus.ToString();
                    //}
                    //else if (input.ThirdPartyType == ThirdPartyType.Twitter)
                    //{
                    //    entity.SocialType = ThirdPartyType.Twitter.ToString();
                    //}


                    var v = db.Users.Where(a => a.Email == users.Email).FirstOrDefault();

                    if (v != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, "Email already registered.", "Message"));
                    }


                    v = db.Users.Where(a => a.UserName == users.UserName).FirstOrDefault();

                    if (v != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, "Username already exists.", "Message"));
                    }


                    if (users.ImageURL != null && users.ImageURL != "")
                    {
                        try
                        {

                            string strTempImageSave = OpozeeLibrary.Utilities.ResizeImage.Download_Image(users.ImageURL);
                            string profileFilePath = _SiteURL + "/ProfileImage/" + strTempImageSave;
                            strIamgeURLfordb = profileFilePath;
                            entity.ImageURL = profileFilePath;
                        }
                        catch (Exception ex)
                        {
                            strThumbnailURLfordb = strThumbnailImage;
                            strIamgeURLfordb = strThumbnailImage;
                        }
                    }
                    else
                    {
                        entity.ImageURL = _SiteURL + "/ProfileImage/opozee-profile.png";
                    }
                    entity.ReferralCode = Helper.GenerateReferralCode();
                    db.Users.Add(entity);
                    db.SaveChanges();

                    int userID = entity.UserID;
                    token.TotalToken = 500;
                    token.BalanceToken = 500;
                    token.UserId = userID;
                    db.Tokens.Add(token);
                    db.SaveChanges();

                    if (users.ReferralCode != null)
                    {
                        try
                        {
                            (bool IsValidCode, int? ReferralUserId) = CheckIfValidReferralCode(users.ReferralCode);
                            if (IsValidCode)
                            {
                                Referral referral = new Referral();
                                referral.ReferredId = ReferralUserId ?? 0;
                                referral.UserId = entity.UserID;
                                referral.CreationDate = DateTime.Now;
                                referral.IsDeleted = false;
                                db.Referrals.Add(referral);
                                db.SaveChanges();
                            }
                        }
                        catch (Exception ex) { }
                    }
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


        [HttpPost]
        [Route("api/WebApi/Login")]
        public UserLoginWeb Login(UserLoginWeb login)
        {
            UserLoginWeb ObjLogin = new UserLoginWeb();
            using (OpozeeDbEntities db = new OpozeeDbEntities())
            {
                // UserLogin userlogin = new UserLogin();
                var v1 = db.Users.Select(s => s).ToList();
                var v = db.Users.Where(a => a.Email == login.Email && (a.IsAdmin ?? false) == false).FirstOrDefault();
                if (v != null)
                {
                    ObjLogin.Token = AesCryptography.Encrypt(login.Password);
                    ObjLogin.Token = AesCryptography.Decrypt(ObjLogin.Token);
                    if (string.Compare(AesCryptography.Encrypt(login.Password), v.Password) == 0)
                    {
                        //int timeout = login.RememberMe ? 525600 : 20; // 525600 min = 1 year
                        //var ticket = new FormsAuthenticationTicket(login.EmailID, login.RememberMe, timeout);
                        //string encrypted = FormsAuthentication.Encrypt(ticket);
                        //var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        //cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        //cookie.HttpOnly = true;
                        //Response.Cookies.Add(cookie);

                        //userlogin.EmailID = login.EmailID;
                        //userlogin.Password = login.Password;
                        login.Id = v.UserID;
                        ObjLogin.Id = v.UserID;
                        ObjLogin.Email = v.Email;
                        ObjLogin.ImageURL = v.ImageURL;

                        ObjLogin.BalanceToken = db.Tokens.Where(x => x.UserId == v.UserID).FirstOrDefault() == null
                        ? 0 : db.Tokens.Where(x => x.UserId == v.UserID).FirstOrDefault().BalanceToken ?? 0;

                        var totalRef = db.Referrals.Where(x => x.ReferredId == v.UserID).ToList();
                        ObjLogin.TotalReferred = totalRef == null ? 0 : totalRef.Count;
                        //update once logged-in
                        try
                        {
                            v.ModifiedDate = DateTime.Now.ToUniversalTime();
                            db.Entry(v).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        catch { }
                        ObjLogin.LastLoginDate = v.ModifiedDate;
                        ObjLogin.ReferralCode = v.ReferralCode;
                        ObjLogin.IsSocialLogin = false;

                        return ObjLogin;
                    }
                    else
                    {
                        return ObjLogin;
                    }
                }
                else
                {
                    return ObjLogin;
                }
            }

        }

        [HttpGet]
        [Route("api/WebApi/CheckReferralCode")]
        public bool CheckReferralCode(string referralCode)
        {
            using (OpozeeDbEntities db = new OpozeeDbEntities())
            {
                try
                {
                    return db.Database
                           .SqlQuery<int>("SELECT COUNT(*) FROM [Users] WHERE [ReferralCode] = @referralCode",
                                new SqlParameter("@referralCode", referralCode))
                           .FirstOrDefault() > 0 ? true : false;
                }
                catch { }
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

        [HttpGet]
        [Route("api/WebApi/GetQuestion")]
        public List<PostQuestionDetail> GetQuestion()
        {
            try
            {
                var questList = new List<PostQuestionDetail>();
                questList = (from q in db.Questions
                             join u in db.Users on q.OwnerUserID equals u.UserID
                             select new PostQuestionDetail
                             {
                                 Id = q.Id,
                                 Question = q.PostQuestion,
                                 OwnerUserID = q.OwnerUserID,
                                 OwnerUserName = u.UserName,
                                 HashTags = q.HashTags,
                                 CreationDate = q.CreationDate,
                                 TotalLikes = (from o in db.Opinions
                                               where o.QuestId == q.Id
                                               select o.Likes).Sum(),
                             }).ToList();









                return questList;



            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                return null;
            }
        }

        [HttpGet]
        [Route("api/WebApi/GetUserALLRecords")]
        public List<PostQuestionDetailWEB> GetUserALLRecords()
        {
            List<PostQuestionDetailWEB> Objlikdelist = new List<PostQuestionDetailWEB>();
            try
            {
                //var questList = new List<PostQuestionDetail>();
                //ObjPostQuestionDetailList = (from q in db.Questions
                //                             join u in db.Users on q.OwnerUserID equals u.UserID
                //                             join o in db.Opinions on q.Id equals o.QuestId
                //                             join n in db.Notifications on o.Id equals n.CommentId
                //                             select new PostQuestionDetail
                //                             {
                //                                 Id = q.Id,
                //                                 //QuestionId = o.Id,
                //                                 //NotificationId = n.CommentId,
                //                                 Question = q.PostQuestion,
                //                                 OwnerUserID = q.OwnerUserID,
                //                                 OwnerUserName = u.UserName,
                //                                 HashTags = q.HashTags,
                //                                 CreationDate = q.CreationDate
                //                             }).OrderBy(x => x.Id).Take(4).ToList();

                SqlConnection connection = new SqlConnection(con);
                var command1 = new SqlCommand("SP_GetTopLikes", connection);
                command1.CommandType = System.Data.CommandType.StoredProcedure;
                connection.Open();
                SqlDataReader reader = command1.ExecuteReader();



                PostQuestionDetailWEB objitem = null;
                while (reader.Read())
                {
                    objitem = new PostQuestionDetailWEB();
                    objitem.Id = Convert.ToInt32(reader["questid"]);
                    //objitem.Like = Convert.ToInt32(reader["Like"]);
                    objitem.OwnerUserName = reader["UserName"].ToString();
                    objitem.Question = reader["PostQuestion"].ToString();
                    objitem.HashTags = reader["HashTags"].ToString();
                    objitem.ImageURL = string.IsNullOrEmpty(reader["ImageURL"].ToString()) ? "" : reader["ImageURL"].ToString();
                    objitem.UserID = Convert.ToInt32(reader["UserID"]);

                    Objlikdelist.Add(objitem);
                }


                return Objlikdelist;
            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                return null;
            }
        }

        public static bool CheckDuplicateQuestion(string input)
        {
            bool Result = false;
            int Status = 0;
            var question = Regex.Replace(input, "<.*?>", String.Empty);
            SqlConnection connection = new SqlConnection(con);
            var command1 = new SqlCommand("spCheckDuplicateQuestion", connection);
            command1.CommandType = CommandType.StoredProcedure;
            command1.Parameters.Add("@Question", SqlDbType.VarChar, 500).Value = question;
            SqlDataAdapter adp = new SqlDataAdapter(command1);
            DataTable dt = new DataTable();
            connection.Open();
            adp.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                Status = Convert.ToInt32(dt.Rows[0][0]);
            }

            if (Status == 1)
            {
                Result = true;
            }
            else
            {
                Result = false;
            }
            return Result;
        }
        [HttpPost]
        [Route("api/WebApi/CheckDuplicateQuestions")]
        public bool CheckDuplicateQuestions([FromBody] Question postQuestion)
        {
            try
            {
                if (!string.IsNullOrEmpty(postQuestion.PostQuestion))
                {
                    return CheckDuplicateQuestion(postQuestion.PostQuestion);
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        [HttpPost]
        [Route("api/WebApi/CheckDuplicateBelief")]
        public bool CheckDuplicateBelief([FromBody] PostAnswerWeb Model)
        {
            try
            {
                if (!string.IsNullOrEmpty(Model.Comment))
                {
                    var _dbComment = db.Opinions.Any(x => x.Comment == null ? false : x.Comment.Contains(Model.Comment) && x.CommentedUserId == Model.CommentedUserId);
                    if (_dbComment) return true;

                    var _Comment = Regex.Replace(Model.Comment, @"<[^>]+>| ", "").TrimStart();
                    return db.Opinions.Any(x => x.Comment == null ? false : x.Comment.Contains(_Comment) && x.CommentedUserId == Model.CommentedUserId);
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #region "Post Question" 
        [HttpPost]
        [Route("api/WebApi/PostQuestionWeb")]
        public HttpResponseMessage PostQuestionWeb([FromBody] Question postQuestion)
        {
            Token ObjToken = null;
            try
            {

                if (!ModelState.IsValid)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, "Invalid State", "Question"));
                }
                ObjToken = db.Tokens.Where(x => x.UserId == postQuestion.OwnerUserID).FirstOrDefault();
                if (ObjToken.BalanceToken <= 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, "Insufficient Tokens", "Question"));
                }
       
                //if (!string.IsNullOrEmpty(postQuestion.PostQuestion))
                //{
                //    if (CheckDuplicateQuestion(postQuestion.PostQuestion))
                //        return null;
                //}

                Question quest = null;
                quest = db.Questions.Where(p => p.Id == postQuestion.Id
                                       ).FirstOrDefault();
                //if (quest != null)
                //{
                //    quest.PostQuestion = postQuestion.PostQuestion;
                //    quest.OwnerUserID = postQuestion.OwnerUserID;
                //    quest.HashTags = postQuestion.HashTags;
                //    quest.IsDeleted = false;
                //    quest.ModifiedDate = DateTime.Now;
                //    db.Entry(quest).State = System.Data.Entity.EntityState.Modified;
                //    db.SaveChanges();
                //    int questID = quest.Id;
                //    quest = db.Questions.Find(questID);
                //    return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, quest, "Question"));
                //}
                //else
                //{
                quest = new Question();
                Token token = new Token();
                quest.PostQuestion = postQuestion.PostQuestion;
                quest.OwnerUserID = postQuestion.OwnerUserID;
                quest.HashTags = postQuestion.HashTags;
                quest.IsDeleted = false;
                quest.CreationDate = DateTime.Now.ToUniversalTime();
                quest.TaggedUser = postQuestion.TaggedUser;
                db.Questions.Add(quest);
                db.SaveChanges();

                Notification notification = new Notification();
                notification = new Notification();
                notification.CommentedUserId = postQuestion.OwnerUserID;
                notification.questId = quest.Id;
                notification.Comment = true;
                notification.CreationDate = DateTime.Now.ToUniversalTime(); ;
                // notification.Status = 1;
                db.Notifications.Add(notification);
                db.SaveChanges();
                //token = db.Tokens.Where(p => p.UserId == postQuestion.OwnerUserID).FirstOrDefault();
                //token.BalanceToken = token.BalanceToken - 1;

                // db.Entry(token).State = System.Data.Entity.EntityState.Modified;
                //db.SaveChanges();
                int questID = quest.Id;
                quest = db.Questions.Find(questID);
                
                //return ObjToken;
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, quest.Id, "Question"));
                //}
            }
            catch (Exception ex)
            {
                //return ObjToken;
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "Question"));
            }
        }
        #endregion

        [HttpPost]
        [Route("api/WebApi/GetAllNotificationByUser")]
        public List<UserNotifications> GetAllNotificationByUser(PagingModel Model)
        {
            List<UserNotifications> userNotifications2 = new List<UserNotifications>();
            try
            {

                try
                {
                    var user = db.Database
                            .SqlQuery<User>("SELECT * FROM [Users] WHERE [UserID] = @UserID", new SqlParameter("@UserID", Model.UserId))
                            .FirstOrDefault();
                    user.ModifiedDate = DateTime.Now.ToUniversalTime();
                    db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                catch { }
                int Total = Model.TotalRecords;
                int pageSize = 10; // set your page size, which is number of records per page
                int page = Model.PageNumber;
                int skip = pageSize * (page - 1);

                UserNotifications userNotifications = new UserNotifications();
                db.Configuration.LazyLoadingEnabled = false;
                if (!ModelState.IsValid)
                {
                    return userNotifications2;
                }
                bool IsActive;
                IsActive = Model.IsChecked;


                if (IsActive)
                {
                    var TotalRecordNotification = (from q1 in db.Questions
                                                   join n1 in db.Notifications on q1.Id equals n1.questId
                                                   where q1.OwnerUserID == Model.UserId && q1.IsDeleted == false
                                                   //     select new UserNotifications { TotalRecordcount = db.Notifications.Count(y1 => y1.Id == n1.Id) }).ToList();
                                                   select new UserNotifications { TotalRecordcount = n1.Id }).ToList().Count();


                    var userNotifications1 = (from q in db.Questions
                                              join o in db.Opinions on q.Id equals o.QuestId
                                              join n in db.Notifications on o.Id equals n.CommentId
                                              join u in db.Users on n.CommentedUserId equals u.UserID
                                              where ((q.OwnerUserID == Model.UserId && o.CommentedUserId!=Model.UserId && n.Comment==true) || //someone left a comment
                                              (o.CommentedUserId == Model.UserId && n.Comment == false)) && q.IsDeleted == false  //someone left a vote
                                             select new UserNotifications
                                              {
                                                  UserId = q.OwnerUserID,
                                                  QuestionId = q.Id,
                                                  Question = q.PostQuestion,
                                                  HashTags = q.HashTags,
                                                  OpinionId = o.Id,
                                                  Opinion = o.Comment,
                                                  Image = u.ImageURL,
                                                  CommentedUserId = o.CommentedUserId,
                                                  UserName = u.UserName,
                                                  Like = ((n.Like ?? false) ? true : false),
                                                  Dislike = ((n.Dislike ?? false) ? true : false),
                                                  Comment = ((n.Comment ?? false) ? true : false),
                                                  IsAgree = ((o.IsAgree ?? false) ? true : false),
                                                  CreationDate = n.CreationDate,
                                                  ModifiedDate = n.ModifiedDate,
                                                  TotalRecordcount = TotalRecordNotification,
                                                  NotificationId = n.Id,
                                              }).ToList().OrderByDescending(x => x.NotificationId).Skip(skip).Take(pageSize).ToList();


                    foreach (var data in userNotifications1)
                    {
                        
                        data.Message = GenerateNotificationTags(data.Like, data.Dislike, data.Comment, data.UserName, false, IsActive);
                        data.Tag = (data.Like == true) ? "Up-vote" : (data.Dislike == true) ? "Down-Vote" : (data.Comment == true) ? "Belief" : "";
                    }
                    return userNotifications1.Where(p => p.Message != "").ToList();
                }
                else
                {
                    var TotalRecordNotification = (from q1 in db.Questions
                                                   join n1 in db.Notifications on q1.Id equals n1.questId
                                                   where q1.OwnerUserID != Model.UserId
                                                   //     select new UserNotifications { TotalRecordcount = db.Notifications.Count(y1 => y1.Id == n1.Id) }).ToList();
                                                   select new UserNotifications { TotalRecordcount = n1.Id }).ToList().Count();

                    var userNotifications1 = (from q in db.Questions
                                              join o in db.Opinions on q.Id equals o.QuestId
                                              join n in db.Notifications on o.Id equals n.CommentId
                                              join u in db.Users on n.CommentedUserId equals u.UserID
                                              where q.IsDeleted == false  
                                              select new UserNotifications
                                              {
                                                  QuestionId = q.Id,
                                                  Question = q.PostQuestion,
                                                  HashTags = q.HashTags,
                                                  OpinionId = o.Id,
                                                  Opinion = o.Comment,
                                                  Image = u.ImageURL,
                                                  CommentedUserId = o.CommentedUserId,
                                                  UserName = u.UserName,
                                                  Like = ((n.Like ?? false) ? true : false),
                                                  Dislike = ((n.Dislike ?? false) ? true : false),
                                                  Comment = ((n.Comment ?? false) ? true : false),
                                                  IsAgree = ((o.IsAgree ?? false) ? true : false),
                                                  CreationDate = n.CreationDate,
                                                  ModifiedDate = n.ModifiedDate,
                                                  TotalRecordcount = TotalRecordNotification,
                                                  NotificationId = n.Id,
                                              }).ToList().OrderByDescending(x => x.NotificationId).Skip(skip).Take(pageSize).ToList();


                    foreach (var data in userNotifications1)
                    {
                        data.Message = GenerateNotificationTags(data.Like, data.Dislike, data.Comment, data.UserName, false, IsActive);
                        data.Tag = (data.Like == true) ? "Up-Vote" : (data.Dislike == true) ? "Down-Vote" : (data.Comment == true) ? "Belief" : "";
                    }
                    return userNotifications1.Where(p => p.Message != "").ToList();
                }

                // return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, userNotifications1, "AllOpinion"));
            }
            catch (Exception ex)
            {
                return userNotifications2;
            }
        }
        

        [HttpPost]
        [Route("api/WebApi/GetProfileNotificationByUser")]
        public List<UserNotifications> GetProfileNotificationByUser(PagingModel Model)
        {
            List<UserNotifications> _userProfileData = new List<UserNotifications>();
            try
            {

                int Total = Model.TotalRecords;
                int pageSize = 500; // set your page size, which is number of records per page
                int page = Model.PageNumber;
                int skip = pageSize * (page - 1);

                UserNotifications userNotifications = new UserNotifications();
                db.Configuration.LazyLoadingEnabled = false;
                if (!ModelState.IsValid)
                {
                    return _userProfileData;
                }

                if (Model.CheckedTab == "mybeliefs")
                {

                    var TotalRecord = (from q1 in db.Questions
                                       join o1 in db.Opinions on q1.Id equals o1.QuestId
                                       where q1.OwnerUserID == Model.UserId
                                       select new UserNotifications { TotalRecordcount = q1.Id }).ToList().Count();

                    _userProfileData = (from q in db.Questions
                                        join o in db.Opinions on q.Id equals o.QuestId
                                        join n in db.Notifications on o.Id equals n.CommentId
                                        join u in db.Users on o.CommentedUserId equals u.UserID
                                        //where q.OwnerUserID == Model.UserId && q.IsDeleted == false
                                        where o.CommentedUserId == Model.UserId && q.IsDeleted == false
                                        select new UserNotifications
                                        {
                                            QuestionId = q.Id,
                                            Question = q.PostQuestion,
                                            HashTags = q.HashTags,
                                            OpinionId = o.Id,
                                            OpinionList = (from p in db.Opinions where p.QuestId == q.Id && p.CommentedUserId == Model.UserId select p.Comment).ToList(),
                                            Image = u.ImageURL,
                                            CommentedUserId = o.CommentedUserId,
                                            //Name = (from p in db.Users where p.UserID == o.CommentedUserId select p.UserName).ToString(),
                                            UserName = u.UserName,
                                            Like = ((n.Like ?? false) ? true : false),
                                            Dislike = ((n.Dislike ?? false) ? true : false),
                                            Comment = ((n.Comment ?? false) ? true : false),
                                            IsAgree = ((o.IsAgree ?? false) ? true : false),
                                            CreationDate = n.CreationDate,
                                            ModifiedDate = n.ModifiedDate,
                                            TotalRecordcount = TotalRecord,
                                            NotificationId = n.Id,
                                            //IsValidToDelete = this.CheckIsValidToDeleteOpinion(o)
                                            QOCreationDate = o.CreationDate
                                        }).ToList().OrderByDescending(x => x.NotificationId).ToList().Skip(skip).Take(pageSize).ToList();

                    List<UserNotifications> NewLoggeduserBelief = new List<UserNotifications>();
                    foreach (var data in _userProfileData)
                    {
                        var count = NewLoggeduserBelief.Where(x => x.QuestionId == data.QuestionId).Count();
                        if (count > 0)
                        {
                            continue;
                        }
                        else
                        {
                            NewLoggeduserBelief.Add(data);
                        }
                    }

                    _userProfileData = NewLoggeduserBelief;

                    return _userProfileData;
                }
                else if (Model.CheckedTab == "myquestions")
                {
                   var TotalRecord = (from q in db.Questions
                                       //join o in db.Opinions on q.Id equals o.QuestId into op
                                       //from o in op.DefaultIfEmpty()
                                       //join n in db.Notifications on o.Id equals n.CommentId into noti
                                       //from n in noti.DefaultIfEmpty()
                                   join u in db.Users on q.OwnerUserID equals u.UserID
                                   where q.OwnerUserID == Model.UserId && q.IsDeleted == false
                                   select q
                    ).ToList().Count();

                    _userProfileData = (from q in db.Questions
                                            //join o in db.Opinions on q.Id equals o.QuestId into op
                                            //from o in op.DefaultIfEmpty()
                                            //join n in db.Notifications on o.Id equals n.CommentId into noti
                                            //from n in noti.DefaultIfEmpty()
                                        join u in db.Users on q.OwnerUserID equals u.UserID
                                        where q.OwnerUserID == Model.UserId && q.IsDeleted == false
                                        select new UserNotifications
                                        {
                                            QuestionId = q.Id,
                                            Question = q.PostQuestion,
                                            HashTags = q.HashTags,
                                            OpinionId = 0,//o == null ? 0 : o.Id,
                                            //OpinionList = null,//o == null ? null : (from p in db.Opinions where p.QuestId == q.Id select p.Comment).ToList(),
                                            Image = u.ImageURL,
                                            CommentedUserId = 0, //o == null ? 0 : o.CommentedUserId,
                                            Name = string.Empty, //o == null ? string.Empty : this.commentedUser(o.CommentedUserId),
                                            UserName = u.UserName,
                                            //Like = ((n.Like ?? false) ? true : false),
                                            //Dislike = ((n.Dislike ?? false) ? true : false),
                                            //Comment = ((n.Comment ?? false) ? true : false),
                                            IsAgree = false,//o == null ? false : ((o.IsAgree ?? false) ? true : false),
                                            //CreationDate = n.CreationDate,
                                            // ModifiedDate = n.ModifiedDate,
                                            TotalRecordcount = TotalRecord,
                                            //NotificationId = n.Id,
                                            //IsValidToDelete = this.CheckIsValidToDeleteQuestion(q)
                                            QOCreationDate = q.CreationDate
                                        }).ToList().OrderByDescending(x => x.QuestionId).ToList().Skip(skip).Take(pageSize).ToList();

                    return _userProfileData;
                }

                //foreach (var data in LoggeduserBelief)
                //{
                //    data.Message = GenerateTags(data.Like, data.Dislike, data.Comment, data.UserName);
                //    data.Tag = (data.Like == true) ? "Like" : (data.Dislike == true) ? "Dislike" : (data.Comment == true) ? "Comment" : "";
                //}
                // return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, userNotifications1, "AllOpinion"));
            }
            catch (Exception ex)
            {

            }
            return _userProfileData;
        }

        private bool CheckIsValidToDeleteOpinion(Opinion o)
        {
            try
            {
                var CreationDate = o.CreationDate.Value;
                return CreationDate == null ? false : (CreationDate.AddMinutes(10) > DateTime.Now ? true : false);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool CheckIsValidToDeleteQuestion(Question q)
        {
            try
            {
                var CreationDate = q.CreationDate.Value;
                return CreationDate == null ? false : (CreationDate.AddMinutes(10) > DateTime.Now ? true : false);
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public string GenerateTags(bool? like, bool? dislike, bool? comment, string UserName)
        {
            string Tag = "";
            if (like == true && dislike == false && comment == false)
            {
                Tag = UserName + " Has Liked your belief.";
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

            return Tag;
        }

        public string GenerateNotificationTags(bool? like, bool? dislike, bool? comment, string UserName , bool you, bool yours)
        {
            string Tag = "";
            string addStr = "";
            if (yours) { addStr = "your"; }
            if (like == true && dislike == false && comment == false)
            {
                Tag = " up-voted "+ addStr+" belief:";
            }
            else if (dislike == true && like == false && comment == false)
            {
                Tag = " down-voted " + addStr + " belief:";
            }
            else if (comment == true && like == false && dislike == false)
            {
                Tag = " posted a belief to answer " + addStr + " question:";
            }
            else if (like == true && dislike == false && comment == true)
            {
                Tag = " posted a belief and up-voted.";
            }
            else if (dislike == true && like == false && comment == true)
            {
                Tag = " posted a belief and down-voted.";
            }


            if (you) { return "You " + Tag; }

            return "@" +UserName+" " + Tag;
        }

        

        #region "Get Similar Questions" 
        [HttpGet]
        [Route("api/WebApi/GetSimilarQuestionsWeb")]
        public List<PostQuestionDetailWebModel> GetSimilarQuestionsList(int qid, string tags)
        {
            //    AllUserQuestions questionDetail = new AllUserQuestions();

            string[] taglist = tags == null ? null : tags.Split(',');

            var r = new Random();
            string tag = "";

            if (taglist != null)
            {
                foreach (var t in taglist)
                {
                    var qList = db.Questions.Where(p => p.HashTags.Contains(tag)).ToList();
                    if (qList.Count > 0)
                    {
                        tag = t;
                        break;
                    }
                }
            }           
            

            int Total = 5;
            int pageSize = 5; // set your page size, which is number of records per page
            int page = 1;
            int skip = pageSize * (page - 1);

            //int canPage = skip < Total;


            List<PostQuestionDetailWebModel> questionDetail = new List<PostQuestionDetailWebModel>();
            try
            {
                db.Configuration.LazyLoadingEnabled = false;


                questionDetail = (from q in db.Questions
                                  join u in db.Users on q.OwnerUserID equals u.UserID
                                  where q.IsDeleted == false && q.HashTags.Contains(tag) && q.Id!=qid
                                  select new PostQuestionDetailWebModel
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
                                      TotalRecordcount = db.Questions.Count(x => x.IsDeleted == false && x.HashTags.Contains(tag)),
                                      LastActivityTime= (DateTime)(db.Notifications.Where(o => o.questId == q.Id).Max(b => b.CreationDate)),
                                      Comments = (from e in db.Opinions
                                                  join t in db.Users on e.CommentedUserId equals t.UserID
                                                  where e.QuestId == q.Id
                                                  select new Comments
                                                  {
                                                      Id = e.Id,
                                                      Comment = e.Comment,
                                                      CommentedUserId = t.UserID,
                                                      Name = t.FirstName + " " + t.LastName,
                                                      UserImage = string.IsNullOrEmpty(t.ImageURL) ? "" : t.ImageURL,
                                                      LikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Like == true).Count(),
                                                      DislikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Dislike == true).Count(),
                                                      Likes = db.Notifications.Where(p => p.CommentedUserId == q.OwnerUserID && p.CommentId == e.Id).Select(b => b.Like.HasValue ? b.Like.Value : false).FirstOrDefault(),
                                                      DisLikes = db.Notifications.Where(p => p.CommentedUserId == q.OwnerUserID && p.CommentId == e.Id).Select(b => b.Dislike.HasValue ? b.Dislike.Value : false).FirstOrDefault(),
                                                      CommentedUserName = t.UserName,
                                                      IsAgree = e.IsAgree,
                                                      CreationDate = e.CreationDate
                                                  }).ToList()


                                  }).OrderByDescending(p => p.LastActivityTime).Skip(skip).Take(pageSize).ToList();



                foreach (var data in questionDetail)
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
                                                    IsAgree = e.IsAgree,
                                                    LikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Like == true).Count(),
                                                    DislikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Dislike == true).Count(),
                                                    CommentedUserName = t.UserName,
                                                    CreationDate = e.CreationDate
                                                }).OrderByDescending(s => s.LikesCount).ThenByDescending(s => s.CreationDate).First();
                        }
                    }
                }
                return questionDetail;
                //return Request.CreateResponse(JsonResponse.GetResponse(ResponseCode.Success, questionDetail, "AllUserQuestions"));
            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                return questionDetail;
            }
        }
        #endregion

        #region BOUNTY QUESTIONS
        [HttpGet]
        [Route("api/WebApi/GetBountyQuestions")]
        public List<BountyQuestionsVM> GetBountyQuestions(DateTime? StartDate, DateTime? EndDate)
        {
            List<BountyQuestionsVM> BountyQuestionList = new List<BountyQuestionsVM>();
            try
            {
                using (var context = new OpozeeDbEntities())
                {
                    var _BountyQuestions = context.SP_GetBountyQuestions(StartDate, EndDate).ToList();
                    foreach (var bq in _BountyQuestions)
                    {
                        BountyQuestionsVM _bQuestion = new BountyQuestionsVM();
                        _bQuestion.BountyId = bq.BountyId;
                        _bQuestion.QuestionId = bq.QuestionId;
                        _bQuestion.StartDate = bq.StartDate;
                        _bQuestion.EndDate = bq.EndDate;
                        _bQuestion.IsActive = bq.IsActive;
                        _bQuestion.BountyCreatedOn = bq.BountyCreatedOn;
                        _bQuestion.PostQuestion = bq.PostQuestion;
                        _bQuestion.HashTags = bq.HashTags;
                        _bQuestion.TaggedUser = bq.TaggedUser;
                        _bQuestion.QuestionCreatedOn = bq.QuestionCreatedOn;
                        _bQuestion.UserId = bq.UserId;
                        _bQuestion.UserName = bq.UserName;
                        _bQuestion.Email = bq.Email;
                        _bQuestion.SocialID = bq.SocialID;
                        _bQuestion.YesCount = bq.YesCount;
                        _bQuestion.NoCount = bq.NoCount;
                        _bQuestion.TotalLikes = bq.TotalLikes;
                        _bQuestion.TotalDisLikes = bq.TotalDisLikes;
                        _bQuestion.Score = bq.Score;
                        _bQuestion.Comments = (from e in db.Opinions
                                               join t in db.Users on e.CommentedUserId equals t.UserID
                                               where e.QuestId == bq.QuestionId
                                               select new Comments
                                               {
                                                   Id = e.Id,
                                                   Comment = e.Comment,
                                                   CommentedUserId = t.UserID,
                                                   Name = t.FirstName + " " + t.LastName,
                                                   UserImage = string.IsNullOrEmpty(t.ImageURL) ? "" : t.ImageURL,
                                                   LikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Like == true).Count(),
                                                   DislikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Dislike == true).Count(),
                                                   Likes = db.Notifications.Where(p => p.CommentedUserId == bq.UserId && p.CommentId == e.Id).Select(b => b.Like.HasValue ? b.Like.Value : false).FirstOrDefault(),
                                                   DisLikes = db.Notifications.Where(p => p.CommentedUserId == bq.UserId && p.CommentId == e.Id).Select(b => b.Dislike.HasValue ? b.Dislike.Value : false).FirstOrDefault(),
                                                   CommentedUserName = t.UserName,
                                                   IsAgree = e.IsAgree,
                                                   CreationDate = e.CreationDate
                                               }).ToList();

                        BountyQuestionList.Add(_bQuestion);
                    }

                    foreach (var bq in BountyQuestionList)
                    {
                        var opinionList = db.Opinions.Where(p => p.QuestId == bq.QuestionId).ToList();
                        if (opinionList.Count > 0)
                        {

                            int? maxYesLike = opinionList.Where(p => p.IsAgree == true).Max(i => i.Likes);
                            int? maxNoLike = opinionList.Where(p => p.IsAgree == false).Max(i => i.Likes);
                            //int? maxDislike = opinionList.Max(i => i.Dislikes);
                            if (maxYesLike != null && maxYesLike > 0)
                            {
                                bq.MostYesLiked = (from e in db.Opinions
                                                   join t in db.Users on e.CommentedUserId equals t.UserID
                                                   join n in db.Notifications on e.QuestId equals n.questId
                                                   where e.IsAgree == true && e.QuestId == bq.QuestionId && n.Like == true
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
                                                   }).OrderByDescending(s => s.LikesCount).ThenByDescending(s => s.CreationDate).First();

                            }
                            if (maxNoLike != null && maxNoLike > 0)
                            {
                                bq.MostNoLiked = (from e in db.Opinions
                                                  join t in db.Users on e.CommentedUserId equals t.UserID
                                                  join n in db.Notifications on e.QuestId equals n.questId
                                                  where e.IsAgree == false && e.QuestId == bq.QuestionId && n.Like == true
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
                                                  }).OrderByDescending(s => s.LikesCount).ThenByDescending(s => s.CreationDate).First();
                            }
                        }
                    }

                    return BountyQuestionList;
                }

            }
            catch (Exception ex)
            {
                //OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
            }
            return BountyQuestionList;
        }
        #endregion


        #region "Get All Slider Posts" 
        [HttpPost]
        [Route("api/WebApi/GetAllSliderPostsWeb")]
        public List<PostQuestionDetailWebModel> GetAllSliderPostsWeb(PagingModel model)
        {
            //    AllUserQuestions questionDetail = new AllUserQuestions();

            model.Search = model.Search ?? "";

            int Total = model.TotalRecords;
            int pageSize = 10; // set your page size, which is number of records per page
            int page = model.PageNumber;
            int skip = pageSize * (page - 1);

            //int canPage = skip < Total;


            List<PostQuestionDetailWebModel> questionDetail = new List<PostQuestionDetailWebModel>();
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
  
     
                    questionDetail = (from q in db.Questions
                                      join u in db.Users on q.OwnerUserID equals u.UserID
                                      where q.IsDeleted == false && q.IsSlider==true
                                      select new PostQuestionDetailWebModel
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
                                          TotalRecordcount = db.Questions.Count(x => x.IsDeleted == false && x.PostQuestion.Contains(model.Search)),
                                          Comments = (from e in db.Opinions
                                                      join t in db.Users on e.CommentedUserId equals t.UserID
                                                      where e.QuestId == q.Id
                                                      select new Comments
                                                      {
                                                          Id = e.Id,
                                                          Comment = e.Comment,
                                                          CommentedUserId = t.UserID,
                                                          Name = t.FirstName + " " + t.LastName,
                                                          UserImage = string.IsNullOrEmpty(t.ImageURL) ? "" : t.ImageURL,
                                                          LikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Like == true).Count(),
                                                          DislikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Dislike == true).Count(),
                                                          Likes = db.Notifications.Where(p => p.CommentedUserId == q.OwnerUserID && p.CommentId == e.Id).Select(b => b.Like.HasValue ? b.Like.Value : false).FirstOrDefault(),
                                                          DisLikes = db.Notifications.Where(p => p.CommentedUserId == q.OwnerUserID && p.CommentId == e.Id).Select(b => b.Dislike.HasValue ? b.Dislike.Value : false).FirstOrDefault(),
                                                          CommentedUserName = t.UserName,
                                                          IsAgree = e.IsAgree,
                                                          CreationDate = e.CreationDate
                                                      }).ToList()


                                      }).OrderByDescending(p => p.Id).Skip(skip).Take(pageSize).ToList();

           

                foreach (var data in questionDetail)
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
                                                    IsAgree = e.IsAgree,
                                                    LikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Like == true).Count(),
                                                    DislikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Dislike == true).Count(),
                                                    CommentedUserName = t.UserName,
                                                    CreationDate = e.CreationDate
                                                }).OrderByDescending(s => s.LikesCount).ThenByDescending(s => s.CreationDate).First();
                        }
                    }
                }
                return questionDetail;
                //return Request.CreateResponse(JsonResponse.GetResponse(ResponseCode.Success, questionDetail, "AllUserQuestions"));
            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                return questionDetail;
            }
        }
        #endregion

        #region "Get Top Earners" 
        [HttpGet]
        [Route("api/WebApi/GetTopEarners")]
        public List<UsersEarnings> GetTopEarners(int days)
        {
            //    AllUserQuestions questionDetail = new AllUserQuestions();
             
            List<UsersEarnings> earnList = new List<UsersEarnings>();
            try
            {
                SqlConnection connection;
                var command1 = new SqlCommand();

                if (days == 0 || days==-1)
                {
                    connection = new SqlConnection(con);
                    command1 = new SqlCommand("SP_GetEarnings_daily_competition", connection);
                    command1.CommandType = System.Data.CommandType.StoredProcedure;
                    
                    command1.Parameters.Add("@lag", SqlDbType.Int).Value = days;
                }   
                else
                {
                    connection = new SqlConnection(con);
                    command1 = new SqlCommand("SP_GetEarnings", connection);
                    command1.CommandType = System.Data.CommandType.StoredProcedure;
                    command1.Parameters.Add("@days", SqlDbType.Int).Value = days;
                }
                connection.Open();
                SqlDataReader reader = command1.ExecuteReader();



                UsersEarnings objitem = null;
                while (reader.Read())
                {
                    objitem = new UsersEarnings();
                    objitem.OwnerUserName = reader["username"].ToString();
                    objitem.Id = Convert.ToInt32(reader["UserId"]);
                    objitem.Earnings = Convert.ToInt32(reader["earnings"]);
                     
                    earnList.Add(objitem);
                }


                return earnList;
            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                return null;
            }
        }
        #endregion


        #region "Get All Posts" 
        [HttpPost]
        [Route("api/WebApi/GetAllPostsWeb")]
        public List<PostQuestionDetailWebModel> GetAllPostsWeb(PagingModel model)
        {
            //    AllUserQuestions questionDetail = new AllUserQuestions();

            model.Search = model.Search ?? "";

            int Total = model.TotalRecords;
            int pageSize = 10; // set your page size, which is number of records per page
            int page = model.PageNumber;
            int skip = pageSize * (page - 1);

            //int canPage = skip < Total;


            List<PostQuestionDetailWebModel> questionDetail = new List<PostQuestionDetailWebModel>();
            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                if (model.IsHashTag)
                {
                    questionDetail = (from q in db.Questions
                                      join u in db.Users on q.OwnerUserID equals u.UserID
                                      where q.IsDeleted == false && q.HashTags.Contains(model.Search)
                                      select new PostQuestionDetailWebModel
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
                                          TotalRecordcount = db.Questions.Count(x => x.IsDeleted == false && x.PostQuestion.Contains(model.Search)),
                                          LastActivityTime = (DateTime)(db.Notifications.Where(o => o.questId == q.Id).Max(b => b.CreationDate)),
                                          Comments = (from e in db.Opinions
                                                      join t in db.Users on e.CommentedUserId equals t.UserID
                                                      where e.QuestId == q.Id
                                                      select new Comments
                                                      {
                                                          Id = e.Id,
                                                          Comment = e.Comment,
                                                          CommentedUserId = t.UserID,
                                                          Name = t.FirstName + " " + t.LastName,
                                                          UserImage = string.IsNullOrEmpty(t.ImageURL) ? "" : t.ImageURL,
                                                          LikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Like == true).Count(),
                                                          DislikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Dislike == true).Count(),
                                                          Likes = db.Notifications.Where(p => p.CommentedUserId == q.OwnerUserID && p.CommentId == e.Id).Select(b => b.Like.HasValue ? b.Like.Value : false).FirstOrDefault(),
                                                          DisLikes = db.Notifications.Where(p => p.CommentedUserId == q.OwnerUserID && p.CommentId == e.Id).Select(b => b.Dislike.HasValue ? b.Dislike.Value : false).FirstOrDefault(),
                                                          CommentedUserName = t.UserName,
                                                          IsAgree = e.IsAgree,
                                                          CreationDate = e.CreationDate
                                                      }).ToList()

                                      }).OrderByDescending(p => p.LastActivityTime).Skip(skip).Take(pageSize).ToList();


                }
                //qid
                else if (model.QId != -1)
                {
          
                        questionDetail = (from q in db.Questions
                                          join u in db.Users on q.OwnerUserID equals u.UserID
                                          where q.IsDeleted == false && q.Id == model.QId
                                          select new PostQuestionDetailWebModel
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
                                              TotalRecordcount = 1,
                                              LastActivityTime = (DateTime)(db.Notifications.Where(o => o.questId == q.Id).Max(b => b.CreationDate)),
                                              Comments = (from e in db.Opinions
                                                          join t in db.Users on e.CommentedUserId equals t.UserID
                                                          where e.QuestId == q.Id
                                                          select new Comments
                                                          {
                                                              Id = e.Id,
                                                              Comment = e.Comment,
                                                              CommentedUserId = t.UserID,
                                                              Name = t.FirstName + " " + t.LastName,
                                                              UserImage = string.IsNullOrEmpty(t.ImageURL) ? "" : t.ImageURL,
                                                              LikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Like == true).Count(),
                                                              DislikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Dislike == true).Count(),
                                                              Likes = db.Notifications.Where(p => p.CommentedUserId == q.OwnerUserID && p.CommentId == e.Id).Select(b => b.Like.HasValue ? b.Like.Value : false).FirstOrDefault(),
                                                              DisLikes = db.Notifications.Where(p => p.CommentedUserId == q.OwnerUserID && p.CommentId == e.Id).Select(b => b.Dislike.HasValue ? b.Dislike.Value : false).FirstOrDefault(),
                                                              CommentedUserName = t.UserName,
                                                              IsAgree = e.IsAgree,
                                                              CreationDate = e.CreationDate
                                                          }).ToList()
                                              // db.Questions.Count(x => x.IsDeleted == false && x.PostQuestion.Contains(model.Search))

                                          }).OrderByDescending(p => p.LastActivityTime).Skip(skip).Take(pageSize).ToList();
                     

                }

                else
                {//plain mode


                    IQueryable<PostQuestionDetailWebModel> qd;//= new IQueryable<PostQuestionDetailWebModel>();

                    questionDetail = (from q in db.Questions
                                      join u in db.Users on q.OwnerUserID equals u.UserID
                                      where q.IsDeleted == false && q.PostQuestion.Contains(model.Search)
                                      select new PostQuestionDetailWebModel
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
                                          TotalRecordcount = db.Questions.Count(x => x.IsDeleted == false && x.PostQuestion.Contains(model.Search)),
                                          LastActivityTime = (DateTime)(db.Notifications.Where(o => o.questId == q.Id).Max(b => b.CreationDate)),
                                          Comments = (from e in db.Opinions
                                                      join t in db.Users on e.CommentedUserId equals t.UserID
                                                      where e.QuestId == q.Id
                                                      select new Comments
                                                      {
                                                          Id = e.Id,
                                                          Comment = e.Comment,
                                                          CommentedUserId = t.UserID,
                                                          Name = t.FirstName + " " + t.LastName,
                                                          UserImage = string.IsNullOrEmpty(t.ImageURL) ? "" : t.ImageURL,
                                                          LikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Like == true).Count(),
                                                          DislikesCount = db.Notifications.Where(p => p.CommentId == e.Id && p.Dislike == true).Count(),
                                                          Likes = db.Notifications.Where(p => p.CommentedUserId == q.OwnerUserID && p.CommentId == e.Id).Select(b => b.Like.HasValue ? b.Like.Value : false).FirstOrDefault(),
                                                          DisLikes = db.Notifications.Where(p => p.CommentedUserId == q.OwnerUserID && p.CommentId == e.Id).Select(b => b.Dislike.HasValue ? b.Dislike.Value : false).FirstOrDefault(),
                                                          CommentedUserName = t.UserName,
                                                          IsAgree = e.IsAgree,
                                                          CreationDate = e.CreationDate
                                                      }).ToList()


                                      }).OrderByDescending(p => p.LastActivityTime).Skip(skip).Take(pageSize).ToList();
                    //if (model.Sort == 0) //sort by last reaction time
                    //{
                    //    questionDetail = questionDetail.OrderByDescending(p => p.LastActivityTime).Skip(skip).Take(pageSize).ToList();
                    //}
                    //else if (model.Sort == 1) //sort by most reactions
                    //{

                    //    questionDetail = questionDetail.OrderByDescending(p => p.TotalLikes+  p.TotalDisLikes + p.YesCount + p.NoCount).Skip(skip).Take(pageSize).ToList();
                    //}

                    //else if (model.Sort == 2)// sort by least reactions
                    //{
                    //    questionDetail= questionDetail.OrderBy(p => (p => p.TotalLikes+  p.TotalDisLikes + p.YesCount + p.NoCount).Skip(skip).Take(pageSize).ToList();

                    //}
                }

                foreach (var data in questionDetail)
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
                                                 }).OrderByDescending(s => s.LikesCount).ThenByDescending(s => s.CreationDate).First();

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
                                                 }).OrderByDescending(s => s.CreationDate).First();

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
                                                }).OrderByDescending(s => s.LikesCount).ThenByDescending(s => s.CreationDate).First();
                        }
                        else if(maxNoLike != null)
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
                                                }).OrderByDescending (s => s.CreationDate).First();
                        }
                    }
                }

                return questionDetail; //.OrderByDescending(p=>p.LastActivityTime);
                //return Request.CreateResponse(JsonResponse.GetResponse(ResponseCode.Success, questionDetail, "AllUserQuestions"));
            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                return questionDetail;
            }
        }
        #endregion


        #region "Get All Opinion by question Id" 
        [HttpGet]
        [Route("api/WebApi/GetAllOpinionWeb")]
        public BookMarkQuestion GetAllOpinionWeb(string questId, int UserId)
        {
            BookMarkQuestion questionDetail = new BookMarkQuestion();
            try
            {
                using (OpozeeDbEntities db = new OpozeeDbEntities())
                {
                    if (!ModelState.IsValid)
                    {
                        return questionDetail;
                    }

                    int id = Convert.ToInt32(questId);
                    //int userId = Convert.ToInt32(userid);
                    questionDetail.PostQuestionDetail = (from q in db.Questions
                                                         join u in db.Users on q.OwnerUserID equals u.UserID
                                                         where q.Id == id && q.IsDeleted == false
                                                         select new BookMarkQuestionDetail
                                                         {
                                                             Id = q.Id,
                                                             Question = q.PostQuestion,
                                                             OwnerUserID = q.OwnerUserID,
                                                             OwnerUserName = u.UserName,
                                                             UserImage = string.IsNullOrEmpty(u.ImageURL) ? "" : u.ImageURL,
                                                             HashTags = q.HashTags,
                                                             YesCount = db.Opinions.Where(o => o.QuestId == q.Id && o.IsAgree == true).Count(),
                                                             NoCount = db.Opinions.Where(o => o.QuestId == q.Id && o.IsAgree == false).Count(),
                                                             CreationDate = q.CreationDate,
                                                             IsBookmark = db.BookMarks.Where(b => b.UserId == UserId && b.QuestionId == id).Select(b => b.IsBookmark.HasValue ? b.IsBookmark.Value : false).FirstOrDefault(),
                                                         }).FirstOrDefault();

                    questionDetail.Comments = this.SortedComments(id, UserId);

                    return questionDetail;
                    // return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, questionDetail, "AllOpinion"));
                }
            }
            catch (Exception ex)
            {
                return questionDetail;
                //  OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                //  return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "AllOpinion"));
            }
        }

        private List<Comments> SortedComments(int id, int UserId)
        {
            try
            {
                var cList = (from e in db.Opinions
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
                                 Likes = db.Notifications.Where(p => p.CommentedUserId == UserId && p.CommentId == e.Id).Select(b => b.Like.HasValue ? b.Like.Value : false).FirstOrDefault(),
                                 DisLikes = db.Notifications.Where(p => p.CommentedUserId == UserId && p.CommentId == e.Id).Select(b => b.Dislike.HasValue ? b.Dislike.Value : false).FirstOrDefault(),
                                 CommentedUserName = t.UserName,
                                 IsAgree = e.IsAgree,
                                 CreationDate = e.CreationDate
                             }).ToList();

                var YesComments = cList.Where(x => x.IsAgree == true).OrderByDescending(x => (x.LikesCount - x.DislikesCount)).ToList();
                var NoComments = cList.Where(x => x.IsAgree == false).OrderByDescending(x => (x.LikesCount - x.DislikesCount)).ToList();

                List<Comments> _commentList = new List<Comments>();
                for (var i = 1; i < cList.Count + 1; i++)
                {
                    Comments comment = null;

                    if (i % 2 == 0) //even=no
                    {
                        if (NoComments.Count > 0)
                        {
                            comment = NoComments[0];
                            NoComments.Remove(comment);
                        }
                        else if (YesComments.Count > 0)
                        {
                            comment = YesComments[0];
                            YesComments.Remove(comment);
                        }
                    }
                    else
                    {
                        if (YesComments.Count > 0)
                        {
                            comment = YesComments[0];
                            YesComments.Remove(comment);
                        }
                        else if (NoComments.Count > 0)
                        {
                            comment = NoComments[0];
                            NoComments.Remove(comment);
                        }
                    }

                    if (comment != null)
                        _commentList.Add(comment);
                }

                return _commentList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        [Route("api/WebApi/BookMarkQuestionWeb")]
        public HttpResponseMessage BookMarkQuestionWeb(QuestionBookmarkWebModel questionBookmark)
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

        [HttpGet]
        [Route("api/WebApi/GetAllBookMarkWebById")]
        public List<PostQuestionDetailWebModel> GetAllBookMarkWebById(int userId)
        {
            List<PostQuestionDetailWebModel> questionDetail = new List<PostQuestionDetailWebModel>();
            try
            {
                db.Configuration.LazyLoadingEnabled = false;


                //int id = Convert.ToInt32(userId);

                questionDetail = (from q in db.Questions
                                  join b in db.BookMarks on q.Id equals b.QuestionId
                                  join u in db.Users on b.UserId equals u.UserID
                                  where q.IsDeleted == false && u.UserID != userId && b.IsBookmark == true
                                  select new PostQuestionDetailWebModel
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
                                      IsSlider = q.IsSlider,
                                      YesCount = db.Opinions.Where(o => o.QuestId == q.Id && o.IsAgree == true).Count(),
                                      NoCount = db.Opinions.Where(o => o.QuestId == q.Id && o.IsAgree == false).Count()
                                  }).OrderByDescending(p => p.CreationDate).ToList();

                return questionDetail;

                // return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, questionDetail, "GetBookmarkQuestion"));
            }
            catch (Exception ex)
            {
                return questionDetail;
                //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "GetBookmarkQuestion"));
            }
        }
                      

        #endregion
        #region "Get User Profile" 
        [HttpGet]
        [Route("api/WebApi/GetUserProfileWeb")]
        public UserProfile GetUserProfileWeb(int userid)
        {
            UserProfile UserProfile = new UserProfile();
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                if (!ModelState.IsValid)
                {
                    //   return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }

                //int id = Convert.ToInt32(UserID);

                UserProfile = (from t in db.Tokens
                               join u in db.Users on t.UserId equals u.UserID
                               where u.UserID == userid
                               select new UserProfile
                               {
                                   UserID = u.UserID,
                                   UserName = u.UserName,
                                   FirstName = u.FirstName,
                                   LastName = u.LastName,
                                   Email = u.Email,
                                   ImageURL = u.ImageURL,
                                   IsSocialLogin = u.SocialID == null ? false : true,
                                   LastLoginDate = u.ModifiedDate,
                                   BalanceToken = t.BalanceToken,
                                   TotalPostedQuestion = db.Questions.Where(p => p.OwnerUserID == userid && p.IsDeleted == false).Count(),
                                   TotalLikes = (from q in db.Questions
                                                 join o in db.Opinions on q.Id equals o.QuestId
                                                 where o.CommentedUserId== userid && q.IsDeleted == false
                                                 select o.Likes).Sum(),
                                   TotalDislikes = (from q in db.Questions
                                                    join o in db.Opinions on q.Id equals o.QuestId
                                                    where o.CommentedUserId == userid && q.IsDeleted == false
                                                    select o.Dislikes).Sum(),
                               }).FirstOrDefault();

                return UserProfile;

                // return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, UserProfile, "UserProfile"));
            }
            catch (Exception ex)
            {
                return UserProfile;
                //  OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                //  return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "UserProfile"));
            }
        }

        [HttpPost]
        [Route("api/WebApi/DeleteMyQuestion")]
        public Question DeleteMyQuestion(PostQuestionModel postQuestion)
        {

            Question quest = null;
            try
            {
                quest = db.Questions.Where(p => p.Id == postQuestion.Id && p.OwnerUserID == postQuestion.OwnerUserID).FirstOrDefault();
                if (quest == null)
                {
                    return quest;
                }

                //var opinionList = db.Opinions.Where(p => p.QuestId == quest.Id).ToList();
                //foreach(var opinion in opinionList)
                //{
                //    opinion.IsDeleted = true;
                //}

                quest.IsDeleted = true;
                quest.ModifiedDate = DateTime.Now.ToUniversalTime();
                db.Entry(quest).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return quest;

            }
            catch (Exception ex)
            {
                return quest;
            }
        }

        [HttpPost]
        [Route("api/WebApi/DeleteMyBelief")]
        public Opinion DeleteMyBelief(PostQuestionModel model)
        {

            Opinion _opinion = null;
            try
            {
                _opinion = db.Opinions.Where(o => o.Id == model.Id && o.CommentedUserId == model.OwnerUserID).FirstOrDefault();
                if (_opinion == null)
                {
                    return _opinion;
                }
                //must have IsDeleted dolumn if we are not going to actually delete it

                db.Opinions.Remove(_opinion);
                db.SaveChanges();

                return new Opinion();
            }
            catch (Exception ex)
            {
                return _opinion;
            }
        }

        [HttpGet]
        [Route("api/WebApi/CheckNotification")]
        public List<Notification> CheckNotification(int userId)
        {
            try
            {
                var user = db.Database
                    .SqlQuery<User>("SELECT * FROM [Users] WHERE [UserID] = @UserID", new SqlParameter("@UserID", userId))
                    .FirstOrDefault();
                
                var _notification = db.Notifications.Where(x => x.CommentedUserId == user.UserID && x.CreationDate >= user.ModifiedDate).ToList();
                if (_notification.Count == 0)
                {
                    _notification = (from q in db.Questions
                                     join n in db.Notifications on q.Id equals n.questId
                                     where q.OwnerUserID == user.UserID && q.IsDeleted == false
                                     && n.CreationDate >= user.ModifiedDate
                                     select n).ToList();
                }

                if (_notification.Count == 0)
                {
                    _notification = (from q in db.Questions
                                     join o in db.Opinions on q.Id equals o.QuestId
                                     join n in db.Notifications on o.Id equals n.CommentId
                                     where o.CommentedUserId == user.UserID && q.IsDeleted == false
                                     && n.CreationDate >= user.ModifiedDate
                                     select n).ToList();
                }

                return _notification;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion


        #region "Get Edit User Profile" 
        [HttpGet]
        [Route("api/WebApi/GetEditUserProfileWeb")]
        public UserModelProfileEditWeb GetEditUserProfileWeb(int userid)
        {
            UserModelProfileEditWeb UserProfile = new UserModelProfileEditWeb();
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                if (!ModelState.IsValid)
                {
                    //   return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
                }

                //int id = Convert.ToInt32(UserID);

                UserProfile = (from u in db.Users
                               where u.UserID == userid
                               select new UserModelProfileEditWeb
                               {
                                   UserId = u.UserID,
                                   UserName = u.UserName,
                                   FirstName = u.FirstName,
                                   LastName = u.LastName,
                                   Email = u.Email,
                                   Password = u.Password,
                                   ImageURL = u.ImageURL,
                                   IsSocialLogin = u.SocialID == null ? false : true
                               }).FirstOrDefault();

                UserProfile.Password = AesCryptography.Decrypt(UserProfile.Password);
                return UserProfile;

                //  UserProfile.Password

                // return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, UserProfile, "UserProfile"));
            }
            catch (Exception ex)
            {
                return UserProfile;
                //  OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                //  return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "UserProfile"));
            }
        }
        #endregion
        

        #region "Edit User Profile" 
        [HttpPost]
        [Route("api/WebApi/EditUserProfileWeb")]
        public dynamic EditUserProfileWeb(UserModelProfileEditWeb Model)
        {
            User UserProfile;
            dynamic _response = new ExpandoObject();
            try
            {
                UserProfile = db.Users.Where(p => p.UserID == Model.UserId).FirstOrDefault();
                if(UserProfile.UserName.Trim() != Model.UserName.Trim())
                {
                    if (CheckUserNameExist(Model.UserName.Trim()).isExist)
                    {
                        _response.success = false;
                        _response.message = "Username already exists. Please enter unique Username.";
                        return _response;
                    }
                }
                if (UserProfile.Email.Trim() != Model.Email.Trim())
                {
                    if (CheckEmailExist(Model.Email.Trim()))
                    {
                        _response.success = false;
                        _response.message = "Email already exists. Please enter valid Email.";
                        return _response;
                    }
                }

                UserProfile.UserName = Model.UserName;
                UserProfile.FirstName = Model.FirstName == null ? UserProfile.FirstName : Model.FirstName;
                UserProfile.LastName = Model.LastName == null ? UserProfile.LastName : Model.LastName;

                if (!Model.IsSocialLogin)
                {
                    UserProfile.Email = Model.Email;
                    UserProfile.Password = AesCryptography.Encrypt(Model.Password);
                }

                db.Entry(UserProfile).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                _response.success = true;
                return _response;
            }
            catch (Exception ex)
            {
                //throw ex;
            }
            return _response;
            #endregion
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

        #region Get Popular Hashtags
        [HttpGet]
        [Route("api/WebApi/GetPopularHashTags")]
        public List<PopularTag> GetPopularHashTags()
        {
            List<PopularTag> TopPopularHashTags = new List<PopularTag>();
            //try
            //{
            //    db.Configuration.LazyLoadingEnabled = false;


            //    var PopularHashTagsList = (from q in db.Questions
            //                                   //join u in db.Users on q.OwnerUserID equals u.UserID
            //                               where q.IsDeleted == false
            //                               select new
            //                               {
            //                                   HashTag = q.HashTags,
            //                                   QuestionId = q.Id
            //                               }).ToList();


            //    int count = 0;

            //    foreach (var item in PopularHashTagsList)
            //    {

            //        string[] splitHastags =  item.HashTag.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            //        foreach (var tag in splitHastags)
            //        {
            //            PopularTag _hashtag = new PopularTag();
            //            _hashtag.HashTag = tag;
            //            //_hashtag.QuestionId = item.QuestionId;
            //            //_hashtag.Count = PopularHashTagsList.Where(x => x.HashTag.Contains(tag)).ToList().Count;
            //            TopPopularHashTags.Add(_hashtag);
            //        }
            //    }

            //    foreach (var tag in TopPopularHashTags)
            //    {
            //        tag.Count = TopPopularHashTags.Where(x => x.HashTag == tag.HashTag).ToList().Count;
            //    }

            //    TopPopularHashTags = TopPopularHashTags.OrderByDescending(x => x.Count).Distinct().ToList();
            //}
            //catch (Exception ex)
            //{
            //    OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
            //}

            string[] tags= { "crypto", "health", "sports", "uspolitics", "india" };

            foreach (var tag in tags)
            {
                PopularTag _hashtag = new PopularTag();
                _hashtag.HashTag = tag;
 
                TopPopularHashTags.Add(_hashtag);
            }
            
            return TopPopularHashTags;
        }
        #endregion

        public class PopularTag
        {
            public string HashTag { get; set; }
            //public int QuestionId { get; set; }
            public int Count { get; set; }
        }


        [HttpPost]
        [Route("api/WebApi/PostOpinionWeb")]
        public Token PostOpinionWeb(PostAnswerWeb Model)
        {
            Token ObjToken = null;
            Opinion ObjOpinion = new Opinion();
            Notification notification = null;
            try
            {


                ObjToken = db.Tokens.Where(x => x.UserId == Model.CommentedUserId).FirstOrDefault();
                if (ObjToken.BalanceToken <= 0)
                {
                    return ObjToken;
                }

                Token token = new Token();
                ObjOpinion.QuestId = Model.QuestId;
                ObjOpinion.Comment = Model.Comment;
                ObjOpinion.CommentedUserId = Model.CommentedUserId;
                ObjOpinion.CreationDate = DateTime.Now.ToUniversalTime(); ;
                ObjOpinion.Likes = Model.Likes;
                ObjOpinion.IsAgree = Model.OpinionAgreeStatus;
                ObjOpinion.Dislikes = Model.Dislikes;
                db.Opinions.Add(ObjOpinion);
                db.SaveChanges();
                int CommentId = ObjOpinion.Id;
                //token = db.Tokens.Where(p => p.UserId == Model.CommentedUserId).FirstOrDefault();
                //token.BalanceToken = token.BalanceToken - 1;

                //db.Entry(token).State = System.Data.Entity.EntityState.Modified;
                //db.SaveChanges();
                notification = new Notification();
                notification.CommentedUserId = Model.CommentedUserId;
                notification.CommentId = CommentId;
                notification.questId = Model.QuestId;
                //notification.Like = Convert.ToBoolean(Model.Likes);
                //notification.Dislike = Convert.ToBoolean(Model.Dislikes);
                notification.Comment = true;
                notification.Dislike = false;
                notification.Like = false;
                notification.CreationDate = DateTime.Now.ToUniversalTime(); ;
                // notification.Status = 2;
                db.Notifications.Add(notification);
                db.SaveChanges();


                //  }

            }
            catch (Exception ex)
            {

            }
            return ObjToken;
        }


        [HttpPost]
        [Route("api/WebApi/PostLikeDislikeWeb")]
        public void PostLikeDislikeWeb(PostLikeDislikeModel Model)
        {


            Notification notification = null;
            Opinion opinion;

            string action = "";
            PushNotifications pNoty = new PushNotifications();
            try
            {
                int prevScore = 0;
                int newScore = 0;
                opinion = db.Opinions.Where(p => p.Id == Model.CommentId).FirstOrDefault();
                prevScore = Math.Max( (int) (opinion.Likes - opinion.Dislikes), 0);
 

                notification = db.Notifications.Where(x => x.CommentedUserId == Model.CommentedUserId && x.questId == Model.QuestId && x.CommentId == Model.CommentId).FirstOrDefault();

                if (notification == null)
                {


                    //if (Model.CommentStatus == CommentStatus.DisLike)
                    //{
                    //    notification.Dislike = true;
                    //    notification.Like = false;
                    //    action = "dislike";
                    //}
                    //else if (Model.CommentStatus == CommentStatus.Like)
                    //{
                    //    notification.Like = true;
                    //    notification.Dislike = false;
                    //    action = "like";
                    //}
                    //if (Model.CommentStatus == CommentStatus.RemoveLike)
                    //{
                    //    notification.Like = false;
                    //    action = "remove like";
                    //}
                    //else if (Model.CommentStatus == CommentStatus.RemoveDisLike)
                    //{
                    //    notification.Dislike = false;
                    //    action = "remove dislike";
                    //}


                    notification = new Notification();
                    notification.CommentedUserId = Model.CommentedUserId;
                    notification.CommentId = Model.CommentId;
                    notification.questId = Model.QuestId;
                    notification.Like = Convert.ToBoolean(Model.Likes);
                    notification.Dislike = Convert.ToBoolean(Model.Dislikes);
                    notification.Comment = false;
                    notification.CreationDate = Model.CreationDate;
                    // notification.Status = 3;
                    db.Notifications.Add(notification);
                    db.SaveChanges();

                    //opinion = db.Opinions.Where(p => p.Id == Model.CommentId).FirstOrDefault();

                    /////notification to mobile app
                    //if (Model.Likes == 0)
                    //{
                    //    //notification.Dislike = true;
                    //    notification.Like = false;
                    //    action = "dislike";
                    //}
                    //else if (Model.Likes == 1)
                    //{
                    //    notification.Like = true;
                    //    //notification.Dislike = false;
                    //    action = "like";
                    //}

                    //if (Model.Dislikes == 0)
                    //{
                    //    notification.Dislike = false;

                    //    action = "dislike";
                    //}
                    //else if (Model.Dislikes == 1)
                    //{

                    //    notification.Dislike = true;
                    //    action = "like";
                    //}

                    //
                    //int questId = opinion[0].QuestId;
                    //Question ques = db.Questions.Where(p => p.Id == questId).FirstOrDefault();
                    //User questOwner = db.Users.Where(u => u.UserID == ques.OwnerUserID).FirstOrDefault();
                    //User user = db.Users.Where(u => u.UserID == notification.CommentedUserId).FirstOrDefault();
                    //int OpinionUserID = opinion[0].CommentedUserId;
                    //User commentOwner = db.Users.Where(u => u.UserID == OpinionUserID).FirstOrDefault();
                    //if (questOwner != null && (!action.Contains("remove")))
                    //{
                    //    if (ques.OwnerUserID != notification.CommentedUserId)
                    //    {
                    //        //***** Notification to question owner
                    //        string finalMessage = GenerateTagsForQuestionWeb(notification.Like, notification.Dislike, false, user.FirstName + " " + user.LastName);

                    //        pNoty.SendNotification_Android(questOwner.DeviceToken, finalMessage, "QD", questId.ToString());

                    //        //***** Notification to Tagged Users
                    //        string taggedUser = ques.TaggedUser;

                    //        if (!string.IsNullOrEmpty(taggedUser))
                    //        {
                    //            var roleIds = taggedUser.Split(',').Select(s => int.Parse(s));
                    //            foreach (int items in roleIds)
                    //            {
                    //                if (notification.CommentedUserId != items)
                    //                {
                    //                    User data = db.Users.Find(items);
                    //                    if (data != null)
                    //                    {
                    //                        string finalMessage1 = user.FirstName + " " + user.LastName + " has " + action + " question in which you're tagged in.";

                    //                        pNoty.SendNotification_Android(data.DeviceToken, finalMessage1, "QD", questId.ToString());
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //    else if (ques.OwnerUserID == notification.CommentedUserId)
                    //    {
                    //        //in this block notification will send to tagged users
                    //        string taggedUser = ques.TaggedUser;

                    //        if (!string.IsNullOrEmpty(taggedUser))
                    //        {
                    //            var roleIds = taggedUser.Split(',').Select(s => int.Parse(s));
                    //            foreach (int items in roleIds)
                    //            {
                    //                User data = db.Users.Find(items);
                    //                if (data != null)
                    //                {
                    //                    string finalMessage = user.FirstName + " " + user.LastName + " has " + action + " question in which you're tagged in.";

                    //                    pNoty.SendNotification_Android(data.DeviceToken, finalMessage, "QD", questId.ToString());
                    //                }
                    //            }
                    //        }
                    //    }
                    //    if (commentOwner.UserID != notification.CommentedUserId)
                    //    {
                    //        //***** Notification to question owner
                    //        string finalMessage = GenerateTagsForOpinionWeb(notification.Like, notification.Dislike, false, user.FirstName + " " + user.LastName);

                    //        pNoty.SendNotification_Android(commentOwner.DeviceToken, finalMessage, "QD", questId.ToString());
                    //    } }


                    opinion.Likes = db.Notifications.Where(p => p.CommentId == opinion.Id && p.Like == true).Count();
                    opinion.Dislikes = db.Notifications.Where(p => p.CommentId == opinion.Id && p.Dislike == true).Count();
                    newScore = Math.Max((int)(opinion.Likes - opinion.Dislikes), 0);
                    db.Entry(opinion).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    

                }
                else
                {


                    notification.CommentedUserId = Model.CommentedUserId;
                    notification.CommentId = Model.CommentId;
                    notification.questId = Model.QuestId;
                    //if (Model.LikeOrDislke){
                    //    notification.Like = Convert.ToBoolean(Model.Likes);
                    //}
                    //else{
                    //    notification.Dislike = Convert.ToBoolean(Model.Dislikes);
                    //}
                    notification.Like = Convert.ToBoolean(Model.Likes);
                    notification.Dislike = Convert.ToBoolean(Model.Dislikes);
                    notification.CreationDate = Model.CreationDate;
                    notification.Comment = false;
                    // notification.Status = 3;
                    db.Entry(notification).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    //opinion = db.Opinions.Where(p => p.Id == Model.CommentId).FirstOrDefault();

                    ///notification to mobile app
                    //if (Model.Likes == 0)
                    //{
                    //    notification.Dislike = true;
                    //    notification.Like = false;
                    //    action = "dislike";
                    //}
                    //else if (Model.Likes == 1)
                    //{
                    //    notification.Like = true;
                    //    notification.Dislike = false;
                    //    action = "like";
                    //}




                    //int questId = opinion[0].QuestId;
                    //Question ques = db.Questions.Where(p => p.Id == questId).FirstOrDefault();
                    //User questOwner = db.Users.Where(u => u.UserID == ques.OwnerUserID).FirstOrDefault();
                    //User user = db.Users.Where(u => u.UserID == notification.CommentedUserId).FirstOrDefault();
                    //int OpinionUserID = opinion[0].CommentedUserId;
                    //User commentOwner = db.Users.Where(u => u.UserID == OpinionUserID).FirstOrDefault();
                    //if (questOwner != null && (!action.Contains("remove")))
                    //{
                    //    if (ques.OwnerUserID != notification.CommentedUserId)
                    //    {
                    //        ***** Notification to question owner
                    //        string finalMessage = GenerateTagsForQuestionWeb(notification.Like, notification.Dislike, false, user.FirstName + " " + user.LastName);

                    //        pNoty.SendNotification_Android(questOwner.DeviceToken, finalMessage, "QD", questId.ToString());

                    //        ***** Notification to Tagged Users
                    //        string taggedUser = ques.TaggedUser;

                    //        if (!string.IsNullOrEmpty(taggedUser))
                    //        {
                    //            var roleIds = taggedUser.Split(',').Select(s => int.Parse(s));
                    //            foreach (int items in roleIds)
                    //            {
                    //                if (notification.CommentedUserId != items)
                    //                {
                    //                    User data = db.Users.Find(items);
                    //                    if (data != null)
                    //                    {
                    //                        string finalMessage1 = user.FirstName + " " + user.LastName + " has " + action + " question in which you're tagged in.";

                    //                        pNoty.SendNotification_Android(data.DeviceToken, finalMessage1, "QD", questId.ToString());
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //    else if (ques.OwnerUserID == notification.CommentedUserId)
                    //    {
                    //        in this block notification will send to tagged users
                    //        string taggedUser = ques.TaggedUser;

                    //        if (!string.IsNullOrEmpty(taggedUser))
                    //        {
                    //            var roleIds = taggedUser.Split(',').Select(s => int.Parse(s));
                    //            foreach (int items in roleIds)
                    //            {
                    //                User data = db.Users.Find(items);
                    //                if (data != null)
                    //                {
                    //                    string finalMessage = user.FirstName + " " + user.LastName + " has " + action + " question in which you're tagged in.";

                    //                    pNoty.SendNotification_Android(data.DeviceToken, finalMessage, "QD", questId.ToString());
                    //                }
                    //            }
                    //        }
                    //    }
                    //    if (commentOwner.UserID != notification.CommentedUserId)
                    //    {
                    //        ***** Notification to question owner
                    //        string finalMessage = GenerateTagsForOpinionWeb(notification.Like, notification.Dislike, false, user.FirstName + " " + user.LastName);

                    //        pNoty.SendNotification_Android(commentOwner.DeviceToken, finalMessage, "QD", questId.ToString());
                    //    }
                    //}


                    opinion.Likes = db.Notifications.Where(p => p.CommentId == opinion.Id && p.Like == true).Count();
                    opinion.Dislikes = db.Notifications.Where(p => p.CommentId == opinion.Id && p.Dislike == true).Count();
                    newScore = Math.Max((int)(opinion.Likes - opinion.Dislikes), 0);
                    db.Entry(opinion).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();



                }

                //give or take tokens 

                Token userToken = db.Tokens.Where(x => x.UserId == opinion.CommentedUserId).FirstOrDefault();

                userToken.BalanceToken = userToken.BalanceToken - prevScore + newScore;
                db.Entry(userToken).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                //Task.Run(() => this.LikeDislikeNotification(Model, notification));

            }
            catch (Exception ex)
            {

            }
        }

        public Boolean LikeDislikeNotification(PostLikeDislikeModel Model, Notification notification)
        {
            string action = "";
            PushNotifications pNoty = new PushNotifications();
            if (notification == null)
            {
                //notification = new Notification();
                //notification.CommentedUserId = Model.CommentedUserId;
                //notification.CommentId = Model.CommentId;
                //notification.questId = Model.QuestId;
                //notification.Like = Convert.ToBoolean(Model.Likes);
                //notification.Dislike = Convert.ToBoolean(Model.Dislikes);
                //notification.CreationDate = Model.CreationDate;
                //db.Notifications.Add(notification);
                //db.SaveChanges();

                ///notification to mobile app
                if (Model.Likes == 0)
                {
                    notification.Dislike = true;
                    notification.Like = false;
                    action = "dislike";
                }
                else if (Model.Likes == 1)
                {
                    notification.Like = true;
                    notification.Dislike = false;
                    action = "like";
                }
                List<Opinion> opinion = db.Opinions.Where(p => p.Id == Model.CommentId).ToList();
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
                        string finalMessage = GenerateTagsForQuestionWeb(notification.Like, notification.Dislike, false, user.FirstName + " " + user.LastName);

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
                                        string finalMessage1 = user.FirstName + " " + user.LastName + " has " + action + " question in which you're tagged in.";

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
                                    string finalMessage = user.FirstName + " " + user.LastName + " has " + action + " question in which you're tagged in.";

                                    pNoty.SendNotification_Android(data.DeviceToken, finalMessage, "QD", questId.ToString());
                                }
                            }
                        }
                    }
                    if (commentOwner.UserID != notification.CommentedUserId)
                    {
                        //***** Notification to question owner
                        string finalMessage = GenerateTagsForOpinionWeb(notification.Like, notification.Dislike, false, user.FirstName + " " + user.LastName);

                        pNoty.SendNotification_Android(commentOwner.DeviceToken, finalMessage, "QD", questId.ToString());
                    }
                }
            }
            else
            {

                //notification.CommentedUserId = Model.CommentedUserId;
                //notification.CommentId = Model.CommentId;
                //notification.questId = Model.QuestId;
                ////if (Model.LikeOrDislke){
                ////    notification.Like = Convert.ToBoolean(Model.Likes);
                ////}
                ////else{
                ////    notification.Dislike = Convert.ToBoolean(Model.Dislikes);
                ////}
                //notification.Like = Convert.ToBoolean(Model.Likes);
                //notification.Dislike = Convert.ToBoolean(Model.Dislikes);
                //notification.CreationDate = Model.CreationDate;
                //db.Entry(notification).State = System.Data.Entity.EntityState.Modified;
                //db.SaveChanges();

                ///notification to mobile app
                if (Model.Likes == 0)
                {
                    notification.Dislike = true;
                    notification.Like = false;
                    action = "dislike";
                }
                else if (Model.Likes == 1)
                {
                    notification.Like = true;
                    notification.Dislike = false;
                    action = "like";
                }

                List<Opinion> opinion = db.Opinions.Where(p => p.Id == Model.CommentId).ToList();
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
                        string finalMessage = GenerateTagsForQuestionWeb(notification.Like, notification.Dislike, false, user.FirstName + " " + user.LastName);

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
                                        string finalMessage1 = user.FirstName + " " + user.LastName + " has " + action + " question in which you're tagged in.";

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
                                    string finalMessage = user.FirstName + " " + user.LastName + " has " + action + " question in which you're tagged in.";

                                    pNoty.SendNotification_Android(data.DeviceToken, finalMessage, "QD", questId.ToString());
                                }
                            }
                        }
                    }
                    if (commentOwner.UserID != notification.CommentedUserId)
                    {
                        //***** Notification to question owner
                        string finalMessage = GenerateTagsForOpinionWeb(notification.Like, notification.Dislike, false, user.FirstName + " " + user.LastName);

                        pNoty.SendNotification_Android(commentOwner.DeviceToken, finalMessage, "QD", questId.ToString());
                    }
                }

            }

            return true;
        }


        public string GenerateTagsForQuestionWeb(bool? like, bool? dislike, bool? comment, string UserName)
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
        public string GenerateTagsForOpinionWeb(bool? like, bool? dislike, bool? comment, string UserName)
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

        public string GenerateTagsForTaggedUsersWeb(bool? like, bool? dislike, bool? comment, string ActionUserName)
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


        #region "Get All Search Users" 
        [HttpGet]
        [Route("api/WebApi/GetAllTaggedDropWeb")]
        public List<ViewModelUser> GetAllTaggedDropWeb()
        {
            List<ViewModelUser> user = new List<ViewModelUser>();
            try
            {

                user = (from u in db.Users
                        where u.IsAdmin == false
                        select new ViewModelUser
                        {
                            UserID = u.UserID,
                            UserName = u.UserName,
                            Name = u.FirstName + " " + u.LastName,
                            ImageURL = string.IsNullOrEmpty(u.ImageURL) ? "" : u.ImageURL,
                            CreatedDate = u.CreatedDate
                        }).OrderBy(p => p.UserID).ToList();


                return user;
            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                return user;
            }
        }
        #endregion



        [HttpPost]
        [Route("api/WebApi/UploadProfileWeb")]
        public HttpResponseMessage UploadProfileWeb()
        {
            string imageName = null;
            string _SiteRoot = WebConfigurationManager.AppSettings["SiteImgPath"];
            string _SiteURL = WebConfigurationManager.AppSettings["SiteImgURL"];

            var httpRequest = HttpContext.Current.Request;
            //Upload Image
            var postedFile = httpRequest.Files["Image"];
            int UserId = Convert.ToInt32(httpRequest["userId"]);
            //Create custom filename
            try
            {


                imageName = new String(Path.GetFileNameWithoutExtension(postedFile.FileName).Take(10).ToArray()).Replace(" ", "-");
                string guid = Guid.NewGuid().ToString();
                imageName = imageName + guid + Path.GetExtension(postedFile.FileName);
                var filePath = HttpContext.Current.Server.MapPath("~/Content/upload/ProfileImage/" + imageName);
                postedFile.SaveAs(filePath);
                // ResizeImage.Resize_Image_Thumb(filePath, filePath, "_T_" + filePath, 400, 400);



                //System.Drawing.Image image = System.Drawing.Image.FromFile(filePath);
                //float aspectRatio = (float)image.Size.Width / (float)image.Size.Height;
                //int newHeight = 200;
                //int newWidth = Convert.ToInt32(aspectRatio * newHeight);
                //System.Drawing.Bitmap thumbBitmap = new System.Drawing.Bitmap(newWidth, newHeight);
                //System.Drawing.Graphics thumbGraph = System.Drawing.Graphics.FromImage(thumbBitmap);
                //thumbGraph.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                //thumbGraph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                //thumbGraph.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                //var imageRectangle = new Rectangle(400, 400, newWidth, newHeight);
                //thumbGraph.DrawImage(image, imageRectangle);
                //thumbBitmap.Save(filePath);
                //thumbGraph.Dispose();
                //thumbBitmap.Dispose();
                //image.Dispose();


                // Save to DB
                User Entry = null;
                using (OpozeeDbEntities db = new OpozeeDbEntities())
                {
                    Entry = db.Users.Where(x => x.UserID == UserId).FirstOrDefault();

                    Entry.ImageURL = _SiteURL + "/ProfileImage/" + imageName;
                    db.Entry(Entry).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    //int userID = entity.UserID;
                    //entity = db.Users.Find(userID);


                }
            }
            catch (Exception exp)
            {

                throw;
            }
            return Request.CreateResponse(HttpStatusCode.Created);
        }
        public Image resizeImage(int newWidth, int newHeight, string stPhotoPath)
        {
            Image imgPhoto = Image.FromFile(stPhotoPath);

            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;

            //Consider vertical pics
            if (sourceWidth < sourceHeight)
            {
                int buff = newWidth;
                newWidth = newHeight;
                newHeight = buff;
            }

            int sourceX = 0, sourceY = 0, destX = 0, destY = 0;
            float nPercent = 0, nPercentW = 0, nPercentH = 0;

            nPercentW = ((float)newWidth / (float)sourceWidth);
            nPercentH = ((float)newHeight / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((newWidth -
                          (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((newHeight -
                          (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);


            Bitmap bmPhoto = new Bitmap(newWidth, newHeight,
                          PixelFormat.Format24bppRgb);

            bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                         imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.Black);
            grPhoto.InterpolationMode =
                System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            imgPhoto.Dispose();
            return bmPhoto;
        }



        #region "Socail Login" 
        [HttpPost]
        [Route("api/WebApi/SigninThirdPartyWeb")]
        public dynamic SigninThirdPartyWeb(InputSignInWithThirdPartyWebModel input)
        {
            UserLoginWeb ObjLogin = new UserLoginWeb();
            dynamic _response = new ExpandoObject();
            try
            {
                if (!ModelState.IsValid)
                {
                    //  return Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
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

                    (bool IsExist, User _user) = this.CheckUserNameExist(entity.UserName);
                    if (IsExist)
                    {
                        if (_user.SocialID != entity.SocialID)
                            entity.UserName += Helper.Random4DigitGenerator();
                    }

                    if (!string.IsNullOrEmpty(input.Password))
                    {
                        entity.Password = AesCryptography.Encrypt(input.Password);
                    }
                    entity.DeviceType = input.DeviceType != null && input.DeviceType != "" ? input.DeviceType : entity.DeviceType;
                    entity.DeviceToken = input.DeviceToken != null && input.DeviceToken != "" ? input.DeviceToken : entity.DeviceToken;
                    entity.ImageURL = entity.ImageURL;
                    entity.ModifiedDate = DateTime.Now.ToUniversalTime();

                    if (entity.ReferralCode == null)
                        entity.ReferralCode = Helper.GenerateReferralCode();

                    db.Entry(entity).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    int userID = entity.UserID;
                    entity = db.Users.Find(userID);
                    
                    ObjLogin.LastLoginDate = entity.ModifiedDate;
                    ObjLogin.ReferralCode = entity.ReferralCode;

                    ObjLogin.Id = entity.UserID;
                    ObjLogin.Email = entity.Email;
                    ObjLogin.ImageURL = entity.ImageURL;
                    ObjLogin.BalanceToken = db.Tokens.Where(x => x.UserId == entity.UserID).FirstOrDefault() == null
                        ? 0 : db.Tokens.Where(x => x.UserId == entity.UserID).FirstOrDefault().BalanceToken ?? 0;

                    var totalRef = db.Referrals.Where(x => x.ReferredId == entity.UserID).ToList();
                    ObjLogin.TotalReferred = totalRef == null ? 0 : totalRef.Count;
                    ObjLogin.IsSocialLogin = true;

                    _response.success = true;
                    _response.data = ObjLogin;
                    return _response;

                   // return ObjLogin;
                    //  return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, entity, "UserData"));
                }
                else
                {
                    entity = new User();
                    entity.UserName = input.FirstName + input.LastName + Helper.Random4DigitGenerator();
                    entity.FirstName = input.FirstName;
                    entity.LastName = input.LastName;
                    entity.Email = input.Email;

                    bool Email = false;
                    Email = OpozeeLibrary.Utilities.Helper.IsValidEmail(input.Email);
                    if (!string.IsNullOrEmpty(input.Password))
                    {
                        entity.Password = AesCryptography.Encrypt(input.Password);
                    }

                    if (CheckEmailExist(input.Email))
                    {
                        _response.success = false;
                        _response.message = "Email already exists.";
                        return _response;
                    }

                    entity.DeviceType = input.DeviceType;
                    entity.DeviceToken = input.DeviceToken;
                    entity.CreatedDate = DateTime.Now.ToUniversalTime(); 
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

                    if (input.ImageURL != null && input.ImageURL != "")
                    {
                        try
                        {
                            string strTempImageSave = OpozeeLibrary.Utilities.ResizeImage.Download_Image(input.ImageURL);
                            string profileFilePath = _SiteURL + "/ProfileImage/" + strTempImageSave;
                            strIamgeURLfordb = profileFilePath;
                            entity.ImageURL = profileFilePath;
                        }
                        catch (Exception ex)
                        {
                            strThumbnailURLfordb = strThumbnailImage;
                            strIamgeURLfordb = strThumbnailImage;
                        }
                    }
                    else
                    {
                        entity.ImageURL = _SiteURL + "/ProfileImage/opozee-profile.png";
                    }
                    // entity.ImageURL = strIamgeURLfordb;
                    entity.ReferralCode = Helper.GenerateReferralCode();
                    db.Users.Add(entity);
                    db.SaveChanges();

                    Token token = new Token();
                    int userID = entity.UserID;
                    token.TotalToken = 500;
                    token.BalanceToken = 500;
                    token.UserId = userID;
                    db.Tokens.Add(token);
                    db.SaveChanges();
                    entity = db.Users.Find(userID);
                                        
                    ObjLogin.LastLoginDate = DateTime.Now.ToUniversalTime();
                    ObjLogin.ReferralCode = entity.ReferralCode;

                    ObjLogin.Id = entity.UserID;
                    ObjLogin.Email = entity.Email;
                    ObjLogin.ImageURL = entity.ImageURL;
                    ObjLogin.BalanceToken = token.BalanceToken ?? 0;
                    ObjLogin.IsSocialLogin = true;

                    _response.success = true;
                    _response.data = ObjLogin;
                    return _response;
                    //return ObjLogin;

                    // return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, entity, "UserData"));
                }
            }
            catch (Exception ex)
            {
                LogHelper.CreateLog3(ex, Request);
                _response.success = true;
                _response.data = ObjLogin;
                return _response;
                //return ObjLogin;
                //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "UserData"));
            }
        }

       

        #endregion


        #region "Get All Posts" 
        [HttpPost]
        [Route("api/WebApi/GetAllPostsQuestionEditWeb")]
        public List<PostQuestionDetailWebModel> GetAllPostsQuestionEditWeb(PagingModel model)
        {
            //    AllUserQuestions questionDetail = new AllUserQuestions();

            model.Search = model.Search ?? "";

            int Total = model.TotalRecords;
            int pageSize = 10; // set your page size, which is number of records per page
            int page = model.PageNumber;
            int skip = pageSize * (page - 1);

            //int canPage = skip < Total;


            List<PostQuestionDetailWebModel> questionDetail = new List<PostQuestionDetailWebModel>();
            try
            {
                db.Configuration.LazyLoadingEnabled = false;


                questionDetail = (from q in db.Questions
                                  join u in db.Users on q.OwnerUserID equals u.UserID
                                  where q.IsDeleted == false && q.OwnerUserID == model.UserId  // && q.PostQuestion.Contains(model.Search)
                                  select new PostQuestionDetailWebModel
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
                                      TotalRecordcount = db.Questions.Count(x => x.IsDeleted == false && x.OwnerUserID == model.UserId)

                                  }).OrderByDescending(p => p.Id).Skip(skip).Take(pageSize).ToList();




                return questionDetail;
                //return Request.CreateResponse(JsonResponse.GetResponse(ResponseCode.Success, questionDetail, "AllUserQuestions"));
            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                return questionDetail;
            }
        }
        #endregion



        #region "Get All Posts" 
        [HttpGet]
        [Route("api/WebApi/GetPostedQuestionEditWeb")]
        public PostQuestionModel GetPostedQuestionEditWeb(int QuestionId)
        {

            PostQuestionModel questionDetail = new PostQuestionModel();
            try
            {
                db.Configuration.LazyLoadingEnabled = false;


                questionDetail = (from q in db.Questions
                                  join u in db.Users on q.OwnerUserID equals u.UserID
                                  where q.Id == QuestionId
                                  select new PostQuestionModel
                                  {
                                      Id = q.Id,
                                      PostQuestion = q.PostQuestion,
                                      TaggedUser = q.TaggedUser,
                                      HashTags = q.HashTags,


                                  }).FirstOrDefault();

                return questionDetail;
                //return Request.CreateResponse(JsonResponse.GetResponse(ResponseCode.Success, questionDetail, "AllUserQuestions"));
            }
            catch (Exception ex)
            {
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                return questionDetail;
            }
        }
        #endregion

        #region "Post Question" 
        [HttpPost]
        [Route("api/WebApi/EditPostQuestionWeb")]
        public Question EditPostQuestionWeb([FromBody] PostQuestionModel postQuestion)
        {

            Question quest = null;
            try
            {

                if (!ModelState.IsValid)
                {
                    return quest; ;
                }

                quest = db.Questions.Where(p => p.Id == postQuestion.Id && p.OwnerUserID == postQuestion.OwnerUserID).FirstOrDefault();
                if (quest == null)
                {
                    return quest;
                }
                //quest = new Question();
                quest.PostQuestion = postQuestion.PostQuestion;
                quest.HashTags = postQuestion.HashTags;
                quest.ModifiedDate = DateTime.Now.ToUniversalTime(); 
                db.Entry(quest).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return quest;
                //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Success, quest, "Question"));
                //}
            }
            catch (Exception ex)
            {
                return quest;
                OpozeeLibrary.Utilities.LogHelper.CreateLog3(ex, Request);
                //return Request.CreateResponse(HttpStatusCode.OK, JsonResponse.GetResponse(ResponseCode.Failure, ex.Message, "Question"));
            }
        }
        #endregion


        #region "Delete Question" 
        [HttpPost]
        [Route("api/WebApi/DeletePostQuestionWeb")]
        public Question DeletePostQuestionWeb(PostQuestionModel postQuestion)
        {

            Question quest = null;
            try
            {
                quest = db.Questions.Where(p => p.Id == postQuestion.Id && p.OwnerUserID == postQuestion.OwnerUserID).FirstOrDefault();
                if (quest == null)
                {
                    return quest;
                }
                //quest = new Question();
                quest.IsDeleted = true;
                quest.ModifiedDate = DateTime.Now.ToUniversalTime(); 
                db.Entry(quest).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return quest;

            }
            catch (Exception ex)
            {
                return quest;

            }
        }
        #endregion

        #region "Mail APIs" 
        [HttpPost]
        [Route("api/WebApi/SendContactMail")]
        public async Task<dynamic> SendContactMail(ContactMail model)
        {
            dynamic _response = new ExpandoObject();
            try
            {

                string recepientName = model.Firstname + " " + model.LastName;
                string recepientEmail = model.Email;
                string subject = "Message from " + recepientName;

                bool isHtml = true;

                string pathHTMLFile = HttpContext.Current.Server.MapPath("~/Content/mail-template/ContactMailTemplate.html");
                string TEMPLATE = File.ReadAllText(pathHTMLFile);
                TEMPLATE = TEMPLATE.Replace("##MESSAGE##", model.Message);
                TEMPLATE = TEMPLATE.Replace("##NAME##", UppercaseFirst(recepientName));
                TEMPLATE = TEMPLATE.Replace("##PHONE##", model.Phone);
                TEMPLATE = TEMPLATE.Replace("##EMAIL##", model.Email);

                string body = TEMPLATE;

                (bool success, string errorMsg) = await EmailSender.SendEmailAsync(recepientName, recepientEmail, subject, body, isHtml, true);

                _response.success = success;

                if (!success)
                    return BadRequest(errorMsg);
            }
            catch (Exception ex)
            {
            }
            return _response;
        }

        [HttpPost]
        [Route("api/WebApi/SendWelcomMail")]
        public async Task<dynamic> SendWelcomMail(ContactMail model)
        {
            dynamic _response = new ExpandoObject();
            try
            {

                string recepientName = model.Firstname;
                string recepientEmail = model.Email;
                string subject = "Welcome to Opozee";

                bool isHtml = true;

                string pathHTMLFile = HttpContext.Current.Server.MapPath("~/Content/mail-template/WelcomeMailTemplate.html");
                string TEMPLATE = File.ReadAllText(pathHTMLFile);
                TEMPLATE = TEMPLATE.Replace("##NAME##", UppercaseFirst(recepientName));

                string body = TEMPLATE;

                (bool success, string errorMsg) = await EmailSender.SendEmailAsync(recepientName, recepientEmail, subject, body, isHtml);

                _response.success = success;

                if (!success)
                    return BadRequest(errorMsg);
            }
            catch (Exception ex)
            {
            }
            return _response;
        }
        #endregion


        static string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }
    }
}