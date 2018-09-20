using StateOfOhioLibrary.Models;
using StateOfOhioLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace StateOfOhioLibrary.Controllers
{
    public class MultiYearTrendController : Controller
    {
        #region Global Declaration
        string domainName = WebConfigurationManager.AppSettings["DomainName"];
        LibraryDataService libraryDataService = new LibraryDataService();
        CustomTemplateService customTemplateService = new CustomTemplateService();
        PDFService pdfService = new PDFService();
        #endregion

        #region ActionResult
        // GET: MultiYearTrend
        /// <summary>
        /// Index
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// Download PDF
        /// </summary>
        /// <param name="libraryName"></param>
        /// <param name="statistics"></param>
        /// <param name="startYear"></param>
        /// <param name="endYear"></param>
        /// <returns></returns>
        public ActionResult DownloadPdf(string libraryName, string statistics, string startYear, string endYear)
        {
            LibraryDataService libraryDataService = new LibraryDataService();
            object result = null;
            if (!string.IsNullOrEmpty(libraryName))
            {
                var customTemplateDetails = customTemplateService.GetCustomTemplateById(Convert.ToInt32(statistics));
                var libraryData = libraryDataService.GetLibraryDataByNameAndYearRange(libraryName, Convert.ToInt32(startYear), Convert.ToInt32(endYear));
                string mappingValue = customTemplateDetails.MappingColumn;
                List<ChartData> chartData = pdfService.BarChartData(libraryData, "MyltiYear", "bar", mappingValue);
                result = (from chart in chartData
                          select new { name = chart.Name, y = chart.Value }).ToList();
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Generate PDF
        /// </summary>
        /// <param name="libraryName"></param>
        /// <param name="statistics"></param>
        /// <param name="startYear"></param>
        /// <param name="endYear"></param>
        /// <returns></returns>
        public string GeneratePDF(string libraryName, string statistics, string startYear, string endYear)
        {
            string chartImage = "";
            if (Request.Form["chartImage"] != null)
            {
                chartImage = Request.Form["chartImage"].ToString().Replace("data:image/png;base64,", "");
                int length = chartImage.Length;
            }
            string result = string.Empty;
            string pdfUrl = string.Empty;
            if (!string.IsNullOrEmpty(libraryName))
            {
                pdfUrl = pdfService.EditMultiYearTrend(libraryName, statistics, startYear, endYear, chartImage);
                if (!string.IsNullOrEmpty(pdfUrl))
                {
                    result = pdfUrl;
                }
            }
            else
            {
                result = "False";
            }
            return result;
        }
        #endregion
    }
}