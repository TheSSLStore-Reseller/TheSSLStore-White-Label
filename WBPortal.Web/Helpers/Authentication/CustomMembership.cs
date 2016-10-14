using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using WBSSLStore.Service;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Domain;
using System.Web.Mvc;

namespace WBSSLStore.Web.Helpers.Authentication
{
    public class CustomMembership : MembershipProvider
    {
        private IRepository<User> repo;

        private int siteid
        {
            get
            {
                //if (CurrentSiteSettings.CurrentSite != null)
                //    return CurrentSiteSettings.SiteID;
                //else
                //{
                    if(!string.IsNullOrEmpty (ApplicationName))
                        return  Convert.ToInt32(ApplicationName);
                    else 
                        return WBSSLStore.Web.Helpers.Caching.SiteCacher.CurrentSite.ID;
                //}
               //   
            }
        }

        public override string ApplicationName
        {
            get;
            set;
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotImplementedException();
        }

        public override bool EnablePasswordReset
        {
            get { throw new NotImplementedException(); }
        }

        public override bool EnablePasswordRetrieval
        {
            get { throw new NotImplementedException(); }
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {


            SSLStoreUser user = null;

            bool AlreadyChecked = false;

            //Attempt to see if User has already being checked during this request
            if (HttpContext.Current.Items.Contains("SSLMEMBERSHIPGETUSER" + this.ApplicationName))
            {
                object o = HttpContext.Current.Items["SSLMEMBERSHIPGETUSER" + this.ApplicationName];
                if (o != null && o is SSLStoreUser)
                {
                    user = (SSLStoreUser)o;
                }
                AlreadyChecked = true;
            }

            if (AlreadyChecked == false)
            {
                repo = DependencyResolver.Current.GetService<IRepository<User>>();
                User usertemp = repo.Find(x => x.Email == username && x.SiteID == siteid && x.RecordStatusID == 1).FirstOrDefault();

                if (usertemp != null && usertemp.ID > 0)
                {
                    user = new SSLStoreUser(usertemp);
                    HttpContext.Current.Items["SSLMEMBERSHIPGETUSER"  +this.ApplicationName] = user;
                }

            }
            return user;

        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { throw new NotImplementedException(); }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { throw new NotImplementedException(); }
        }

        public override int MinRequiredPasswordLength
        {
            get { throw new NotImplementedException(); }
        }

        public override int PasswordAttemptWindow
        {
            get { throw new NotImplementedException(); }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { throw new NotImplementedException(); }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { throw new NotImplementedException(); }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { throw new NotImplementedException(); }
        }

        public override bool RequiresUniqueEmail
        {
            get { throw new NotImplementedException(); }
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        public override bool ValidateUser(string username, string password)
        {
            bool result = false;


            bool AlreadyChecked = false;
            //Checks if User has already being checked during lifetime of this request.
            if (HttpContext.Current.Items["SSLMEMBERSHIPVALIDATEUSER" + ApplicationName] != null)
            {
                result = Convert.ToBoolean(HttpContext.Current.Items["SSLMEMBERSHIPVALIDATEUSER" + ApplicationName].ToString());
                AlreadyChecked = true;
            }

            if (AlreadyChecked == false)
            {
                repo = DependencyResolver.Current.GetService<IRepository<User>>();
                User usertemp = repo.Find(x => x.Email == username  && x.SiteID == siteid  && x.RecordStatusID == 1).FirstOrDefault();
                

                if (usertemp != null && usertemp.ID > 0 && usertemp.PasswordHash == WBHelper.CreatePasswordHash(password, usertemp.PasswordSalt))
                {
                   result = true;
                }
                HttpContext.Current.Items["SSLMEMBERSHIPVALIDATEUSER" + ApplicationName] = result;
              
               
            }

            return result;
        }
    }
}