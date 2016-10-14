using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Mvc;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Domain;

namespace WBSSLStore.Web.Helpers.Authentication
{
    public class CustomRoleProvider : RoleProvider
    {
        private IRepository<User> repo;
        private int Siteid
        {
            get
            {
                if (!string.IsNullOrEmpty(ApplicationName))
                    return Convert.ToInt32(ApplicationName);
                else
                    return WBSSLStore.Web.Helpers.Caching.SiteCacher.GetCached().ID;
            }
        }

        public override string ApplicationName
        {
            get;
            set;
        }
        public override string[] GetRolesForUser(string username)
        {
            // use the DB to build a list of roles and return them.  
            string[] roles = null;

            repo = DependencyResolver.Current.GetService<IRepository<User>>();
            User usertemp = repo.Find(x => x.Email == username && x.SiteID == Siteid && x.RecordStatusID == 1).FirstOrDefault();
            if (usertemp != null)
            {
                roles = new string[] { ((UserType)usertemp.UserTypeID).ToString().ToLower() };
            }
            else
            {
                throw new Exception("No User Found With Username:==>" + username);
            }
            return roles;
        }

        // Implement these methods as needed

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }
        public override string[] GetAllRoles()
        {
            System.Collections.ArrayList data = new System.Collections.ArrayList();
            foreach (string enm in Enum.GetNames(typeof(WBSSLStore.Domain.UserType)))
            {
                data.Add(enm.ToString().ToLower());
            }
            return (string[])data.ToArray(typeof(string));
        }
        public override string[] GetUsersInRole(string roleName)
        {

            throw new NotImplementedException();
        }
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }
        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }
        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool IsUserInRole(string username, string roleName)
        {

            repo = DependencyResolver.Current.GetService<IRepository<User>>();
            User usertemp = repo.Find(x => x.Email == username && x.SiteID == Siteid && x.RecordStatusID == 1).FirstOrDefault();
            if (usertemp != null && ((UserType)usertemp.UserTypeID).ToString().ToLower().Equals(roleName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }

        }
      
    }
}