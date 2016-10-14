using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WBSSLStore.Domain;
using WBSSLStore.Service;
using WBSSLStore.Data.Infrastructure;
using WBSSLStore.Web.Helpers.Base;

namespace WBSSLStore.Web.Areas.Admin.Controllers
{
    [CustomAuthorizeAttribute]
    [HandleError]
    public class ContractController : WBController<Contract, IRepository<Contract>, IProductService>
    {
        //
        // GET: /Admin/Contract/

        public ActionResult Index()
        {
            List<Contract> objContract = _repository.Find(con => con.RecordStatusID != (int)RecordStatus.DELETED && con.isForReseller == true && con.SiteID == Site.ID).ToList();
            return View(objContract);
        }
        [HttpPost]
        public ActionResult ChangeStatus(int id)
        {
            return Json(UpdateStatus(id, false));
        }
        public ActionResult Delete(int id)
        {
            return Json(UpdateStatus(id, true));
        }
        public ActionResult Create()
        {
            _viewModel.SiteID = Site.ID;
            _viewModel.isForReseller = true;
            return View(_viewModel);
        }

        [HttpPost]
        public ActionResult Create(Contract model, FormCollection collection)
        {
            bool IsImport = model.ID <= 0 ? Convert.ToBoolean(collection["chkImportAll"]) : false;
            if (ModelState.IsValid)
            {
                bool isAdd = model.ID <= 0;
                if (SaveContract(model))
                {
                    if (IsImport && isAdd)
                    {
                        _service.ImportProductsInContract(Site, Convert.ToDecimal(collection["txtPriceMargin"]), model.ID);
                    }
                    return RedirectToAction("Index");
                }
                else
                    return View(model);
            }
            return View(model);
        }
        public ActionResult Edit(int id)
        {
            if (id > 0)
            {
                _viewModel = _repository.Find(con => con.ID == id && con.SiteID == Site.ID).FirstOrDefault();
                if (_viewModel != null)
                    return View("Create", _viewModel);
                else
                    return RedirectToAction("Index");
            }
            else
                return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult Edit(Contract model)
        {
            if (ModelState.IsValid)
            {
                Create(model, null);
                return RedirectToAction("Index");
            }
            return View("create", model);            
        }
        #region PrivateMethod
        private bool UpdateStatus(int ID, bool IsDelete)
        {
            if (ID > 0)
            {
                Contract oContract = _repository.Find(con => con.ID == ID && con.SiteID == Site.ID).SingleOrDefault();
                if (oContract != null)
                {
                    if (IsDelete)
                    {
                        oContract.RecordStatusID = (int)RecordStatus.DELETED;
                    }
                    else
                    {

                        if (oContract.RecordStatusID == (int)RecordStatus.ACTIVE)
                            oContract.RecordStatusID = (int)RecordStatus.INACTIVE;
                        else
                            oContract.RecordStatusID = (int)RecordStatus.ACTIVE;

                    }
                    _repository.Update(oContract);
                    _unitOfWork.Commit();
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }
        private bool SaveContract(Contract model)
        {
            if (model.ID > 0)
                _repository.Update(model);
            else
                _repository.Add(model);
            _unitOfWork.Commit();

            return true;
        }
        #endregion
    }
}
