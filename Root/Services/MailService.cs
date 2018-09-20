using StateOfOhioLibrary.Data.Models;
using StateOfOhioLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Web;
using System.Web.Configuration;

namespace StateOfOhioLibrary.Services
{
    public class MailService
    {
        #region Global Declarations
        DatabaseContext dbContext = new DatabaseContext();
        CommonService commonService = new CommonService();
        string className = MethodBase.GetCurrentMethod().DeclaringType.Name;
        string smtpServer = Convert.ToString(WebConfigurationManager.AppSettings["SMTPServer"]);
        string smtpPort = Convert.ToString(WebConfigurationManager.AppSettings["SMTPPort"]);
        string smtpUsername = Convert.ToString(WebConfigurationManager.AppSettings["SMTPUsername"]);
        string smtpPassword = Convert.ToString(WebConfigurationManager.AppSettings["SMTPPassword"]);
        string maiCC = Convert.ToString(WebConfigurationManager.AppSettings["MailCC"]);
        string mailBCC = Convert.ToString(WebConfigurationManager.AppSettings["MailBCC"]);
        bool smptIsSSL = Convert.ToBoolean(WebConfigurationManager.AppSettings["SMTPIsSSL"]);
        bool isMailSend = Convert.ToBoolean(WebConfigurationManager.AppSettings["IsMailSend"]);
        string companyName = Convert.ToString(WebConfigurationManager.AppSettings["CompanyName"]);
        string domainName = Convert.ToString(WebConfigurationManager.AppSettings["DomainName"]);
        string fromEmail = Convert.ToString(WebConfigurationManager.AppSettings["FromEmail"]);

        #endregion

        #region Public Methods

        /// <summary>
        /// Method to send email with attachment
        /// </summary>
        /// <param name="mailToName"></param>
        /// <param name="mailToEmail"></param>
        /// <param name="mailBody"></param>
        /// <param name="mailSubject"></param>
        /// <param name="attachment"></param>
        public void SendMail(string mailToName, string mailToEmail, string mailBody, string mailSubject)
        {
            try
            {
                MailAddress mailFrom = new MailAddress(fromEmail, companyName);
                MailAddress mailTo = new MailAddress(mailToEmail, mailToName);

                System.Net.Mail.MailMessage email = new System.Net.Mail.MailMessage(mailFrom, mailTo);

                email.IsBodyHtml = true;
                email.Subject = mailSubject;
                email.Body = mailBody;
                email.From = mailFrom;

                if (mailBCC != "")
                {
                    if (mailBCC.Contains(","))
                    {
                        string[] bccEmails = mailBCC.Split(',');

                        foreach (string bccEmail in bccEmails)
                        {
                            email.Bcc.Add(new MailAddress(bccEmail));
                        }
                    }
                    else
                    {
                        email.Bcc.Add(new MailAddress(mailBCC));
                    }
                }

                if (maiCC != "")
                {
                    if (maiCC.Contains(","))
                    {
                        string[] ccEmails = maiCC.Split(',');

                        foreach (string ccEmail in ccEmails)
                        {
                            email.CC.Add(new MailAddress(ccEmail));
                        }
                    }
                    else
                    {
                        email.CC.Add(new MailAddress(maiCC));
                    }
                }

                System.Net.NetworkCredential emailAuthentication = new System.Net.NetworkCredential(smtpUsername, smtpPassword);
                System.Net.Mail.SmtpClient emailSMTP = new System.Net.Mail.SmtpClient(smtpServer, Convert.ToInt32(smtpPort));
                emailSMTP.Credentials = emailAuthentication;

                if (smtpServer == "smtp.gmail.com")
                    emailSMTP.EnableSsl = true;
                else
                {
                    if (smptIsSSL)
                        emailSMTP.EnableSsl = true;
                    else
                        emailSMTP.EnableSsl = false;
                }

                if (isMailSend == true)
                {
                    emailSMTP.Send(email);
                }
            }
            catch (Exception exception)
            {
                commonService.LogException(className, MethodBase.GetCurrentMethod().Name, exception);
            }
        }

        /// <summary>
        /// Method to create the header portion of the email body
        /// </summary>
        /// <returns></returns>
        public string HeaderTemplate()
        {
            string strHtml = string.Empty;

            strHtml += "<table width=\"100%\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">";
            strHtml += "<tr><td align=\"center\" style=\"background-color:#fafafa;padding:50px 0px;\">";

            strHtml += "<table width=\"600\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">";
            strHtml += "<tr>";
            strHtml += "<td align=\"center\" valign=\"top\" style=\"border:#cccccc 1px solid; padding:10px;background:#fff;\"><table width=\"100%\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">";
            strHtml += "<tr>";
            strHtml += "<td align=\"center\" valign=\"top\"><table width=\"100%\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">";
            strHtml += "<tr>";
            strHtml += "<td align=\"center\" valign=\"top\" style=\"border-bottom:#e9edf2 1px solid; padding-bottom:5px; background:#e9edf2;\">";
            strHtml += "<a><img src=\"" + domainName + "Content/images/logo.png\" alt=\"mail_logo\" border=\"0\" /></a>";
            strHtml += "</td>";
            strHtml += "</tr>";

            return strHtml;
        }

        /// <summary>
        /// Method to create the description portion of the email body
        /// </summary>
        /// <param name="strHeader"></param>
        /// <param name="strDescription"></param>
        /// <returns></returns>
        public string DescriptionTemplate(string name, string description)
        {
            string strHtml = string.Empty;

            if (!String.IsNullOrEmpty(name))
            {
                strHtml += "<tr>";
                strHtml += "<td align=\"left\" valign=\"top\" style=\"padding:5px; color: #000000;font-family: Verdana,Arial,Helvetica,sans-serif;font-size: 13px;font-weight: normal\">Hello " + name + ",</td>";
                strHtml += "</tr>";
            }
            else
            {
                strHtml += "<tr>";
                strHtml += "<td align=\"left\" valign=\"top\" style=\"padding:5px; color: #000000;font-family: Verdana,Arial,Helvetica,sans-serif;font-size: 13px;font-weight: normal\">Hello,</td>";
                strHtml += "</tr>";
            }

            if (!String.IsNullOrEmpty(description))
            {
                strHtml += "<tr>";
                strHtml += "<td align=\"left\" valign=\"top\" style=\"padding:0px 5px 10px 5px; color: #000000; font-family: Verdana,Arial,Helvetica,sans-serif;font-size: 13px; font-weight: normal\">" + description + "</td>";
                strHtml += "</tr>";
            }

            return strHtml;
        }

        /// <summary>
        /// Method to give description in the footer
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public string FooterDescriptionTemplate(string description)
        {
            string strHtml = string.Empty;

            strHtml += "<tr><td align=\"left\" valign=\"top\" style=\"padding:10px 5px 10px 5px; color: #000000; font-family: Verdana,Arial,Helvetica,sans-serif;font-size: 13px; font-weight: normal\">" + description + "</td></tr>";

            return strHtml;
        }

        /// <summary>
        /// Method to make the footer portion of the email body
        /// </summary>
        /// <returns></returns>
        public string FooterTemplate()
        {
            string strHtml = string.Empty;

            strHtml += "<tr><td align=\"left\" valign=\"top\" style=\"padding:10px 5px 0px 5px; color: #000000; font-family: Verdana,Arial,Helvetica,sans-serif;font-size: 13px; font-weight: normal\">Thank you</td></tr>";
            strHtml += "<tr><td align=\"left\" valign=\"top\" style=\"padding:10px 5px 10px 5px; color: #000000; font-family: Verdana,Arial,Helvetica,sans-serif;font-size: 13px; font-weight: normal\">Customer Service</td></tr>";
            strHtml += "<tr><td align=\"left\" valign=\"top\" style=\"padding:0px 5px 10px 5px; color: #000000; font-family: Verdana,Arial,Helvetica,sans-serif;font-size: 13px; font-weight: normal\">" + companyName + "</td></tr>";
            strHtml += "<tr><td align=\"center\" valign=\"top\" style=\"padding:20px; color: #4a4c4d;font-family: Verdana,Arial,Helvetica,sans-serif;font-size: 11px;font-weight: normal; background-color:#e9edf2;\">&copy; Copyright " + DateTime.Now.Year + ". " + companyName + ". All rights reserved.</td>";
            strHtml += "</tr></table></td></tr></table></td></tr></table></td></tr></table>";

            return strHtml;
        }


        #endregion
    }
}