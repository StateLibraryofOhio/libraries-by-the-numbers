using StateOfOhioLibrary.Data.Models;
using StateOfOhioLibrary.Models;
using StateOfOhioLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;

namespace StateOfOhioLibrary.Controllers
{
    public class LoginController : Controller
    {

        #region Global Declaration
        DatabaseContext dbContext = new DatabaseContext();
        SecurityService securityService = new SecurityService();
        CommonService commonService = new CommonService();
        UserService userService = new UserService();
        MailService mailService = new MailService();
        string domainName = Convert.ToString(WebConfigurationManager.AppSettings["DomainName"]);
        string forgotPasswordMailSubject = Convert.ToString(WebConfigurationManager.AppSettings["ForgotPasswordMailSubject"]);
        private bool sslEnabled = Convert.ToBoolean(WebConfigurationManager.AppSettings["SSLRedirection"]);
        #endregion

        #region Action Result
        // GET: Login
        /// <summary>
        /// Index
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            LoginModel model = new LoginModel();
            if (Request.QueryString["ReturnUrl"] != null)
            {
                model.ReturnUrl = Request.QueryString["ReturnUrl"];
            }
            return View(model);
        }
        /// <summary>
        /// Index
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(LoginModel model)
        {
            var username = model.UserName;
            var password = model.Password;
            try
            {
                if (ModelState.IsValid)
                {
                    var user = userService.AuthenticateUser(username, password);
                    if (user != null)
                    {
                        if (Session["User"] != null)
                        {
                            Session.Remove("User");
                        }
                        Session["User"] = user;
                        FormsAuthentication.SetAuthCookie(username, model.RememberMe);

                        if (!String.IsNullOrEmpty(model.ReturnUrl))
                        {
                            Response.Redirect(model.ReturnUrl, false);
                        }
                        else
                        {
                            Response.Redirect("/Dashboard", false);
                        }
                    }
                    else
                    {
                        model.LoginStatus = ErrorMessageContainer.UsernamePasswordInvalid;
                        model.UserName = "";
                        model.Password = "";
                    }
                }
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }
            return View(model);
        }
        /// <summary>
        /// Forgot Password
        /// </summary>
        /// <returns></returns>
        public ActionResult ForgotPassword()
        {
            ForgotPasswordModel model = new ForgotPasswordModel();
            return View(model);
        }
        /// <summary>
        /// Forgot Password
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    UserModel user = userService.GetUserByEmail(model.Email);
                    if (user == null)
                    {
                        model.PasswordStatus = ErrorMessageContainer.EmailNotFound;
                        model.Email = "";
                    }
                    else
                    {
                        userService.UpdatePasswordResetToken(user);
                        var url = new Uri(string.Concat(domainName.TrimEnd('/'), Url.Action("ResetPassword", "Login", new { id = user.UserId, token = user.PasswordResetToken })));
                        var sb = new StringBuilder();
                        sb.AppendFormat("<br /><a href=\"{0}\">Reset your password</a>", url);
                        string mailBody = SendPasswordResetMail(user, sb.ToString());
                        SendMail(mailBody, user.Name, user.Email, forgotPasswordMailSubject);
                        return Redirect("PasswordResetSent");
                    }
                }
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }
            return View(model);
        }

        /// <summary>
        /// Reset Password  Sent
        /// </summary>
        /// <returns></returns>
        public ViewResult PasswordResetSent()
        {
            return View();
        }

        /// <summary>
        /// Reset Password  
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        public ViewResult ResetPassword(int id, string token)
        {
            var model = new ResetPasswordViewModel
            {
                Id = id,
                Token = token
            };

            if (id == 0 || String.IsNullOrEmpty(token))
            {
                ModelState.AddModelError("InvalidTokenError", ErrorMessageContainer.InvalidToken);
            }

            return View(model);
        }
        /// <summary>
        /// Reset Password
        /// </summary>
        /// <param name="postedModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel postedModel)
        {
            if (!ModelState.IsValid)
            {
                return View(postedModel);
            }
            if (postedModel.Id != 0)
            {
                var user = userService.GetUserById(postedModel.Id);
                if (user == null || user.PasswordResetToken == null || !userService.IsPasswordResetTokenValid(user, postedModel.Token))
                {
                    ModelState.AddModelError("InvalidTokenError", ErrorMessageContainer.InvalidToken);
                    ViewBag.Message = "Error";
                    return View(postedModel);
                }
                try
                {
                    userService.ResetPassword(user, postedModel.NewPassword);
                    userService.ClearPasswordResetToken(user);
                    ViewBag.Message = "Successful";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "Error";
                    commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, ex);
                    return View(postedModel);
                }
            }
            return Redirect("PasswordChanged");
        }
        /// <summary>
        /// Changed Password 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ViewResult PasswordChanged()
        {
            return View();
        }
        /// <summary>
        /// Change Password
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangePassword()
        {
            commonService.CheckLoggedIn();
            ChangePasswordViewModel model = new ChangePasswordViewModel();
            if (Request.UrlReferrer != null && !Request.UrlReferrer.ToString().ToLower().Contains("login"))
            {
                model.PreviousPageUrl = Request.UrlReferrer.AbsoluteUri;
            }
            else
            {
                if (sslEnabled)
                {
                    model.PreviousPageUrl = "https://" + Request.ServerVariables["HTTP_HOST"] + "/" + "Dashboard";
                }
                else
                {
                    model.PreviousPageUrl = "http://" + Request.ServerVariables["HTTP_HOST"] + "/" + "Dashboard";
                }
            }

            model.OldPassword = "";
            model.NewPassword = "";
            model.ConfirmPassword = "";

            if (Request.QueryString["id"] != null)
                model.MemberId = Convert.ToInt32(Request.QueryString["id"]);
            else
                model.MemberId = 0;
            return View(model);
        }
        /// <summary>
        ///  Change Password
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            var changePasswordSucceeded = false;
            UserModel currentUser = new UserModel();
            commonService.CheckLoggedIn();
            if (ModelState.IsValid)
            {
                try
                {
                    currentUser = commonService.GetCurrentUser();
                    changePasswordSucceeded = userService.ChangePassword(currentUser, model.OldPassword, model.NewPassword);
                    if (changePasswordSucceeded)
                    {
                        ViewBag.Message = "Successful";
                    }
                    else
                    {
                        ViewBag.Message = ErrorMessageContainer.Wrongoldpassword;
                    }
                }
                catch (Exception ex)
                {
                    commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, ex);
                    changePasswordSucceeded = false;
                }
            }
            return View(model);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Method to get the mail body of password reset message
        /// </summary>
        /// <param name="model"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public string SendPasswordResetMail(UserModel user, string url)
        {
            string mailBody = "";
            try
            {
                string mailDescription = "A request has been made to reset your password. To reset your password, click on the link below.<br /><br /> If you did not make this request, then please ignore this email. No further action is required and your password will not be changed.<br />";
                mailDescription += url;
                string name = user.Name;
                string email = user.Email;

                mailBody = mailService.HeaderTemplate();
                mailBody += mailService.DescriptionTemplate(name, mailDescription);
                mailBody += mailService.FooterDescriptionTemplate("If you have any questions, please let us know.");
                mailBody += mailService.FooterTemplate();
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }
            return mailBody;
        }

        /// <summary>
        /// Method to send email
        /// </summary>
        /// <returns></returns>
        public void SendMail(string mailBody, string name, string email, string subject)
        {
            try
            {
                mailService.SendMail(name, email, mailBody, subject);
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }
        }

        #endregion
    }
}