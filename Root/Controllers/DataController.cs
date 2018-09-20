
using StateOfOhioLibrary.Data.Models;
using StateOfOhioLibrary.Models;
using StateOfOhioLibrary.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace StateOfOhioLibrary.Controllers
{
    public class DataController : Controller
    {
        #region Global Declaration

        LibraryDataService libraryDataService = new LibraryDataService();
        DataUploadHistoryService dataUploadHistoryService = new DataUploadHistoryService();
        CommonService commonService = new CommonService();
        LogService logService = new LogService();
        ExcelParser excelParser = new ExcelParser();
        PDFService pdfService = new PDFService();
        string consString = Convert.ToString(WebConfigurationManager.AppSettings["ConnectionString"]);
        string libraryDataPath = WebConfigurationManager.AppSettings["LibraryDataPath"];
        string libraryDataHistoryPath = WebConfigurationManager.AppSettings["LibraryDataHistoryPath"];
        int intPageSize = Convert.ToInt32(WebConfigurationManager.AppSettings["PaginationCount"]);

        #endregion

        #region Action Result
        // GET: Data
        /// <summary>
        /// Index
        /// </summary>
        /// <param name="dataYear"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="sortdir"></param>
        /// <returns></returns>
        public ActionResult Index(int dataYear = 0, int page = 1, int pageSize = 1, string sort = "UploadDate", string sortdir = "DESC")
        {
            DataModel model = new DataModel();
            try
            {
                commonService.CheckLoggedIn();
                model = libraryDataService.GetLibraryDataUploadHistory(dataYear, sort, sortdir, page, intPageSize);
                #region Binding Year
                model.YearList.Add(new SelectListItem { Text = "--Select--", Value = "0", Selected = true });

                for (int i = 0; i < 10; i++)
                {
                    int year = DateTime.Now.AddYears(-i).Year;
                    model.YearList.Add(new SelectListItem { Text = year.ToString(), Value = year.ToString() });
                }
                #endregion
                return View(model);
            }
            catch (Exception exception)
            {
                throw exception;
            }

        }
        /// <summary>
        /// Method to get data list
        /// </summary>
        /// <param name="dataYear"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="sortdir"></param>
        /// <returns></returns>
        public ActionResult DataList(int dataYear = 0, int page = 1, int pageSize = 1, string sort = "UploadDate", string sortdir = "DESC")
        {
            var model = new DataModel();
            try
            {
                model = libraryDataService.GetLibraryDataUploadHistory(dataYear, sort, sortdir, page, intPageSize);
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }
            return PartialView("_DataView", model);
        }
        /// <summary>
        /// Upload file
        /// </summary>
        /// <returns></returns>
        public ActionResult _FileUpload()
        {
            DataModel model = new DataModel();
            try
            {
                #region Binding Year
                model.YearList.Add(new SelectListItem { Text = "--Select", Value = "0" });

                for (int i = 0; i < 10; i++)
                {
                    int year = DateTime.Now.AddYears(-i).Year;
                    model.YearList.Add(new SelectListItem { Text = year.ToString(), Value = year.ToString() });
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return View(model);
        }
        #endregion

        #region Ajax Methods

        /// <summary>
        /// Method to Add Template
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddLibraryData(int year, string name)
        {
            string result = string.Empty;
            var currentUser = commonService.GetCurrentUser();
            DataTable dt = new DataTable();
            if (currentUser == null)
            {
                result = "Login";
                return Json(result);
            }
            string fileName = string.Empty;
            DataModel model = new DataModel();
            try
            {
                CleanFiles(libraryDataHistoryPath + "Temp//" + year);
                if (year != 0)
                {
                    libraryDataService.DeleteDataByYear(year);
                    model.DataYear = year;
                    model.Status = EnumStatus.Active.ToString();
                    model.Name = name;
                }
                #region Excel Uploads

                if (Request.Files != null && Request.Files.Count > 0)
                {
                    var requestfile = Request.Files[0];
                    string currentFilePath = Path.GetFullPath(Request.Files[0].FileName);
                    fileName = Path.GetFileName(requestfile.FileName);
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        string directoryPath = Server.MapPath(libraryDataHistoryPath + "Temp//" + year + "//");
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }
                        var path = Path.Combine(directoryPath, requestfile.FileName);
                        requestfile.SaveAs(path);
                        result = UploadData(requestfile, model, currentUser, path);
                    }
                }

                #endregion
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }
            return Json(result);
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Method to get selected column list from actual excel sheet
        /// </summary>
        /// <returns></returns>
        public string[] GetSelectedColumns()
        {
            string[] columns = new string[44];
            columns[0] = "2015 Public Library Statistics";//Name
            columns[1] = "F3";//County - 1.11
            columns[2] = "F9";//PopulationLegalSvcArea - 1.24
            columns[3] = "F10";//RegAdults - 1.25
            columns[4] = "F11";//RegChildren - 1.26
            columns[5] = "F16";//CentralLibraries - 2.43
            columns[6] = "F17";//Branches - 2.44
            columns[7] = "F20";//AnnualHrsOpen - 2.47
            columns[8] = "F21";//InternetPC - 2.48
            columns[9] = "F22";//AnnualUses - 2.49
            columns[10] = "F23";//AnnualWirelessSessions - 2.50
            columns[11] = "F24";//CircPhysicalMaterial 
            columns[12] = "F25";//CircDownloadableMaterial - 2.51
            columns[13] = "F26";//CircTotal - 2.52
            columns[14] = "F27";//CircAdult - 2.53
            columns[15] = "F28";//CircJuvenile - 2.54
            columns[16] = "F36";//LibraryStaffTotal - 5.18
            columns[17] = "F37";//PrintMaterials - 7.1
            columns[18] = "F38";//PrintSubscription - 7.2
            columns[19] = "F39";//PhysicalVideo - 7.3
            columns[20] = "F40";//DownlableVideo - 7.4
            columns[21] = "F41";//PhysicalAudio - 7.5
            columns[22] = "F42";//DownloadableAudio - 7.6
            columns[23] = "F43";//Ebooks - 7.7  
            columns[24] = "F44";//ComputerSoftware - 7.8
            columns[25] = "F47";//Databases - 7.11
            columns[26] = "F48";//ILLProvided - 8.1
            columns[27] = "F49";//ILLReceived - 8.2
            columns[28] = "F50";//AttendanceTypicalWeek - 8.3
            columns[29] = "F53";//AnnualReferenceTransaction - 8.6
            columns[30] = "F54";//ChildrenPrograms - 8.7
            columns[31] = "F55";//YoungAdultPrograms - 8.8
            columns[32] = "F57";//TotalLibraryPrograms - 8.10 //Total # of Library Programs
            columns[33] = "F58";//ChildrenProgramAttendance - 8.11
            columns[34] = "F59";//YoungAdultProgramAttendance - 8.12
            columns[35] = "F60";//TotalProgramAttendance - 8.13 //Adult Program Attendance
            columns[36] = "F61";//TotalLibraryProgramAttendance - 8.14
            columns[37] = "F177";//SummerReadingChildren - 13.1
            columns[38] = "F178";//SummerReadingTeens - 13.2
            columns[39] = "F179";//SummerReadingAdult - 13.3
            columns[40] = "F18";//BookMobiles - 2.45
            columns[41] = "F181";//HomeWorkSessions - 13.5
            columns[42] = "F182";//MealsSnacksSFSP - 13.6
            columns[43] = "F51";//AnnualAttendanceInLibrary - 8.4

            return columns;
        }
        /// <summary>
        /// Method to get selected column list from actual excel XSLX sheet
        /// </summary>
        /// <returns></returns>
        public string[] GetSelectedColumnsXSLX()
        {
            string[] columns = new string[44];
            columns[0] = "2015 Public Library Statistics";//Name
            columns[1] = "1#11";//County - 1.11
            columns[2] = "1#24";//PopulationLegalSvcArea - 1.24
            columns[3] = "1#25";//RegAdults - 1.25
            columns[4] = "1#26";//RegChildren - 1.26
            columns[5] = "2#43";//CentralLibraries - 2.43
            columns[6] = "2#44";//Branches - 2.44
            columns[7] = "2#47";//AnnualHrsOpen - 2.47
            columns[8] = "2#48";//InternetPC - 2.48
            columns[9] = "2#49";//AnnualUses - 2.49
            columns[10] = "2#50";//AnnualWirelessSessions - 2.50
            columns[11] = "F24";//CircPhysicalMaterial 
            columns[12] = "2#51";//CircDownloadableMaterial - 2.51
            columns[13] = "2#52";//CircTotal - 2.52
            columns[14] = "2#53";//CircAdult - 2.53
            columns[15] = "2#54";//CircJuvenile - 2.54
            columns[16] = "5#18";//LibraryStaffTotal - 5.18
            columns[17] = "7#1";//PrintMaterials - 7.1
            columns[18] = "7#2";//PrintSubscription - 7.2
            columns[19] = "7#3";//PhysicalVideo - 7.3
            columns[20] = "7#4";//DownlableVideo - 7.4
            columns[21] = "7#5";//PhysicalAudio - 7.5
            columns[22] = "7#6";//DownloadableAudio - 7.6
            columns[23] = "7#7";//Ebooks - 7.7
            columns[24] = "7#8";//ComputerSoftware - 7.8
            columns[25] = "7#11";//Databases - 7.11
            columns[26] = "8#1";//ILLProvided - 8.1
            columns[27] = "8#2";//ILLReceived - 8.2
            columns[28] = "8#3";//AttendanceTypicalWeek - 8.3
            columns[29] = "8#6";//AnnualReferenceTransaction - 8.6
            columns[30] = "8#7";//ChildrenPrograms - 8.7
            columns[31] = "8#8";//YoungAdultPrograms - 8.8
            columns[32] = "8#10";//TotalLibraryPrograms - 8.10 // Adult Programs
            columns[33] = "8#11";//ChildrenProgramAttendance - 8.11
            columns[34] = "8#12";//YoungAdultProgramAttendance - 8.12
            columns[35] = "8#13";//TotalProgramAttendance - 8.13 //Adult Program Attendance
            columns[36] = "8#14";//TotalLibraryProgramAttendance - 8.14
            columns[37] = "13#1";//SummerReadingChildren - 13.1
            columns[38] = "13#2";//SummerReadingTeens - 13.2
            columns[39] = "13#3";//SummerReadingAdult - 13.3
            columns[40] = "2#45";//BookMobiles - 2.45
            columns[41] = "13#5";//HomeWorkSessions - 13.5
            columns[42] = "13#6";//MealsSnacksSFSP - 13.6
            columns[43] = "8#4";//AnnualAttendanceInLibrary - 8.4

            return columns;
        }
        /// <summary>
        /// Generate Library ID
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GenerateLibraryID(int index)
        {
            string libraryID = "";
            if (index >= 0 && index <= 9)
            {
                libraryID = "00" + (index + 1);
            }
            else
            {
                libraryID = "0" + (index + 1);
            }
            return libraryID;

        }
        /// <summary>
        /// Insert data from excel to DB
        /// </summary>
        /// <param name="dtFinalColumns"></param>
        /// <param name="model"></param>
        /// <param name="libraryHistoryId"></param>
        /// <param name="currentUser"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public string InsertDataFromExcelToDB(DataTable dtFinalColumns, DataModel model, UserModel currentUser, HttpPostedFileBase file)
        {
            string result = "";
            int count = 0;
            try
            {
                logService.WriteInfo("Excel parsing started.");
                #region Library Data Insert
                DataRow[] foundRows;
                string filter = "[" + model.DataYear + " Public Library Statistics] = 'Location' AND [F3]='County'";
                foundRows = dtFinalColumns.Select(filter);
                int index = 0;
                foreach (DataRow dr in foundRows)
                {
                    index = dr.Table.Rows.IndexOf(dr);
                }
                for (int i = (index + 1); i < dtFinalColumns.Rows.Count; i++)
                {
                    LibraryDataModel libraryData = new LibraryDataModel();
                    libraryData.Status = model.Status;
                    libraryData.DataYear = model.DataYear;
                    libraryData.CreatedBy = currentUser.UserId;
                    libraryData.CreatedAt = DateTime.Now;
                    if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["2015 Public Library Statistics"].ToString()))
                    {
                        try
                        {
                            libraryData.LibraryID = GenerateLibraryID(i);
                            logService.WriteInfo("Row No. " + (i + 1).ToString() + " Inputs:");
                            logService.WriteInfo("Data Year - " + libraryData.DataYear);
                            logService.WriteInfo("Library ID - " + libraryData.LibraryID);

                            libraryData.LibraryName = dtFinalColumns.Rows[i]["2015 Public Library Statistics"].ToString();
                            logService.WriteInfo("Library Name - " + libraryData.LibraryName);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F3"].ToString()))
                                libraryData.County = dtFinalColumns.Rows[i]["F3"].ToString();
                            logService.WriteInfo("County - " + libraryData.County);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F21"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F21"].ToString() != "N/A")
                                    libraryData.InternetPC = Convert.ToInt32(dtFinalColumns.Rows[i]["F21"].ToString().Replace(",", ""));
                                else
                                    libraryData.InternetPC = 0;
                            }
                            logService.WriteInfo("InternetPC - " + libraryData.InternetPC);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F22"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F22"].ToString() != "N/A")
                                    libraryData.AnnualUses = Convert.ToInt32(dtFinalColumns.Rows[i]["F22"].ToString().Replace(",", ""));
                                else
                                    libraryData.AnnualUses = 0;
                            }
                            logService.WriteInfo("AnnualUses - " + libraryData.AnnualUses);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F23"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F23"].ToString() != "N/A")
                                    libraryData.AnnualWirelessSessions = Convert.ToInt32(dtFinalColumns.Rows[i]["F23"].ToString().Replace(",", ""));
                                else
                                    libraryData.AnnualWirelessSessions = 0;
                            }
                            logService.WriteInfo("AnnualWirelessSessions - " + libraryData.AnnualWirelessSessions);


                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F24"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F24"].ToString() != "N/A")
                                    libraryData.CircPhysicalMaterial = Convert.ToInt32(dtFinalColumns.Rows[i]["F24"].ToString().Replace(",", ""));
                                else
                                    libraryData.CircPhysicalMaterial = 0;
                            }

                            logService.WriteInfo("CircPhysicalMaterial - " + libraryData.CircPhysicalMaterial);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F25"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F25"].ToString() != "N/A")
                                    libraryData.CircDownloadableMaterial = Convert.ToInt32(dtFinalColumns.Rows[i]["F25"].ToString().Replace(",", ""));
                                else
                                    libraryData.CircDownloadableMaterial = 0;
                            }
                            logService.WriteInfo("CircDownloadableMaterial - " + libraryData.CircDownloadableMaterial);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F26"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F26"].ToString() != "N/A")
                                    libraryData.CircTotal = Convert.ToInt32(dtFinalColumns.Rows[i]["F26"].ToString().Replace(",", ""));
                                else
                                    libraryData.CircTotal = 0;
                            }
                            logService.WriteInfo("CircTotal - " + libraryData.CircTotal);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F27"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F27"].ToString() != "N/A")
                                    libraryData.CircAdult = Convert.ToInt32(dtFinalColumns.Rows[i]["F27"].ToString().Replace(",", ""));
                                else
                                    libraryData.CircAdult = 0;
                            }
                            logService.WriteInfo("CircAdult - " + libraryData.CircAdult);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F28"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F28"].ToString() != "N/A")
                                    libraryData.CircJuvenile = Convert.ToInt32(dtFinalColumns.Rows[i]["F28"].ToString().Replace(",", ""));
                                else
                                    libraryData.CircJuvenile = 0;
                            }
                            logService.WriteInfo("CircJuvenile - " + libraryData.CircJuvenile);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F47"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F47"].ToString() != "N/A")
                                    libraryData.Databases = Convert.ToInt32(dtFinalColumns.Rows[i]["F47"].ToString().Replace(",", ""));
                                else
                                    libraryData.Databases = 0;
                            }
                            logService.WriteInfo("Databases - " + libraryData.Databases);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F37"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F37"].ToString() != "N/A")
                                    libraryData.PrintMaterial = Convert.ToInt32(dtFinalColumns.Rows[i]["F37"].ToString().Replace(",", ""));
                                else
                                    libraryData.PrintMaterial = 0;
                            }
                            logService.WriteInfo("PrintMaterial - " + libraryData.PrintMaterial);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F39"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F39"].ToString() != "N/A")
                                    libraryData.PhysicalVideo = Convert.ToInt32(dtFinalColumns.Rows[i]["F39"].ToString().Replace(",", ""));
                                else
                                    libraryData.PhysicalVideo = 0;
                            }
                            logService.WriteInfo("PhysicalVideo - " + libraryData.PhysicalVideo);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F41"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F41"].ToString() != "N/A")
                                    libraryData.PhysicalAudio = Convert.ToInt32(dtFinalColumns.Rows[i]["F41"].ToString().Replace(",", ""));
                                else
                                    libraryData.PhysicalAudio = 0;
                            }
                            logService.WriteInfo("PhysicalAudio - " + libraryData.PhysicalAudio);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F43"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F43"].ToString() != "N/A")
                                    libraryData.Ebooks = Convert.ToInt32(dtFinalColumns.Rows[i]["F43"].ToString().Replace(",", ""));
                                else
                                    libraryData.Ebooks = 0;
                            }
                            logService.WriteInfo("Ebooks - " + libraryData.Ebooks);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F9"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F9"].ToString() != "N/A")
                                    libraryData.PopulationLegalSvcArea = Convert.ToInt32(dtFinalColumns.Rows[i]["F9"].ToString().Replace(",", ""));
                                else
                                    libraryData.PopulationLegalSvcArea = 0;
                            }
                            logService.WriteInfo("PopulationLegalSvcArea - " + libraryData.PopulationLegalSvcArea);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F10"].ToString()))
                                libraryData.RegAdults = Convert.ToInt32(dtFinalColumns.Rows[i]["F10"].ToString().Replace(",", ""));
                            logService.WriteInfo("RegAdults - " + libraryData.RegAdults);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F11"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F11"].ToString() != "N/A")
                                    libraryData.RegChildren = Convert.ToInt32(dtFinalColumns.Rows[i]["F11"].ToString().Replace(",", ""));
                                else
                                    libraryData.RegChildren = 0;
                            }
                            logService.WriteInfo("RegChildren - " + libraryData.RegChildren);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F16"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F16"].ToString() != "N/A")
                                    libraryData.CentralLibraries = Convert.ToInt32(dtFinalColumns.Rows[i]["F16"].ToString().Replace(",", ""));
                                else
                                    libraryData.CentralLibraries = 0;
                            }
                            logService.WriteInfo("CentralLibraries - " + libraryData.CentralLibraries);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F17"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F17"].ToString() != "N/A")
                                    libraryData.Branches = Convert.ToInt32(dtFinalColumns.Rows[i]["F17"].ToString().Replace(",", ""));
                                else
                                    libraryData.Branches = 0;
                            }
                            logService.WriteInfo("Branches - " + libraryData.Branches);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F20"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F20"].ToString() != "N/A")
                                    libraryData.AnnualHrsOpen = Convert.ToDecimal(dtFinalColumns.Rows[i]["F20"].ToString().Replace(",", ""));
                                else
                                    libraryData.AnnualHrsOpen = 0;
                            }
                            logService.WriteInfo("CentralLibraries - " + libraryData.CentralLibraries);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F36"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F36"].ToString() != "N/A")
                                    libraryData.LibraryStaffTotal = Convert.ToDecimal(dtFinalColumns.Rows[i]["F36"].ToString().Replace(",", ""));
                                else
                                    libraryData.LibraryStaffTotal = 0;
                            }
                            logService.WriteInfo("LibraryStaffTotal - " + libraryData.LibraryStaffTotal);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F37"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F37"].ToString() != "N/A")
                                    libraryData.PrintMaterials = Convert.ToInt32(dtFinalColumns.Rows[i]["F37"].ToString().Replace(",", ""));
                                else
                                    libraryData.PrintMaterials = 0;
                            }
                            logService.WriteInfo("PrintMaterials - " + libraryData.PrintMaterials);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F38"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F38"].ToString() != "N/A")
                                    libraryData.PrintSubscription = Convert.ToInt32(dtFinalColumns.Rows[i]["F38"].ToString().Replace(",", ""));
                                else
                                    libraryData.PrintSubscription = 0;
                            }
                            logService.WriteInfo("PrintSubscription - " + libraryData.PrintSubscription);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F40"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F40"].ToString() != "N/A")
                                    libraryData.DownloadableVideo = Convert.ToInt32(dtFinalColumns.Rows[i]["F40"].ToString().Replace(",", ""));
                                else
                                    libraryData.DownloadableVideo = 0;
                            }
                            logService.WriteInfo("DownloadableVideo - " + libraryData.DownloadableVideo);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F42"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F42"].ToString() != "N/A")
                                    libraryData.DownloadableAudio = Convert.ToInt32(dtFinalColumns.Rows[i]["F42"].ToString().Replace(",", ""));
                                else
                                    libraryData.DownloadableAudio = 0;
                            }
                            logService.WriteInfo("DownloadableAudio - " + libraryData.DownloadableAudio);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F44"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F44"].ToString() != "N/A")
                                    libraryData.ComputerSoftware = Convert.ToInt32(dtFinalColumns.Rows[i]["F44"].ToString().Replace(",", ""));
                                else
                                    libraryData.ComputerSoftware = 0;
                            }
                            logService.WriteInfo("ComputerSoftware - " + libraryData.ComputerSoftware);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F48"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F48"].ToString() != "N/A")
                                    libraryData.ILLProvided = Convert.ToInt32(dtFinalColumns.Rows[i]["F48"].ToString().Replace(",", ""));
                                else
                                    libraryData.ILLProvided = 0;
                            }
                            logService.WriteInfo("ILLProvided - " + libraryData.ILLProvided);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F49"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F49"].ToString() != "N/A")
                                    libraryData.ILLReceived = Convert.ToInt32(dtFinalColumns.Rows[i]["F49"].ToString().Replace(",", ""));
                                else
                                    libraryData.ILLReceived = 0;
                            }
                            logService.WriteInfo("ILLReceived - " + libraryData.ILLReceived);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F50"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F50"].ToString() != "N/A")
                                    libraryData.AttendanceTypicalWeek = Convert.ToInt32(dtFinalColumns.Rows[i]["F50"].ToString().Replace(",", ""));
                                else
                                    libraryData.AttendanceTypicalWeek = 0;
                            }
                            logService.WriteInfo("AttendanceTypicalWeek - " + libraryData.AttendanceTypicalWeek);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F53"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F53"].ToString() != "N/A")
                                    libraryData.AnnualReferenceTransaction = Convert.ToInt32(dtFinalColumns.Rows[i]["F53"].ToString().Replace(",", ""));
                                else
                                    libraryData.AnnualReferenceTransaction = 0;
                            }
                            logService.WriteInfo("AnnualReferenceTransaction - " + libraryData.AnnualReferenceTransaction);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F54"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F54"].ToString() != "N/A")
                                    libraryData.ChildrenPrograms = Convert.ToInt32(dtFinalColumns.Rows[i]["F54"].ToString().Replace(",", ""));
                                else
                                    libraryData.ChildrenPrograms = 0;
                            }
                            logService.WriteInfo("ChildrenPrograms - " + libraryData.ChildrenPrograms);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F55"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F55"].ToString() != "N/A")
                                    libraryData.YoungAdultPrograms = Convert.ToInt32(dtFinalColumns.Rows[i]["F55"].ToString().Replace(",", ""));
                                else
                                    libraryData.YoungAdultPrograms = 0;
                            }
                            logService.WriteInfo("YoungAdultPrograms - " + libraryData.YoungAdultPrograms);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F57"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F57"].ToString() != "N/A")
                                    libraryData.TotalLibraryPrograms = Convert.ToInt32(dtFinalColumns.Rows[i]["F57"].ToString().Replace(",", ""));
                                else
                                    libraryData.TotalLibraryPrograms = 0;
                            }
                            logService.WriteInfo("TotalLibraryPrograms - " + libraryData.TotalLibraryPrograms);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F58"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F58"].ToString() != "N/A")
                                    libraryData.ChildrenProgramAttendance = Convert.ToInt32(dtFinalColumns.Rows[i]["F58"].ToString().Replace(",", ""));
                                else
                                    libraryData.ChildrenProgramAttendance = 0;
                            }
                            logService.WriteInfo("ChildrenProgramAttendance - " + libraryData.ChildrenProgramAttendance);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F59"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F59"].ToString() != "N/A")
                                    libraryData.YoungAdultProgramAttendance = Convert.ToInt32(dtFinalColumns.Rows[i]["F59"].ToString().Replace(",", ""));
                                else
                                    libraryData.YoungAdultProgramAttendance = 0;
                            }
                            logService.WriteInfo("YoungAdultProgramAttendance - " + libraryData.YoungAdultProgramAttendance);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F60"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F60"].ToString() != "N/A")
                                    libraryData.TotalProgramAttendance = Convert.ToInt32(dtFinalColumns.Rows[i]["F60"].ToString().Replace(",", ""));
                                else
                                    libraryData.TotalProgramAttendance = 0;
                            }
                            logService.WriteInfo("TotalProgramAttendance - " + libraryData.TotalProgramAttendance);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F61"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F61"].ToString() != "N/A")
                                    libraryData.TotalLibraryProgramAttendance = Convert.ToInt32(dtFinalColumns.Rows[i]["F61"].ToString().Replace(",", ""));
                                else
                                    libraryData.TotalLibraryProgramAttendance = 0;
                            }
                            logService.WriteInfo("TotalLibraryProgramAttendance - " + libraryData.TotalLibraryProgramAttendance);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F177"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F177"].ToString() != "N/A")
                                    libraryData.SummerReadingChildren = Convert.ToInt32(dtFinalColumns.Rows[i]["F177"].ToString().Replace(",", ""));
                                else
                                    libraryData.SummerReadingChildren = 0;
                            }
                            logService.WriteInfo("SummerReadingChildren - " + libraryData.SummerReadingChildren);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F178"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F178"].ToString() != "N/A")
                                    libraryData.SummerReadingTeens = Convert.ToInt32(dtFinalColumns.Rows[i]["F178"].ToString().Replace(",", ""));
                                else
                                    libraryData.SummerReadingTeens = 0;
                            }
                            logService.WriteInfo("SummerReadingTeens - " + libraryData.SummerReadingTeens);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F179"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F179"].ToString() != "N/A")
                                    libraryData.SummerReadingAdult = Convert.ToInt32(dtFinalColumns.Rows[i]["F179"].ToString().Replace(",", ""));
                                else
                                    libraryData.SummerReadingAdult = 0;
                            }
                            logService.WriteInfo("SummerReadingTeens - " + libraryData.SummerReadingAdult);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F18"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F18"].ToString() != "N/A" && dtFinalColumns.Rows[i]["F18"].ToString() != "-1")
                                    libraryData.BookMobiles = Convert.ToInt32(dtFinalColumns.Rows[i]["F18"].ToString().Replace(",", ""));
                                else
                                    libraryData.BookMobiles = 0;
                            }
                            logService.WriteInfo("BookMobiles - " + libraryData.BookMobiles);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F181"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F181"].ToString() != "N/A")
                                    libraryData.HomeWorkSessions = Convert.ToInt32(dtFinalColumns.Rows[i]["F181"].ToString().Replace(",", ""));
                                else
                                    libraryData.HomeWorkSessions = 0;
                            }
                            logService.WriteInfo("HomeWorkSessions - " + libraryData.HomeWorkSessions);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F182"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F182"].ToString() != "N/A")
                                    libraryData.MealsSnacksSFSP = Convert.ToInt32(dtFinalColumns.Rows[i]["F182"].ToString().Replace(",", ""));
                                else
                                    libraryData.MealsSnacksSFSP = 0;
                            }
                            logService.WriteInfo("MealsSnacksSFSP - " + libraryData.MealsSnacksSFSP);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F51"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F51"].ToString() != "N/A")
                                    libraryData.AnnualAttendanceLibrary = Convert.ToInt32(dtFinalColumns.Rows[i]["F51"].ToString().Replace(",", ""));
                                else
                                    libraryData.AnnualAttendanceLibrary = 0;
                            }
                            logService.WriteInfo("AnnualAttendanceLibrary - " + libraryData.AnnualAttendanceLibrary);

                            libraryDataService.AddLibraryData(libraryData);
                            logService.WriteInfo("Row No. " + (i + 1) + " data inserted successfully.");
                            count++;
                        }
                        catch (Exception ex)
                        {
                            logService.WriteLog("Data was not inserted in Row No. " + (i + 1) + " due to " + ex.Message);
                            //throw ex;
                        }
                    }
                    else
                    {
                        logService.WriteInfo("No Library Name found.");
                    }
                }
                #endregion
                logService.WriteInfo("Excel parsing end.");
                result = count.ToString() + " " + ErrorMessageContainer.DataInsertSuccess;
                logService.WriteInfo("Total " + count.ToString() + " row(s) inserted.");
                #region Library Data Upload History
                logService.WriteInfo("Data upload history insert.");
                DataUploadHistoryModel datauploadHistoryModel = new DataUploadHistoryModel();
                datauploadHistoryModel.DataYear = model.DataYear;
                datauploadHistoryModel.Status = EnumStatus.Active.ToString();
                datauploadHistoryModel.UploadDate = DateTime.Now;
                datauploadHistoryModel.UploadedFileName = file.FileName;
                datauploadHistoryModel.Name = model.Name;
                datauploadHistoryModel.CreatedBy = currentUser.UserId;
                datauploadHistoryModel.CreatedAt = DateTime.Now;
                dataUploadHistoryService.AddLibraryDataUploadHistory(datauploadHistoryModel);
                int libraryHistoryId = datauploadHistoryModel.DataUploadHistoryId;
                logService.WriteInfo("Data upload history inserted successfully.");
                string directoryPath = Server.MapPath(libraryDataHistoryPath + datauploadHistoryModel.DataUploadHistoryId + "//");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                var path = Path.Combine(directoryPath, file.FileName);
                file.SaveAs(path);
                logService.WriteInfo("Data upload history file saved successfully.");

                #endregion
            }
            catch (Exception ex)
            {
                result = ErrorMessageContainer.DataInsertFailure;
                throw ex;
            }

            return result;
        }
        /// <summary>
        /// Insert data from excel XSLX to DB
        /// </summary>
        /// <param name="dtFinalColumns"></param>
        /// <param name="model"></param>
        /// <param name="libraryHistoryId"></param>
        /// <param name="currentUser"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public string InsertDataFromExcelXSLXToDB(DataTable dtFinalColumns, DataModel model, UserModel currentUser, HttpPostedFileBase file)
        {
            string result = "";
            int count = 0;
            try
            {
                logService.WriteInfo("Excel parsing started.");
                #region Library Data Insert
                DataRow[] foundRows;
                string filter = "[" + model.DataYear + " Public Library Statistics] = 'Location' AND [1#11]='County'";
                foundRows = dtFinalColumns.Select(filter);
                int index = 0;
                foreach (DataRow dr in foundRows)
                {
                    index = dr.Table.Rows.IndexOf(dr);
                }
                for (int i = (index + 1); i < dtFinalColumns.Rows.Count; i++)
                {
                    LibraryDataModel libraryData = new LibraryDataModel();
                    libraryData.Status = model.Status;
                    libraryData.DataYear = model.DataYear;
                    libraryData.CreatedBy = currentUser.UserId;
                    libraryData.CreatedAt = DateTime.Now;
                    if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["2015 Public Library Statistics"].ToString()))
                    {
                        try
                        {
                            libraryData.LibraryID = GenerateLibraryID(i);
                            logService.WriteInfo("Row No. " + (i + 1).ToString() + " Inputs:");
                            logService.WriteInfo("Data Year - " + libraryData.DataYear);
                            logService.WriteInfo("Library ID - " + libraryData.LibraryID);

                            libraryData.LibraryName = dtFinalColumns.Rows[i]["2015 Public Library Statistics"].ToString();
                            logService.WriteInfo("Library Name - " + libraryData.LibraryName);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["1#11"].ToString()))
                                libraryData.County = dtFinalColumns.Rows[i]["1#11"].ToString();
                            logService.WriteInfo("County - " + libraryData.County);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["2#48"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["2#48"].ToString() != "N/A")
                                    libraryData.InternetPC = Convert.ToInt32(dtFinalColumns.Rows[i]["2#48"].ToString().Replace(",", ""));
                                else
                                    libraryData.InternetPC = 0;
                            }
                            logService.WriteInfo("InternetPC - " + libraryData.InternetPC);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["2#49"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["2#49"].ToString() != "N/A")
                                    libraryData.AnnualUses = Convert.ToInt32(dtFinalColumns.Rows[i]["2#49"].ToString().Replace(",", ""));
                                else
                                    libraryData.AnnualUses = 0;
                            }
                            logService.WriteInfo("AnnualUses - " + libraryData.AnnualUses);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["2#50"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["2#50"].ToString() != "N/A")
                                    libraryData.AnnualWirelessSessions = Convert.ToInt32(dtFinalColumns.Rows[i]["2#50"].ToString().Replace(",", ""));
                                else
                                    libraryData.AnnualWirelessSessions = 0;
                            }
                            logService.WriteInfo("AnnualWirelessSessions - " + libraryData.AnnualWirelessSessions);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["F24"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["F24"].ToString() != "N/A")
                                    libraryData.CircPhysicalMaterial = Convert.ToInt32(dtFinalColumns.Rows[i]["F24"].ToString().Replace(",", ""));
                                else
                                    libraryData.CircPhysicalMaterial = 0;
                            }
                            logService.WriteInfo("CircPhysicalMaterial - " + libraryData.CircPhysicalMaterial);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["2#51"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["2#51"].ToString() != "N/A")
                                    libraryData.CircDownloadableMaterial = Convert.ToInt32(dtFinalColumns.Rows[i]["2#51"].ToString().Replace(",", ""));
                                else
                                    libraryData.CircDownloadableMaterial = 0;
                            }
                            logService.WriteInfo("CircDownloadableMaterial - " + libraryData.CircDownloadableMaterial);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["2#52"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["2#52"].ToString() != "N/A")
                                    libraryData.CircTotal = Convert.ToInt32(dtFinalColumns.Rows[i]["2#52"].ToString().Replace(",", ""));
                                else
                                    libraryData.CircTotal = 0;
                            }
                            logService.WriteInfo("CircTotal - " + libraryData.CircTotal);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["2#53"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["2#53"].ToString() != "N/A")
                                    libraryData.CircAdult = Convert.ToInt32(dtFinalColumns.Rows[i]["2#53"].ToString().Replace(",", ""));
                                else
                                    libraryData.CircAdult = 0;
                            }
                            logService.WriteInfo("CircAdult - " + libraryData.CircAdult);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["2#54"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["2#54"].ToString() != "N/A")
                                    libraryData.CircJuvenile = Convert.ToInt32(dtFinalColumns.Rows[i]["2#54"].ToString().Replace(",", ""));
                                else
                                    libraryData.CircJuvenile = 0;
                            }
                            logService.WriteInfo("CircJuvenile - " + libraryData.CircJuvenile);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["7#11"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["7#11"].ToString() != "N/A")
                                    libraryData.Databases = Convert.ToInt32(dtFinalColumns.Rows[i]["7#11"].ToString().Replace(",", ""));
                                else
                                    libraryData.Databases = 0;
                            }
                            logService.WriteInfo("Databases - " + libraryData.Databases);
                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["7#1"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["7#1"].ToString() != "N/A")
                                    libraryData.PrintMaterial = Convert.ToInt32(dtFinalColumns.Rows[i]["7#1"].ToString().Replace(",", ""));
                                else
                                    libraryData.PrintMaterial = 0;
                            }
                            logService.WriteInfo("PrintMaterial - " + libraryData.PrintMaterial);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["7#3"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["7#3"].ToString() != "N/A")
                                    libraryData.PhysicalVideo = Convert.ToInt32(dtFinalColumns.Rows[i]["7#3"].ToString().Replace(",", ""));
                                else
                                    libraryData.PhysicalVideo = 0;
                            }
                            logService.WriteInfo("PhysicalVideo - " + libraryData.PhysicalVideo);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["7#5"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["7#5"].ToString() != "N/A")
                                    libraryData.PhysicalAudio = Convert.ToInt32(dtFinalColumns.Rows[i]["7#5"].ToString().Replace(",", ""));
                                else
                                    libraryData.PhysicalAudio = 0;
                            }
                            logService.WriteInfo("PhysicalAudio - " + libraryData.PhysicalAudio);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["7#7"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["7#7"].ToString() != "N/A")
                                    libraryData.Ebooks = Convert.ToInt32(dtFinalColumns.Rows[i]["7#7"].ToString().Replace(",", ""));
                                else
                                    libraryData.Ebooks = 0;
                            }
                            logService.WriteInfo("Ebooks - " + libraryData.Ebooks);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["1#24"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["1#24"].ToString() != "N/A")
                                    libraryData.PopulationLegalSvcArea = Convert.ToInt32(dtFinalColumns.Rows[i]["1#24"].ToString().Replace(",", ""));
                                else
                                    libraryData.PopulationLegalSvcArea = 0;
                            }
                            logService.WriteInfo("PopulationLegalSvcArea - " + libraryData.PopulationLegalSvcArea);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["1#25"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["1#25"].ToString() != "N/A")
                                    libraryData.RegAdults = Convert.ToInt32(dtFinalColumns.Rows[i]["1#25"].ToString().Replace(",", ""));
                                else
                                    libraryData.RegAdults = 0;
                            }

                            logService.WriteInfo("RegAdults - " + libraryData.RegAdults);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["1#26"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["1#26"].ToString() != "N/A")
                                    libraryData.RegChildren = Convert.ToInt32(dtFinalColumns.Rows[i]["1#26"].ToString().Replace(",", ""));
                                else
                                    libraryData.RegChildren = 0;
                            }
                            logService.WriteInfo("RegChildren - " + libraryData.RegChildren);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["2#43"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["2#43"].ToString() != "N/A")
                                    libraryData.CentralLibraries = Convert.ToInt32(dtFinalColumns.Rows[i]["2#43"].ToString().Replace(",", ""));
                                else
                                    libraryData.CentralLibraries = 0;
                            }
                            logService.WriteInfo("CentralLibraries - " + libraryData.CentralLibraries);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["2#44"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["2#44"].ToString() != "N/A")
                                    libraryData.Branches = Convert.ToInt32(dtFinalColumns.Rows[i]["2#44"].ToString().Replace(",", ""));
                                else
                                    libraryData.Branches = 0;
                            }
                            logService.WriteInfo("Branches - " + libraryData.Branches);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["2#47"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["2#47"].ToString() != "N/A")
                                    libraryData.AnnualHrsOpen = Convert.ToDecimal(dtFinalColumns.Rows[i]["2#47"].ToString().Replace(",", ""));
                                else
                                    libraryData.AnnualHrsOpen = 0;
                            }
                            logService.WriteInfo("CentralLibraries - " + libraryData.CentralLibraries);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["5#18"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["5#18"].ToString() != "N/A")
                                    libraryData.LibraryStaffTotal = Convert.ToDecimal(dtFinalColumns.Rows[i]["5#18"].ToString().Replace(",", ""));
                                else
                                    libraryData.LibraryStaffTotal = 0;
                            }
                            logService.WriteInfo("LibraryStaffTotal - " + libraryData.LibraryStaffTotal);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["7#1"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["7#1"].ToString() != "N/A")
                                    libraryData.PrintMaterials = Convert.ToInt32(dtFinalColumns.Rows[i]["7#1"].ToString().Replace(",", ""));
                                else
                                    libraryData.PrintMaterials = 0;
                            }
                            logService.WriteInfo("PrintMaterials - " + libraryData.PrintMaterials);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["7#2"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["7#2"].ToString() != "N/A")
                                    libraryData.PrintSubscription = Convert.ToInt32(dtFinalColumns.Rows[i]["7#2"].ToString().Replace(",", ""));
                                else
                                    libraryData.PrintSubscription = 0;
                            }
                            logService.WriteInfo("PrintSubscription - " + libraryData.PrintSubscription);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["7#4"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["7#4"].ToString() != "N/A")
                                    libraryData.DownloadableVideo = Convert.ToInt32(dtFinalColumns.Rows[i]["7#4"].ToString().Replace(",", ""));
                                else
                                    libraryData.DownloadableVideo = 0;
                            }
                            logService.WriteInfo("DownloadableVideo - " + libraryData.DownloadableVideo);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["7#6"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["7#6"].ToString() != "N/A")
                                    libraryData.DownloadableAudio = Convert.ToInt32(dtFinalColumns.Rows[i]["7#6"].ToString().Replace(",", ""));
                                else
                                    libraryData.DownloadableAudio = 0;
                            }
                            logService.WriteInfo("DownloadableAudio - " + libraryData.DownloadableAudio);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["7#8"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["7#8"].ToString() != "N/A")
                                    libraryData.ComputerSoftware = Convert.ToInt32(dtFinalColumns.Rows[i]["7#8"].ToString().Replace(",", ""));
                                else
                                    libraryData.ComputerSoftware = 0;
                            }
                            logService.WriteInfo("ComputerSoftware - " + libraryData.ComputerSoftware);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["8#1"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["8#1"].ToString() != "N/A")
                                    libraryData.ILLProvided = Convert.ToInt32(dtFinalColumns.Rows[i]["8#1"].ToString().Replace(",", ""));
                                else
                                    libraryData.ILLProvided = 0;
                            }
                            logService.WriteInfo("ILLProvided - " + libraryData.ILLProvided);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["8#2"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["8#2"].ToString() != "N/A")
                                    libraryData.ILLReceived = Convert.ToInt32(dtFinalColumns.Rows[i]["8#2"].ToString().Replace(",", ""));
                                else
                                    libraryData.ILLReceived = 0;
                            }
                            logService.WriteInfo("ILLReceived - " + libraryData.ILLReceived);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["8#3"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["8#3"].ToString() != "N/A")
                                    libraryData.AttendanceTypicalWeek = Convert.ToInt32(dtFinalColumns.Rows[i]["8#3"].ToString().Replace(",", ""));
                                else
                                    libraryData.AttendanceTypicalWeek = 0;
                            }
                            logService.WriteInfo("AttendanceTypicalWeek - " + libraryData.AttendanceTypicalWeek);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["8#6"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["8#6"].ToString() != "N/A")
                                    libraryData.AnnualReferenceTransaction = Convert.ToInt32(dtFinalColumns.Rows[i]["8#6"].ToString().Replace(",", ""));
                                else
                                    libraryData.AnnualReferenceTransaction = 0;
                            }
                            logService.WriteInfo("AnnualReferenceTransaction - " + libraryData.AnnualReferenceTransaction);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["8#7"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["8#7"].ToString() != "N/A")
                                    libraryData.ChildrenPrograms = Convert.ToInt32(dtFinalColumns.Rows[i]["8#7"].ToString().Replace(",", ""));
                                else
                                    libraryData.ChildrenPrograms = 0;
                            }
                            logService.WriteInfo("ChildrenPrograms - " + libraryData.ChildrenPrograms);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["8#8"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["8#8"].ToString() != "N/A")
                                    libraryData.YoungAdultPrograms = Convert.ToInt32(dtFinalColumns.Rows[i]["8#8"].ToString().Replace(",", ""));
                                else
                                    libraryData.YoungAdultPrograms = 0;
                            }
                            logService.WriteInfo("YoungAdultPrograms - " + libraryData.YoungAdultPrograms);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["8#10"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["8#10"].ToString() != "N/A")
                                    libraryData.TotalLibraryPrograms = Convert.ToInt32(dtFinalColumns.Rows[i]["8#10"].ToString().Replace(",", ""));
                                else
                                    libraryData.TotalLibraryPrograms = 0;
                            }
                            logService.WriteInfo("TotalLibraryPrograms - " + libraryData.TotalLibraryPrograms);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["8#11"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["8#11"].ToString() != "N/A")
                                    libraryData.ChildrenProgramAttendance = Convert.ToInt32(dtFinalColumns.Rows[i]["8#11"].ToString().Replace(",", ""));
                                else
                                    libraryData.ChildrenProgramAttendance = 0;
                            }
                            logService.WriteInfo("ChildrenProgramAttendance - " + libraryData.ChildrenProgramAttendance);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["8#12"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["8#12"].ToString() != "N/A")
                                    libraryData.YoungAdultProgramAttendance = Convert.ToInt32(dtFinalColumns.Rows[i]["8#12"].ToString().Replace(",", ""));
                                else
                                    libraryData.YoungAdultProgramAttendance = 0;
                            }
                            logService.WriteInfo("YoungAdultProgramAttendance - " + libraryData.YoungAdultProgramAttendance);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["8#13"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["8#13"].ToString() != "N/A")
                                    libraryData.TotalProgramAttendance = Convert.ToInt32(dtFinalColumns.Rows[i]["8#13"].ToString().Replace(",", ""));
                                else
                                    libraryData.TotalProgramAttendance = 0;
                            }
                            logService.WriteInfo("TotalProgramAttendance - " + libraryData.TotalProgramAttendance);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["8#14"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["8#14"].ToString() != "N/A")
                                    libraryData.TotalLibraryProgramAttendance = Convert.ToInt32(dtFinalColumns.Rows[i]["8#14"].ToString().Replace(",", ""));
                                else
                                    libraryData.TotalLibraryProgramAttendance = 0;
                            }
                            logService.WriteInfo("TotalLibraryProgramAttendance - " + libraryData.TotalLibraryProgramAttendance);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["13#1"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["13#1"].ToString() != "N/A")
                                    libraryData.SummerReadingChildren = Convert.ToInt32(dtFinalColumns.Rows[i]["13#1"].ToString().Replace(",", ""));
                                else
                                    libraryData.SummerReadingChildren = 0;
                            }
                            logService.WriteInfo("SummerReadingChildren - " + libraryData.SummerReadingChildren);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["13#2"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["13#2"].ToString() != "N/A")
                                    libraryData.SummerReadingTeens = Convert.ToInt32(dtFinalColumns.Rows[i]["13#2"].ToString().Replace(",", ""));
                                else
                                    libraryData.SummerReadingTeens = 0;
                            }
                            logService.WriteInfo("SummerReadingTeens - " + libraryData.SummerReadingTeens);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["13#3"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["13#3"].ToString() != "N/A")
                                    libraryData.SummerReadingAdult = Convert.ToInt32(dtFinalColumns.Rows[i]["13#3"].ToString().Replace(",", ""));
                                else
                                    libraryData.SummerReadingAdult = 0;
                            }
                            logService.WriteInfo("SummerReadingTeens - " + libraryData.SummerReadingAdult);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["2#45"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["2#45"].ToString() != "N/A" && dtFinalColumns.Rows[i]["2#45"].ToString() != "-1")
                                    libraryData.BookMobiles = Convert.ToInt32(dtFinalColumns.Rows[i]["2#45"].ToString().Replace(",", ""));
                                else
                                    libraryData.BookMobiles = 0;
                            }
                            logService.WriteInfo("BookMobiles - " + libraryData.BookMobiles);
                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["13#5"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["13#5"].ToString() != "N/A")
                                    libraryData.HomeWorkSessions = Convert.ToInt32(dtFinalColumns.Rows[i]["13#5"].ToString().Replace(",", ""));
                                else
                                    libraryData.HomeWorkSessions = 0;
                            }
                            logService.WriteInfo("HomeWorkSessions - " + libraryData.HomeWorkSessions);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["13#6"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["13#6"].ToString() != "N/A")
                                    libraryData.MealsSnacksSFSP = Convert.ToInt32(dtFinalColumns.Rows[i]["13#6"].ToString().Replace(",", ""));
                                else
                                    libraryData.MealsSnacksSFSP = 0;
                            }
                            logService.WriteInfo("MealsSnacksSFSP - " + libraryData.MealsSnacksSFSP);

                            if (!String.IsNullOrEmpty(dtFinalColumns.Rows[i]["8#4"].ToString()))
                            {
                                if (dtFinalColumns.Rows[i]["8#4"].ToString() != "N/A")
                                    libraryData.AnnualAttendanceLibrary = Convert.ToInt32(dtFinalColumns.Rows[i]["8#4"].ToString().Replace(",", ""));
                                else
                                    libraryData.AnnualAttendanceLibrary = 0;
                            }
                            logService.WriteInfo("AnnualAttendanceLibrary - " + libraryData.AnnualAttendanceLibrary);


                            libraryDataService.AddLibraryData(libraryData);
                            logService.WriteInfo("Row No. " + (i + 1) + " data inserted successfully.");
                            count++;
                        }
                        catch (Exception ex)
                        {
                            logService.WriteLog("Data was not inserted in Row No. " + (i + 1) + " due to " + ex.Message);
                        }
                    }
                    else
                    {
                        logService.WriteInfo("No Library Name found.");
                    }
                }
                #endregion
                logService.WriteInfo("Excel parsing end.");
                result = count.ToString() + " " + ErrorMessageContainer.DataInsertSuccess;
                logService.WriteInfo("Total " + count.ToString() + " row(s) inserted.");

                #region Library Data Upload History
                logService.WriteInfo("Data upload history insert.");
                DataUploadHistoryModel datauploadHistoryModel = new DataUploadHistoryModel();
                datauploadHistoryModel.DataYear = model.DataYear;
                datauploadHistoryModel.Status = EnumStatus.Active.ToString();
                datauploadHistoryModel.UploadDate = DateTime.Now;
                datauploadHistoryModel.UploadedFileName = file.FileName;
                datauploadHistoryModel.Name = model.Name;
                datauploadHistoryModel.CreatedBy = currentUser.UserId;
                datauploadHistoryModel.CreatedAt = DateTime.Now;
                dataUploadHistoryService.AddLibraryDataUploadHistory(datauploadHistoryModel);
                int libraryHistoryId = datauploadHistoryModel.DataUploadHistoryId;
                logService.WriteInfo("Data upload history inserted successfully.");
                string directoryPath = Server.MapPath(libraryDataHistoryPath + datauploadHistoryModel.DataUploadHistoryId + "//");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                var path = Path.Combine(directoryPath, file.FileName);
                file.SaveAs(path);
                logService.WriteInfo("Data upload history file saved successfully.");

                #endregion

            }
            catch (Exception ex)
            {
                result = ErrorMessageContainer.DataInsertFailure;
                throw ex;
            }

            return result;
        }
        /// <summary>
        /// Method to upload data
        /// </summary>
        /// <param name="file"></param>
        /// <param name="model"></param>
        /// <param name="currentUser"></param>
        /// <param name="libraryHistoryId"></param>
        /// <returns></returns>
        public string UploadData(HttpPostedFileBase file, DataModel model, UserModel currentUser, string filePath)
        {
            string result = "";
            string extension = System.IO.Path.GetExtension(filePath);
            string connString = "";
            DataTable dt = new DataTable();
            try
            {
                using (var stream = System.IO.File.OpenRead(filePath))
                using (var reader = new StreamReader(stream))
                {
                    if (extension.ToLower().Trim() == ".xls")
                    {
                        connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filePath + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=1\"";
                        dt = excelParser.ConvertXSLXtoDataTable(filePath, connString);
                        System.Data.DataView view = new System.Data.DataView(dt);
                        string[] columns = GetSelectedColumns();
                        DataTable dtFinalColumns = view.ToTable(false, columns);
                        ViewBag.Data = dt;
                        model.Status = "Active";
                        result = InsertDataFromExcelToDB(dtFinalColumns, model, currentUser, file);
                    }
                    else if (extension.ToLower().Trim() == ".xlsx")
                    {
                        connString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filePath + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                        dt = excelParser.ConvertXSLXtoDataTable(filePath, connString);
                        System.Data.DataView view = new System.Data.DataView(dt);
                        string[] columns = GetSelectedColumnsXSLX();
                        DataTable dtFinalColumns = view.ToTable(false, columns);
                        ViewBag.Data = dt;
                        model.Status = "Active";
                        result = InsertDataFromExcelXSLXToDB(dtFinalColumns, model, currentUser, file);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
        /// <summary>
        /// Method to check file lock
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static Boolean IsFileLocked(FileInfo file)
        {
            FileStream stream = null;
            try
            {
                stream = file.Open
                (
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.None
                );
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return false;
        }
        /// <summary>
        /// Method to clean file
        /// </summary>
        /// <param name="folderPath"></param>
        public static void CleanFiles(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                var directory = new DirectoryInfo(folderPath);
                foreach (FileInfo file in directory.GetFiles())
                {
                    if (!IsFileLocked(file))
                        file.Delete();
                }
            }
        }
        #endregion
    }
}