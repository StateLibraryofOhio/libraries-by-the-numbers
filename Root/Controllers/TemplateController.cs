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
    public class TemplateController : Controller
    {
        #region Global Declaration

        LibraryDataService libraryDataService = new LibraryDataService();
        CommonService commonService = new CommonService();
        TemplateService templateService = new TemplateService();

        string templatePath = WebConfigurationManager.AppSettings["TemplatePath"];
        int intPageSize = Convert.ToInt32(WebConfigurationManager.AppSettings["PaginationCount"]);
        #endregion

        #region Action Result
        public ActionResult Index(int page = 1, int pageSize = 1, string sort = "CreatedAt", string sortdir = "DESC")
        {
            commonService.CheckLoggedIn();
            var model = new TemplateViewModel();
            try
            {
                model = templateService.GetTemplates(sort, sortdir, page, intPageSize);
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }

            return View(model);
        }
        /// <summary>
        /// Method to get template list
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="sortdir"></param>
        /// <returns></returns>
        public ActionResult TemplateList(int page = 1, int pageSize = 1, string sort = "CreatedAt", string sortdir = "DESC")
        {
            var model = new TemplateViewModel();
            try
            {
                model = templateService.GetTemplates(sort, sortdir, page, intPageSize);
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }


            return PartialView("_TemplateView", model);
        }

        #endregion

        #region Ajax Methods

        /// <summary>
        /// Method to Add Template
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddTemplate(string name)
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
            int templateId = 0;
            TemplateModel model = new TemplateModel();
            try
            {
                if (!string.IsNullOrEmpty(name))
                {
                    model.Name = name;
                    model.Status = EnumStatus.Active.ToString();
                    model.CreatedAt = DateTime.Now;
                    model.CreatedBy = currentUser.UserId;
                    templateId = templateService.AddTemplate(model).TemplateId;
                }

                #region Attachment Uploads
                IList<HttpPostedFileBase> files = new List<HttpPostedFileBase>();
                if (Request.Files != null)
                {
                    if (Request.Files.Count > 0)
                    {
                        var requestfile = Request.Files[0];
                        files.Add(requestfile);
                        fileName = Path.GetFileName(requestfile.FileName);

                        if (!string.IsNullOrEmpty(fileName))
                        {
                            directoryPath = Server.MapPath(templatePath + model.TemplateId + "//");

                            if (!Directory.Exists(directoryPath))
                            {
                                Directory.CreateDirectory(directoryPath);

                            }
                            var path = Path.Combine(directoryPath, fileName);
                            requestfile.SaveAs(path);

                            model.FileLocation = Path.Combine(model.TemplateId + "//" + fileName);
                        }
                    }
                }

                #endregion

                if (!string.IsNullOrEmpty(model.FileLocation) && templateId > 0)
                {
                    var template = templateService.GetTemplateById(templateId);
                    if (template != null)
                    {
                        template.FileLocation = model.FileLocation;
                        template.UpdatedAt = DateTime.Now;
                        template.UpdatedBy = currentUser.UserId;

                        templateService.UpdateTemplate(template);
                        result = "True";
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
        public ActionResult EditTemplate(string templateId, string name)
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

                if (!string.IsNullOrEmpty(templateId))
                {

                    var model = templateService.GetTemplateById(Convert.ToInt32(templateId));
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
                                    directoryPath = Server.MapPath(templatePath + model.TemplateId + "//");

                                    if (!Directory.Exists(directoryPath))
                                    {
                                        Directory.CreateDirectory(directoryPath);

                                    }
                                    var path = Path.Combine(directoryPath, fileName);
                                    requestFile.SaveAs(path);

                                    model.FileLocation = Path.Combine(model.TemplateId + "//" + fileName);
                                }
                            }
                        }

                        #endregion

                        model.Name = name;
                        model.UpdatedAt = DateTime.Now;
                        model.UpdatedBy = currentUser.UserId;
                        templateService.UpdateTemplate(model);
                        result = "True";
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
        /// Method to delete Template
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string DeleteTemplate(int id)
        {
            commonService.CheckLoggedIn();
            var currentUser = commonService.GetCurrentUser();
            string result = string.Empty;
            try
            {
                if (id > 0)
                {
                    var template = templateService.GetTemplateById(id);
                    if (template != null)
                    {
                        template.Status = EnumStatus.Deleted.ToString();
                        template.UpdatedAt = DateTime.Now;
                        template.UpdatedBy = currentUser.UserId;
                        templateService.UpdateTemplate(template);
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