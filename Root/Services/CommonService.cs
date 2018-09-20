
using StateOfOhioLibrary.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace StateOfOhioLibrary.Services
{
    public class CommonService
    {
        #region Global Declarations
        DatabaseContext dbContext = new DatabaseContext();
        public string className = MethodBase.GetCurrentMethod().DeclaringType.Name;
        public string menuFilePath = "~/Files/UserMenu.xml";
        public string errorLogFilePath = Convert.ToString(WebConfigurationManager.AppSettings["LogFilePath"]);
        public enum EnumStatus
        {
            [Description("Active")]
            Active,
            [Description("Inactive")]
            Inactive,
            [Description("Deleted")]
            Deleted
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Method to Log Exception
        /// </summary>
        /// <param name="objException"></param>
        public void LogException(string className, string methodName, Exception exception)
        {
            string strLogMessage = string.Empty;

            strLogMessage += "Class Name: " + className;
            strLogMessage += "Method Name: " + methodName;
            strLogMessage += "Log Date & Time: " + DateTime.Now.ToString("MM/dd/yyyy") + " " + DateTime.Now.ToString("HH:mm:ss tt") + Environment.NewLine;
            strLogMessage += "Message: " + exception.Message + Environment.NewLine;
            if (exception.InnerException != null)
            { strLogMessage += "Inner Message: " + exception.InnerException.ToString() + Environment.NewLine; }
            strLogMessage += "Source: " + exception.Source + Environment.NewLine;
            if (exception.TargetSite != null)
            { strLogMessage += "TargetSite: " + exception.TargetSite.ToString() + Environment.NewLine; }
            strLogMessage += Environment.NewLine;
            strLogMessage += Environment.NewLine;

            string logPath = HttpContext.Current.Server.MapPath(errorLogFilePath);
            if (!Directory.Exists(logPath))
            { Directory.CreateDirectory(logPath); }
            logPath += "\\Log (" + DateTime.Now.ToString("MM/dd/yyyy").Replace('/', '-') + ").txt";

            File.AppendAllText(logPath, strLogMessage);
        }
        /// <summary>
        /// Method to Log Exception
        /// </summary>
        /// <param name="objException"></param>
        public void CSVLog(string message, string rowNo, string email, string memberEmail)
        {
            string strLogMessage = string.Empty;

            strLogMessage += "Row No: " + rowNo + " | ";
            strLogMessage += "Email: " + email + " | ";
            strLogMessage += "Member: " + memberEmail + " | ";
            strLogMessage += "Message: " + message + " | ";
            strLogMessage += "Log Date & Time: " + DateTime.Now.ToString("MM/dd/yyyy") + " " + DateTime.Now.ToString("HH:mm:ss tt") + Environment.NewLine;

            strLogMessage += Environment.NewLine;
            strLogMessage += Environment.NewLine;

            string logPath = HttpContext.Current.Server.MapPath(errorLogFilePath);
            if (!Directory.Exists(logPath))
            { Directory.CreateDirectory(logPath); }
            logPath += "\\CSVLog (" + DateTime.Now.ToString("MM/dd/yyyy").Replace('/', '-') + ").txt";

            File.AppendAllText(logPath, strLogMessage);
        }


        /// <summary>
        /// Method for file upload
        /// </summary>
        /// <param name="path"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public string UploadFile(string path, HttpPostedFileBase file)
        {
            string fileName = "";

            try
            {
                if (file != null)
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    fileName = System.IO.Path.GetFileName(file.FileName);
                    path = System.IO.Path.Combine(path, fileName);
                    file.SaveAs(path);
                }
            }
            catch (Exception exception)
            {
                LogException(className, MethodBase.GetCurrentMethod().Name, exception);
            }
            return fileName;
        }

        /// <summary>
        /// Method to check user is logged in
        /// </summary>
        public void CheckLoggedIn()
        {
            try
            {
                if (HttpContext.Current.Session["User"] == null)
                {
                    HttpContext.Current.Response.Redirect("/Login?ReturnUrl=" + HttpContext.Current.Request.Url.AbsolutePath);
                }
            }
            catch (Exception exception)
            {
                LogException(className, MethodBase.GetCurrentMethod().Name, exception);
            }
        }

        /// <summary>
        /// Method to get current logged in user
        /// </summary>
        public UserModel GetCurrentUser()
        {
            UserModel user = new UserModel();

            try
            {
                if (HttpContext.Current.Session["User"] != null)
                {
                    user = (UserModel)HttpContext.Current.Session["User"];
                }
                else
                    user = null;
            }
            catch (Exception exception)
            {
                LogException(className, MethodBase.GetCurrentMethod().Name, exception);
            }
            return user;
        }


        #endregion
    }
}