//Reference:- Install "iTextSharp" Package from NuGet Package Manager
//using iTextSharp.text;
//using iTextSharp.text.pdf;
//using iTextSharp.text.pdf.parser;
using StateOfOhioLibrary.Data.Models;
using StateOfOhioLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.WebControls;

namespace StateOfOhioLibrary.Services
{
    public class CustomColumnService
    {
        #region Global Declarations
        DatabaseContext dbContext = new DatabaseContext();
        CommonService commonService = new CommonService();
        CustomTemplateService customTemplateService = new CustomTemplateService();
        LibraryDataService libraryDataService = new LibraryDataService();
        public string className = MethodBase.GetCurrentMethod().DeclaringType.Name;
        string dataFilesLocation = "";
        string newDataFielsLocation = "";
        string customTemplatePath = WebConfigurationManager.AppSettings["CustomTemplatePath"];
        string customTemplatesDomainName = WebConfigurationManager.AppSettings["CustomTemplatesDomainName"];
        string domainName = WebConfigurationManager.AppSettings["DomainName"];
        public string errorLogFilePath = Convert.ToString(WebConfigurationManager.AppSettings["LogFilePath"]);
        #endregion
        #region Methods
        public List<SelectListItem> PopulateCustomColumn()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem
            {
                Text = "--Select a Field--",
                Value = "0"
            });
            List<CustomColumnDataModel> list = GetAllCustomColumn().Where(x => x.Status == CommonService.EnumStatus.Active.ToString()).Distinct().ToList();
            foreach (CustomColumnDataModel customColumn in list)
            {
                items.Add(new SelectListItem
                {
                    Text = customColumn.Name,
                    Value = customColumn.CustomColumnId.ToString()
                });
            }
            return items;
        }
        public List<CustomColumnDataModel> GetAllCustomColumn()
        {
            return dbContext.CustomColumn.ToList();
        }
        public byte[] GetImage(int customId)
        {
            byte[] photo;
            string query = (from custom in dbContext.CustomTemplate
                            where custom.CustomColumnId == customId && custom.Status == CommonService.EnumStatus.Active.ToString()
                            select custom.FileLocation).FirstOrDefault();
            string path = HttpContext.Current.Server.MapPath(query);
            photo = File.ReadAllBytes(path);
            return photo;
        }
        public byte[] GetBytesFromImage(String imageFile)
        {
            System.Drawing.Image img = System.Drawing.Image.FromFile(imageFile);
            byte[] bytes;
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                bytes = ms.ToArray();
            }
            return bytes;
        }

        /// <summary>
        /// Get pdf data file location
        /// </summary>
        /// <returns></returns>
        public string GetDataFileLocation()
        {
            dataFilesLocation = HttpContext.Current.Server.MapPath(customTemplatePath);
            return dataFilesLocation;
        }
        /// <summary>
        /// Get New pdf data file location with
        /// </summary>
        /// <returns></returns>
        public string NewDataFileLocation()
        {
            newDataFielsLocation = HttpContext.Current.Server.MapPath(customTemplatePath + "\\" + "New");

            return newDataFielsLocation;
        }
        public string DownloadPdf(string libraryId, int year, int customColumnIdField1, int customColumnIdField2, int customColumnIdField3, int customColumnIdField4, int customColumnIdField5, int customColumnIdField6)
        {

            GetDataFileLocation();
            string oldFile = @"C:\Temp\CustomTemplate.pdf";// "oldFile.pdf";
            string newFile = @"C:\Temp\CustomTemplate -new.pdf";//"newFile.pdf";
            string fileName = "";
            NewDataFileLocation();
            if (!Directory.Exists(newDataFielsLocation))
            {
                Directory.CreateDirectory(newDataFielsLocation);

            }
            oldFile = @"" + dataFilesLocation + "TemplateCustom.pdf";
            Uri filePath = new Uri(customTemplatePath);
            fileName = System.IO.Path.GetFileName(filePath.LocalPath);
            newFile = @"" + newDataFielsLocation + "\\" + "TemplateCustom.pdf";
            if (!string.IsNullOrEmpty(newFile))
            {
                if (File.Exists(newFile))
                {
                    File.Delete(newFile);
                }

            }
            PdfReader reader = new PdfReader(oldFile);
            FileStream fs = new FileStream(newFile, FileMode.Create, FileAccess.Write);
            PdfStamper stamper = new PdfStamper(reader, fs);
            PdfContentByte canvas = stamper.GetOverContent(1);
            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            var FontColor = new BaseColor(64, 168, 94);
            var HeaderColor = new BaseColor(48, 47, 65);
            var TextColor = new BaseColor(40, 51, 78);
            var LabelColor = new BaseColor(16, 128, 126);



            var libraryData = libraryDataService.GetLibraryDataByIdAndYear(libraryId, year);
            if (libraryData != null)
            {


                var customTemplateDetails = customTemplateService.GetCustomTemplateById(customColumnIdField1);
                if (customTemplateDetails != null)
                {
                    string imagePath = HttpContext.Current.Server.MapPath(customTemplatePath + "\\" + customTemplateDetails.FileLocation);
                    iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imagePath);

                    img.ScaleToFit(220f, 156f);
                    img.SetAbsolutePosition(80f, 700f);
                    img.Alignment = Element.ALIGN_LEFT;
                    canvas.AddImage(img);
                    switch (customColumnIdField1)
                    {
                        case 1:
                            string populationofLegalSvcAreaText = "Serves a population";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(populationofLegalSvcAreaText, new Font(bf, 20, 0, LabelColor)),
                             60, 692, 0
                         );
                            populationofLegalSvcAreaText = "of ";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(populationofLegalSvcAreaText, new Font(bf, 20, 0, LabelColor)),
                             60, 672, 0
                             );
                            string populationofLegalSvcArea = libraryData.PopulationLegalSvcArea.ToString();
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(populationofLegalSvcArea, new Font(bf, 20, 0, TextColor)),
                             84, 672, 0
                         );
                            populationofLegalSvcAreaText = " Ohioans";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(populationofLegalSvcAreaText, new Font(bf, 20, 0, LabelColor)),
                             149, 672, 0
                             );

                            break;
                        case 2:
                            string regAdults = libraryData.RegAdults.ToString();
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regAdults, new Font(bf, 20, 0, TextColor)),
                             60, 692, 0
                         );
                            string regAdultsText = " adults have";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regAdultsText, new Font(bf, 20, 0, LabelColor)),
                             124, 692, 0
                             );
                            regAdultsText = " library cards";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regAdultsText, new Font(bf, 20, 0, LabelColor)),
                             94, 672, 0
                             );

                            break;
                        case 3:
                            string regChildren = libraryData.RegChildren.ToString();
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regChildren, new Font(bf, 20, 0, TextColor)),
                             60, 682, 0
                         );
                            string regChildrenText = " children have";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regChildrenText, new Font(bf, 20, 0, LabelColor)),
                             124, 682, 0
                             );
                            regChildrenText = " library cards";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regChildrenText, new Font(bf, 20, 0, LabelColor)),
                             94, 662, 0
                             );
                            break;
                        case 4:
                            string cardHolders = string.Format("{0:n0}", (libraryData.RegAdults + libraryData.RegChildren));
                            if (cardHolders == "0")
                                cardHolders = "N/A";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(cardHolders, new Font(bf, 14, 0, BaseColor.WHITE)),
                             144, 765, 0);
                            string cardHoldersText = " card";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(cardHoldersText, new Font(bf, 12, 0, TextColor)),
                             195, 765, 0
                             );
                            cardHoldersText = " holders";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(cardHoldersText, new Font(bf, 12, 0, TextColor)),
                             150, 752, 0
                             );
                            break;

                    }

                }

                customTemplateDetails = customTemplateService.GetCustomTemplateById(customColumnIdField2);
                if (customTemplateDetails != null)
                {
                    string imagePath = HttpContext.Current.Server.MapPath(customTemplatePath + "\\" + customTemplateDetails.FileLocation);
                    iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imagePath);

                    img.ScaleToFit(220f, 156f);
                    img.SetAbsolutePosition(490f, 700f);
                    img.Alignment = Element.ALIGN_LEFT;
                    canvas.AddImage(img);
                    switch (customColumnIdField2)
                    {

                        case 1:
                            string populationofLegalSvcAreaText = "Serves a population";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(populationofLegalSvcAreaText, new Font(bf, 20, 0, LabelColor)),
                             470, 692, 0
                         );
                            populationofLegalSvcAreaText = "of ";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(populationofLegalSvcAreaText, new Font(bf, 20, 0, LabelColor)),
                             470, 672, 0
                             );
                            string populationofLegalSvcArea = libraryData.PopulationLegalSvcArea.ToString();
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(populationofLegalSvcArea, new Font(bf, 20, 0, TextColor)),
                             494, 672, 0
                         );
                            populationofLegalSvcAreaText = " Ohioans";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(populationofLegalSvcAreaText, new Font(bf, 20, 0, LabelColor)),
                             559, 672, 0
                             );
                            break;
                        case 2:
                            string regAdults = libraryData.RegAdults.ToString();
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regAdults, new Font(bf, 20, 0, TextColor)),
                             420, 692, 0
                         );
                            string regAdultsText = " adults have";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regAdultsText, new Font(bf, 20, 0, LabelColor)),
                            484, 692, 0
                             );
                            regAdultsText = " library cards";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regAdultsText, new Font(bf, 20, 0, LabelColor)),
                            454, 672, 0
                             );
                            break;
                        case 3:
                            string regChildren = libraryData.RegChildren.ToString();
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regChildren, new Font(bf, 20, 0, TextColor)),
                             420, 682, 0
                         );
                            string regChildrenText = " children have";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regChildrenText, new Font(bf, 20, 0, LabelColor)),
                            482, 682, 0
                             );
                            regChildrenText = " library cards";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regChildrenText, new Font(bf, 20, 0, LabelColor)),
                            452, 662, 0
                             );
                            break;
                        case 4:
                            string cardHolders = string.Format("{0:n0}", (libraryData.RegAdults + libraryData.RegChildren));
                            if (cardHolders == "0")
                                cardHolders = "N/A";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(cardHolders, new Font(bf, 14, 0, BaseColor.WHITE)),
                             554, 765, 0);
                            string cardHoldersText = " card";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(cardHoldersText, new Font(bf, 12, 0, TextColor)),
                             605, 765, 0
                             );
                            cardHoldersText = " holders";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(cardHoldersText, new Font(bf, 12, 0, TextColor)),
                             560, 752, 0
                             );
                            break;

                    }

                }
                customTemplateDetails = customTemplateService.GetCustomTemplateById(customColumnIdField3);
                if (customTemplateDetails != null)
                {
                    string imagePath = HttpContext.Current.Server.MapPath(customTemplatePath + "\\" + customTemplateDetails.FileLocation);
                    iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imagePath);

                    img.ScaleToFit(220f, 156f);
                    img.SetAbsolutePosition(80f, 475f);
                    img.Alignment = Element.ALIGN_LEFT;
                    canvas.AddImage(img);
                    switch (customColumnIdField3)
                    {

                        case 1:
                            string populationofLegalSvcAreaText = "Serves a population";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(populationofLegalSvcAreaText, new Font(bf, 20, 0, LabelColor)),
                             60, 467, 0
                         );
                            populationofLegalSvcAreaText = "of ";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(populationofLegalSvcAreaText, new Font(bf, 20, 0, LabelColor)),
                             60, 447, 0
                             );
                            string populationofLegalSvcArea = libraryData.PopulationLegalSvcArea.ToString();
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(populationofLegalSvcArea, new Font(bf, 20, 0, TextColor)),
                             84, 447, 0
                         );
                            populationofLegalSvcAreaText = " Ohioans";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(populationofLegalSvcAreaText, new Font(bf, 20, 0, LabelColor)),
                             149, 447, 0
                             );
                            break;
                        case 2:
                            string regAdults = libraryData.RegAdults.ToString();
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regAdults, new Font(bf, 20, 0, TextColor)),
                             60, 467, 0
                         );
                            string regAdultsText = " adults have";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regAdultsText, new Font(bf, 20, 0, LabelColor)),
                             124, 467, 0
                             );
                            regAdultsText = " library cards";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regAdultsText, new Font(bf, 20, 0, LabelColor)),
                             124, 447, 0
                             );

                            break;
                        case 3:
                            string regChildren = libraryData.RegChildren.ToString();
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regChildren, new Font(bf, 20, 0, TextColor)),
                             60, 457, 0
                         );
                            string regChildrenText = " children have";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regChildrenText, new Font(bf, 20, 0, LabelColor)),
                             124, 457, 0
                             );
                            regChildrenText = " library cards";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regChildrenText, new Font(bf, 20, 0, LabelColor)),
                             124, 437, 0
                             );
                            break;
                        case 4:
                            string cardHolders = string.Format("{0:n0}", (libraryData.RegAdults + libraryData.RegChildren));
                            if (cardHolders == "0")
                                cardHolders = "N/A";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(cardHolders, new Font(bf, 14, 0, BaseColor.WHITE)),
                             144, 540, 0);
                            string cardHoldersText = " card";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(cardHoldersText, new Font(bf, 12, 0, TextColor)),
                             195, 540, 0
                             );
                            cardHoldersText = " holders";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(cardHoldersText, new Font(bf, 12, 0, TextColor)),
                             152, 527, 0
                             );
                            break;

                    }

                }
                customTemplateDetails = customTemplateService.GetCustomTemplateById(customColumnIdField4);
                if (customTemplateDetails != null)
                {
                    string imagePath = HttpContext.Current.Server.MapPath(customTemplatePath + "\\" + customTemplateDetails.FileLocation);
                    iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imagePath);
                    img.ScaleToFit(220f, 156f);

                    img.SetAbsolutePosition(490f, 475f);
                    img.Alignment = Element.ALIGN_LEFT;
                    canvas.AddImage(img);
                    switch (customColumnIdField4)
                    {

                        case 1:
                            string populationofLegalSvcAreaText = "Serves a population";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(populationofLegalSvcAreaText, new Font(bf, 20, 0, LabelColor)),
                             470, 467, 0
                         );
                            populationofLegalSvcAreaText = "of ";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(populationofLegalSvcAreaText, new Font(bf, 20, 0, LabelColor)),
                             470, 447, 0
                             );
                            string populationofLegalSvcArea = libraryData.PopulationLegalSvcArea.ToString();
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(populationofLegalSvcArea, new Font(bf, 20, 0, TextColor)),
                             494, 447, 0
                         );
                            populationofLegalSvcAreaText = " Ohioans";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(populationofLegalSvcAreaText, new Font(bf, 20, 0, LabelColor)),
                             559, 447, 0
                             );
                            break;
                        case 2:
                            string regAdults = libraryData.RegAdults.ToString();
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regAdults, new Font(bf, 20, 0, TextColor)),
                             420, 467, 0
                         );
                            string regAdultsText = " adults have";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regAdultsText, new Font(bf, 20, 0, LabelColor)),
                            484, 467, 0
                             );
                            regAdultsText = " library cards";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regAdultsText, new Font(bf, 20, 0, LabelColor)),
                            454, 447, 0
                             );

                            break;
                        case 3:
                            string regChildren = libraryData.RegChildren.ToString();
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regChildren, new Font(bf, 20, 0, TextColor)),
                             420, 457, 0
                         );
                            string regChildrenText = " children have";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regChildrenText, new Font(bf, 20, 0, LabelColor)),
                            482, 457, 0
                             );
                            regChildrenText = " library cards";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regChildrenText, new Font(bf, 20, 0, LabelColor)),
                            452, 437, 0
                             );

                            break;
                        case 4:
                            string cardHolders = string.Format("{0:n0}", (libraryData.RegAdults + libraryData.RegChildren));
                            if (cardHolders == "0")
                                cardHolders = "N/A";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(cardHolders, new Font(bf, 14, 0, BaseColor.WHITE)),
                             554, 540, 0);
                            string cardHoldersText = " card";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(cardHoldersText, new Font(bf, 12, 0, TextColor)),
                             605, 540, 0
                             );
                            cardHoldersText = " holders";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(cardHoldersText, new Font(bf, 12, 0, TextColor)),
                             560, 527, 0
                             );
                            break;

                    }

                }
                customTemplateDetails = customTemplateService.GetCustomTemplateById(customColumnIdField5);
                if (customTemplateDetails != null)
                {
                    string imagePath = HttpContext.Current.Server.MapPath(customTemplatePath + "\\" + customTemplateDetails.FileLocation);
                    iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imagePath);

                    img.ScaleToFit(220f, 156f);
                    img.SetAbsolutePosition(80f, 250f);
                    img.Alignment = Element.ALIGN_LEFT;
                    canvas.AddImage(img);
                    switch (customColumnIdField5)
                    {

                        case 1:
                            string populationofLegalSvcAreaText = "Serves a population";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(populationofLegalSvcAreaText, new Font(bf, 20, 0, LabelColor)),
                             60, 242, 0
                         );
                            populationofLegalSvcAreaText = "of ";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(populationofLegalSvcAreaText, new Font(bf, 20, 0, LabelColor)),
                             60, 222, 0
                             );
                            string populationofLegalSvcArea = libraryData.PopulationLegalSvcArea.ToString();
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(populationofLegalSvcArea, new Font(bf, 20, 0, TextColor)),
                             84, 222, 0
                         );
                            populationofLegalSvcAreaText = " Ohioans";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(populationofLegalSvcAreaText, new Font(bf, 20, 0, LabelColor)),
                             149, 222, 0
                             );
                            break;
                        case 2:
                            string regAdults = libraryData.RegAdults.ToString();
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regAdults, new Font(bf, 20, 0, TextColor)),
                             60, 242, 0
                         );
                            string regAdultsText = " adults have";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regAdultsText, new Font(bf, 20, 0, LabelColor)),
                            124, 242, 0
                             );
                            regAdultsText = " library cards";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regAdultsText, new Font(bf, 20, 0, LabelColor)),
                            94, 222, 0
                             );
                            break;
                        case 3:
                            string regChildren = libraryData.RegChildren.ToString();
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regChildren, new Font(bf, 20, 0, TextColor)),
                             60, 232, 0
                         );
                            string regChildrenText = " children have";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regChildrenText, new Font(bf, 20, 0, LabelColor)),
                            124, 232, 0
                             );
                            regChildrenText = " library cards";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regChildrenText, new Font(bf, 20, 0, LabelColor)),
                            94, 212, 0
                             );
                            break;
                        case 4:
                            string cardHolders = string.Format("{0:n0}", (libraryData.RegAdults + libraryData.RegChildren));
                            if (cardHolders == "0")
                                cardHolders = "N/A";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(cardHolders, new Font(bf, 14, 0, BaseColor.WHITE)),
                             144, 315, 0);
                            string cardHoldersText = " card";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(cardHoldersText, new Font(bf, 12, 0, TextColor)),
                             195, 315, 0
                             );
                            cardHoldersText = " holders";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(cardHoldersText, new Font(bf, 12, 0, TextColor)),
                             152, 302, 0
                             );
                            break;

                    }

                }
                customTemplateDetails = customTemplateService.GetCustomTemplateById(customColumnIdField6);
                if (customTemplateDetails != null)
                {
                    string imagePath = HttpContext.Current.Server.MapPath(customTemplatePath + "\\" + customTemplateDetails.FileLocation);
                    iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imagePath);
                    img.ScaleToFit(220f, 156f);

                    img.SetAbsolutePosition(490f, 250f);
                    img.Alignment = Element.ALIGN_LEFT;
                    canvas.AddImage(img);
                    switch (customColumnIdField6)
                    {

                        case 1:
                            string populationofLegalSvcAreaText = "Serves a population";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(populationofLegalSvcAreaText, new Font(bf, 20, 0, LabelColor)),
                             470, 242, 0
                         );
                            populationofLegalSvcAreaText = "of ";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(populationofLegalSvcAreaText, new Font(bf, 20, 0, LabelColor)),
                             470, 222, 0
                             );
                            string populationofLegalSvcArea = libraryData.PopulationLegalSvcArea.ToString();
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(populationofLegalSvcArea, new Font(bf, 20, 0, TextColor)),
                             494, 222, 0
                         );
                            populationofLegalSvcAreaText = " Ohioans";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(populationofLegalSvcAreaText, new Font(bf, 20, 0, LabelColor)),
                             559, 222, 0
                             );
                            break;
                        case 2:
                            string regAdults = libraryData.RegAdults.ToString();
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regAdults, new Font(bf, 20, 0, TextColor)),
                             420, 242, 0
                         );
                            string regAdultsText = " adults have";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regAdultsText, new Font(bf, 20, 0, LabelColor)),
                            484, 242, 0
                             );
                            regAdultsText = " library cards";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regAdultsText, new Font(bf, 20, 0, LabelColor)),
                            454, 222, 0
                             );
                            break;
                        case 3:
                            string regChildren = libraryData.RegChildren.ToString();
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regChildren, new Font(bf, 20, 0, TextColor)),
                             420, 232, 0
                         );
                            string regChildrenText = " children have";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regChildrenText, new Font(bf, 20, 0, LabelColor)),
                            482, 232, 0
                             );
                            regChildrenText = " library cards";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(regChildrenText, new Font(bf, 20, 0, LabelColor)),
                            452, 212, 0
                             );
                            break;
                        case 4:
                            string cardHolders = string.Format("{0:n0}", (libraryData.RegAdults + libraryData.RegChildren));
                            if (cardHolders == "0")
                                cardHolders = "N/A";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(cardHolders, new Font(bf, 14, 0, BaseColor.WHITE)),
                             554, 315, 0);
                            string cardHoldersText = " card";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(cardHoldersText, new Font(bf, 12, 0, TextColor)),
                             605, 315, 0
                             );
                            cardHoldersText = " holders";
                            ColumnText.ShowTextAligned(
                             canvas,
                             Element.ALIGN_LEFT,
                             new Phrase(cardHoldersText, new Font(bf, 12, 0, TextColor)),
                             560, 302, 0
                             );
                            break;

                    }

                }

                ColumnText ct = new ColumnText(canvas);

                if (libraryData.LibraryName.Length > 0 && libraryData.LibraryName.Length <= 20)
                {
                    ct.SetSimpleColumn(new Phrase(new Chunk(libraryData.LibraryName, new Font(bf, 40, 0, HeaderColor))),
                                  40, 940, 700, 980, 10, Element.ALIGN_CENTER | Element.ALIGN_MIDDLE);
                }
                else if (libraryData.LibraryName.Length > 0 && libraryData.LibraryName.Length <= 35)
                {
                    ct.SetSimpleColumn(new Phrase(new Chunk(libraryData.LibraryName, new Font(bf, 35, 0, HeaderColor))),
                                  40, 740, 700, 900, 10, Element.ALIGN_CENTER | Element.ALIGN_MIDDLE);
                }
                else if (libraryData.LibraryName.Length > 35 && libraryData.LibraryName.Length <= 60)
                {
                    ct.SetSimpleColumn(new Phrase(new Chunk(libraryData.LibraryName, new Font(bf, 30, 0, HeaderColor))),
                                  40, 940, 700, 980, 10, Element.ALIGN_CENTER | Element.ALIGN_MIDDLE);
                }
                else
                {
                    ct.SetSimpleColumn(new Phrase(new Chunk(libraryData.LibraryName, new Font(bf, 30, 0, HeaderColor))),
                                40, 940, 700, 980, 10, Element.ALIGN_CENTER | Element.ALIGN_MIDDLE);
                }

                ct.Go();


            }
            stamper.Close();
            byte[] pdfByteArray = System.IO.File.ReadAllBytes(newFile);
            string base64EncodedPDF = System.Convert.ToBase64String(pdfByteArray);
            return base64EncodedPDF;

        }

        public string GeneratePdf(string libraryId, int year, int customColumnIdField1, int customColumnIdField2, int customColumnIdField3, int customColumnIdField4, int customColumnIdField5, int customColumnIdField6)
        {
            string pdfHtml = "";

            var customTemplateDetailsField1 = customTemplateService.GetCustomTemplateById(customColumnIdField1);
            var customTemplateDetailsField2 = customTemplateService.GetCustomTemplateById(customColumnIdField2);
            var customTemplateDetailsField3 = customTemplateService.GetCustomTemplateById(customColumnIdField3);
            var customTemplateDetailsField4 = customTemplateService.GetCustomTemplateById(customColumnIdField4);
            var customTemplateDetailsField5 = customTemplateService.GetCustomTemplateById(customColumnIdField5);
            var customTemplateDetailsField6 = customTemplateService.GetCustomTemplateById(customColumnIdField6);

            var libraryData = libraryDataService.GetLibraryDataByIdAndYear(libraryId, year);
            if (libraryData != null)
            {


                pdfHtml += "<table width=\"100%\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">";
                pdfHtml += "<tr>";
                if (libraryData.LibraryName.Length > 0 && libraryData.LibraryName.Length <= 20)
                {
                    pdfHtml += "<td align=\"center\" valign=\"top\" style=\"color:#302F41; font-family:Arial, Helvetica-Bold, sans-serif; font-size:40px; font-weight:bold; padding:0px;\">" + libraryData.LibraryName + "</td>";
                }
                else if (libraryData.LibraryName.Length > 0 && libraryData.LibraryName.Length <= 35)
                {
                    pdfHtml += "<td align=\"center\" valign=\"top\" style=\"color:#302F41; font-family:Arial, Helvetica-Bold, sans-serif; font-size:35px; font-weight:bold; padding:0px;\">" + libraryData.LibraryName + "</td>";
                }
                else if (libraryData.LibraryName.Length > 35 && libraryData.LibraryName.Length <= 60)
                {
                    pdfHtml += "<td align=\"center\" valign=\"top\" style=\"color:#302F41; font-family:Arial, Helvetica-Bold, sans-serif; font-size:25px; font-weight:bold; padding:0px;\">" + libraryData.LibraryName + "</td>";
                }
                else
                {
                    pdfHtml += "<td align=\"center\" valign=\"top\" style=\"color:#302F41; font-family:Arial, Helvetica-Bold, sans-serif; font-size:30px; font-weight:bold; padding:0px;\">" + libraryData.LibraryName + "</td>";
                }

                pdfHtml += "</tr>";

                pdfHtml += "<tr>";
                pdfHtml += "<td align=\"center\" valign=\"top\" style=\"height:1050px;\">";
                pdfHtml += "<table width=\"100%\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">";
                pdfHtml += "<tr>";

                pdfHtml += "<td align=\"center\" valign=\"top\" width=\"50%\">";
                if (customTemplateDetailsField1 != null)
                {
                    if (customTemplateDetailsField1.DynamicTextPosition == true && customTemplateDetailsField1.CustomColumnId == 4)
                    {
                        pdfHtml += "<label style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:22px; padding:10px; font-weight:normal; text-align:center; position: absolute;line-height:22px; margin: 82px 0 0 190px; \">" + customTemplateDetailsField1.ColumnText + "</label>";
                    }
                    else if (customTemplateDetailsField1.DynamicTextPosition == true && customTemplateDetailsField1.CustomColumnId == 12)
                    {
                        pdfHtml += "<label style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:22px; padding:10px; font-weight:normal; text-align:center; position: absolute;line-height:22px; margin: 42px 0 0 112px; \">" + customTemplateDetailsField1.ColumnText + "</label>";
                    }
                    else if (customTemplateDetailsField1.DynamicTextPosition == true && customTemplateDetailsField1.CustomColumnId == 14)
                    {
                        pdfHtml += "<label style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:22px; padding:10px; font-weight:normal; text-align:center; position: absolute;line-height:22px; margin: 80px 0 0 150px; \">" + customTemplateDetailsField1.ColumnText + "</label>";
                    }

                    pdfHtml += "<img style=\"padding:10px;height:32%; width:auto;\" src=\"" + customTemplatesDomainName + customTemplateDetailsField1.FileLocation + "\"/>";
                }
                pdfHtml += "</td>";
                pdfHtml += "<td align=\"center\" valign=\"top\" width=\"50%\">";
                if (customTemplateDetailsField1 != null && customTemplateDetailsField1.ColumnText != null && customTemplateDetailsField1.DynamicTextPosition == true)
                {
                    string columnValue = "";
                    string columnValueTwo = "";
                    if (customTemplateDetailsField1.CustomColumnId == 4)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.RegAdults + libraryData.RegChildren));
                    }
                    else if (customTemplateDetailsField1.CustomColumnId == 5)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.CentralLibraries + libraryData.Branches));
                    }
                    else if (customTemplateDetailsField1.CustomColumnId == 26)
                    {

                        columnValue = string.Format("{0:n0}", (libraryData.AnnualAttendanceLibrary));
                    }
                    else if (customTemplateDetailsField1.CustomColumnId == 28)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.ChildrenPrograms));
                        columnValueTwo = string.Format("{0:n0}", (libraryData.YoungAdultPrograms));
                    }
                    else {
                        string mappingValue = customTemplateDetailsField1.MappingColumn;

                        foreach (PropertyInfo info in libraryData.GetType().GetProperties())
                        {
                            if (info.CanRead && info.Name == mappingValue)
                            {
                                columnValue = string.Format("{0:n0}", (info.GetValue(libraryData, null)));

                                break;
                            }
                        }
                    }
                    Regex re = new Regex(@"\[(.*?)\]");
                    MatchCollection mc = re.Matches(pdfHtml);
                    foreach (Match m in mc)
                    {
                        switch (m.Value)
                        {
                            case "[DynamicText]":
                                pdfHtml = pdfHtml.Replace(m.Value, columnValue);
                                break;
                            case "[DynamicTextTwo]":
                                pdfHtml = pdfHtml.Replace(m.Value, columnValueTwo);
                                break;

                            default:
                                break;
                        }
                    }
                }
                if (customTemplateDetailsField2 != null)
                {
                    if (customTemplateDetailsField2.DynamicTextPosition == true && customTemplateDetailsField2.CustomColumnId == 4)
                    {
                        pdfHtml += "<label style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:22px; padding:10px; font-weight:normal; text-align:center; position: absolute;line-height:22px; margin: 82px 0 0 190px;\">" + customTemplateDetailsField2.ColumnText + "</label>";
                    }
                    else if (customTemplateDetailsField2.DynamicTextPosition == true && customTemplateDetailsField2.CustomColumnId == 12)
                    {
                        pdfHtml += "<label style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:22px; padding:10px; font-weight:normal; text-align:center; position: absolute;line-height:22px; margin: 42px 0 0 112px;\">" + customTemplateDetailsField2.ColumnText + "</label>";
                    }
                    else if (customTemplateDetailsField2.DynamicTextPosition == true && customTemplateDetailsField2.CustomColumnId == 14)
                    {
                        pdfHtml += "<label style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:22px; padding:10px; font-weight:normal; text-align:center; position: absolute; line-height:22px; margin: 80px 0 0 150px;\">" + customTemplateDetailsField2.ColumnText + "</label>";
                    }
                    pdfHtml += "<img style=\"padding:10px;height:32%; width:auto;\" src=\"" + customTemplatesDomainName + customTemplateDetailsField2.FileLocation + "\"/>";
                }

                if (customTemplateDetailsField2 != null && customTemplateDetailsField2.ColumnText != null && customTemplateDetailsField2.DynamicTextPosition == true)
                {
                    string columnValue = "";
                    string columnValueTwo = "";
                    if (customTemplateDetailsField2.CustomColumnId == 4)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.RegAdults + libraryData.RegChildren));
                    }
                    else if (customTemplateDetailsField2.CustomColumnId == 5)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.CentralLibraries + libraryData.Branches));
                    }
                    else if (customTemplateDetailsField2.CustomColumnId == 26)
                    {

                        columnValue = string.Format("{0:n0}", (libraryData.AnnualAttendanceLibrary));
                    }
                    else if (customTemplateDetailsField2.CustomColumnId == 28)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.ChildrenPrograms));
                        columnValueTwo = string.Format("{0:n0}", (libraryData.YoungAdultPrograms));
                    }
                    else {
                        string mappingValue = customTemplateDetailsField2.MappingColumn;

                        foreach (PropertyInfo info in libraryData.GetType().GetProperties())
                        {
                            if (info.CanRead && info.Name == mappingValue)
                            {
                                columnValue = string.Format("{0:n0}", (info.GetValue(libraryData, null)));

                                break;
                            }
                        }
                    }
                    Regex re = new Regex(@"\[(.*?)\]");
                    MatchCollection mc = re.Matches(pdfHtml);
                    foreach (Match m in mc)
                    {
                        switch (m.Value)
                        {
                            case "[DynamicText]":
                                pdfHtml = pdfHtml.Replace(m.Value, columnValue);
                                break;
                            case "[DynamicTextTwo]":
                                pdfHtml = pdfHtml.Replace(m.Value, columnValueTwo);
                                break;


                            default:
                                break;
                        }
                    }
                }
                pdfHtml += "</td>";
                pdfHtml += "</tr>";
                pdfHtml += "<tr>";
                if (customTemplateDetailsField1 != null && customTemplateDetailsField1.DynamicTextPosition == false)
                {
                    pdfHtml += "<td align=\"center\" valign=\"top\" width=\"50%\" style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:33px; padding: 10px; font-weight:normal; \"><span style =\"color:#0E5959\">" + customTemplateDetailsField1.ColumnText + "</span></td>";
                }
                else
                {
                    pdfHtml += "<td align=\"center\" valign=\"top\" width=\"50%\" style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:33px; padding: 10px; font-weight:normal; \"></td>";
                }
                if (customTemplateDetailsField1 != null && customTemplateDetailsField1.ColumnText != null && customTemplateDetailsField1.DynamicTextPosition == false)
                {
                    string columnValue = "";
                    string columnValueTwo = "";
                    if (customTemplateDetailsField1.CustomColumnId == 4)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.RegAdults + libraryData.RegChildren));
                    }
                    else if (customTemplateDetailsField1.CustomColumnId == 5)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.CentralLibraries + libraryData.Branches));
                    }
                    else if (customTemplateDetailsField1.CustomColumnId == 26)
                    {

                        columnValue = string.Format("{0:n0}", (libraryData.AnnualAttendanceLibrary));
                    }
                    else if (customTemplateDetailsField1.CustomColumnId == 28)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.ChildrenPrograms));
                        columnValueTwo = string.Format("{0:n0}", (libraryData.YoungAdultPrograms));
                    }
                    else {
                        string mappingValue = customTemplateDetailsField1.MappingColumn;

                        foreach (PropertyInfo info in libraryData.GetType().GetProperties())
                        {
                            if (info.CanRead && info.Name == mappingValue)
                            {
                                columnValue = string.Format("{0:n0}", (info.GetValue(libraryData, null)));

                                break;
                            }
                        }
                    }
                    Regex re = new Regex(@"\[(.*?)\]");
                    MatchCollection mc = re.Matches(pdfHtml);
                    foreach (Match m in mc)
                    {
                        switch (m.Value)
                        {
                            case "[DynamicText]":
                                pdfHtml = pdfHtml.Replace(m.Value, columnValue);
                                break;
                            case "[DynamicTextTwo]":
                                pdfHtml = pdfHtml.Replace(m.Value, columnValueTwo);
                                break;


                            default:
                                break;
                        }
                    }
                }
                if (customTemplateDetailsField2 != null && customTemplateDetailsField2.DynamicTextPosition == false)
                    pdfHtml += "<td align=\"center\" valign=\"top\" width=\"50%\" style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:33px; padding: 10px; font-weight:normal; \"><span style =\"color:#0E5959\">" + customTemplateDetailsField2.ColumnText + "</span></td>";
                else
                    pdfHtml += "<td align=\"center\" valign=\"top\" width=\"50%\" style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:33px; padding: 10px; font-weight:normal; \"></td>";
                if (customTemplateDetailsField2 != null && customTemplateDetailsField2.ColumnText != null && customTemplateDetailsField2.DynamicTextPosition == false)
                {
                    string columnValue = "";
                    string columnValueTwo = "";
                    if (customTemplateDetailsField2.CustomColumnId == 4)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.RegAdults + libraryData.RegChildren));
                    }
                    else if (customTemplateDetailsField2.CustomColumnId == 5)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.CentralLibraries + libraryData.Branches));
                    }
                    else if (customTemplateDetailsField2.CustomColumnId == 26)
                    {

                        columnValue = string.Format("{0:n0}", (libraryData.AnnualAttendanceLibrary));
                    }
                    else if (customTemplateDetailsField2.CustomColumnId == 28)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.ChildrenPrograms));
                        columnValueTwo = string.Format("{0:n0}", (libraryData.YoungAdultPrograms));
                    }
                    else {
                        string mappingValue = customTemplateDetailsField2.MappingColumn;

                        foreach (PropertyInfo info in libraryData.GetType().GetProperties())
                        {
                            if (info.CanRead && info.Name == mappingValue)
                            {
                                columnValue = string.Format("{0:n0}", (info.GetValue(libraryData, null)));
                                break;
                            }
                        }
                    }
                    Regex re = new Regex(@"\[(.*?)\]");
                    MatchCollection mc = re.Matches(pdfHtml);
                    foreach (Match m in mc)
                    {
                        switch (m.Value)
                        {
                            case "[DynamicText]":
                                pdfHtml = pdfHtml.Replace(m.Value, columnValue);
                                break;
                            case "[DynamicTextTwo]":
                                pdfHtml = pdfHtml.Replace(m.Value, columnValueTwo);
                                break;


                            default:
                                break;
                        }
                    }
                }
                pdfHtml += "</tr>";
                pdfHtml += "<tr>";
                pdfHtml += "<td align=\"center\" valign=\"top\" width=\"50%\">";
                if (customTemplateDetailsField3 != null)
                {
                    if (customTemplateDetailsField3.DynamicTextPosition == true && customTemplateDetailsField3.CustomColumnId == 4)
                    {
                        pdfHtml += "<label style=\"font-family:Arial, Helvetica-Bold, sans-serif;font-size:22px; padding:10px; font-weight:normal; text-align:center; position: absolute;line-height:22px; margin: 82px 0 0 190px; \">" + customTemplateDetailsField3.ColumnText + "</label>";
                    }
                    else if (customTemplateDetailsField3.DynamicTextPosition == true && customTemplateDetailsField3.CustomColumnId == 12)
                    {
                        pdfHtml += "<label style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:22px; padding:10px; font-weight:normal; text-align:center; position: absolute;line-height:22px; margin: 42px 0 0 112px; \">" + customTemplateDetailsField3.ColumnText + "</label>";
                    }
                    else if (customTemplateDetailsField3.DynamicTextPosition == true && customTemplateDetailsField3.CustomColumnId == 14)
                    {
                        pdfHtml += "<label style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:22px; padding:10px; font-weight:normal; text-align:center; position: absolute; line-height:22px; margin: 80px 0 0 150px;  \">" + customTemplateDetailsField3.ColumnText + "</label>";
                    }

                    pdfHtml += "<img style=\"padding:10px;height:32%; width:auto;\" src=\"" + customTemplatesDomainName + customTemplateDetailsField3.FileLocation + "\"/>";
                }
                pdfHtml += "</td>";
                if (customTemplateDetailsField3 != null && customTemplateDetailsField3.ColumnText != null && customTemplateDetailsField3.DynamicTextPosition == true)
                {
                    string columnValue = "";
                    string columnValueTwo = "";
                    if (customTemplateDetailsField3.CustomColumnId == 4)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.RegAdults + libraryData.RegChildren));
                    }
                    else if (customTemplateDetailsField3.CustomColumnId == 5)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.CentralLibraries + libraryData.Branches));
                    }
                    else if (customTemplateDetailsField3.CustomColumnId == 26)
                    {

                        columnValue = string.Format("{0:n0}", (libraryData.AnnualAttendanceLibrary));
                    }
                    else if (customTemplateDetailsField3.CustomColumnId == 28)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.ChildrenPrograms));
                        columnValueTwo = string.Format("{0:n0}", (libraryData.YoungAdultPrograms));
                    }
                    else {
                        string mappingValue = customTemplateDetailsField3.MappingColumn;

                        foreach (PropertyInfo info in libraryData.GetType().GetProperties())
                        {
                            if (info.CanRead && info.Name == mappingValue)
                            {
                                columnValue = string.Format("{0:n0}", (info.GetValue(libraryData, null)));

                                break;
                            }
                        }
                    }
                    Regex re = new Regex(@"\[(.*?)\]");
                    MatchCollection mc = re.Matches(pdfHtml);
                    foreach (Match m in mc)
                    {
                        switch (m.Value)
                        {
                            case "[DynamicText]":
                                pdfHtml = pdfHtml.Replace(m.Value, columnValue);
                                break;
                            case "[DynamicTextTwo]":
                                pdfHtml = pdfHtml.Replace(m.Value, columnValueTwo);
                                break;


                            default:
                                break;
                        }
                    }
                }
                pdfHtml += "<td align=\"center\" valign=\"top\" width=\"50%\">";
                if (customTemplateDetailsField4 != null)
                {
                    if (customTemplateDetailsField4.DynamicTextPosition == true && customTemplateDetailsField4.CustomColumnId == 4)
                    {
                        pdfHtml += "<label style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:22px; padding:10px; font-weight:normal; text-align:center; position: absolute;line-height:22px; margin: 82px 0 0 190px; \">" + customTemplateDetailsField4.ColumnText + "</label>";
                    }
                    else if (customTemplateDetailsField4.DynamicTextPosition == true && customTemplateDetailsField4.CustomColumnId == 12)
                    {
                        pdfHtml += "<label style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:22px; padding:10px; font-weight:normal; text-align:center; position: absolute;line-height:22px; margin: 42px 0 0 112px; \">" + customTemplateDetailsField4.ColumnText + "</label>";
                    }
                    else if (customTemplateDetailsField4.DynamicTextPosition == true && customTemplateDetailsField4.CustomColumnId == 14)
                    {
                        pdfHtml += "<label style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:22px; padding:10px; font-weight:normal; text-align:center; position: absolute;  line-height:22px; margin: 80px 0 0 150px; \">" + customTemplateDetailsField4.ColumnText + "</label>";
                    }
                    pdfHtml += "<img style=\"padding:10px;height:32%; width:auto;\" src=\"" + customTemplatesDomainName + customTemplateDetailsField4.FileLocation + "\"/>";
                }
                if (customTemplateDetailsField4 != null && customTemplateDetailsField4.ColumnText != null && customTemplateDetailsField4.DynamicTextPosition == true)
                {
                    string columnValue = "";
                    string columnValueTwo = "";
                    if (customTemplateDetailsField4.CustomColumnId == 4)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.RegAdults + libraryData.RegChildren));
                    }
                    else if (customTemplateDetailsField4.CustomColumnId == 5)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.CentralLibraries + libraryData.Branches));
                    }
                    else if (customTemplateDetailsField4.CustomColumnId == 26)
                    {

                        columnValue = string.Format("{0:n0}", (libraryData.AnnualAttendanceLibrary));
                    }
                    else if (customTemplateDetailsField4.CustomColumnId == 28)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.ChildrenPrograms));
                        columnValueTwo = string.Format("{0:n0}", (libraryData.YoungAdultPrograms));
                    }
                    else {
                        string mappingValue = customTemplateDetailsField4.MappingColumn;

                        foreach (PropertyInfo info in libraryData.GetType().GetProperties())
                        {
                            if (info.CanRead && info.Name == mappingValue)
                            {
                                columnValue = string.Format("{0:n0}", (info.GetValue(libraryData, null)));

                                break;
                            }
                        }
                    }
                    Regex re = new Regex(@"\[(.*?)\]");
                    MatchCollection mc = re.Matches(pdfHtml);
                    foreach (Match m in mc)
                    {
                        switch (m.Value)
                        {
                            case "[DynamicText]":
                                pdfHtml = pdfHtml.Replace(m.Value, columnValue);
                                break;
                            case "[DynamicTextTwo]":
                                pdfHtml = pdfHtml.Replace(m.Value, columnValueTwo);
                                break;


                            default:
                                break;
                        }
                    }
                }
                pdfHtml += "</td>";
                pdfHtml += "</tr>";
                pdfHtml += "<tr>";
                if (customTemplateDetailsField3 != null && customTemplateDetailsField3.DynamicTextPosition == false)
                    pdfHtml += "<td align=\"center\" valign=\"top\" width=\"50%\" style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:33px; padding: 10px; font-weight:normal; \"><span style =\"color:#0E5959\">" + customTemplateDetailsField3.ColumnText + " </span></td>";
                else
                    pdfHtml += "<td align=\"center\" valign=\"top\" width=\"50%\" style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:33px; padding: 10px; font-weight:normal; \"></td>";
                if (customTemplateDetailsField3 != null && customTemplateDetailsField3.ColumnText != null && customTemplateDetailsField3.DynamicTextPosition == false)
                {

                    string columnValue = "";
                    string columnValueTwo = "";
                    if (customTemplateDetailsField3.CustomColumnId == 4)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.RegAdults + libraryData.RegChildren));
                    }
                    else if (customTemplateDetailsField3.CustomColumnId == 5)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.CentralLibraries + libraryData.Branches));
                    }
                    else if (customTemplateDetailsField3.CustomColumnId == 26)
                    {

                        columnValue = string.Format("{0:n0}", (libraryData.AnnualAttendanceLibrary));
                    }
                    else if (customTemplateDetailsField3.CustomColumnId == 28)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.ChildrenPrograms));
                        columnValueTwo = string.Format("{0:n0}", (libraryData.YoungAdultPrograms));
                    }
                    else {
                        string mappingValue = customTemplateDetailsField3.MappingColumn;

                        foreach (PropertyInfo info in libraryData.GetType().GetProperties())
                        {
                            if (info.CanRead && info.Name == mappingValue)
                            {
                                columnValue = string.Format("{0:n0}", (info.GetValue(libraryData, null)));
                                break;
                            }
                        }
                    }
                    Regex re = new Regex(@"\[(.*?)\]");
                    MatchCollection mc = re.Matches(pdfHtml);
                    foreach (Match m in mc)
                    {
                        switch (m.Value)
                        {
                            case "[DynamicText]":
                                pdfHtml = pdfHtml.Replace(m.Value, columnValue);
                                break;
                            case "[DynamicTextTwo]":
                                pdfHtml = pdfHtml.Replace(m.Value, columnValueTwo);
                                break;


                            default:
                                break;
                        }
                    }
                }
                if (customTemplateDetailsField4 != null && customTemplateDetailsField4.DynamicTextPosition == false)
                {
                    pdfHtml += "<td align=\"center\" valign=\"top\" width=\"50%\" style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:33px; padding: 10px; font-weight:normal; \"><span style =\"color:#0E5959\">" + customTemplateDetailsField4.ColumnText + "</span></td>";
                }
                else
                    pdfHtml += "<td align=\"center\" valign=\"top\" width=\"50%\" style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:33px; padding: 10px; font-weight:normal; \"></td>";
                if (customTemplateDetailsField4 != null && customTemplateDetailsField4.ColumnText != null && customTemplateDetailsField4.DynamicTextPosition == false)
                {
                    string columnValue = "";
                    string columnValueTwo = "";
                    if (customTemplateDetailsField4.CustomColumnId == 4)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.RegAdults + libraryData.RegChildren));
                    }
                    else if (customTemplateDetailsField4.CustomColumnId == 5)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.CentralLibraries + libraryData.Branches));
                    }
                    else if (customTemplateDetailsField4.CustomColumnId == 26)
                    {

                        columnValue = string.Format("{0:n0}", (libraryData.AnnualAttendanceLibrary));
                    }
                    else if (customTemplateDetailsField4.CustomColumnId == 28)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.ChildrenPrograms));
                        columnValueTwo = string.Format("{0:n0}", (libraryData.YoungAdultPrograms));
                    }
                    else {
                        string mappingValue = customTemplateDetailsField4.MappingColumn;

                        foreach (PropertyInfo info in libraryData.GetType().GetProperties())
                        {
                            if (info.CanRead && info.Name == mappingValue)
                            {
                                columnValue = string.Format("{0:n0}", (info.GetValue(libraryData, null)));
                                break;
                            }
                        }
                    }
                    Regex re = new Regex(@"\[(.*?)\]");
                    MatchCollection mc = re.Matches(pdfHtml);
                    foreach (Match m in mc)
                    {
                        switch (m.Value)
                        {
                            case "[DynamicText]":
                                pdfHtml = pdfHtml.Replace(m.Value, columnValue);
                                break;
                            case "[DynamicTextTwo]":
                                pdfHtml = pdfHtml.Replace(m.Value, columnValueTwo);
                                break;

                            default:
                                break;
                        }
                    }
                }
                pdfHtml += "</tr>";
                pdfHtml += "<tr>";
                pdfHtml += "<td align=\"center\" valign=\"top\" width=\"50%\">";
                if (customTemplateDetailsField5 != null)
                {
                    if (customTemplateDetailsField5.DynamicTextPosition == true && customTemplateDetailsField5.CustomColumnId == 4)
                    {
                        pdfHtml += "<label style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:22px; padding:10px; font-weight:normal; text-align:center; position: absolute;line-height:22px; margin: 82px 0 0 190px; \">" + customTemplateDetailsField5.ColumnText + "</label>";
                    }
                    else if (customTemplateDetailsField5.DynamicTextPosition == true && customTemplateDetailsField5.CustomColumnId == 12)
                    {
                        pdfHtml += "<label style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:22px; padding:10px; font-weight:normal; text-align:center; position: absolute;line-height:22px; margin: 42px 0 0 112px; \">" + customTemplateDetailsField5.ColumnText + "</label>";
                    }
                    else if (customTemplateDetailsField5.DynamicTextPosition == true && customTemplateDetailsField5.CustomColumnId == 14)
                    {
                        pdfHtml += "<label style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:22px; padding:10px; font-weight:normal; text-align:center; position: absolute; line-height:22px; margin: 80px 0 0 150px; \">" + customTemplateDetailsField5.ColumnText + "</label>";
                    }

                    pdfHtml += "<img style=\"padding:10px;height:32%; width:auto;\" src=\"" + customTemplatesDomainName + customTemplateDetailsField5.FileLocation + "\"/>";
                }
                pdfHtml += "</td>";
                if (customTemplateDetailsField5 != null && customTemplateDetailsField5.ColumnText != null && customTemplateDetailsField5.DynamicTextPosition == true)
                {
                    string columnValue = "";
                    string columnValueTwo = "";
                    if (customTemplateDetailsField5.CustomColumnId == 4)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.RegAdults + libraryData.RegChildren));
                    }
                    else if (customTemplateDetailsField5.CustomColumnId == 5)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.CentralLibraries + libraryData.Branches));
                    }
                    else if (customTemplateDetailsField5.CustomColumnId == 26)
                    {

                        columnValue = string.Format("{0:n0}", (libraryData.AnnualAttendanceLibrary));
                    }
                    else if (customTemplateDetailsField5.CustomColumnId == 28)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.ChildrenPrograms));
                        columnValueTwo = string.Format("{0:n0}", (libraryData.YoungAdultPrograms));
                    }
                    else {
                        string mappingValue = customTemplateDetailsField5.MappingColumn;

                        foreach (PropertyInfo info in libraryData.GetType().GetProperties())
                        {
                            if (info.CanRead && info.Name == mappingValue)
                            {
                                columnValue = string.Format("{0:n0}", (info.GetValue(libraryData, null)));

                                break;
                            }
                        }
                    }
                    Regex re = new Regex(@"\[(.*?)\]");
                    MatchCollection mc = re.Matches(pdfHtml);
                    foreach (Match m in mc)
                    {
                        switch (m.Value)
                        {
                            case "[DynamicText]":
                                pdfHtml = pdfHtml.Replace(m.Value, columnValue);
                                break;
                            case "[DynamicTextTwo]":
                                pdfHtml = pdfHtml.Replace(m.Value, columnValueTwo);
                                break;


                            default:
                                break;
                        }
                    }
                }
                pdfHtml += "<td align=\"center\" valign=\"top\" width=\"50%\">&nbsp;";
                if (customTemplateDetailsField6 != null)
                {
                    if (customTemplateDetailsField6.DynamicTextPosition == true && customTemplateDetailsField6.CustomColumnId == 4)
                    {
                        pdfHtml += "<label style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:22px; padding:10px; font-weight:normal; text-align:center; position: absolute;line-height:22px; margin: 82px 0 0 190px;\">" + customTemplateDetailsField6.ColumnText + "</label>";
                    }
                    else if (customTemplateDetailsField6.DynamicTextPosition == true && customTemplateDetailsField6.CustomColumnId == 12)
                    {
                        pdfHtml += "<label style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:22px; padding:10px; font-weight:normal; text-align:center; position: absolute;line-height:22px; margin: 42px 0 0 38px; \">" + customTemplateDetailsField6.ColumnText + "</label>";
                    }
                    else if (customTemplateDetailsField6.DynamicTextPosition == true && customTemplateDetailsField6.CustomColumnId == 14)
                    {
                        pdfHtml += "<label style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:22px; padding:10px; font-weight:normal; text-align:center; position: absolute; line-height:22px; margin: 78px 0 0 70px;\">" + customTemplateDetailsField6.ColumnText + "</label>";
                    }
                    pdfHtml += "<img style=\"padding:10px;height:32%; width:auto;\" src=\"" + customTemplatesDomainName + customTemplateDetailsField6.FileLocation + "\"/>";
                }
                if (customTemplateDetailsField6 != null && customTemplateDetailsField6.ColumnText != null && customTemplateDetailsField6.DynamicTextPosition == true)
                {
                    string columnValue = "";
                    string columnValueTwo = "";
                    if (customTemplateDetailsField6.CustomColumnId == 4)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.RegAdults + libraryData.RegChildren));
                    }
                    else if (customTemplateDetailsField6.CustomColumnId == 5)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.CentralLibraries + libraryData.Branches));
                    }
                    else if (customTemplateDetailsField6.CustomColumnId == 26)
                    {

                        columnValue = string.Format("{0:n0}", (libraryData.AnnualAttendanceLibrary));
                    }
                    else if (customTemplateDetailsField6.CustomColumnId == 28)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.ChildrenPrograms));
                        columnValueTwo = string.Format("{0:n0}", (libraryData.YoungAdultPrograms));
                    }
                    else {
                        string mappingValue = customTemplateDetailsField6.MappingColumn;

                        foreach (PropertyInfo info in libraryData.GetType().GetProperties())
                        {
                            if (info.CanRead && info.Name == mappingValue)
                            {
                                columnValue = string.Format("{0:n0}", (info.GetValue(libraryData, null)));

                                break;
                            }
                        }
                    }
                    Regex re = new Regex(@"\[(.*?)\]");
                    MatchCollection mc = re.Matches(pdfHtml);
                    foreach (Match m in mc)
                    {
                        switch (m.Value)
                        {
                            case "[DynamicText]":
                                pdfHtml = pdfHtml.Replace(m.Value, columnValue);
                                break;
                            case "[DynamicTextTwo]":
                                pdfHtml = pdfHtml.Replace(m.Value, columnValueTwo);
                                break;


                            default:
                                break;
                        }
                    }
                }
                pdfHtml += "</td>";
                pdfHtml += "</tr>";
                pdfHtml += "<tr>";
                if (customTemplateDetailsField5 != null && customTemplateDetailsField5.DynamicTextPosition == false)
                {
                    pdfHtml += "<td align=\"center\" valign=\"top\" width=\"50%\" style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:33px; padding: 10px; font-weight:normal; \"><span style =\"color:#0E5959\">" + customTemplateDetailsField5.ColumnText + "</span></td>";
                }
                else
                    pdfHtml += "<td align=\"center\" valign=\"top\" width=\"50%\" style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:33px; padding: 10px; font-weight:normal; \"></td>";
                if (customTemplateDetailsField5 != null && customTemplateDetailsField5.ColumnText != null && customTemplateDetailsField5.DynamicTextPosition == false)
                {
                    string columnValue = "";
                    string columnValueTwo = "";
                    if (customTemplateDetailsField5.CustomColumnId == 4)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.RegAdults + libraryData.RegChildren));
                    }
                    else if (customTemplateDetailsField5.CustomColumnId == 5)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.CentralLibraries + libraryData.Branches));
                    }
                    else if (customTemplateDetailsField5.CustomColumnId == 26)
                    {

                        columnValue = string.Format("{0:n0}", (libraryData.AnnualAttendanceLibrary));
                    }
                    else if (customTemplateDetailsField5.CustomColumnId == 28)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.ChildrenPrograms));
                        columnValueTwo = string.Format("{0:n0}", (libraryData.YoungAdultPrograms));
                    }
                    else {
                        string mappingValue = customTemplateDetailsField5.MappingColumn;

                        foreach (PropertyInfo info in libraryData.GetType().GetProperties())
                        {
                            if (info.CanRead && info.Name == mappingValue)
                            {
                                columnValue = string.Format("{0:n0}", (info.GetValue(libraryData, null)));
                                break;
                            }
                        }
                    }
                    Regex re = new Regex(@"\[(.*?)\]");
                    MatchCollection mc = re.Matches(pdfHtml);
                    foreach (Match m in mc)
                    {
                        switch (m.Value)
                        {
                            case "[DynamicText]":
                                pdfHtml = pdfHtml.Replace(m.Value, columnValue);
                                break;
                            case "[DynamicTextTwo]":
                                pdfHtml = pdfHtml.Replace(m.Value, columnValueTwo);
                                break;


                            default:
                                break;
                        }
                    }
                }
                if (customTemplateDetailsField6 != null && customTemplateDetailsField6.DynamicTextPosition == false && customTemplateDetailsField6.DynamicTextPosition == false)
                {
                    pdfHtml += "<td align=\"center\" valign=\"top\" width=\"50%\" style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:33px; padding: 10px; font-weight:normal; \"><span style =\"color:#0E5959\">" + customTemplateDetailsField6.ColumnText + "</span></td>";
                }
                else
                    pdfHtml += "<td align=\"center\" valign=\"top\" width=\"50%\" style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:33px; padding: 10px; font-weight:normal; \"></td>";
                if (customTemplateDetailsField6 != null && customTemplateDetailsField6.ColumnText != null)
                {
                    string columnValue = "";
                    string columnValueTwo = "";
                    if (customTemplateDetailsField6.CustomColumnId == 4)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.RegAdults + libraryData.RegChildren));
                    }
                    else if (customTemplateDetailsField6.CustomColumnId == 5)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.CentralLibraries + libraryData.Branches));
                    }
                    else if (customTemplateDetailsField6.CustomColumnId == 26)
                    {

                        columnValue = string.Format("{0:n0}", (libraryData.AnnualAttendanceLibrary));
                    }
                    else if (customTemplateDetailsField6.CustomColumnId == 28)
                    {
                        columnValue = string.Format("{0:n0}", (libraryData.ChildrenPrograms));
                        columnValueTwo = string.Format("{0:n0}", (libraryData.YoungAdultPrograms));
                    }
                    else {
                        string mappingValue = customTemplateDetailsField6.MappingColumn;

                        foreach (PropertyInfo info in libraryData.GetType().GetProperties())
                        {
                            if (info.CanRead && info.Name == mappingValue)
                            {
                                columnValue = string.Format("{0:n0}", (info.GetValue(libraryData, null)));
                                break;
                            }
                        }
                    }
                    Regex re = new Regex(@"\[(.*?)\]");
                    MatchCollection mc = re.Matches(pdfHtml);
                    foreach (Match m in mc)
                    {
                        switch (m.Value)
                        {
                            case "[DynamicText]":
                                pdfHtml = pdfHtml.Replace(m.Value, columnValue);
                                break;
                            case "[DynamicTextTwo]":
                                pdfHtml = pdfHtml.Replace(m.Value, columnValueTwo);
                                break;


                            default:
                                break;
                        }
                    }
                }
                pdfHtml += "</tr>";

                pdfHtml += "</table>";
                pdfHtml += "</td>";
                pdfHtml += "</tr>";
                pdfHtml += "<tr><td align=\"center\" valign=\"top\" width=\"50%\" style=\"font-family:Arial, Helvetica-Bold, sans-serif; font-size:16px; padding:10px; font-weight:normal;\">";
                pdfHtml += "<span>This data is drawn from the " + year + " Public Library Statistics survey</span>";
                pdfHtml += "</td></tr>";
                pdfHtml += "<tr>";

                pdfHtml += "<td align=\"center\" valign=\"bottom\" style=\"padding: 0px;\"><img style=\"width:100%;\" src=\"" + domainName + "Content/images/footer.png\" /></td>";

                pdfHtml += "</tr>";
                pdfHtml += "</table>";

                return pdfHtml;
            }

            return pdfHtml;
        }
        #endregion
    }
}