//Reference:- Install "iTextSharp" Package from NuGet Package Manager
//using iTextSharp.text;
//using iTextSharp.text.pdf;
//using iTextSharp.text.pdf.parser;
using StateOfOhioLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.WebControls;

namespace StateOfOhioLibrary.Services
{
    public class PDFService
    {
        #region Global Declaration
        TemplateService templateService = new TemplateService();
        LibraryDataService libraryDataService = new LibraryDataService();
        CustomColumnService customColumnService = new CustomColumnService();
        CustomTemplateService customTemplateService = new CustomTemplateService();

        string enableSSL = System.Configuration.ConfigurationManager.AppSettings["EnableSSl"].ToString();
        string templatePath = WebConfigurationManager.AppSettings["TemplatePath"];
        string pdfImagePath = WebConfigurationManager.AppSettings["PDFImagePath"];
        string dataFilesLocation = "";
        string newDataFielsLocation = "";
        #endregion

        #region Public Methods
        /// <summary>
        /// Get pdf data file location
        /// </summary>
        /// <returns></returns>
        public string GetDataFileLocation()
        {
            dataFilesLocation = HttpContext.Current.Server.MapPath(templatePath);
            return dataFilesLocation;
        }
        /// <summary>
        /// Get new pdf data file location with templateId
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        public string NewDataFileLocation(int templateId)
        {
            newDataFielsLocation = HttpContext.Current.Server.MapPath(templatePath + "\\" + "New" + "\\" + templateId);
            return newDataFielsLocation;
        }

        public void writeText(PdfContentByte cb, string text, int x, int y, BaseFont font, int size)
        {
            cb.SetFontAndSize(font, size);
            cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, text, x, y, 0);
        }

        /// <summary>
        /// Edit pdf for adult services
        /// </summary>
        /// <param name="libraryName"></param>
        /// <param name="templateName"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public string EditPdfAdultServices(string libraryName, string templateName, int year)
        {
            GetDataFileLocation();
            string oldFile = @"C:\Temp\Template01 (Adult Services).pdf";
            string newFile = @"C:\Temp\Template01 (Adult Services) -new.pdf";
            string fileName = "";
            if (year <= 0)
            {
                year = Convert.ToInt32(DateTime.Now.Year);
            }

            var template = templateService.GetAllTemplate().Where(x => x.Name == templateName).FirstOrDefault();
            if (template != null)
            {
                NewDataFileLocation(template.TemplateId);
                if (!Directory.Exists(newDataFielsLocation))
                {
                    Directory.CreateDirectory(newDataFielsLocation);
                }
                oldFile = @"" + dataFilesLocation + "\\" + template.FileLocation;
                Uri filePath = new Uri(templatePath + "\\" + template.FileLocation);
                fileName = System.IO.Path.GetFileName(filePath.LocalPath);
                newFile = @"" + newDataFielsLocation + "\\" + fileName;
            }

            if (!string.IsNullOrEmpty(oldFile) && !string.IsNullOrEmpty(newFile))
            {
                if (File.Exists(newFile))
                {
                    File.Delete(newFile);
                }
                System.IO.File.Copy(oldFile, newFile);
            }

            PdfReader reader = new PdfReader(oldFile);
            FileStream fs = new FileStream(newFile, FileMode.Create, FileAccess.Write);
            PdfStamper stamper = new PdfStamper(reader, fs);
            PdfContentByte canvas = stamper.GetOverContent(1);
            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            var FontColor = new BaseColor(64, 168, 94);
            var HeaderColor = new BaseColor(48, 47, 65);
            var TextColor = new BaseColor(40, 51, 78);
            BaseFont yearBf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            var YearTextColor = new BaseColor(35, 31, 32);

            var libraryData = libraryDataService.GetLibraryDataByName(libraryName, year);
            if (libraryData != null)
            {

                #region Library Name

                PdfPTable tableName = new PdfPTable(3);
                PdfPCell cellName = new PdfPCell();
                if (libraryData.LibraryName.Length > 0 && libraryData.LibraryName.Length <= 35)
                {
                    cellName = new PdfPCell(new Phrase(libraryData.LibraryName, new Font(bf, 30, 0, HeaderColor)));
                }
                else if (libraryData.LibraryName.Length > 35 && libraryData.LibraryName.Length <= 60)
                {
                    cellName = new PdfPCell(new Phrase(libraryData.LibraryName, new Font(bf, 28, 0, HeaderColor)));
                }
                else
                {
                    cellName = new PdfPCell(new Phrase(libraryData.LibraryName, new Font(bf, 30, 0, HeaderColor)));
                }

                tableName.TotalWidth = 710;
                cellName.Border = Rectangle.NO_BORDER;
                cellName.Colspan = 3;
                cellName.HorizontalAlignment = Element.ALIGN_CENTER;
                tableName.AddCell(cellName);
                tableName.WriteSelectedRows(0, -1, 30, 990, canvas);
                #endregion

                #region Library Cards

                string regAdults = string.Format("{0:n0}", libraryData.RegAdults);
                if (regAdults == "0" || regAdults == "-1" || string.IsNullOrEmpty(regAdults))
                    regAdults = "N/A";
                PdfPTable tableLibrary = new PdfPTable(3);
                PdfPCell cellLibrary = new PdfPCell();
                cellLibrary = new PdfPCell(new Phrase(regAdults, new Font(bf, 20, 0, BaseColor.WHITE)));
                tableLibrary.TotalWidth = 100;
                cellLibrary.Border = Rectangle.NO_BORDER;
                cellLibrary.Colspan = 3;
                cellLibrary.HorizontalAlignment = Element.ALIGN_CENTER;
                tableLibrary.AddCell(cellLibrary);
                tableLibrary.WriteSelectedRows(0, -1, 160, 835, canvas);
                #endregion

                #region Adults Attended Programs

                string totalProgramAttendance = string.Format("{0:n0}", libraryData.TotalProgramAttendance);
                if (totalProgramAttendance == "0" || totalProgramAttendance == "-1" || string.IsNullOrEmpty(totalProgramAttendance))
                    totalProgramAttendance = "N/A";
                PdfPTable tableAttendance = new PdfPTable(3);
                PdfPCell cellAttendance = new PdfPCell();
                cellAttendance = new PdfPCell(new Phrase(totalProgramAttendance, new Font(bf, 22, 0, FontColor)));
                tableAttendance.TotalWidth = 100;
                cellAttendance.Border = Rectangle.NO_BORDER;
                cellAttendance.Colspan = 3;
                cellAttendance.HorizontalAlignment = Element.ALIGN_RIGHT;
                tableAttendance.AddCell(cellAttendance);
                tableAttendance.WriteSelectedRows(0, -1, 450, 862, canvas);
                #endregion

                ColumnText.ShowTextAligned(
                     canvas,
                     Element.ALIGN_LEFT,
                     new Phrase(year != 0 ? year.ToString() : "N/A", new Font(bf, 20, 0, TextColor)),
                     595, 700, 0
                     );

                #region Items Checked Out

                string circAdult = string.Format("{0:n0}", libraryData.CircAdult);
                if (circAdult == "0" || circAdult == "-1" || string.IsNullOrEmpty(circAdult))
                    circAdult = "N/A";
                PdfPTable tableChecked = new PdfPTable(3);
                PdfPCell cellChecked = new PdfPCell();
                cellChecked = new PdfPCell(new Phrase(circAdult, new Font(bf, 20, 0, TextColor)));
                tableChecked.TotalWidth = 100;
                cellChecked.Border = Rectangle.NO_BORDER;
                cellChecked.Colspan = 3;
                cellChecked.HorizontalAlignment = Element.ALIGN_RIGHT;
                tableChecked.AddCell(cellChecked);
                tableChecked.WriteSelectedRows(0, -1, 72, 523, canvas);
                #endregion

                #region Reference Questions Answered

                var referenceValue = string.Format("{0:n0}", libraryData.AnnualReferenceTransaction);
                if (referenceValue == "0" || referenceValue == "-1" || string.IsNullOrEmpty(referenceValue))
                    referenceValue = "N/A";
                PdfPTable tableReference = new PdfPTable(3);
                PdfPCell cellReference = new PdfPCell();
                cellReference = new PdfPCell(new Phrase(referenceValue, new Font(bf, 20, 0, TextColor)));
                tableReference.TotalWidth = 100;
                cellReference.Border = Rectangle.NO_BORDER;
                cellReference.Colspan = 3;
                cellReference.HorizontalAlignment = Element.ALIGN_RIGHT;
                tableReference.AddCell(cellReference);
                tableReference.WriteSelectedRows(0, -1, 428, 526, canvas);
                #endregion

                #region Library Loan Items Exchanged
                string loanItemsExchanged = string.Format("{0:n0}", (libraryData.ILLProvided + libraryData.ILLReceived));
                if (loanItemsExchanged == "0" || loanItemsExchanged == "-1" || string.IsNullOrEmpty(loanItemsExchanged))
                    loanItemsExchanged = "N/A";
                PdfPTable tableLoan = new PdfPTable(3);
                PdfPCell cellLoan = new PdfPCell();
                cellLoan = new PdfPCell(new Phrase(loanItemsExchanged, new Font(bf, 20, 0, BaseColor.WHITE)));
                tableLoan.TotalWidth = 120;
                cellLoan.Border = Rectangle.NO_BORDER;
                cellLoan.Colspan = 3;
                cellLoan.HorizontalAlignment = Element.ALIGN_CENTER;
                tableLoan.AddCell(cellLoan);
                tableLoan.WriteSelectedRows(0, -1, 100, 355, canvas);
                #endregion

                #region Computers    
                string computers = libraryData.InternetPC != 0 ? libraryData.InternetPC.ToString() : "N/A";
                if (computers == "-1")
                    computers = "N/A";
                PdfPTable tableComputer = new PdfPTable(3);
                PdfPCell cellComputer = new PdfPCell();
                cellComputer = new PdfPCell(new Phrase(computers, new Font(bf, 40, 0, FontColor)));
                tableComputer.TotalWidth = 128;
                cellComputer.Border = Rectangle.NO_BORDER;
                cellComputer.Colspan = 3;
                cellComputer.HorizontalAlignment = Element.ALIGN_CENTER;
                tableComputer.AddCell(cellComputer);
                tableComputer.WriteSelectedRows(0, -1, 500, 385, canvas);
                #endregion

                #region Each Used Over Times Per Year

                string userOverTimesPerYear = string.Format("{0:n0}", (libraryData.AnnualUses / libraryData.InternetPC));
                if (userOverTimesPerYear == "0" || userOverTimesPerYear == "-1" || string.IsNullOrEmpty(userOverTimesPerYear))
                    userOverTimesPerYear = "N/A";
                PdfPTable tableTimes = new PdfPTable(3);
                PdfPCell cellTimes = new PdfPCell();
                cellTimes = new PdfPCell(new Phrase(userOverTimesPerYear, new Font(bf, 20, 0, TextColor)));
                tableTimes.TotalWidth = 90;
                cellTimes.Border = Rectangle.NO_BORDER;
                cellTimes.Colspan = 3;
                cellTimes.HorizontalAlignment = Element.ALIGN_RIGHT;
                tableTimes.AddCell(cellTimes);
                tableTimes.WriteSelectedRows(0, -1, 435, 214, canvas);
                #endregion

                string strYear = year.ToString();
                ColumnText.ShowTextAligned(
                 canvas,
                 Element.ALIGN_RIGHT,
                 new Phrase(strYear, new Font(yearBf, 14, 0, YearTextColor)),
                 390, 116, 0
             );
                stamper.Close();
            }

            byte[] pdfByteArray = System.IO.File.ReadAllBytes(newFile);
            string base64EncodedPDF = System.Convert.ToBase64String(pdfByteArray);
            return base64EncodedPDF;
        }
        /// <summary>
        /// Edit pdf for youth services
        /// </summary>
        /// <param name="libraryName"></param>
        /// <param name="templateName"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public string EditPdfYouthServices(string libraryName, string templateName, int year)
        {
            GetDataFileLocation();
            string oldFile = @"C:\Temp\Template06 (Youth Services).pdf";
            string newFile = @"C:\Temp\Template06 (Youth Services) - New.pdf";
            string fileName = "";
            if (year <= 0)
            {
                year = Convert.ToInt32(DateTime.Now.Year);
            }

            var template = templateService.GetAllTemplate().Where(x => x.Name == templateName).FirstOrDefault();
            if (template != null)
            {
                NewDataFileLocation(template.TemplateId);
                if (!Directory.Exists(newDataFielsLocation))
                {
                    Directory.CreateDirectory(newDataFielsLocation);

                }
                oldFile = @"" + dataFilesLocation + "\\" + template.FileLocation;
                Uri filePath = new Uri(templatePath + "\\" + template.FileLocation);
                fileName = System.IO.Path.GetFileName(filePath.LocalPath);
                newFile = @"" + newDataFielsLocation + "\\" + fileName;


            }

            if (!string.IsNullOrEmpty(oldFile) && !string.IsNullOrEmpty(newFile))
            {
                if (File.Exists(newFile))
                {
                    File.Delete(newFile);
                }
                System.IO.File.Copy(oldFile, newFile);
            }


            PdfReader reader = new PdfReader(oldFile);
            FileStream fs = new FileStream(newFile, FileMode.Create, FileAccess.Write);
            PdfStamper stamper = new PdfStamper(reader, fs);
            PdfContentByte canvas = stamper.GetOverContent(1);
            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            var HeaderColor = new BaseColor(48, 47, 65);
            var TextColor = new BaseColor(40, 51, 78);
            BaseFont yearBf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            var YearTextColor = new BaseColor(35, 31, 32);

            var libraryData = libraryDataService.GetLibraryDataByName(libraryName, year);
            if (libraryData != null)
            {

                //Add pie chart 
                List<ChartData> chartData = ChartData(libraryData, "Youth Services", "Pie");
                iTextSharp.text.Image pieimg = iTextSharp.text.Image.GetInstance(CreatePieChart(chartData, 240, 240));
                pieimg.SetAbsolutePosition(50f, 500f);
                pieimg.Alignment = Element.ALIGN_LEFT;
                canvas.AddImage(pieimg);

                #region Library Name

                PdfPTable tableName = new PdfPTable(3);
                PdfPCell cellName = new PdfPCell();
                if (libraryData.LibraryName.Length > 0 && libraryData.LibraryName.Length <= 35)
                {
                    cellName = new PdfPCell(new Phrase(libraryData.LibraryName, new Font(bf, 30, 0, HeaderColor)));
                }
                else if (libraryData.LibraryName.Length > 35 && libraryData.LibraryName.Length <= 60)
                {
                    cellName = new PdfPCell(new Phrase(libraryData.LibraryName, new Font(bf, 28, 0, HeaderColor)));
                }
                else
                {
                    cellName = new PdfPCell(new Phrase(libraryData.LibraryName, new Font(bf, 30, 0, HeaderColor)));
                }

                tableName.TotalWidth = 710;
                cellName.Border = Rectangle.NO_BORDER;
                cellName.Colspan = 3;
                cellName.HorizontalAlignment = Element.ALIGN_CENTER;
                tableName.AddCell(cellName);
                tableName.WriteSelectedRows(0, -1, 30, 1005, canvas);

                #endregion

                #region Library Cards         

                string regChildren = string.Format("{0:n0}", libraryData.RegChildren);
                if (regChildren == "0" || regChildren == "-1" || string.IsNullOrEmpty(regChildren))
                    regChildren = "N/A";
                PdfPTable tableLibrary = new PdfPTable(3);
                PdfPCell cellLibrary = new PdfPCell();
                cellLibrary = new PdfPCell(new Phrase(regChildren, new Font(bf, 20, 0, BaseColor.WHITE)));
                tableLibrary.TotalWidth = 90;
                cellLibrary.Border = Rectangle.NO_BORDER;
                cellLibrary.Colspan = 3;
                cellLibrary.HorizontalAlignment = Element.ALIGN_CENTER;
                tableLibrary.AddCell(cellLibrary);
                tableLibrary.WriteSelectedRows(0, -1, 128, 844, canvas);
                #endregion

                #region Children Summer

                string summerChildrenSummer = string.Format("{0:n0}", libraryData.SummerReadingChildren);
                if (summerChildrenSummer == "0" || summerChildrenSummer == "-1" || string.IsNullOrEmpty(summerChildrenSummer))
                    summerChildrenSummer = "N/A";
                PdfPTable tableReadingChildren = new PdfPTable(3);
                PdfPCell cellReadingChildren = new PdfPCell();
                cellReadingChildren = new PdfPCell(new Phrase(summerChildrenSummer, new Font(bf, 20, 0, TextColor)));
                tableReadingChildren.TotalWidth = 90;
                cellReadingChildren.Border = Rectangle.NO_BORDER;
                cellReadingChildren.Colspan = 3;
                cellReadingChildren.HorizontalAlignment = Element.ALIGN_RIGHT;
                tableReadingChildren.AddCell(cellReadingChildren);
                tableReadingChildren.WriteSelectedRows(0, -1, 445, 858, canvas);
                #endregion

                #region Teens Summer

                string summerTeensSummer = string.Format("{0:n0}", libraryData.SummerReadingTeens);
                if (summerTeensSummer == "0" || summerTeensSummer == "-1" || string.IsNullOrEmpty(summerTeensSummer))
                    summerTeensSummer = "N/A";
                PdfPTable tableTeensSummer = new PdfPTable(3);
                PdfPCell cellTeensSummer = new PdfPCell();
                cellTeensSummer = new PdfPCell(new Phrase(summerTeensSummer, new Font(bf, 20, 0, TextColor)));
                tableTeensSummer.TotalWidth = 80;
                cellTeensSummer.Border = Rectangle.NO_BORDER;
                cellTeensSummer.Colspan = 3;
                cellTeensSummer.HorizontalAlignment = Element.ALIGN_LEFT;
                tableTeensSummer.AddCell(cellTeensSummer);
                tableTeensSummer.WriteSelectedRows(0, -1, 660, 858, canvas);
                #endregion

                #region Children Library Programming

                string childrenProgramAttendance = string.Format("{0:n0}", libraryData.ChildrenProgramAttendance);
                if (childrenProgramAttendance == "0" || childrenProgramAttendance == "-1" || string.IsNullOrEmpty(childrenProgramAttendance))
                    childrenProgramAttendance = "N/A";
                PdfPTable tableCPAttendance = new PdfPTable(3);
                PdfPCell cellCPAttendance = new PdfPCell();
                cellCPAttendance = new PdfPCell(new Phrase(childrenProgramAttendance, new Font(bf, 20, 0, TextColor)));
                tableCPAttendance.TotalWidth = 90;
                cellCPAttendance.Border = Rectangle.NO_BORDER;
                cellCPAttendance.Colspan = 3;
                cellCPAttendance.HorizontalAlignment = Element.ALIGN_RIGHT;
                tableCPAttendance.AddCell(cellCPAttendance);
                tableCPAttendance.WriteSelectedRows(0, -1, 310, 602, canvas);
                #endregion

                #region Teens Library Programming

                string teensProgramAttendance = string.Format("{0:n0}", libraryData.YoungAdultProgramAttendance);
                if (teensProgramAttendance == "0" || teensProgramAttendance == "-1" || string.IsNullOrEmpty(teensProgramAttendance))
                    teensProgramAttendance = "N/A";
                PdfPTable tableTPAttendance = new PdfPTable(3);
                PdfPCell cellTPAttendance = new PdfPCell();
                cellTPAttendance = new PdfPCell(new Phrase(teensProgramAttendance, new Font(bf, 20, 0, TextColor)));
                tableTPAttendance.TotalWidth = 90;
                cellTPAttendance.Border = Rectangle.NO_BORDER;
                cellTPAttendance.Colspan = 3;
                cellTPAttendance.HorizontalAlignment = Element.ALIGN_LEFT;
                tableTPAttendance.AddCell(cellTPAttendance);
                tableTPAttendance.WriteSelectedRows(0, -1, 530, 603, canvas);
                #endregion

                #region % Library's Card Holders

                int total = libraryData.RegAdults + libraryData.RegChildren;
                int children = libraryData.RegChildren;
                var value = ((double)children / total) * 100;
                var percentage = Convert.ToInt32(Math.Round(value, 0));
                PdfPTable tableCardHolders = new PdfPTable(3);
                PdfPCell cellCardHolders = new PdfPCell();
                cellCardHolders = new PdfPCell(new Phrase(percentage != 0 ? percentage.ToString() : "N/A", new Font(bf, 24, 0, TextColor)));
                tableCardHolders.TotalWidth = 60;
                cellCardHolders.Border = Rectangle.NO_BORDER;
                cellCardHolders.Colspan = 3;
                cellCardHolders.HorizontalAlignment = Element.ALIGN_RIGHT;
                tableCardHolders.AddCell(cellCardHolders);
                tableCardHolders.WriteSelectedRows(0, -1, 12, 508, canvas);
                #endregion

                #region Children's Books

                string circJuvenile = string.Format("{0:n0}", libraryData.CircJuvenile);
                if (circJuvenile == "0" || circJuvenile == "-1" || string.IsNullOrEmpty(circJuvenile))
                    circJuvenile = "N/A";
                PdfPTable tableCircJuvenile = new PdfPTable(3);
                PdfPCell cellCircJuvenile = new PdfPCell();
                cellCircJuvenile = new PdfPCell(new Phrase(circJuvenile, new Font(bf, 20, 0, TextColor)));
                tableCircJuvenile.TotalWidth = 90;
                cellCircJuvenile.Border = Rectangle.NO_BORDER;
                cellCircJuvenile.Colspan = 3;
                cellCircJuvenile.HorizontalAlignment = Element.ALIGN_RIGHT;
                tableCircJuvenile.AddCell(cellCircJuvenile);
                tableCircJuvenile.WriteSelectedRows(0, -1, 65, 255, canvas);
                #endregion

                #region Electronic Database

                string elecDatabase = string.Format("{0:n0}", libraryData.Databases);
                if (elecDatabase == "0" || elecDatabase == "-1" || string.IsNullOrEmpty(elecDatabase))
                    elecDatabase = "N/A";
                PdfPTable tableDatabase = new PdfPTable(3);
                PdfPCell cellDatabase = new PdfPCell();
                cellDatabase = new PdfPCell(new Phrase(elecDatabase, new Font(bf, 20, 0, TextColor)));
                tableDatabase.TotalWidth = 90;
                cellDatabase.Border = Rectangle.NO_BORDER;
                cellDatabase.Colspan = 3;
                cellDatabase.HorizontalAlignment = Element.ALIGN_RIGHT;
                tableDatabase.AddCell(cellDatabase);
                tableDatabase.WriteSelectedRows(0, -1, 505, 365, canvas);
                #endregion

                string strYear = year.ToString();
                ColumnText.ShowTextAligned(
                 canvas,
                 Element.ALIGN_RIGHT,
                 new Phrase(strYear, new Font(yearBf, 14, 0, YearTextColor)),
                 390, 115, 0
             );

                stamper.Close();
            }

            byte[] pdfByteArray = System.IO.File.ReadAllBytes(newFile);
            string base64EncodedPDF = System.Convert.ToBase64String(pdfByteArray);
            return base64EncodedPDF;
        }
        /// <summary>
        /// Edit pdf for support technology
        /// </summary>
        /// <param name="libraryName"></param>
        /// <param name="templateName"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public string EditPdfSupportTechnology(string libraryName, string templateName, int year)
        {
            GetDataFileLocation();

            string oldFile = @"C:\Temp\Template04 (Technology).pdf";// "oldFile.pdf";
            string newFile = @"C:\Temp\Template04 (Technology) - New.pdf";//"newFile.pdf";
            string fileName = "";
            if (year <= 0)
            {
                year = Convert.ToInt32(DateTime.Now.Year);
            }

            var template = templateService.GetAllTemplate().Where(x => x.Name == templateName).FirstOrDefault();
            if (template != null)
            {
                NewDataFileLocation(template.TemplateId);
                if (!Directory.Exists(newDataFielsLocation))
                {
                    Directory.CreateDirectory(newDataFielsLocation);
                }
                oldFile = @"" + dataFilesLocation + "\\" + template.FileLocation;
                Uri filePath = new Uri(templatePath + "\\" + template.FileLocation);
                fileName = System.IO.Path.GetFileName(filePath.LocalPath);
                newFile = @"" + newDataFielsLocation + "\\" + fileName;
            }

            if (!string.IsNullOrEmpty(oldFile) && !string.IsNullOrEmpty(newFile))
            {
                if (File.Exists(newFile))
                {
                    File.Delete(newFile);
                }
                System.IO.File.Copy(oldFile, newFile);
            }

            PdfReader reader = new PdfReader(oldFile);
            FileStream fs = new FileStream(newFile, FileMode.Create, FileAccess.Write);
            PdfStamper stamper = new PdfStamper(reader, fs);
            PdfContentByte canvas = stamper.GetOverContent(1);
            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            var FontColor = new BaseColor(50, 130, 197);
            var HeaderColor = new BaseColor(48, 47, 65);
            var TextColor = new BaseColor(40, 51, 78);
            BaseFont yearBf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            var YearTextColor = new BaseColor(35, 31, 32);

            var libraryData = libraryDataService.GetLibraryDataByName(libraryName, year);
            if (libraryData != null)
            {
                #region Chart Area

                List<ChartData> doughnutData = ChartData(libraryData, "Technology", "Doughnut");
                iTextSharp.text.Image doughnutimg = iTextSharp.text.Image.GetInstance(CreateDoughnutChart(doughnutData, 150, 150));
                doughnutimg.SetAbsolutePosition(110f, 490f);
                doughnutimg.Alignment = Element.ALIGN_LEFT;
                canvas.AddImage(doughnutimg);
                #endregion

                #region Library Name

                PdfPTable tableName = new PdfPTable(3);
                PdfPCell cellName = new PdfPCell();
                if (libraryData.LibraryName.Length > 0 && libraryData.LibraryName.Length <= 35)
                {
                    cellName = new PdfPCell(new Phrase(libraryData.LibraryName, new Font(bf, 30, 0, HeaderColor)));
                }
                else if (libraryData.LibraryName.Length > 35 && libraryData.LibraryName.Length <= 60)
                {
                    cellName = new PdfPCell(new Phrase(libraryData.LibraryName, new Font(bf, 28, 0, HeaderColor)));
                }
                else
                {
                    cellName = new PdfPCell(new Phrase(libraryData.LibraryName, new Font(bf, 30, 0, HeaderColor)));
                }

                tableName.TotalWidth = 710;
                cellName.Border = Rectangle.NO_BORDER;
                cellName.Colspan = 3;
                cellName.HorizontalAlignment = Element.ALIGN_CENTER;
                tableName.AddCell(cellName);
                tableName.WriteSelectedRows(0, -1, 30, 1005, canvas);

                #endregion

                #region Computers available

                PdfPTable tableComputer = new PdfPTable(3);
                PdfPCell cellComputer = new PdfPCell();
                cellComputer = new PdfPCell(new Phrase(libraryData.InternetPC != 0 ? libraryData.InternetPC.ToString() : "N/A", new Font(bf, 50, 0, FontColor)));
                tableComputer.TotalWidth = 128;
                cellComputer.Border = Rectangle.NO_BORDER;
                cellComputer.Colspan = 3;
                cellComputer.HorizontalAlignment = Element.ALIGN_CENTER;
                tableComputer.AddCell(cellComputer);
                tableComputer.WriteSelectedRows(0, -1, 125, 850, canvas);
                #endregion

                #region Each Computer User Over

                string computerUsed = string.Format("{0:n0}", (libraryData.AnnualUses / libraryData.InternetPC));
                if (computerUsed == "0" || computerUsed == "-1" || string.IsNullOrEmpty(computerUsed))
                    computerUsed = "N/A";
                PdfPTable tableEachComputer = new PdfPTable(3);
                PdfPCell cellEachComputer = new PdfPCell();
                cellEachComputer = new PdfPCell(new Phrase(computerUsed, new Font(bf, 20, 0, TextColor)));
                tableEachComputer.TotalWidth = 100;
                cellEachComputer.Border = Rectangle.NO_BORDER;
                cellEachComputer.Colspan = 3;
                cellEachComputer.HorizontalAlignment = Element.ALIGN_RIGHT;
                tableEachComputer.AddCell(cellEachComputer);
                tableEachComputer.WriteSelectedRows(0, -1, 352, 694, canvas);
                #endregion

                #region Resources Downloaded

                string circDownloadableMaterial = string.Format("{0:n0}", libraryData.CircDownloadableMaterial);
                if (circDownloadableMaterial == "0" || circDownloadableMaterial == "-1" || string.IsNullOrEmpty(circDownloadableMaterial))
                    circDownloadableMaterial = "N/A";
                PdfPTable tableResources = new PdfPTable(3);
                PdfPCell cellResources = new PdfPCell();
                cellResources = new PdfPCell(new Phrase(circDownloadableMaterial, new Font(bf, 20, 0, TextColor)));
                tableResources.TotalWidth = 100;
                cellResources.Border = Rectangle.NO_BORDER;
                cellResources.Colspan = 3;
                cellResources.HorizontalAlignment = Element.ALIGN_RIGHT;
                tableResources.AddCell(cellResources);
                tableResources.WriteSelectedRows(0, -1, 415, 415, canvas);

                #endregion

                #region Wireless internet sessions per year

                var wirelessValue = string.Format("{0:n0}", libraryData.AnnualWirelessSessions);
                if (wirelessValue == "0" || wirelessValue == "-1" || string.IsNullOrEmpty(wirelessValue))
                {

                    wirelessValue = "N/A";
                }
                PdfPTable tableWireless = new PdfPTable(3);
                PdfPCell cellWireless = new PdfPCell();
                cellWireless = new PdfPCell(new Phrase(wirelessValue, new Font(bf, 20, 0, TextColor)));
                tableWireless.TotalWidth = 100;
                cellWireless.Border = Rectangle.NO_BORDER;
                cellWireless.Colspan = 3;
                cellWireless.HorizontalAlignment = Element.ALIGN_RIGHT;
                tableWireless.AddCell(cellWireless);
                tableWireless.WriteSelectedRows(0, -1, 48, 190, canvas);
                #endregion

                #region Electronic

                var electronicValue = libraryData.Databases.ToString();
                if (electronicValue == "0" || electronicValue == "-1" || string.IsNullOrEmpty(wirelessValue))
                {
                    electronicValue = "N/A";
                }
                PdfPTable tableElectronic = new PdfPTable(3);
                PdfPCell cellElectronic = new PdfPCell();
                cellElectronic = new PdfPCell(new Phrase(electronicValue, new Font(bf, 20, 0, TextColor)));
                tableElectronic.TotalWidth = 100;
                cellElectronic.Border = Rectangle.NO_BORDER;
                cellElectronic.Colspan = 3;
                cellElectronic.HorizontalAlignment = Element.ALIGN_RIGHT;
                tableElectronic.AddCell(cellElectronic);
                tableElectronic.WriteSelectedRows(0, -1, 415, 190, canvas);

                #endregion
                string strYear = year.ToString();
                ColumnText.ShowTextAligned(
                 canvas,
                 Element.ALIGN_RIGHT,
                 new Phrase(strYear, new Font(yearBf, 14, 0, YearTextColor)),
                 390, 112, 0
             );

                stamper.Close();
            }


            byte[] pdfByteArray = System.IO.File.ReadAllBytes(newFile);
            string base64EncodedPDF = System.Convert.ToBase64String(pdfByteArray);
            return base64EncodedPDF;
        }
        /// <summary>
        /// Edit pdf for materials and circulation
        /// </summary>
        /// <param name="libraryName"></param>
        /// <param name="templateName"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public string EditPdfMaterialsandCirculation(string libraryName, string templateName, int year, string chartImage)
        {
            GetDataFileLocation();

            string oldFile = @"C:\Temp\Template03 (Materials and Circulation).pdf";// "oldFile.pdf";
            string newFile = @"C:\Temp\Template03 (Materials and Circulation) - New.pdf";//"newFile.pdf";
            string fileName = "";
            if (year <= 0)
            {
                year = Convert.ToInt32(DateTime.Now.Year);
            }

            var template = templateService.GetAllTemplate().Where(x => x.Name == templateName).FirstOrDefault();
            if (template != null)
            {
                NewDataFileLocation(template.TemplateId);
                if (!Directory.Exists(newDataFielsLocation))
                {
                    Directory.CreateDirectory(newDataFielsLocation);

                }
                oldFile = @"" + dataFilesLocation + "\\" + template.FileLocation;
                Uri filePath = new Uri(templatePath + "\\" + template.FileLocation);
                fileName = System.IO.Path.GetFileName(filePath.LocalPath);
                newFile = @"" + newDataFielsLocation + "\\" + fileName;

            }

            if (!string.IsNullOrEmpty(oldFile) && !string.IsNullOrEmpty(newFile))
            {
                if (File.Exists(newFile))
                {
                    File.Delete(newFile);
                }
                System.IO.File.Copy(oldFile, newFile);
            }

            PdfReader reader = new PdfReader(oldFile);
            FileStream fs = new FileStream(newFile, FileMode.Create, FileAccess.Write);
            PdfStamper stamper = new PdfStamper(reader, fs);
            PdfContentByte canvas = stamper.GetOverContent(1);
            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            var HeaderColor = new BaseColor(48, 47, 65);
            var TextColor = new BaseColor(40, 51, 78);
            BaseFont yearBf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            var YearTextColor = new BaseColor(35, 31, 32);

            var libraryData = libraryDataService.GetLibraryDataByName(libraryName, year);
            if (libraryData != null)
            {
                //Add doughnut chart 
                List<ChartData> doughnutData = ChartData(libraryData, "Materials & Circulation", "Doughnut");
                iTextSharp.text.Image doughnutimg = iTextSharp.text.Image.GetInstance(CreateDoughnutChart(doughnutData, 100, 80));
                doughnutimg.SetAbsolutePosition(110f, 490f);
                doughnutimg.Alignment = Element.ALIGN_LEFT;
                canvas.AddImage(doughnutimg);

                //Add pie chart 
                byte[] data = Convert.FromBase64String(chartImage);
                Stream inputImageStream = new MemoryStream(data, 0, data.Length, true, true);
                iTextSharp.text.Image pieimg = iTextSharp.text.Image.GetInstance(inputImageStream);

                pieimg.SetAbsolutePosition(276f, 140f);
                pieimg.Alignment = Element.ALIGN_LEFT;
                canvas.AddImage(pieimg);

                #region Data

                ColumnText ct = new ColumnText(canvas);

                if (libraryData.LibraryName.Length > 0 && libraryData.LibraryName.Length <= 35)
                {
                    if (libraryData.LibraryName.Length >= 20 && libraryData.LibraryName.Length <= 35)
                    {
                        ct.SetSimpleColumn(new Phrase(new Chunk(libraryData.LibraryName, new Font(bf, 30, 0, HeaderColor))),
                                           20, 780, 250, 1000, 45, Element.ALIGN_LEFT | Element.ALIGN_TOP);
                    }
                    else
                    {
                        ct.SetSimpleColumn(new Phrase(new Chunk(libraryData.LibraryName, new Font(bf, 35, 0, HeaderColor))),
                                       20, 780, 250, 1000, 45, Element.ALIGN_LEFT | Element.ALIGN_TOP);
                    }
                }
                else
                {
                    ct.SetSimpleColumn(new Phrase(new Chunk(libraryData.LibraryName, new Font(bf, 28, 0, HeaderColor))),
                                       20, 780, 250, 1000, 45, Element.ALIGN_LEFT | Element.ALIGN_TOP);
                }

                ct.Go();

                #region Print Books
                string printMaterials = string.Format("{0:n0}", libraryData.PrintMaterials);
                if (printMaterials == "0" || printMaterials == "-1" || string.IsNullOrEmpty(printMaterials))
                    printMaterials = "N/A";
                PdfPTable tableMaterials = new PdfPTable(3);
                PdfPCell cellMaterials = new PdfPCell();
                cellMaterials = new PdfPCell(new Phrase(printMaterials, new Font(bf, 20, 0, TextColor)));
                tableMaterials.TotalWidth = 128;
                cellMaterials.Border = Rectangle.NO_BORDER;
                cellMaterials.Colspan = 3;
                cellMaterials.HorizontalAlignment = Element.ALIGN_CENTER;
                tableMaterials.AddCell(cellMaterials);
                tableMaterials.WriteSelectedRows(0, -1, 300, 930, canvas);
                #endregion

                #region Phusical Video

                string physicalVideo = string.Format("{0:n0}", libraryData.PhysicalVideo);
                if (physicalVideo == "0" || physicalVideo == "-1" || string.IsNullOrEmpty(physicalVideo))
                    physicalVideo = "N/A";
                PdfPTable tableVideo = new PdfPTable(3);
                PdfPCell cellVideo = new PdfPCell();
                cellVideo = new PdfPCell(new Phrase(physicalVideo, new Font(bf, 20, 0, TextColor)));
                tableVideo.TotalWidth = 128;
                cellVideo.Border = Rectangle.NO_BORDER;
                cellVideo.Colspan = 3;
                cellVideo.HorizontalAlignment = Element.ALIGN_CENTER;
                tableVideo.AddCell(cellVideo);
                tableVideo.WriteSelectedRows(0, -1, 560, 930, canvas);
                #endregion

                #region EBooks

                string ebooks = string.Format("{0:n0}", libraryData.Ebooks);
                if (ebooks == "0" || ebooks == "-1" || string.IsNullOrEmpty(ebooks))
                    ebooks = "N/A";
                PdfPTable tableEBooks = new PdfPTable(3);
                PdfPCell cellEBooks = new PdfPCell();
                cellEBooks = new PdfPCell(new Phrase(ebooks, new Font(bf, 20, 0, TextColor)));
                tableEBooks.TotalWidth = 128;
                cellEBooks.Border = Rectangle.NO_BORDER;
                cellEBooks.Colspan = 3;
                cellEBooks.HorizontalAlignment = Element.ALIGN_CENTER;
                tableEBooks.AddCell(cellEBooks);
                tableEBooks.WriteSelectedRows(0, -1, 320, 730, canvas);
                #endregion

                #region Items on Audio

                string physicalAudio = string.Format("{0:n0}", libraryData.PhysicalAudio);
                if (physicalAudio == "0" || physicalAudio == "-1" || string.IsNullOrEmpty(physicalAudio))
                    physicalAudio = "N/A";
                PdfPTable tableAudio = new PdfPTable(3);
                PdfPCell cellAudio = new PdfPCell();
                cellAudio = new PdfPCell(new Phrase(physicalAudio, new Font(bf, 20, 0, TextColor)));
                tableAudio.TotalWidth = 128;
                cellAudio.Border = Rectangle.NO_BORDER;
                cellAudio.Colspan = 3;
                cellAudio.HorizontalAlignment = Element.ALIGN_CENTER;
                tableAudio.AddCell(cellAudio);
                tableAudio.WriteSelectedRows(0, -1, 570, 730, canvas);
                #endregion

                #region Items Checked Out

                string circTotal = string.Format("{0:n0}", libraryData.CircTotal);
                if (circTotal == "0" || circTotal == "-1" || string.IsNullOrEmpty(circTotal))
                    circTotal = "N/A";
                PdfPTable tableTotal = new PdfPTable(3);
                PdfPCell cellTotal = new PdfPCell();
                cellTotal = new PdfPCell(new Phrase(circTotal, new Font(bf, 20, 0, TextColor)));
                tableTotal.TotalWidth = 128;
                cellTotal.Border = Rectangle.NO_BORDER;
                cellTotal.Colspan = 3;
                cellTotal.HorizontalAlignment = Element.ALIGN_CENTER;
                tableTotal.AddCell(cellTotal);
                tableTotal.WriteSelectedRows(0, -1, 70, 330, canvas);

                #endregion

                ColumnText.ShowTextAligned(
                 canvas,
                 Element.ALIGN_LEFT,
                 new Phrase((libraryData.CircTotal / libraryData.PopulationLegalSvcArea) != 0 ? (libraryData.CircTotal / libraryData.PopulationLegalSvcArea).ToString() : "N/A", new Font(bf, 20, 0, TextColor)),
                 220, 180, 0
             );

                string strYear = year.ToString();
                ColumnText.ShowTextAligned(
                 canvas,
                 Element.ALIGN_RIGHT,
                 new Phrase(strYear, new Font(yearBf, 14, 0, YearTextColor)),
                 390, 113, 0
             );
                #endregion
                stamper.Close();
            }

            byte[] pdfByteArray = System.IO.File.ReadAllBytes(newFile);
            string base64EncodedPDF = System.Convert.ToBase64String(pdfByteArray);
            return base64EncodedPDF;
        }

        /// <summary>
        /// Edit pdf for programming
        /// </summary>
        /// <param name="libraryName"></param>
        /// <param name="templateName"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public string EditPdfProgramming(string libraryName, string templateName, int year, string chartImage)
        {
            GetDataFileLocation();

            string oldFile = @"C:\Temp\Template05 (Programming).pdf";
            string newFile = @"C:\Temp\Template05 (Programming) - New.pdf";
            string fileName = "";
            if (year <= 0)
            {
                year = Convert.ToInt32(DateTime.Now.Year);
            }

            var template = templateService.GetAllTemplate().Where(x => x.Name == templateName).FirstOrDefault();
            if (template != null)
            {
                NewDataFileLocation(template.TemplateId);
                if (!Directory.Exists(newDataFielsLocation))
                {
                    Directory.CreateDirectory(newDataFielsLocation);

                }
                oldFile = @"" + dataFilesLocation + "\\" + template.FileLocation;
                Uri filePath = new Uri(templatePath + "\\" + template.FileLocation);
                fileName = System.IO.Path.GetFileName(filePath.LocalPath);
                newFile = @"" + newDataFielsLocation + "\\" + fileName;

            }

            if (!string.IsNullOrEmpty(oldFile) && !string.IsNullOrEmpty(newFile))
            {
                if (File.Exists(newFile))
                {
                    File.Delete(newFile);
                }
                System.IO.File.Copy(oldFile, newFile);
            }

            PdfReader reader = new PdfReader(oldFile);
            FileStream fs = new FileStream(newFile, FileMode.Create, FileAccess.Write);
            PdfStamper stamper = new PdfStamper(reader, fs);
            PdfContentByte canvas = stamper.GetOverContent(1);
            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            var HeaderColor = new BaseColor(48, 47, 65);
            var TextColor = new BaseColor(14, 16, 49);
            var LeftTextColor = new BaseColor(40, 51, 78);
            BaseFont yearBf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            var YearTextColor = new BaseColor(35, 31, 32);

            var libraryData = libraryDataService.GetLibraryDataByName(libraryName, year);
            if (libraryData != null)
            {

                #region Library Name

                ColumnText ct = new ColumnText(canvas);
                if (libraryData.LibraryName.Length > 0 && libraryData.LibraryName.Length <= 35)
                {
                    if (libraryData.LibraryName.Length >= 20 && libraryData.LibraryName.Length <= 35)
                    {
                        ct.SetSimpleColumn(new Phrase(new Chunk(libraryData.LibraryName, new Font(bf, 30, 0, HeaderColor))),
                                       20, 780, 250, 1000, 45, Element.ALIGN_LEFT | Element.ALIGN_TOP);
                    }
                    else
                    {
                        ct.SetSimpleColumn(new Phrase(new Chunk(libraryData.LibraryName, new Font(bf, 35, 0, HeaderColor))),
                                       20, 780, 250, 1000, 45, Element.ALIGN_LEFT | Element.ALIGN_TOP);
                    }
                }
                else
                {
                    ct.SetSimpleColumn(new Phrase(new Chunk(libraryData.LibraryName, new Font(bf, 28, 0, HeaderColor))),
                                       20, 780, 250, 1000, 45, Element.ALIGN_LEFT | Element.ALIGN_TOP);
                }
                ct.Go();
                #endregion

                #region Chart Area

                //Add pie chart 
                List<ChartData> chartData = ChartData(libraryData, "Programming", "Pie");
                iTextSharp.text.Image pieimg = iTextSharp.text.Image.GetInstance(CreatePieChart(chartData, 210, 210));
                pieimg.SetAbsolutePosition(70f, 240f);
                pieimg.Alignment = Element.ALIGN_LEFT;
                canvas.AddImage(pieimg);
                #endregion

                #region Children Attended Programs

                string childrenProgramAttendance = string.Format("{0:n0}", libraryData.ChildrenProgramAttendance);
                if (childrenProgramAttendance == "0" || childrenProgramAttendance == "-1" || string.IsNullOrEmpty(childrenProgramAttendance))
                    childrenProgramAttendance = "N/A";
                PdfPTable tableChildren = new PdfPTable(3);
                PdfPCell cellChildren = new PdfPCell();
                cellChildren = new PdfPCell(new Phrase(childrenProgramAttendance, new Font(bf, 20, 0, TextColor)));
                tableChildren.TotalWidth = 90;
                cellChildren.Border = Rectangle.NO_BORDER;
                cellChildren.Colspan = 3;
                cellChildren.HorizontalAlignment = Element.ALIGN_LEFT;
                tableChildren.AddCell(cellChildren);
                tableChildren.WriteSelectedRows(0, -1, 410, 950, canvas);
                #endregion

                #region Children Participated in Summer Reading

                string summerReadingChildren = string.Format("{0:n0}", libraryData.SummerReadingChildren);
                if (summerReadingChildren == "0" || summerReadingChildren == "-1" || string.IsNullOrEmpty(summerReadingChildren))
                    summerReadingChildren = "N/A";
                PdfPTable tableChildrenParticipated = new PdfPTable(3);
                PdfPCell cellChildrenParticipated = new PdfPCell();
                cellChildrenParticipated = new PdfPCell(new Phrase(summerReadingChildren, new Font(bf, 20, 0, TextColor)));
                tableChildrenParticipated.TotalWidth = 90;
                cellChildrenParticipated.Border = Rectangle.NO_BORDER;
                cellChildrenParticipated.Colspan = 3;
                cellChildrenParticipated.HorizontalAlignment = Element.ALIGN_LEFT;
                tableChildrenParticipated.AddCell(cellChildrenParticipated);
                tableChildrenParticipated.WriteSelectedRows(0, -1, 670, 950, canvas);
                #endregion

                #region Teens Attended Programs

                string youngAdultProgramAttendance = string.Format("{0:n0}", libraryData.YoungAdultProgramAttendance);
                if (youngAdultProgramAttendance == "0" || youngAdultProgramAttendance == "-1" || string.IsNullOrEmpty(youngAdultProgramAttendance))
                    youngAdultProgramAttendance = "N/A";
                PdfPTable tableTeens = new PdfPTable(3);
                PdfPCell cellTeens = new PdfPCell();
                cellTeens = new PdfPCell(new Phrase(youngAdultProgramAttendance, new Font(bf, 20, 0, TextColor)));
                tableTeens.TotalWidth = 90;
                cellTeens.Border = Rectangle.NO_BORDER;
                cellTeens.Colspan = 3;
                cellTeens.HorizontalAlignment = Element.ALIGN_LEFT;
                tableTeens.AddCell(cellTeens);
                tableTeens.WriteSelectedRows(0, -1, 360, 670, canvas);
                #endregion

                #region Teens Participated in Summer Reading

                string summerReadingTeens = string.Format("{0:n0}", libraryData.SummerReadingTeens);
                if (summerReadingTeens == "0" || summerReadingTeens == "-1" || string.IsNullOrEmpty(summerReadingTeens))
                    summerReadingTeens = "N/A";
                PdfPTable tableTeensReading = new PdfPTable(3);
                PdfPCell cellTeensReading = new PdfPCell();
                cellTeensReading = new PdfPCell(new Phrase(summerReadingTeens, new Font(bf, 20, 0, TextColor)));
                tableTeensReading.TotalWidth = 90;
                cellTeensReading.Border = Rectangle.NO_BORDER;
                cellTeensReading.Colspan = 3;
                cellTeensReading.HorizontalAlignment = Element.ALIGN_CENTER;
                tableTeensReading.AddCell(cellTeensReading);
                tableTeensReading.WriteSelectedRows(0, -1, 570, 670, canvas);
                #endregion

                #region Number of Programs

                string numberPrograms = libraryData.TotalLibraryPrograms.ToString();
                if (numberPrograms == "0" || numberPrograms == "-1" || string.IsNullOrEmpty(numberPrograms))
                    numberPrograms = "N/A";
                PdfPTable tableNumber = new PdfPTable(3);
                PdfPCell cellNumber = new PdfPCell();
                cellNumber = new PdfPCell(new Phrase(numberPrograms, new Font(bf, 30, 0, LeftTextColor)));
                tableNumber.TotalWidth = 100;
                cellNumber.Border = Rectangle.NO_BORDER;
                cellNumber.Colspan = 3;
                cellNumber.HorizontalAlignment = Element.ALIGN_CENTER;
                tableNumber.AddCell(cellNumber);
                tableNumber.WriteSelectedRows(0, -1, 92, 510, canvas);
                #endregion

                #region Adults Attended Programs

                string totalProgramAttendance = string.Format("{0:n0}", libraryData.TotalProgramAttendance);
                if (totalProgramAttendance == "0" || totalProgramAttendance == "-1" || string.IsNullOrEmpty(totalProgramAttendance))
                    totalProgramAttendance = "N/A";
                PdfPTable tableAdults = new PdfPTable(3);
                PdfPCell cellAdults = new PdfPCell();
                cellAdults = new PdfPCell(new Phrase(totalProgramAttendance, new Font(bf, 20, 0, TextColor)));
                tableAdults.TotalWidth = 90;
                cellAdults.Border = Rectangle.NO_BORDER;
                cellAdults.Colspan = 3;
                cellAdults.HorizontalAlignment = Element.ALIGN_CENTER;
                tableAdults.AddCell(cellAdults);
                tableAdults.WriteSelectedRows(0, -1, 320, 370, canvas);
                #endregion

                #region Adults Attended Programs Year

                ColumnText.ShowTextAligned(
                    canvas,
                    Element.ALIGN_LEFT,
                    new Phrase(year != 0 ? year.ToString() : "N/A", new Font(bf, 20, 0, TextColor)),
                    454, 197, 0
                    );
                #endregion

                #region Adults Participated in Summer Reading

                string summerReadingAdult = string.Format("{0:n0}", libraryData.SummerReadingAdult);
                if (summerReadingAdult == "0" || summerReadingAdult == "-1" || string.IsNullOrEmpty(summerReadingAdult))
                    summerReadingAdult = "N/A";
                PdfPTable tableAdultsSummer = new PdfPTable(3);
                PdfPCell cellAdultsSummer = new PdfPCell();
                cellAdultsSummer = new PdfPCell(new Phrase(summerReadingAdult, new Font(bf, 20, 0, TextColor)));
                tableAdultsSummer.TotalWidth = 90;
                cellAdultsSummer.Border = Rectangle.NO_BORDER;
                cellAdultsSummer.Colspan = 3;
                cellAdultsSummer.HorizontalAlignment = Element.ALIGN_LEFT;
                tableAdultsSummer.AddCell(cellAdultsSummer);
                tableAdultsSummer.WriteSelectedRows(0, -1, 660, 370, canvas);
                #endregion

                string strYear = year.ToString();
                ColumnText.ShowTextAligned(
                 canvas,
                 Element.ALIGN_RIGHT,
                 new Phrase(strYear, new Font(yearBf, 14, 0, YearTextColor)),
                 395, 116, 0
             );

                stamper.Close();
            }

            byte[] pdfByteArray = System.IO.File.ReadAllBytes(newFile);
            string base64EncodedPDF = System.Convert.ToBase64String(pdfByteArray);
            return base64EncodedPDF;
        }
        /// <summary>
        /// Edit pdf for library overview
        /// </summary>
        /// <param name="libraryName"></param>
        /// <param name="templateName"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public string EditPdfLibraryOverview(string libraryName, string templateName, int year, string chartImage)
        {
            GetDataFileLocation();

            string oldFile = @"C:\Temp\Template02 (Library Overview).pdf";// "oldFile.pdf";
            string newFile = @"C:\Temp\Template02 (Library Overview) - New.pdf";//"newFile.pdf";
            string fileName = "";
            if (year <= 0)
            {
                year = Convert.ToInt32(DateTime.Now.Year);
            }

            var template = templateService.GetAllTemplate().Where(x => x.Name == templateName).FirstOrDefault();
            if (template != null)
            {
                NewDataFileLocation(template.TemplateId);
                if (!Directory.Exists(newDataFielsLocation))
                {
                    Directory.CreateDirectory(newDataFielsLocation);
                }
                oldFile = @"" + dataFilesLocation + "\\" + template.FileLocation;
                Uri filePath = new Uri(templatePath + "\\" + template.FileLocation);
                fileName = System.IO.Path.GetFileName(filePath.LocalPath);
                newFile = @"" + newDataFielsLocation + "\\" + fileName;
            }

            if (!string.IsNullOrEmpty(oldFile) && !string.IsNullOrEmpty(newFile))
            {
                if (File.Exists(newFile))
                {
                    File.Delete(newFile);
                }
                System.IO.File.Copy(oldFile, newFile);
            }

            PdfReader reader = new PdfReader(oldFile);
            FileStream fs = new FileStream(newFile, FileMode.Create, FileAccess.Write);
            PdfStamper stamper = new PdfStamper(reader, fs);
            PdfContentByte canvas = stamper.GetOverContent(1);
            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            var FontColor = new BaseColor(9, 166, 165);
            var HeaderColor = new BaseColor(48, 47, 65);
            var TextColor = new BaseColor(40, 51, 78);
            var TextDarkGreen = new BaseColor(14, 89, 89);
            BaseFont yearBf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            var YearTextColor = new BaseColor(35, 31, 32);

            var libraryData = libraryDataService.GetLibraryDataByName(libraryName, year);
            if (libraryData != null)
            {
                #region Chart Area

                //Add pie chart
                List<ChartData> chartData = ChartData(libraryData, "Library Overview", "Pie");
                iTextSharp.text.Image pieimg = iTextSharp.text.Image.GetInstance(CreatePieChart(chartData, 210, 210));
                pieimg.SetAbsolutePosition(565f, 780f);
                pieimg.Alignment = Element.ALIGN_LEFT;
                canvas.AddImage(pieimg);

                //Add doughnut chart                 
                List<ChartData> doughnutData = ChartData(libraryData, "Library Overview", "Doughnut");
                iTextSharp.text.Image doughnutimg = iTextSharp.text.Image.GetInstance(CreateDoughnutChart(doughnutData, 100, 100));
                doughnutimg.SetAbsolutePosition(350f, 600f);
                doughnutimg.Alignment = Element.ALIGN_LEFT;
                canvas.AddImage(doughnutimg);
                #endregion

                #region Library Name

                ColumnText ct = new ColumnText(canvas);
                if (libraryData.LibraryName.Length > 0 && libraryData.LibraryName.Length <= 35)
                {
                    if (libraryData.LibraryName.Length >= 20 && libraryData.LibraryName.Length <= 30)
                    {
                        ct.SetSimpleColumn(new Phrase(new Chunk(libraryData.LibraryName, new Font(bf, 30, 0, HeaderColor))),
                                       20, 820, 250, 1000, 45, Element.ALIGN_LEFT | Element.ALIGN_TOP);
                    }
                    else
                    {
                        ct.SetSimpleColumn(new Phrase(new Chunk(libraryData.LibraryName, new Font(bf, 35, 0, HeaderColor))),
                                       20, 820, 250, 1000, 45, Element.ALIGN_LEFT | Element.ALIGN_TOP);
                    }
                }
                else
                {
                    ct.SetSimpleColumn(new Phrase(new Chunk(libraryData.LibraryName, new Font(bf, 30, 0, HeaderColor))),
                                       20, 820, 250, 1000, 45, Element.ALIGN_LEFT | Element.ALIGN_TOP);
                }
                ct.Go();
                #endregion

                #region Card Holders

                string cardHolders = string.Format("{0:n0}", (libraryData.RegAdults + libraryData.RegChildren));
                if (cardHolders == "0" || cardHolders == "-1" || string.IsNullOrEmpty(cardHolders))
                    cardHolders = "N/A";
                PdfPTable tableCard = new PdfPTable(3);
                PdfPCell cellCard = new PdfPCell();
                cellCard = new PdfPCell(new Phrase(cardHolders, new Font(bf, 20, 0, BaseColor.WHITE)));
                tableCard.TotalWidth = 80;
                cellCard.Border = Rectangle.NO_BORDER;
                cellCard.Colspan = 3;
                cellCard.HorizontalAlignment = Element.ALIGN_RIGHT;
                tableCard.AddCell(cellCard);
                tableCard.WriteSelectedRows(0, -1, 415, 904, canvas);
                #endregion

                #region Serving a Population

                string populationLegalSvcArea = string.Format("{0:n0}", libraryData.PopulationLegalSvcArea);
                if (populationLegalSvcArea == "0" || populationLegalSvcArea == "-1" || string.IsNullOrEmpty(populationLegalSvcArea))
                    populationLegalSvcArea = "N/A";
                PdfPTable tableServing = new PdfPTable(3);
                PdfPCell cellServing = new PdfPCell();
                cellServing = new PdfPCell(new Phrase(populationLegalSvcArea, new Font(bf, 20, 0, TextColor)));
                tableServing.TotalWidth = 80;
                cellServing.Border = Rectangle.NO_BORDER;
                cellServing.Colspan = 3;
                cellServing.HorizontalAlignment = Element.ALIGN_CENTER;
                tableServing.AddCell(cellServing);
                tableServing.WriteSelectedRows(0, -1, 390, 805, canvas);
                #endregion

                #region Library Sites

                string librarySites = (libraryData.CentralLibraries + libraryData.Branches).ToString();
                if (librarySites == "0" || librarySites == "-1" || string.IsNullOrEmpty(librarySites))
                    librarySites = "N/A";
                PdfPTable tableSites = new PdfPTable(3);
                PdfPCell cellSites = new PdfPCell();
                cellSites = new PdfPCell(new Phrase(librarySites, new Font(bf, 22, 0, TextDarkGreen)));
                tableSites.TotalWidth = 80;
                cellSites.Border = Rectangle.NO_BORDER;
                cellSites.Colspan = 3;
                cellSites.HorizontalAlignment = Element.ALIGN_RIGHT;
                tableSites.AddCell(cellSites);
                tableSites.WriteSelectedRows(0, -1, 58, 758, canvas);
                #endregion

                #region Staff Members

                string staffMembers = libraryData.LibraryStaffTotal.ToString();
                if (staffMembers == "0" || staffMembers == "-1" || string.IsNullOrEmpty(staffMembers))
                    staffMembers = "N/A";
                PdfPTable tableStaff = new PdfPTable(3);
                PdfPCell cellStaff = new PdfPCell();
                cellStaff = new PdfPCell(new Phrase(staffMembers, new Font(bf, 22, 0, TextDarkGreen)));
                tableStaff.TotalWidth = 80;
                cellStaff.Border = Rectangle.NO_BORDER;
                cellStaff.Colspan = 3;
                cellStaff.HorizontalAlignment = Element.ALIGN_RIGHT;
                tableStaff.AddCell(cellStaff);
                tableStaff.WriteSelectedRows(0, -1, 22, 700, canvas);
                #endregion

                #region Items Checked Out

                string circTotal = string.Format("{0:n0}", libraryData.CircTotal);
                if (circTotal == "0" || circTotal == "-1" || string.IsNullOrEmpty(circTotal))
                {
                    circTotal = "N/A";
                }
                PdfPTable tableItems = new PdfPTable(3);
                PdfPCell cellItems = new PdfPCell();
                cellItems = new PdfPCell(new Phrase(circTotal, new Font(bf, 24, 0, TextColor)));
                tableItems.TotalWidth = 150;
                cellItems.Border = Rectangle.NO_BORDER;
                cellItems.Colspan = 3;
                cellItems.HorizontalAlignment = Element.ALIGN_CENTER;
                tableItems.AddCell(cellItems);
                tableItems.WriteSelectedRows(0, -1, 560, 680, canvas);
                #endregion

                #region Annual Hours of Service

                string annualHrsOpen = string.Format("{0:n0}", libraryData.AnnualHrsOpen);
                if (annualHrsOpen == "0" || annualHrsOpen == "-1" || string.IsNullOrEmpty(annualHrsOpen))
                {
                    annualHrsOpen = "N/A";
                }
                PdfPTable tableHours = new PdfPTable(3);
                PdfPCell cellHours = new PdfPCell();
                cellHours = new PdfPCell(new Phrase(annualHrsOpen, new Font(bf, 40, 0, TextColor)));
                tableHours.TotalWidth = 150;
                cellHours.Border = Rectangle.NO_BORDER;
                cellHours.Colspan = 3;
                cellHours.HorizontalAlignment = Element.ALIGN_CENTER;
                tableHours.AddCell(cellHours);
                tableHours.WriteSelectedRows(0, -1, 135, 600, canvas);
                #endregion

                #region Averaging Nearly

                string averagingNearly = (libraryData.CircTotal / libraryData.PopulationLegalSvcArea).ToString();
                if (averagingNearly == "0" || averagingNearly == "-1" || string.IsNullOrEmpty(averagingNearly))
                {
                    averagingNearly = "N/A";
                }
                PdfPTable tableNearly = new PdfPTable(3);
                PdfPCell cellNearly = new PdfPCell();
                cellNearly = new PdfPCell(new Phrase(averagingNearly, new Font(bf, 20, 0, TextColor)));
                tableNearly.TotalWidth = 40;
                cellNearly.Border = Rectangle.NO_BORDER;
                cellNearly.Colspan = 3;
                cellNearly.HorizontalAlignment = Element.ALIGN_LEFT;
                tableNearly.AddCell(cellNearly);
                tableNearly.WriteSelectedRows(0, -1, 723, 548, canvas);
                #endregion

                #region Computers

                string computers = libraryData.InternetPC.ToString();
                if (computers == "0" || computers == "-1" || string.IsNullOrEmpty(computers))
                {
                    computers = "N/A";
                }
                PdfPTable tableComputers = new PdfPTable(3);
                PdfPCell cellComputers = new PdfPCell();
                cellComputers = new PdfPCell(new Phrase(computers, new Font(bf, 40, 0, FontColor)));
                tableComputers.TotalWidth = 140;
                cellComputers.Border = Rectangle.NO_BORDER;
                cellComputers.Colspan = 3;
                cellComputers.HorizontalAlignment = Element.ALIGN_CENTER;
                tableComputers.AddCell(cellComputers);
                tableComputers.WriteSelectedRows(0, -1, 80, 385, canvas);
                #endregion

                #region Each Used Over

                string annualUsers = string.Format("{0:n0}", (libraryData.AnnualUses / libraryData.InternetPC));
                if (annualUsers == "0" || annualUsers == "-1" || string.IsNullOrEmpty(annualUsers))
                    annualUsers = "N/A";
                PdfPTable tableUsed = new PdfPTable(3);
                PdfPCell cellUsed = new PdfPCell();
                cellUsed = new PdfPCell(new Phrase(annualUsers, new Font(bf, 20, 0, TextColor)));
                tableUsed.TotalWidth = 90;
                cellUsed.Border = Rectangle.NO_BORDER;
                cellUsed.Colspan = 3;
                cellUsed.HorizontalAlignment = Element.ALIGN_RIGHT;
                tableUsed.AddCell(cellUsed);
                tableUsed.WriteSelectedRows(0, -1, 18, 218, canvas);
                #endregion

                #region Number of Programs

                string totalLibraryPrograms = string.Format("{0:n0}", libraryData.TotalLibraryPrograms);
                if (totalLibraryPrograms == "0" || totalLibraryPrograms == "-1" || string.IsNullOrEmpty(totalLibraryPrograms))
                    totalLibraryPrograms = "N/A";
                PdfPTable tableNumber = new PdfPTable(3);
                PdfPCell cellNumber = new PdfPCell();
                cellNumber = new PdfPCell(new Phrase(totalLibraryPrograms, new Font(bf, 30, 0, TextColor)));
                tableNumber.TotalWidth = 140;
                cellNumber.Border = Rectangle.NO_BORDER;
                cellNumber.Colspan = 3;
                cellNumber.HorizontalAlignment = Element.ALIGN_CENTER;
                tableNumber.AddCell(cellNumber);
                tableNumber.WriteSelectedRows(0, -1, 358, 235, canvas);
                #endregion

                #region Program Attendance

                string totalLibraryProgramAttendance = string.Format("{0:n0}", libraryData.TotalLibraryProgramAttendance);
                if (totalLibraryProgramAttendance == "0" || totalLibraryProgramAttendance == "-1" || string.IsNullOrEmpty(totalLibraryProgramAttendance))
                    totalLibraryProgramAttendance = "N/A";
                PdfPTable tableAttendance = new PdfPTable(3);
                PdfPCell cellAttendance = new PdfPCell();
                cellAttendance = new PdfPCell(new Phrase(totalLibraryProgramAttendance, new Font(bf, 30, 0, TextColor)));
                tableAttendance.TotalWidth = 140;
                cellAttendance.Border = Rectangle.NO_BORDER;
                cellAttendance.Colspan = 3;
                cellAttendance.HorizontalAlignment = Element.ALIGN_CENTER;
                tableAttendance.AddCell(cellAttendance);
                tableAttendance.WriteSelectedRows(0, -1, 595, 235, canvas);
                #endregion

                string strYear = year.ToString();
                ColumnText.ShowTextAligned(
                 canvas,
                 Element.ALIGN_RIGHT,
                 new Phrase(strYear, new Font(yearBf, 14, 0, YearTextColor)),
                 390, 116, 0
             );

                stamper.Close();
            }

            byte[] pdfByteArray = System.IO.File.ReadAllBytes(newFile);
            string base64EncodedPDF = System.Convert.ToBase64String(pdfByteArray);
            return base64EncodedPDF;
        }

        #region Dynamic Doughnut
        public MemoryStream CreateDoughnutChart(List<ChartData> chartData, int width, int height)
        {
            Chart chart = new Chart();
            chart.Width = width;
            chart.Height = height;


            chart.Series.Add(CreateSeries(chartData));
            chart.ChartAreas.Add(CreateChartArea());
            chart.Series["Data"].ChartType = SeriesChartType.Doughnut;
            chart.Series["Data"]["PieLabelStyle"] = "Disabled";


            MemoryStream ms = new MemoryStream();
            chart.SaveImage(ms, ChartImageFormat.Jpeg);
            ms.Position = 0;

            return ms;
        }
        #endregion

        #region Dynamic Pie Charts

        public MemoryStream CreatePieChart(List<ChartData> chartData, int width, int height)
        {
            Chart chart = new Chart();
            chart.Width = width;
            chart.Height = height;
            chart.RenderType = RenderType.ImageTag;
            chart.AntiAliasing = AntiAliasingStyles.All;
            chart.TextAntiAliasingQuality = TextAntiAliasingQuality.High;

            chart.Series.Add(CreateSeries(chartData));
            chart.ChartAreas.Add(CreateChartArea());
            chart.Series["Data"].ChartType = SeriesChartType.Pie;
            chart.Series["Data"]["PieLabelStyle"] = "Outside";
            chart.Series["Data"]["PieLineColor"] = "Black";

            chart.Series[0].SmartLabelStyle.CalloutLineAnchorCapStyle = LineAnchorCapStyle.Round;

            chart.ChartAreas[0].AxisX.ArrowStyle = AxisArrowStyle.Triangle;

            MemoryStream ms = new MemoryStream();
            chart.SaveImage(ms, ChartImageFormat.Jpeg);
            ms.Position = 0;
            return ms;
        }
        public Series CreateSeries(List<ChartData> results)
        {
            Series seriesDetail = new Series();
            seriesDetail.Name = "Data";
            seriesDetail.IsValueShownAsLabel = false;
            seriesDetail.Color = System.Drawing.Color.FromArgb(198, 99, 99);

            seriesDetail.BorderWidth = 2;

            DataPoint point;

            foreach (ChartData result in results)
            {
                point = new DataPoint();
                point.AxisLabel = result.Name;
                point.YValues = new double[] { (result.Value) };
                point.Color = result.Color;
                point.Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold);


                seriesDetail.Points.Add(point);
            }
            seriesDetail.ChartArea = "Data";
            return seriesDetail;
        }
        public ChartArea CreateChartArea()
        {
            ChartArea chartArea = new ChartArea();
            chartArea.Name = "Data";

            chartArea.AxisX.IsMarginVisible = true;
            chartArea.AxisX.IsLabelAutoFit = false;
            chartArea.AxisY.IsLabelAutoFit = false;


            chartArea.AxisX.LabelStyle.Font =
               new System.Drawing.Font("Arial",
                        8F, System.Drawing.FontStyle.Regular);
            chartArea.AxisY.LabelStyle.Font =
               new System.Drawing.Font("Arial",
                        8F, System.Drawing.FontStyle.Regular);


            return chartArea;
        }
        public List<ChartData> ChartData(Data.Models.LibraryDataModel libraryData, string type, string chartType)
        {
            List<ChartData> _chartDataModel = new List<ChartData>();

            if (chartType.ToLower() == "pie")
            {
                if (type.ToLower().Contains("library overview"))
                {
                    _chartDataModel.Add(new ChartData { Name = "Children", Value = libraryData.RegChildren, Color = System.Drawing.Color.FromArgb(3, 127, 124) });
                    _chartDataModel.Add(new ChartData { Name = "Adults", Value = libraryData.RegAdults, Color = System.Drawing.Color.FromArgb(9, 166, 165) });
                }
                else if (type.ToLower().Contains("materials & circulation"))
                {
                    _chartDataModel.Add(new ChartData { Name = "Items on Audio", Value = libraryData.PhysicalAudio, Color = System.Drawing.Color.FromArgb(225, 74, 56) });//#E14A38
                    _chartDataModel.Add(new ChartData { Name = "Movies", Value = libraryData.PhysicalVideo, Color = System.Drawing.Color.FromArgb(235, 107, 87) });//#EB6B57
                    _chartDataModel.Add(new ChartData { Name = "eBooks", Value = libraryData.Ebooks, Color = System.Drawing.Color.FromArgb(163, 143, 132) });//#A38F84
                    _chartDataModel.Add(new ChartData { Name = "eAudio", Value = libraryData.DownloadableAudio, Color = System.Drawing.Color.FromArgb(209, 213, 216) });//#D1D6D8
                    _chartDataModel.Add(new ChartData { Name = "Print Books", Value = libraryData.PrintMaterials, Color = System.Drawing.Color.FromArgb(184, 51, 47) });//#B8332F
                }
                else if (type.ToLower().Contains("programming"))
                {
                    int numberOfYouth = libraryData.ChildrenPrograms + libraryData.YoungAdultPrograms;
                    int numberOfAdult = libraryData.TotalLibraryPrograms - numberOfYouth;
                    _chartDataModel.Add(new ChartData { Name = "Youth", Value = (numberOfYouth), Color = System.Drawing.Color.FromArgb(147, 102, 171) });
                    _chartDataModel.Add(new ChartData { Name = "Adult", Value = numberOfAdult, Color = System.Drawing.Color.FromArgb(86, 58, 130) });

                }
                else if (type.ToLower().Contains("youth services"))
                {
                    _chartDataModel.Add(new ChartData { Name = "Children", Value = libraryData.RegChildren, Color = System.Drawing.Color.FromArgb(243, 122, 53) });
                    _chartDataModel.Add(new ChartData { Name = "Adults", Value = libraryData.RegAdults, Color = System.Drawing.Color.FromArgb(249, 160, 40) });
                }
            }
            else if (chartType.ToLower() == "doughnut")
            {
                if (type.ToLower().Contains("library overview"))
                {
                    _chartDataModel.Add(new ChartData { Name = "Tangible", Value = libraryData.CircPhysicalMaterial, Color = System.Drawing.Color.FromArgb(9, 166, 165) });
                    _chartDataModel.Add(new ChartData { Name = "Electronic", Value = libraryData.CircTotal, Color = System.Drawing.Color.FromArgb(209, 213, 216) });
                }
                else if (type.ToLower().Contains("technology"))
                {
                    _chartDataModel.Add(new ChartData { Name = "Tangible", Value = libraryData.CircPhysicalMaterial, Color = System.Drawing.Color.FromArgb(42, 106, 176) });
                    _chartDataModel.Add(new ChartData { Name = "Electronic", Value = libraryData.CircTotal, Color = System.Drawing.Color.FromArgb(209, 213, 216) });
                }
                else if (type.ToLower().Contains("materials & circulation"))
                {
                    _chartDataModel.Add(new ChartData { Name = "Tangible", Value = libraryData.CircPhysicalMaterial, Color = System.Drawing.Color.FromArgb(209, 73, 65) });
                    _chartDataModel.Add(new ChartData { Name = "Electronic", Value = libraryData.CircTotal, Color = System.Drawing.Color.FromArgb(209, 213, 216) });
                }
            }

            return _chartDataModel;
        }
        public List<ChartData> PieChartData(Data.Models.LibraryDataModel libraryData, string type, string chartType)
        {
            List<ChartData> _chartDataModel = new List<ChartData>();

            if (chartType.ToLower() == "pie")
            {
                if (type.ToLower().Contains("library overview"))
                {
                    _chartDataModel.Add(new ChartData { Name = "Children", Value = libraryData.RegChildren, Color = System.Drawing.Color.FromArgb(3, 127, 124) });
                    _chartDataModel.Add(new ChartData { Name = "Adults", Value = libraryData.RegAdults, Color = System.Drawing.Color.FromArgb(9, 166, 165) });
                }
                else if (type.ToLower().Contains("materials & circulation"))
                {
                    _chartDataModel.Add(new ChartData { Name = "Items on Audio", Value = libraryData.PhysicalAudio, Color = System.Drawing.Color.FromArgb(225, 74, 56) });
                    _chartDataModel.Add(new ChartData { Name = "Movies", Value = libraryData.PhysicalVideo, Color = System.Drawing.Color.FromArgb(235, 107, 87) });
                    _chartDataModel.Add(new ChartData { Name = "eBooks", Value = libraryData.Ebooks, Color = System.Drawing.Color.FromArgb(163, 143, 132) });
                    _chartDataModel.Add(new ChartData { Name = "eAudio", Value = libraryData.DownloadableAudio, Color = System.Drawing.Color.FromArgb(209, 213, 216) });
                    _chartDataModel.Add(new ChartData { Name = "Print Books", Value = libraryData.PrintMaterials, Color = System.Drawing.Color.FromArgb(184, 51, 47) });
                }
                else if (type.ToLower().Contains("programming"))
                {
                    _chartDataModel.Add(new ChartData { Name = "Youth", Value = libraryData.ChildrenPrograms + libraryData.YoungAdultPrograms, Color = System.Drawing.Color.FromArgb(147, 102, 171) });
                    _chartDataModel.Add(new ChartData { Name = "Adult", Value = libraryData.TotalProgramAttendance, Color = System.Drawing.Color.FromArgb(86, 58, 130) });
                }
                else if (type.ToLower().Contains("youth services"))
                {
                    _chartDataModel.Add(new ChartData { Name = "Children", Value = libraryData.RegChildren, Color = System.Drawing.Color.FromArgb(243, 122, 53) });
                    _chartDataModel.Add(new ChartData { Name = "Adults", Value = libraryData.RegAdults, Color = System.Drawing.Color.FromArgb(249, 160, 40) });
                }
            }
            else if (chartType.ToLower() == "doughnut")
            {
                if (type.ToLower().Contains("library overview"))
                {
                    _chartDataModel.Add(new ChartData { Name = "Tangible", Value = libraryData.CircPhysicalMaterial, Color = System.Drawing.Color.FromArgb(9, 166, 165) });

                    _chartDataModel.Add(new ChartData { Name = "Electronic", Value = libraryData.CircTotal, Color = System.Drawing.Color.FromArgb(209, 213, 216) });
                }
                else if (type.ToLower().Contains("technology"))
                {
                    _chartDataModel.Add(new ChartData { Name = "Tangible", Value = libraryData.CircPhysicalMaterial, Color = System.Drawing.Color.FromArgb(42, 106, 176) });

                    _chartDataModel.Add(new ChartData { Name = "Electronic", Value = libraryData.CircTotal, Color = System.Drawing.Color.FromArgb(209, 213, 216) });
                }
                else if (type.ToLower().Contains("materials & circulation"))
                {
                    _chartDataModel.Add(new ChartData { Name = "Tangible", Value = libraryData.CircPhysicalMaterial, Color = System.Drawing.Color.FromArgb(209, 73, 65) });

                    _chartDataModel.Add(new ChartData { Name = "Electronic", Value = libraryData.CircTotal, Color = System.Drawing.Color.FromArgb(209, 213, 216) });
                }
            }


            return _chartDataModel;
        }
        #endregion

        #endregion

        #region MultiYearTrend Bar Chart
        public string EditMultiYearTrend(string libraryName, string statistics, string startYear, string endYear, string chartImage)
        {
            GetDataFileLocation();

            string oldFile = @"C:\Websites\DataFiles\StateLibrary\Templates\CustomTemplate.pdf";// "oldFile.pdf";
            string newFile = @"C:\Websites\DataFiles\StateLibrary\Templates\CustomTemplate -new.pdf";//"newFile.pdf";

            string statisticsName = customColumnService.GetAllCustomColumn().Where(x => x.CustomColumnId == Convert.ToInt32(statistics)).FirstOrDefault().Name;

            if (!string.IsNullOrEmpty(oldFile) && !string.IsNullOrEmpty(newFile))
            {
                if (File.Exists(newFile))
                {
                    File.Delete(newFile);
                }
                System.IO.File.Copy(oldFile, newFile);
            }

            PdfReader reader = new PdfReader(oldFile);
            FileStream fs = new FileStream(newFile, FileMode.Create, FileAccess.Write);
            PdfStamper stamper = new PdfStamper(reader, fs);
            PdfContentByte canvas = stamper.GetOverContent(1);
            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            BaseFont bfSub = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            var FontColor = new BaseColor(48, 47, 65);
            var HeaderColor = new BaseColor(48, 47, 65);
            var TextColor = new BaseColor(40, 51, 78);

            var customTemplateDetails = customTemplateService.GetCustomTemplateById(Convert.ToInt32(statistics));
            var libraryData = libraryDataService.GetLibraryDataByNameAndYearRange(libraryName, Convert.ToInt32(startYear), Convert.ToInt32(endYear));
            string mappingValue = customTemplateDetails.MappingColumn;
            if (libraryData != null)
            {
                //Add doughnut chart 
                List<ChartData> doughnutData = new List<Models.ChartData>();
                if (!string.IsNullOrEmpty(mappingValue))
                    doughnutData = BarChartData(libraryData, "MyltiYear", "bar", mappingValue);
                else
                    doughnutData = BarChartDataByColumnId(libraryData, "MyltiYear", "bar", statistics);


                iTextSharp.text.Image doughnutimg = iTextSharp.text.Image.GetInstance(CreateBarChart(doughnutData, 600, 600));
                doughnutimg.SetAbsolutePosition(40f, 180f);
                doughnutimg.Alignment = Element.ALIGN_LEFT;
                canvas.AddImage(doughnutimg);

                #region Library Name

                PdfPTable tableName = new PdfPTable(3);
                PdfPCell cellName = new PdfPCell();
                if (libraryData.FirstOrDefault().LibraryName.Length > 0 && libraryData.FirstOrDefault().LibraryName.Length <= 35)
                {
                    cellName = new PdfPCell(new Phrase(libraryData.FirstOrDefault().LibraryName, new Font(bf, 30, 0, HeaderColor)));
                }
                else if (libraryData.FirstOrDefault().LibraryName.Length > 35 && libraryData.FirstOrDefault().LibraryName.Length <= 60)
                {
                    cellName = new PdfPCell(new Phrase(libraryData.FirstOrDefault().LibraryName, new Font(bf, 28, 0, HeaderColor)));
                }
                else
                {
                    cellName = new PdfPCell(new Phrase(libraryData.FirstOrDefault().LibraryName, new Font(bf, 30, 0, HeaderColor)));
                }

                tableName.TotalWidth = 710;
                cellName.Border = Rectangle.NO_BORDER;
                cellName.Colspan = 3;
                cellName.HorizontalAlignment = Element.ALIGN_CENTER;
                tableName.AddCell(cellName);
                tableName.WriteSelectedRows(0, -1, 30, 920, canvas);

                #endregion

                #region SubHeaderSection
                string statisticsNameAndyearValue = statisticsName + " from " + startYear + " - " + endYear;

                if (statisticsNameAndyearValue.Length > 0 && statisticsNameAndyearValue.Length <= 20)
                {
                    ColumnText.ShowTextAligned(
                    canvas,
                    Element.ALIGN_RIGHT,
                    new Phrase(statisticsNameAndyearValue, new Font(bfSub, 30, 0, FontColor)),
                    520, 860, 0);
                }
                else if (statisticsNameAndyearValue.Length > 0 && statisticsNameAndyearValue.Length <= 30)
                {
                    ColumnText.ShowTextAligned(
                    canvas,
                    Element.ALIGN_RIGHT,
                    new Phrase(statisticsNameAndyearValue, new Font(bfSub, 30, 0, FontColor)),
                    560, 860, 0);
                }
                else if (statisticsNameAndyearValue.Length > 30 && statisticsNameAndyearValue.Length <= 35)
                {
                    ColumnText.ShowTextAligned(
                    canvas,
                    Element.ALIGN_RIGHT,
                    new Phrase(statisticsNameAndyearValue, new Font(bfSub, 30, 0, FontColor)),
                    620, 860, 0);
                }
                else if (statisticsNameAndyearValue.Length > 35 && statisticsNameAndyearValue.Length <= 45)
                {
                    ColumnText.ShowTextAligned(
                    canvas,
                    Element.ALIGN_RIGHT,
                    new Phrase(statisticsNameAndyearValue, new Font(bfSub, 25, 0, FontColor)),
                    650, 860, 0);
                }
                else if (statisticsNameAndyearValue.Length > 45 && statisticsNameAndyearValue.Length <= 55)
                {
                    ColumnText.ShowTextAligned(
                    canvas,
                    Element.ALIGN_RIGHT,
                    new Phrase(statisticsNameAndyearValue, new Font(bfSub, 20, 0, FontColor)),
                    645, 860, 0);
                }
                else if (statisticsNameAndyearValue.Length > 55 && statisticsNameAndyearValue.Length <= 60)
                {
                    ColumnText.ShowTextAligned(
                    canvas,
                    Element.ALIGN_RIGHT,
                    new Phrase(statisticsNameAndyearValue, new Font(bfSub, 20, 0, FontColor)),
                    670, 860, 0);
                }
                else if (statisticsNameAndyearValue.Length > 60 && statisticsNameAndyearValue.Length <= 75)
                {
                    ColumnText.ShowTextAligned(
                    canvas,
                    Element.ALIGN_RIGHT,
                    new Phrase(statisticsNameAndyearValue, new Font(bfSub, 20, 0, FontColor)),
                    660, 860, 0);
                }
                else
                {
                    ColumnText.ShowTextAligned(
                    canvas,
                    Element.ALIGN_RIGHT,
                    new Phrase(statisticsNameAndyearValue, new Font(bfSub, 20, 0, FontColor)),
                    710, 860, 0);
                }


                #endregion

                stamper.Close();
            }

            byte[] pdfByteArray = System.IO.File.ReadAllBytes(newFile);
            string base64EncodedPDF = System.Convert.ToBase64String(pdfByteArray);
            return base64EncodedPDF;
        }

        public List<ChartData> BarChartData(List<Data.Models.LibraryDataModel> libraryData, string type, string chartType, string mappingValue)
        {
            List<ChartData> _chartDataModel = new List<ChartData>();
            if (chartType.ToLower() == "bar")
            {


                double columnValue = 0;
                foreach (var libraryValue in libraryData)
                {
                    foreach (PropertyInfo info in libraryValue.GetType().GetProperties())
                    {
                        if (info.CanRead && info.Name == mappingValue)
                        {
                            columnValue = Convert.ToDouble(info.GetValue(libraryValue, null));
                            _chartDataModel.Add(new ChartData { Name = libraryValue.DataYear.ToString(), Value = columnValue, Color = System.Drawing.Color.FromArgb(9, 166, 165) });
                            break;
                        }
                    }
                }
            }


            return _chartDataModel;
        }

        public List<ChartData> BarChartDataByColumnId(List<Data.Models.LibraryDataModel> libraryData, string type, string chartType, string statisticsId)
        {
            List<ChartData> _chartDataModel = new List<ChartData>();
            if (chartType.ToLower() == "bar")
            {
                double columnValue = 0;
                foreach (var libraryValue in libraryData)
                {
                    if (statisticsId == "4")
                        columnValue = Convert.ToDouble(libraryValue.RegAdults + libraryValue.RegChildren);
                    else if (statisticsId == "5")
                        columnValue = Convert.ToDouble(libraryValue.CentralLibraries + libraryValue.Branches);
                    else if (statisticsId == "26")
                    {

                        columnValue = Convert.ToDouble(libraryValue.AnnualAttendanceLibrary);
                    }


                    _chartDataModel.Add(new ChartData { Name = libraryValue.DataYear.ToString(), Value = columnValue, Color = System.Drawing.Color.FromArgb(9, 166, 165) });

                }
            }


            return _chartDataModel;
        }
        /// <summary>
        /// Create bar chart
        /// </summary>
        /// <param name="chartData"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public MemoryStream CreateBarChart(List<ChartData> chartData, int width, int height)
        {
            Chart chart = new Chart();
            chart.Width = width;
            chart.Height = height;

            chart.Series.Add(CreateSeries(chartData));
            chart.ChartAreas.Add(CreateChartArea());
            chart.Series["Data"].ChartType = SeriesChartType.Column;
            chart.Series["Data"]["BarLabelStyle"] = "Disabled";


            MemoryStream ms = new MemoryStream();
            chart.SaveImage(ms, ChartImageFormat.Jpeg);
            ms.Position = 0;

            return ms;
        }

        #endregion
    }
}