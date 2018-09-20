using StateOfOhioLibrary.Models;
using StateOfOhioLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace StateOfOhioLibrary.Controllers
{
    public class DashboardController : Controller
    {

        #region Global Declaration
        DashboardService dashboardService = new DashboardService();
        CommonService commonService = new CommonService();
        #endregion

        #region Action Result
        /// <summary>
        /// Index
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            DashboardModel model = new DashboardModel();
            try
            {
                CheckLoggedIn();
                model = dashboardService.GetDashboardData();
                return View(model);
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }
            return View(model);
        }
        /// <summary>
        /// Method to check logged in user
        /// </summary>
        public void CheckLoggedIn()
        {
            try
            {
                if (Session["User"] == null)
                {
                    Response.Redirect("/Login?ReturnUrl=" + Request.Url.AbsolutePath);
                }
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }
        }

        #endregion
    }
}