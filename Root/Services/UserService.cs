using StateOfOhioLibrary.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;

namespace StateOfOhioLibrary.Services
{
    public class UserService
    {

        #region Global Declaration
        DatabaseContext dbContext = new DatabaseContext();
        SecurityService securityService = new SecurityService();
        CommonService commonService = new CommonService();
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
        /// <summary>
        /// Method to create current user login
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public UserModel AuthenticateUser(string email, string password)
        {
            UserModel query = new UserModel();
            try
            {
                string encryptedPassword = securityService.EncryptText(password);
                query = (from u in dbContext.User
                         where u.Email == email && u.Password == encryptedPassword
                          && u.Status == EnumStatus.Active.ToString()
                         select u).FirstOrDefault();
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }
            return query;
        }
        /// <summary>
        /// Set current user in a session
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public void SetUserSession(UserModel user)
        {
            try
            {
                if (HttpContext.Current.Session["User"] != null)
                {
                    HttpContext.Current.Session.Remove("User");
                }
                HttpContext.Current.Session["User"] = user;
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }
        }

        #region User
        /// <summary>
        /// Get user by email 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public UserModel GetUserByEmail(string email)
        {
            UserModel query = new UserModel();
            try
            {
                query = (from u in dbContext.User
                         where u.Email == email && u.Status == EnumStatus.Active.ToString()
                         select u).FirstOrDefault();

            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }
            return query;
        }

        /// <summary>
        /// Get user by id
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public UserModel GetUserById(int id)
        {
            UserModel query = new UserModel();
            try
            {
                query = (from u in dbContext.User
                         where u.UserId == id && u.Status == EnumStatus.Active.ToString()
                         select u).FirstOrDefault();

            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }
            return query;
        }

        #endregion

        #region Password
        /// <summary>
        /// Update password reset token  
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool UpdatePasswordResetToken(UserModel user)
        {
            user.PasswordResetToken = CreatePasswordResetToken();
            user.PasswordResetTokenCreatedAt = DateTime.UtcNow;
            user.UpdatedBy = user.UserId;
            user.UpdatedAt = DateTime.Now;
            dbContext.SaveChanges();
            return true;
        }
        /// <summary>
        /// Remove the password reset security token and timestamp from the user record
        /// </summary>
        public bool ClearPasswordResetToken(UserModel user)
        {
            try
            {
                if (user == null)
                {
                    return false;
                }
                user.PasswordResetToken = null;
                user.PasswordResetTokenCreatedAt = null;
                user.UpdatedBy = user.UserId;
                user.UpdatedAt = DateTime.Now;
                dbContext.SaveChanges();
                return true;
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
                return false;
            }
        }

        /// <summary>
        /// To be valid:
        /// - The user record must contain a password reset token
        /// - The given token must match the token in the user record
        /// - The token timestamp must be less than 24 hours ago
        /// </summary>
        public bool IsPasswordResetTokenValid(UserModel user, string token)
        {
            try
            {
                if (user == null || string.IsNullOrEmpty(user.PasswordResetToken))
                {
                    return false;
                }
                if (user.PasswordResetTokenCreatedAt == null)
                {
                    return false;
                }
                return user.PasswordResetToken == token;
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
                return false;
            }
        }

        /// <summary>
        /// Generate a password reset token, a guid is sufficient
        /// </summary>
        private static string CreatePasswordResetToken()
        {
            return Guid.NewGuid().ToString().ToLower().Replace("-", "");
        }

        /// <summary>
        /// Reset user's password
        /// </summary>
        /// <param name="user"></param>
        /// <param name="newPassword"> </param>
        /// <returns></returns>
        public bool ResetPassword(UserModel user, string newPassword)
        {
            try
            {
                var encryptedPassword = securityService.EncryptText(newPassword);
                user.Password = encryptedPassword;
                user.UpdatedBy = user.UserId;
                user.UpdatedAt = DateTime.Now;
                dbContext.SaveChanges();
                return true;
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
                return false;
            }
        }

        /// <summary>
        /// Change user's password
        /// </summary>
        /// <param name="user"> </param>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public bool ChangePassword(UserModel user, string oldPassword, string newPassword)
        {
            var existingUser = GetUserById(user.UserId);
            if (oldPassword != securityService.DecryptText(existingUser.Password))
            {
                return false;
            }
            var encryptedPassword = securityService.EncryptText(newPassword);
            existingUser.Password = encryptedPassword;
            existingUser.UpdatedBy = user.UserId;
            existingUser.UpdatedAt = DateTime.Now;
            this.dbContext.SaveChanges();
            return true;
        }
        #endregion
    }
}