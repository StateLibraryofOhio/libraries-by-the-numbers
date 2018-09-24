
//Reference:- Install "iTextSharp" Package from NuGet Package Manager
//using iTextSharp.text.pdf;
using StateOfOhioLibrary.Models;
using StateOfOhioLibrary.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StateOfOhioLibrary.Controllers
{
    public class CustomController : Controller
    {
        #region Global Declaration
        CustomColumnService customColumnService = new CustomColumnService();
        #endregion

        #region ActionResult
        /// <summary>
        /// Index
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            CustomTemplateModel model = new CustomTemplateModel();
            List<SelectListItem> columns = customColumnService.PopulateCustomColumn();

            model.AvailableCustomColumn = columns;
            return View(model);

        }
        /// <summary>
        /// Action Result for download a pdf
        /// </summary>
        /// <param name="customColumnIdField1"></param>
        /// <param name="customColumnIdField2"></param>
        /// <param name="customColumnIdField3"></param>
        /// <param name="customColumnIdField4"></param>
        /// <param name="customColumnIdField5"></param>
        /// <param name="customColumnIdField6"></param>
        /// <returns></returns>
        public FileResult DownloadPdf(int customColumnIdField1, int customColumnIdField2, int customColumnIdField3, int customColumnIdField4, int customColumnIdField5, int customColumnIdField6)
        {

            string pdfHtml = "";
            string libraryId = "";
            int year = 0;
            if (!String.IsNullOrEmpty(Request.QueryString["LibraryId"]))
            {
                libraryId = Request.QueryString["LibraryId"];
            }
            if (!String.IsNullOrEmpty(Request.QueryString["Year"]))
            {
                year = Convert.ToInt32(Request.QueryString["Year"]);
            }
            pdfHtml = customColumnService.GeneratePdf(libraryId, year, customColumnIdField1, customColumnIdField2, customColumnIdField3, customColumnIdField4, customColumnIdField5, customColumnIdField6);
            byte[] bytes = CreatePDF(pdfHtml);
            return File(bytes, "application/pdf", string.Format("CustomTemplate_" + DateTime.Now.ToString("MMddyyyy") + ".pdf", libraryId));

        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Create PDF
        /// </summary>
        /// <param name="pdfBody"></param>
        /// <returns></returns>
        public byte[] CreatePDF(string pdfBody)
        {
            var htmlContent = String.Format(pdfBody);

            //Reference:- Install "NReco" Package from NuGet Package Manager
            //var htmlToPdf = new NReco.PdfGenerator.HtmlToPdfConverter();

            var pdfBytes = htmlToPdf.GeneratePdf(htmlContent);

            return pdfBytes;
        }
        #endregion
    }
}