using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Configuration;

namespace StateOfOhioLibrary.Services
{
    public class SecurityService
    {
        #region Global Declaration
        CommonService commonService = new CommonService();

        string className = MethodBase.GetCurrentMethod().DeclaringType.Name;
        private string strEncryptionKey = Convert.ToString(WebConfigurationManager.AppSettings["EncryptionKey"]);
        #endregion

        #region Public Methods
        /// <summary>
        /// Method To Encrypt Text
        /// </summary>
        /// <param name="strPlainText">Text To Encrypt</param>
        /// <param name="strEncryptionPrivateKey">Encryption Private Key</param>
        /// <returns>Encrypted Text</returns>
        public virtual string EncryptText(string strPlainText, string strEncryptionPrivateKey = "")
        {
            try
            {
                if (string.IsNullOrEmpty(strPlainText))
                    return strPlainText;

                if (String.IsNullOrEmpty(strEncryptionPrivateKey))
                    strEncryptionPrivateKey = strEncryptionKey;

                var objTripleDESCAlgorithm = new TripleDESCryptoServiceProvider();
                objTripleDESCAlgorithm.Key = new ASCIIEncoding().GetBytes(strEncryptionPrivateKey.Substring(0, 16));
                objTripleDESCAlgorithm.IV = new ASCIIEncoding().GetBytes(strEncryptionPrivateKey.Substring(8, 8));

                byte[] bEncryptedBinary = EncryptTextToMemory(strPlainText, objTripleDESCAlgorithm.Key, objTripleDESCAlgorithm.IV);
                return Convert.ToBase64String(bEncryptedBinary);
            }
            catch (Exception exception)
            {
                commonService.LogException(className, MethodBase.GetCurrentMethod().Name, exception);
            }

            return null;
        }

        /// <summary>
        /// Method To Decrypt Text
        /// </summary>
        /// <param name="strCipherText">Text To Decrypt</param>
        /// <param name="strEncryptionPrivateKey">Encryption Private Key</param>
        /// <returns>Decrypted Text</returns>
        public virtual string DecryptText(string strCipherText, string strEncryptionPrivateKey = "")
        {
            try
            {
                if (String.IsNullOrEmpty(strCipherText))
                    return strCipherText;

                if (String.IsNullOrEmpty(strEncryptionPrivateKey))
                    strEncryptionPrivateKey = strEncryptionKey;

                var objTripleDESCAlgorithm = new TripleDESCryptoServiceProvider();
                objTripleDESCAlgorithm.Key = new ASCIIEncoding().GetBytes(strEncryptionPrivateKey.Substring(0, 16));
                objTripleDESCAlgorithm.IV = new ASCIIEncoding().GetBytes(strEncryptionPrivateKey.Substring(8, 8));

                byte[] bBuffer = Convert.FromBase64String(strCipherText);
                return DecryptTextFromMemory(bBuffer, objTripleDESCAlgorithm.Key, objTripleDESCAlgorithm.IV);
            }
            catch (Exception exception)
            {
                commonService.LogException(className, MethodBase.GetCurrentMethod().Name, exception);
            }

            return null;
        }

        /// <summary>
        /// Method To Decrypt Text
        /// </summary>
        /// <param name="strCipherText">Text To Decrypt</param>
        /// <param name="strEncryptionPrivateKey">Encryption Private Key</param>
        /// <returns>Decrypted Text</returns>
        public virtual string DecryptText(string strCipherText)
        {
            try
            {
                string strEncryptionPrivateKey = "";
                if (String.IsNullOrEmpty(strCipherText))
                    return strCipherText;

                if (String.IsNullOrEmpty(strEncryptionPrivateKey))
                    strEncryptionPrivateKey = strEncryptionKey;

                var objTripleDESCAlgorithm = new TripleDESCryptoServiceProvider();
                objTripleDESCAlgorithm.Key = new ASCIIEncoding().GetBytes(strEncryptionPrivateKey.Substring(0, 16));
                objTripleDESCAlgorithm.IV = new ASCIIEncoding().GetBytes(strEncryptionPrivateKey.Substring(8, 8));

                byte[] bBuffer = Convert.FromBase64String(strCipherText);
                return DecryptTextFromMemory(bBuffer, objTripleDESCAlgorithm.Key, objTripleDESCAlgorithm.IV);
            }
            catch (Exception exception)
            {
                commonService.LogException(className, MethodBase.GetCurrentMethod().Name, exception);
            }

            return null;
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Method To Encrypt Text To Memory
        /// </summary>
        /// <param name="strData"></param>
        /// <param name="bKey"></param>
        /// <param name="biv"></param>
        /// <returns></returns>
        private byte[] EncryptTextToMemory(string strData, byte[] bKey, byte[] biv)
        {
            try
            {
                using (var objMemoryStream = new MemoryStream())
                {
                    using (var objCryptoStream = new CryptoStream(objMemoryStream, new TripleDESCryptoServiceProvider().CreateEncryptor(bKey, biv), CryptoStreamMode.Write))
                    {
                        byte[] bToEncrypt = new UnicodeEncoding().GetBytes(strData);
                        objCryptoStream.Write(bToEncrypt, 0, bToEncrypt.Length);
                        objCryptoStream.FlushFinalBlock();
                    }

                    return objMemoryStream.ToArray();
                }
            }
            catch (Exception exception)
            {
                commonService.LogException(className, MethodBase.GetCurrentMethod().Name, exception);
            }

            return null;
        }

        /// <summary>
        /// Method To Decrypt Text From Memory
        /// </summary>
        /// <param name="bData"></param>
        /// <param name="bKey"></param>
        /// <param name="biv"></param>
        /// <returns></returns>
        private string DecryptTextFromMemory(byte[] bData, byte[] bKey, byte[] biv)
        {
            try
            {
                using (var objMemoryStream = new MemoryStream(bData))
                {
                    using (var objCryptoStream = new CryptoStream(objMemoryStream, new TripleDESCryptoServiceProvider().CreateDecryptor(bKey, biv), CryptoStreamMode.Read))
                    {
                        var objStreamReader = new StreamReader(objCryptoStream, new UnicodeEncoding());
                        return objStreamReader.ReadLine();
                    }
                }
            }
            catch (Exception exception)
            {
                commonService.LogException(className, MethodBase.GetCurrentMethod().Name, exception);
            }

            return "Error";
        }
        #endregion
    }
}