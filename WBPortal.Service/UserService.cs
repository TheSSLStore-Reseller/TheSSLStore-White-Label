using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WBSSLStore.Domain;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Service.ViewModels;
using WBSSLStore.Data.Repository;
using System.Web;

namespace WBSSLStore.Service
{
    public interface IUserService
    {
        decimal? GetTotalPurchase(int UserID);
        int GetCertificateCount(int UserID);
        bool SaveReseller(ResellerContract resellercontract, int LangID, int SMTPID);
        bool SaveReseller(ResellerContract resellercontract, int LangID, int SMTPID, UserOptions objUserOption);
        bool SaveCustomer(User user, int LangID, int SMTPID);
        bool SaveAdminusers(User user, int LangID, int SMTPID);
        bool UpdateUserStatus(int UserID, int SiteID, int? LangID, int? SMTPID);
        bool DeleteUser(int UserID, int SiteID);
        bool EmailExist(string Email, int SiteID, int UserID);
        IQueryable<UserExt> GetResellerData(int SiteID, string ResellerName, string Email, RecordStatus? eRecordStatus, DateTime dtRegisterFromDate, DateTime dtRegisterToDate, decimal dMinPrice, decimal dMaxPrice);
        IQueryable<UserExt> GetCustomerList(int SiteID, string ResellerName, string Email);
        IQueryable<Contract> GetAllContract(int SiteID);
        User GetUser(int UserID, int SiteID);
        UserOptions GetUserOptions(int UserID, int SiteID);
    }
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<ResellerContract> _resellercontract;
        private readonly IRepository<OrderDetail> _orderdetail;
        private readonly IRepository<User> _user;
        private readonly IRepository<Address> _address;
        private readonly IRepository<Audit> _audit;
        private readonly IRepository<Contract> _contract;
        private readonly WBSSLStore.Data.Repository.IWBRepository _repo;
        private readonly IRepository<EmailTemplates> _emailtemplates;
        private readonly IRepository<EmailQueue> _emailqueue;
        private readonly IRepository<UserOptions> _useroptions;
        private readonly IEmailQueueService _EmailQueueService;

        public UserService(IRepository<OrderDetail> OrderDetail, IUnitOfWork UnitOfWork, IRepository<User> User, IRepository<Address> Address, IRepository<Audit> Audit
                , IRepository<ResellerContract> ResellerContract, IRepository<Contract> Contract, IWBRepository wbrepository
                , IRepository<EmailTemplates> EmailTemplates, IRepository<EmailQueue> EmailQueue, IEmailQueueService pEmailQueueService, IRepository<UserOptions> pUserOptions)
        {
            _unitOfWork = UnitOfWork;
            _orderdetail = OrderDetail;
            _user = User;
            _address = Address;
            _audit = Audit;
            _resellercontract = ResellerContract;
            _repo = wbrepository;
            _contract = Contract;
            _emailtemplates = EmailTemplates;
            _emailqueue = EmailQueue;
            _EmailQueueService = pEmailQueueService;
            _useroptions = pUserOptions;
        }

        public User GetUser(int UserID, int SiteID)
        {
            return _user.FindByID(UserID);
        }

        public UserOptions GetUserOptions(int UserID, int SiteID)
        {
            return _useroptions.Find(x => x.UserID == UserID && x.SiteID == SiteID).FirstOrDefault();
        }
        public decimal? GetTotalPurchase(int UserID)
        {
            return _orderdetail.Find(od => od.Order.UserID == UserID && od.OrderStatusID != (int)OrderStatus.REJECTED && od.OrderStatusID != (int)OrderStatus.REFUNDED).Sum(od => (decimal?)od.Price);
        }

        public int GetCertificateCount(int UserID)
        {
            return _orderdetail.Find(od => od.Order.UserID == UserID && od.OrderStatusID != (int)OrderStatus.REJECTED && od.OrderStatusID != (int)OrderStatus.REFUNDED).Count();
        }

        public IQueryable<Contract> GetAllContract(int SiteID)
        {
            return _contract.Find(c => c.isForReseller == true && c.RecordStatusID == (int)RecordStatus.ACTIVE && c.SiteID == SiteID);
        }

        public bool SaveReseller(ResellerContract resellercontract, int LangID, int SMTPID)
        {
            bool bIsNewCust = false, bResellerActivation = false;
            if (resellercontract.Reseller.ID > 0)
            {
                WBSSLStore.Domain.ResellerContract newrc = _resellercontract.Find(c => c.Reseller.ID == resellercontract.Reseller.ID && c.Reseller.SiteID == resellercontract.SiteID).EagerLoad(c => c.Reseller, c => c.Contract).FirstOrDefault();

                newrc.ContractID = resellercontract.ContractID;
                newrc.Reseller.CompanyName = resellercontract.Reseller.CompanyName;
                newrc.Reseller.FirstName = resellercontract.Reseller.FirstName;
                newrc.Reseller.LastName = resellercontract.Reseller.LastName;
                newrc.Reseller.Email = resellercontract.Reseller.Email;
                newrc.Reseller.AlternativeEmail = resellercontract.Reseller.AlternativeEmail;
                newrc.Reseller.Address.Street = resellercontract.Reseller.Address.Street;
                newrc.Reseller.Address.City = resellercontract.Reseller.Address.City;
                newrc.Reseller.Address.State = resellercontract.Reseller.Address.State;
                newrc.Reseller.Address.Zip = resellercontract.Reseller.Address.Zip;
                newrc.Reseller.Address.CountryID = resellercontract.Reseller.Address.CountryID;
                newrc.Reseller.Address.Phone = resellercontract.Reseller.Address.Phone;
                newrc.Reseller.Address.Fax = resellercontract.Reseller.Address.Fax;
                newrc.Reseller.Address.Mobile = resellercontract.Reseller.Address.Mobile;
                newrc.Reseller.HeardBy = resellercontract.Reseller.HeardBy;
                newrc.Reseller.ConfirmPassword = newrc.Reseller.PasswordHash;

                if (newrc.Reseller.RecordStatusID == (int)RecordStatus.INACTIVE && resellercontract.Reseller.RecordStatusID == (int)RecordStatus.ACTIVE)
                {
                    bResellerActivation = true;
                    newrc.Reseller.RecordStatusID = resellercontract.Reseller.RecordStatusID;
                }
                //_useroptions.Add()
                _resellercontract.Update(newrc);
            }
            else
            {
                resellercontract.Reseller.AuditDetails.DateCreated = DateTimeWithZone.Now;
                _resellercontract.Add(resellercontract);
                bIsNewCust = true;
            }
            _unitOfWork.Commit();

            if (bIsNewCust) //Send customer welcome email
            {
                _EmailQueueService.PrepareEmailQueue(resellercontract.SiteID, LangID, EmailType.RESELLER_WELCOME_EMAIL, SMTPID, resellercontract.Reseller.Email, resellercontract.Reseller);
                _unitOfWork.Commit();
            }
            if (bResellerActivation) //Send customer welcome email
            {
                _EmailQueueService.PrepareEmailQueue(resellercontract.SiteID, LangID, EmailType.RESELLER_ACCOUNT_ACTIVATION_EMAIL, SMTPID, resellercontract.Reseller.Email, resellercontract.Reseller);
                _unitOfWork.Commit();
            }
            return true;
        }

        public bool SaveReseller(ResellerContract resellercontract, int LangID, int SMTPID, UserOptions objUserOption)
        {
            bool bIsNewCust = false, bResellerActivation = false;
            if (resellercontract.Reseller.ID > 0)
            {
                WBSSLStore.Domain.ResellerContract newrc = _resellercontract.Find(c => c.Reseller.ID == resellercontract.Reseller.ID && c.Reseller.SiteID == resellercontract.SiteID).EagerLoad(c => c.Reseller, c => c.Contract).FirstOrDefault();

                newrc.ContractID = resellercontract.ContractID;
                newrc.Reseller.CompanyName = resellercontract.Reseller.CompanyName;
                newrc.Reseller.FirstName = resellercontract.Reseller.FirstName;
                newrc.Reseller.LastName = resellercontract.Reseller.LastName;
                newrc.Reseller.Email = resellercontract.Reseller.Email;
                newrc.Reseller.AlternativeEmail = resellercontract.Reseller.AlternativeEmail;
                newrc.Reseller.Address.Street = resellercontract.Reseller.Address.Street;
                newrc.Reseller.Address.City = resellercontract.Reseller.Address.City;
                newrc.Reseller.Address.State = resellercontract.Reseller.Address.State;
                newrc.Reseller.Address.Zip = resellercontract.Reseller.Address.Zip;
                newrc.Reseller.Address.CountryID = resellercontract.Reseller.Address.CountryID;
                newrc.Reseller.Address.Phone = resellercontract.Reseller.Address.Phone;
                newrc.Reseller.Address.Fax = resellercontract.Reseller.Address.Fax;
                newrc.Reseller.Address.Mobile = resellercontract.Reseller.Address.Mobile;
                newrc.Reseller.HeardBy = resellercontract.Reseller.HeardBy;
                newrc.Reseller.ConfirmPassword = newrc.Reseller.PasswordHash;

                if (newrc.Reseller.RecordStatusID == (int)RecordStatus.INACTIVE && resellercontract.Reseller.RecordStatusID == (int)RecordStatus.ACTIVE)
                {
                    bResellerActivation = true;
                    newrc.Reseller.RecordStatusID = resellercontract.Reseller.RecordStatusID;
                }

                if (objUserOption != null && objUserOption.ID > 0)
                {
                    WBSSLStore.Domain.UserOptions _objuOp = _useroptions.Find(x => x.ID == objUserOption.ID).FirstOrDefault();
                    _objuOp.StopResellerCustomerEmail = objUserOption.StopResellerCustomerEmail;
                    _objuOp.StopResellerEmail = objUserOption.StopResellerEmail;                    
                    _useroptions.Update(_objuOp);
                }
                else
                {
                    _useroptions.Add(objUserOption); 
                }

                _resellercontract.Update(newrc);
            }
            else
            {
                resellercontract.Reseller.AuditDetails.DateCreated = DateTimeWithZone.Now;
                _resellercontract.Add(resellercontract);

                bIsNewCust = true;
            }
            _unitOfWork.Commit();

            if (objUserOption != null && objUserOption.SiteID > 0 && bIsNewCust)
            {
                objUserOption.UserID = resellercontract.UserID;
                _useroptions.Add(objUserOption);

                _unitOfWork.Commit();
            }

            if (bIsNewCust) //Send customer welcome email
            {
                _EmailQueueService.PrepareEmailQueue(resellercontract.SiteID, LangID, EmailType.RESELLER_WELCOME_EMAIL, SMTPID, resellercontract.Reseller.Email, resellercontract.Reseller);
                _unitOfWork.Commit();
            }
            if (bResellerActivation) //Send customer welcome email
            {
                _EmailQueueService.PrepareEmailQueue(resellercontract.SiteID, LangID, EmailType.RESELLER_ACCOUNT_ACTIVATION_EMAIL, SMTPID, resellercontract.Reseller.Email, resellercontract.Reseller);
                _unitOfWork.Commit();
            }
            return true;
        }

        public bool SaveCustomer(User user, int LangID, int SMTPID)
        {
            bool bIsNewCust = false;
            if (user.ID > 0)
            {
                User objUser = _user.Find(u => u.ID == user.ID && u.SiteID == user.SiteID).EagerLoad(u => u.Address, u => u.AuditDetails).FirstOrDefault();

                objUser.CompanyName = user.CompanyName;
                objUser.FirstName = user.FirstName;
                objUser.LastName = user.LastName;
                objUser.Email = user.Email;
                objUser.AlternativeEmail = user.AlternativeEmail;
                objUser.Address.Street = user.Address.Street;
                objUser.Address.City = user.Address.City;
                objUser.Address.State = user.Address.State;
                objUser.Address.Zip = user.Address.Zip;
                objUser.Address.CountryID = user.Address.CountryID;
                objUser.Address.Phone = user.Address.Phone;
                objUser.Address.Fax = user.Address.Fax;
                objUser.Address.Mobile = user.Address.Mobile;
                objUser.RecordStatusID = user.RecordStatusID;
                objUser.HeardBy = user.HeardBy;
                objUser.ConfirmPassword = objUser.PasswordHash;

                _user.Update(objUser);
            }
            else
            {
                user.AuditDetails.DateCreated = DateTimeWithZone.Now;
                _user.Add(user);
                bIsNewCust = true;
            }
            _unitOfWork.Commit();

            if (bIsNewCust) //Send customer welcome email
            {
                _EmailQueueService.PrepareEmailQueue(user.SiteID, LangID, EmailType.CUSTOMER_WELCOME_EMAIL, SMTPID, user.Email, user);
                _unitOfWork.Commit();
            }

            return true;
        }

        public bool SaveAdminusers(User user, int LangID, int SMTPID)
        {
            bool bIsNewCust = false;
            if (user.ID > 0)
            {
                User objUser = _user.Find(u => u.ID == user.ID && u.SiteID == user.SiteID).EagerLoad(u => u.Address, u => u.AuditDetails).FirstOrDefault();

                objUser.CompanyName = user.CompanyName;
                objUser.FirstName = user.FirstName;
                objUser.LastName = user.LastName;
                objUser.Email = user.Email;
                objUser.AlternativeEmail = user.AlternativeEmail;
                objUser.Address.Street = user.Address.Street;
                objUser.Address.City = user.Address.City;
                objUser.Address.State = user.Address.State;
                objUser.Address.Zip = user.Address.Zip;
                objUser.Address.CountryID = user.Address.CountryID;
                objUser.Address.Phone = user.Address.Phone;
                objUser.Address.Fax = user.Address.Fax;
                objUser.Address.Mobile = user.Address.Mobile;
                objUser.RecordStatusID = user.RecordStatusID;
                objUser.HeardBy = user.HeardBy;
                objUser.ConfirmPassword = objUser.PasswordHash;
                objUser.UserTypeID = user.UserTypeID;

                _user.Update(objUser);
            }
            else
            {
                user.AuditDetails.DateCreated = DateTimeWithZone.Now;
                _user.Add(user);
                bIsNewCust = true;
            }
            _unitOfWork.Commit();

            if (bIsNewCust) //Send customer welcome email
            {
                _EmailQueueService.PrepareEmailQueue(user.SiteID, LangID, EmailType.ADMIN_NEWUSER_WELCOME_EMAIL, SMTPID, user.Email, user);
                _unitOfWork.Commit();
            }

            return true;
        }

        public bool UpdateUserStatus(int UserID, int SiteID, int? LangID, int? SMTPID)
        {
            User objUser = _user.Find(u => u.ID == UserID && u.SiteID == SiteID).FirstOrDefault();
            if (objUser != null)
            {
                objUser.ConfirmPassword = objUser.PasswordHash;
                objUser.RecordStatus = (objUser.RecordStatus == RecordStatus.ACTIVE ? RecordStatus.INACTIVE : RecordStatus.ACTIVE);
                _user.Update(objUser);

                if (objUser.RecordStatus == RecordStatus.ACTIVE && objUser.UserType == UserType.RESELLER)
                    _EmailQueueService.PrepareEmailQueue(objUser.SiteID, Convert.ToInt16(LangID), EmailType.RESELLER_ACCOUNT_ACTIVATION_EMAIL, Convert.ToInt16(SMTPID), objUser.Email, objUser);

                _unitOfWork.Commit();
                return true;
            }
            return false;
        }

        public bool DeleteUser(int UserID, int SiteID)
        {
            User objUser = _user.Find(u => u.ID == UserID && u.SiteID == SiteID).FirstOrDefault();
            if (objUser != null)
            {
                objUser.RecordStatus = RecordStatus.DELETED;
                objUser.ConfirmPassword = objUser.PasswordHash;
                _user.Update(objUser);
                _unitOfWork.Commit();
                return true;
            }
            return false;
        }

        public bool EmailExist(string Email, int SiteID, int UserID)
        {
            User objUser = _user.Find(u => u.ID != UserID && u.Email == Email && u.SiteID == SiteID && u.RecordStatusID != (int)RecordStatus.DELETED).FirstOrDefault();
            if (objUser != null)
                return true;
            else
                return false;
        }

        public IQueryable<UserExt> GetResellerData(int SiteID, string ResellerName, string Email, RecordStatus? eRecordStatus, DateTime dtRegisterFromDate, DateTime dtRegisterToDate, decimal dMinPrice, decimal dMaxPrice)
        {
            return _repo.GetResellerList(SiteID, ResellerName, Email, eRecordStatus, dtRegisterFromDate, dtRegisterToDate, dMinPrice, dMaxPrice);
        }

        public IQueryable<UserExt> GetCustomerList(int SiteID, string ResellerName, string Email)
        {
            return _repo.GetCustomerList(SiteID, ResellerName, Email);
        }
    }
}
