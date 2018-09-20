using StateOfOhioLibrary.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace StateOfOhioLibrary.Models
{
    public class CustomTemplateModel
    {
        public CustomTemplateModel()
        {
            CustomTemplate = new PagedList<CustomTemplateModel>();
        }
        public int CustomTemplateCount { get; set; }
        public PagedList<CustomTemplateModel> CustomTemplate { get; set; }
        public int CustomTemplateId { get; set; }
        public string SectionName { get; set; }
        public int CustomColumnId { get; set; }
        public List<SelectListItem> AvailableCustomColumn { get; set; }
        public string Status { get; set; }
        public string FileLocation { get; set; }
        public string ColumnText { get; set; }
        public string Notes { get; set; }
        public string PreviousPageUrl { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int CustomColumnIdField1 { get; set; }
        public int CustomColumnIdField2 { get; set; }
        public int CustomColumnIdField3 { get; set; }
        public int CustomColumnIdField4 { get; set; }
        public int CustomColumnIdField5 { get; set; }
        public int CustomColumnIdField6 { get; set; }
    }

}