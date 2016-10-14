using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using WBSSLStore.Domain;
using WBSSLStore.Data.Infrastructure;
using System.Web.Mvc;

namespace WBSSLStore.Web.Helpers.Authentication
{
    public class SSLStoreUser : System.Web.Security.MembershipUser, IComparable
    {
        //private SiteSetting _siteSetting;
        private WBSSLStore.Domain.User _user;

        private IRepository<User> repo;

        public SSLStoreUser(int UserID)
        {
            //CheckPortal();
            repo = DependencyResolver.Current.GetService<IRepository<User>>();
             _user = repo.Find(x => x.ID == UserID && x.RecordStatusID == 1).FirstOrDefault();
            
        }

        /// <summary>
        /// Initialize directly with Row without db trip
        /// </summary>
        /// <param name="objUser"></param>
        public SSLStoreUser(User objUser)
        {
            _user = objUser;
        }

        /// <summary>
        /// Gets Access to the details of the User.
        /// </summary>
        public User Details
        {
            get
            {
                return _user;
            }
        }


        public override bool ChangePassword(string oldPassword, string newPassword)
        {
            return base.ChangePassword(oldPassword, newPassword);
        }

        public override bool ChangePasswordQuestionAndAnswer(string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            return base.ChangePasswordQuestionAndAnswer(password, newPasswordQuestion, newPasswordAnswer);
        }

        public override string Email
        {
            get
            {
                if (_user != null)
                    return _user.Email;
                else
                    return string.Empty;
            }
            set
            {
                _user.Email = value;
            }
        }

        public override DateTime CreationDate
        {
            get
            {
                
                return DateTimeWithZone.Now;
            }
        }

        public override bool IsApproved
        {
            get
            {
                return base.IsApproved;
            }
            set
            {
                base.IsApproved = value;
            }
        }

        public override DateTime LastActivityDate
        {
            get
            {
                return base.LastActivityDate;
            }
            set
            {
                base.LastActivityDate = value;
            }
        }


        public override DateTime LastLoginDate
        {
            get
            {
                return base.LastLoginDate;
            }
            set
            {
                base.LastLoginDate = value;
            }
        }

        public override string ResetPassword()
        {
            return base.ResetPassword();
        }

        public override bool Equals(object obj)
        {
            bool result = false;
            if (obj != null && obj is SSLStoreUser)
            {
                SSLStoreUser user = (SSLStoreUser)obj;
                if (user.Details.ID == this.Details.ID)
                    result = true;
            }
            return result;

        }


        public override bool IsLockedOut
        {
            get
            {
                return base.IsLockedOut;
            }
        }

        public override DateTime LastLockoutDate
        {
            get
            {
                return base.LastLockoutDate;
            }
        }


        public override DateTime LastPasswordChangedDate
        {
            get
            {
                return base.LastPasswordChangedDate;
            }
        }

        public override string GetPassword()
        {
            return base.GetPassword();
        }

        public override string GetPassword(string passwordAnswer)
        {
            return base.GetPassword(passwordAnswer);
        }

        public override string PasswordQuestion
        {
            get
            {
                return base.PasswordQuestion;
            }
        }

        public override string ProviderName
        {
            get
            {
                return base.ProviderName;
            }
        }

        public override bool UnlockUser()
        {
            return base.UnlockUser();
        }

        public override string UserName
        {
            get
            {
                return base.UserName;
            }
        }

        public override string ResetPassword(string passwordAnswer)
        {
            return base.ResetPassword(passwordAnswer);
        }

        public override string ToString()
        {
            if (_user != null)
                return _user.ID.ToString();
            else
                return "UnAuthenticated";

        }

        /// <summary>
        /// Compares, Useful in Sorting.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            int result = 0;
            if (obj != null && obj is SSLStoreUser)
            {
                SSLStoreUser user = (SSLStoreUser)obj;
                if (user.Details.ID == this.Details.ID)
                    result = 0;
                else if (user.Details.ID < this.Details.ID)
                    result = 1;
                else
                    result = -1;
            }
            return result;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}