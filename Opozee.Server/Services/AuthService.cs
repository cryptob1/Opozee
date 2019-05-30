using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Opozee.Models.Models;
using OpozeeLibrary.Utilities;
using Opozee.Models;
using opozee.Enums;

namespace Opozee.Server.Services
{
    public class AuthService 
    {
        OpozeeDbEntities db = new OpozeeDbEntities();

        public (bool status, User user) Login(string Email, string Password)
        {
            try
            {

                var userInfo = db.Users.Where(a => a.Email == Email && a.EmailConfirmed == true && (a.IsAdmin ?? false) == false).FirstOrDefault();
                if (userInfo != null)
                {
                    return ((string.Compare(AesCryptography.Encrypt(Password), userInfo.Password) == 0), userInfo);
                }
                else
                {
                    return (false, null);
                }
            }
            catch (Exception ex)
            {
                return (false, null);
            }
        }

        public (STATUS status, UserLoginWeb user) LoginUser(string Email, string Password)
        {
            try
            {
                UserLoginWeb ObjLogin = new UserLoginWeb();
                using (OpozeeDbEntities db = new OpozeeDbEntities())
                {
                    var v = db.Users.Where(a => a.Email == Email && (a.IsAdmin ?? false) == false).FirstOrDefault();
                    if (v != null)
                    {
                        if (v.EmailConfirmed == true)
                        {
                            ObjLogin.Token = AesCryptography.Encrypt(Password);
                            //ObjLogin.Token = AesCryptography.Decrypt(ObjLogin.Token);
                            if (string.Compare(AesCryptography.Encrypt(Password), v.Password) == 0)
                            {
                                ObjLogin.Id = v.UserID;
                                ObjLogin.Email = v.Email;
                                ObjLogin.ImageURL = v.ImageURL;

                                ObjLogin.BalanceToken = db.Tokens.Where(x => x.UserId == v.UserID).FirstOrDefault() == null
                                ? 0 : db.Tokens.Where(x => x.UserId == v.UserID).FirstOrDefault().BalanceToken ?? 0;

                                var totalRef = db.Referrals.Where(x => x.ReferralUserId == v.UserID).ToList();
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
                                ObjLogin.UserName = v.UserName;

                                return (STATUS.SUCCESS, ObjLogin); //succes
                            }
                            else
                            {
                                return (STATUS.WRONG_PASSWORD, ObjLogin); //wrong password
                            }
                        }
                        else
                        {
                            return (STATUS.EMAIL_NOT_VERIFIED, ObjLogin); //email not verified
                        }
                    }
                    else
                    {
                        ObjLogin.Id = -1;
                        return (STATUS.NOT_EXIST, ObjLogin); //not exist
                    }
                }
            }
            catch (Exception ex)
            {
                return (STATUS.EXCEPTION, null); // error
            }
        }
    }
}