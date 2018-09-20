using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using StateOfOhioLibrary.Models;
using StateOfOhioLibrary.Services;
using System.IO;
using System.Net;
using StateOfOhioLibrary.Data.Models;
using System.Web.Helpers;
using System.Drawing;
using System.Globalization;

namespace StateOfOhioLibrary.Controllers
{
    public class HomeController : Controller
    {
        #region Global Declaration
        PDFService pdfService = new PDFService();
        LibraryDataService libraryService = new LibraryDataService();
        CustomColumnService customColumnService = new CustomColumnService();
        CustomTemplateService customTemplateService = new CustomTemplateService();
        #endregion

        #region Public Action Result
        /// <summary>
        /// Index 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            HomeModel model = new HomeModel();
            List<SelectListItem> columns = customColumnService.PopulateCustomColumn();
            model.AvailableCustomColumn = columns;

            return View(model);
        }
        /// <summary>
        /// About
        /// </summary>
        /// <returns></returns>
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }
        /// <summary>
        /// Contact
        /// </summary>
        /// <returns></returns>
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        #endregion

        #region Ajax Methods
        /// <summary>
        /// Autocomplete ajax methods
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AutoComplete(string prefix)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            HomeModel model = new HomeModel();
            var library = libraryService.GetAllLibraryData().Where(x => x.Status == "Active");
            var libraryName = (from name in library
                               where name.LibraryName.ToUpper().Contains(prefix.ToUpper())
                               select new
                               {
                                   label = textInfo.ToTitleCase(name.LibraryName.ToLower()),
                                   val = textInfo.ToTitleCase(name.LibraryName.ToLower()),
                                   libraryId = name.LibraryID
                               }).OrderBy(x => x.label);
            var distinctLibraryName = libraryName.GroupBy(x => x.val).Select(y => y.Distinct()).ToList();

            return Json(distinctLibraryName);
        }
        /// <summary>
        /// Bind year by library name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult BindYear(string name)
        {
            var listYear = new List<LibraryDataModel>();
            var libraryData = libraryService.GetLibraryDataListByName(name);
            listYear = libraryData.GroupBy(x => x.DataYear).Select(y => y.First()).OrderByDescending(x => x.DataYear).ToList();
            return Json(listYear);
        }
        /// <summary>
        /// Bind library id
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult BindLibraryId(string name)
        {
            var libraryId = libraryService.GetLibraryDataListByName(name).FirstOrDefault().LibraryID;
            return Json(libraryId);
        }
        /// <summary>
        /// Get chart data
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public ActionResult GetChartData(string name, string type, int year)
        {
            LibraryDataService libraryDataService = new LibraryDataService();
            object result = null;
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(type))
            {
                if (type == HomeModel.EnumType.MaterialsandCirculation.ToString())
                {
                    var libraryData = libraryDataService.GetLibraryDataByName(name, year);
                    if (libraryData != null)
                    {
                        List<ChartData> chartData = pdfService.ChartData(libraryData, "Materials & Circulation", "Pie");
                        result = (from chart in chartData
                                  select new { name = chart.Name, y = chart.Value }).ToList();
                    }
                    else
                    {
                        result = "False";
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (type == HomeModel.EnumType.LibraryOverview.ToString())
                {
                    var libraryData = libraryDataService.GetLibraryDataByName(name, year);
                    if (libraryData != null)
                    {
                        List<ChartData> chartData = pdfService.ChartData(libraryData, "Library Overview", "Pie");
                        result = (from chart in chartData
                                  select new { name = chart.Name, y = chart.Value }).ToList();
                    }
                    else
                    {
                        result = "False";
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (type == HomeModel.EnumType.Programming.ToString())
                {
                    var libraryData = libraryDataService.GetLibraryDataByName(name, year);
                    if (libraryData != null)
                    {
                        List<ChartData> chartData = pdfService.ChartData(libraryData, "Programming", "Pie");
                        result = (from chart in chartData
                                  select new { name = chart.Name, y = chart.Value }).ToList();
                    }
                    else
                    {
                        result = "False";
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Generate pdf ajax methods
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public string GeneratePDF(string name, string type, int year)
        {
            string chartImage = "";
            if (Request.Form["chartImage"] != null)
            {
                chartImage = Request.Form["chartImage"].ToString().Replace("data:image/png;base64,", "");

                int length = chartImage.Length;
            }
            string result = string.Empty;
            string pdfUrl = string.Empty;
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(type))
            {
                if (type == HomeModel.EnumType.LibraryOverview.ToString())
                {
                    pdfUrl = pdfService.EditPdfLibraryOverview(name, "Library Overview", year, chartImage);
                    if (!string.IsNullOrEmpty(pdfUrl))
                    {
                        result = pdfUrl;
                    }
                }
                else if (type == HomeModel.EnumType.MaterialsandCirculation.ToString())
                {
                    pdfUrl = pdfService.EditPdfMaterialsandCirculation(name, "Materials and Circulation", year, chartImage);
                    if (!string.IsNullOrEmpty(pdfUrl))
                    {
                        result = pdfUrl;
                    }
                }
                else if (type == HomeModel.EnumType.SupportTechnology.ToString())
                {
                    pdfUrl = pdfService.EditPdfSupportTechnology(name, "Technology", year);
                    if (!string.IsNullOrEmpty(pdfUrl))
                    {
                        result = pdfUrl;
                    }
                }
                else if (type == HomeModel.EnumType.Programming.ToString())
                {
                    pdfUrl = pdfService.EditPdfProgramming(name, "Programming", year, chartImage);
                    if (!string.IsNullOrEmpty(pdfUrl))
                    {
                        result = pdfUrl;
                    }
                }
                else if (type == HomeModel.EnumType.YouthServices.ToString())
                {
                    pdfUrl = pdfService.EditPdfYouthServices(name, "Youth Services", year);
                    if (!string.IsNullOrEmpty(pdfUrl))
                    {
                        result = pdfUrl;
                    }
                }
                else if (type == HomeModel.EnumType.AdultServices.ToString())
                {
                    pdfUrl = pdfService.EditPdfAdultServices(name, "Adult Services", year);
                    if (!string.IsNullOrEmpty(pdfUrl))
                    {
                        result = pdfUrl;
                    }
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