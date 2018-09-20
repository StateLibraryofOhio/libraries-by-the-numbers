using StateOfOhioLibrary.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StateOfOhioLibrary.Models
{
    public class DataModel
    {
        public DataModel()
        {
            YearList = new List<SelectListItem>();
        }
        public int TotalNoOfData { get; set; }
        public PagedList<DataUploadHistoryModel> DataList { get; set; }

        public string Status { get; set; }
        public int DataYear { get; set; }
        public string Name { get; set; }
        public IList<SelectListItem> YearList { get; set; }

    }
    public class DataUploadHistoryViewModel
    {
        public int DataUploadHistoryId { get; set; }
        public string Status { get; set; }
        public int DataYear { get; set; }

        public DateTime? UploadDate { get; set; }
        public string UploadedFileName { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }

    }
}