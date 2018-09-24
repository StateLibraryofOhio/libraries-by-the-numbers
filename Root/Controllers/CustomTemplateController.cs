using System;
using System.IO;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using System.Reflection;
using System.Web.Configuration;
using System.Collections.Generic;
using StateOfOhioLibrary.Models;
using StateOfOhioLibrary.Services;
using StateOfOhioLibrary.Data.Models;

namespace StateOfOhioLibrary.Controllers
{
    public class CustomTemplateController : Controller
    {

        #region Global Declaration
        CustomColumnService customColumn = new CustomColumnService();
        CommonService commonService = new CommonService();
        CustomTemplateService customTemplateService = new CustomTemplateService();
        int intPageSize = Convert.ToInt32(WebConfigurationManager.AppSettings["PaginationCount"]);
        string customTemplatePath = WebConfigurationManager.AppSettings["CustomTemplatePath"];
        #endregion

        #region ActionResult
        // GET: CustomTemplate
        /// <summary>
        /// Index
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="sortdir"></param>
        /// <returns></returns>
        public ActionResult Index(int page = 1, int pageSize = 1, string sort = "CreatedAt", string sortdir = "DESC")
        {
            CustomTemplateModel CustomTemplate = new CustomTemplateModel();
            try
            {
                commonService.CheckLoggedIn();
                CustomTemplate = customTemplateService.GetCustomTemplates(sort, sortdir, page, intPageSize);
                List<SelectListItem> columns = customColumn.PopulateCustomColumn();
                CustomTemplate.AvailableCustomColumn = columns;
                if (Request.UrlReferrer != null)
                {
                    CustomTemplate.PreviousPageUrl = Request.UrlReferrer.AbsoluteUri;
                }
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }
            return View(CustomTemplate);
        }
        /// <summary>
        /// Method to get custom template list
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="sortdir"></param>
        /// <returns></returns>
        public ActionResult CustomTemplateList(int page = 1, int pageSize = 1, string sort = "CreatedAt", string sortdir = "DESC")
        {
            var model = new CustomTemplateModel();
            try
            {
                model = customTemplateService.GetCustomTemplates(sort, sortdir, page, intPageSize);
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }


            return PartialView("_CustomTemplateView", model);
        }
        /// <summary>
        /// Add custom template
        /// </summary>
        /// <param name="customColumnId"></param>
        /// <param name="notes"></param>
        /// <param name="columnText"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddCustomTemplate(string customColumnId, string notes, string columnText)
        {
            CustomTemplateModel customTemplate = new CustomTemplateModel();

            if (Request.UrlReferrer != null)
            {
                customTemplate.PreviousPageUrl = Request.UrlReferrer.AbsoluteUri;
            }
            string result = string.Empty;
            var currentUser = commonService.GetCurrentUser();
            if (currentUser == null)
            {
                result = "Login";
                return Json(result);
            }
            string fileName = string.Empty;
            string directoryPath = "";

            try
            {

                if (!string.IsNullOrEmpty(customColumnId))
                {
                    var customTemplateExist = customTemplateService.GetCustomTemplateById(Convert.ToInt32(customColumnId));
                    if (customTemplateExist == null)
                    {
                        CustomTemplateDataModel model = new CustomTemplateDataModel();

                        if (model != null)
                        {
                            #region Attachment Uploads
                            IList<HttpPostedFileBase> files = new List<HttpPostedFileBase>();
                            if (Request.Files != null)
                            {
                                if (Request.Files.Count > 0)
                                {
                                    var requestFile = Request.Files[0];
                                    files.Add(requestFile);
                                    fileName = Path.GetFileName(requestFile.FileName);

                                    if (!string.IsNullOrEmpty(fileName))
                                    {
                                        directoryPath = Server.MapPath(customTemplatePath + customColumnId + "//");

                                        if (!Directory.Exists(directoryPath))
                                        {
                                            Directory.CreateDirectory(directoryPath);

                                        }
                                        var path = Path.Combine(directoryPath, fileName);
                                        requestFile.SaveAs(path);

                                        model.FileLocation = Path.Combine(customColumnId + "//" + fileName);
                                    }
                                }
                            }

                            #endregion
                            model.CustomColumnId = Convert.ToInt32(customColumnId);

                            model.CreatedAt = DateTime.Now;
                            model.CreatedBy = currentUser.UserId;
                            model.UpdatedAt = DateTime.Now;
                            model.Status = CommonService.EnumStatus.Active.ToString();
                            model.ColumnText = columnText;
                            model.Notes = notes;
                            customTemplateService.AddcustomTemplate(model);
                            result = "True";
                        }
                    }
                    else
                    {
                        result = "Existing";
                    }
                }

            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }

            return Json(result);
        }
        /// <summary>
        /// Method to Edit Template
        /// </summary>
        /// <param name="templateId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditCustomTemplate(string customTemplateId, string customColumnId, string notes = null, string columnText = null)
        {
            string result = string.Empty;
            var currentUser = commonService.GetCurrentUser();
            if (currentUser == null)
            {
                result = "Login";
                return Json(result);
            }
            string fileName = string.Empty;
            string directoryPath = "";
            try
            {

                if (!string.IsNullOrEmpty(customTemplateId))
                {


                    var model = customTemplateService.GetCustomTemplateByCustomTemplateId(Convert.ToInt32(customTemplateId));

                    if (model.CustomColumnId == Convert.ToInt32(customColumnId))
                    {
                        if (model != null)
                        {
                            #region Attachment Uploads
                            IList<HttpPostedFileBase> files = new List<HttpPostedFileBase>();
                            if (Request.Files != null)
                            {
                                if (Request.Files.Count > 0)
                                {
                                    var requestFile = Request.Files[0];
                                    files.Add(requestFile);
                                    fileName = Path.GetFileName(requestFile.FileName);

                                    if (!string.IsNullOrEmpty(fileName))
                                    {
                                        directoryPath = Server.MapPath(customTemplatePath + customColumnId + "//");

                                        if (!Directory.Exists(directoryPath))
                                        {
                                            Directory.CreateDirectory(directoryPath);

                                        }
                                        var path = Path.Combine(directoryPath, fileName);
                                        requestFile.SaveAs(path);

                                        model.FileLocation = Path.Combine(customColumnId + "//" + fileName);
                                    }
                                }
                            }

                            #endregion

                            model.CustomColumnId = Convert.ToInt32(customColumnId);

                            model.UpdatedAt = DateTime.Now;
                            model.UpdatedBy = currentUser.UserId;
                            model.Status = CommonService.EnumStatus.Active.ToString();
                            model.ColumnText = columnText;
                            model.Notes = notes;
                            customTemplateService.UpdateCustomTemplate(model);
                            result = "True";
                        }
                    }
                    else
                    {
                        result = "Existing";
                    }
                }
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }

            return Json(result);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Method to delete Custom Template
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string DeleteCustomTemplate(int id)
        {
            commonService.CheckLoggedIn();
            var currentUser = commonService.GetCurrentUser();
            string result = string.Empty;
            try
            {
                if (id > 0)
                {
                    var customTemplate = customTemplateService.GetCustomTemplateById(id);
                    if (customTemplate != null)
                    {
                        customTemplate.Status = EnumStatus.Deleted.ToString();
                        customTemplate.UpdatedAt = DateTime.Now;
                        customTemplate.UpdatedBy = currentUser.UserId;
                        customTemplateService.UpdateCustomTemplate(customTemplate);
                        result = "True";
                    }
                }
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }

            return result;
        }
        #endregion

    }
}