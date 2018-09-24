using StateOfOhioLibrary.Data.Models;
using StateOfOhioLibrary.Models;
using StateOfOhioLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace StateOfOhioLibrary.Controllers
{
    public class CommonController : Controller
    {
        #region Global Declaration
        CommonService commonService = new CommonService();
        #endregion
        #region Action Results
        /// <summary>
        /// Index
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// Method for Logging Off User
        /// </summary>
        /// <returns></returns>
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            if (Session["User"] != null)
            {
                Session.Remove("User");
            }
            return Redirect("/Login");
        }
        /// <summary>
        /// Method For Page Not Found 
        /// </summary>
        /// <returns></returns>
        public ActionResult PageNotFound()
        {
            try
            {
                return View();
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }

            return View();
        }

        /// <summary>
        /// Method For Error Page
        /// </summary>
        /// <returns></returns>
        public ActionResult Error()
        {
            try
            {
                return View();
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }

            return View();
        }

        /// <summary>
        /// Method for back button
        /// </summary>
        /// <returns></returns>
        public ActionResult BackButton()
        {
            return PartialView();
        }
        /// <summary>
        /// Method for cancel button
        /// </summary>
        /// <param name="previousPageURL"></param>
        /// <returns></returns>
        public ActionResult CancelButton(string previousPageURL)
        {
            return PartialView();
        }
        /// <summary>
        /// Method to show the page under construction
        /// </summary>
        /// <returns></returns>
        public ActionResult PageUnderConstruction()
        {
            try
            {
                return View();
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }

            return View();
        }
        /// <summary>
        /// Method to bind header part in layout
        /// </summary>
        /// <returns></returns>
        public ActionResult _Header()
        {
            CommonModel model = new CommonModel();
            try
            {
                UserModel currentUser = new UserModel();
                currentUser = commonService.GetCurrentUser();
                if (currentUser != null)
                {
                    model.IsLoggedIn = true;
                    model.Type = currentUser.Type;
                }

                else
                    model.IsLoggedIn = false;
                return PartialView(model);
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }

            return PartialView(model);
        }
        #endregion
    }
}