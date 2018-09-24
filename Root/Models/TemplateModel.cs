using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StateOfOhioLibrary.Data.Models;
using System.ComponentModel;

namespace StateOfOhioLibrary.Models
{
    public enum EnumStatus
    {
        [Description("Active")]
        Active,
        [Description("Inactive")]
        Inactive,
        [Description("Deleted")]
        Deleted
    }
    /// <summary>
    /// View templates
    /// </summary>
    public class TemplateViewModel
    {
        public TemplateViewModel()
        {
            Template = new PagedList<TemplateModel>();
        }

        public PagedList<TemplateModel> Template { get; set; }

        public int TemplateId { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }

        public string FileLocation { get; set; }

        public string Notes { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int TemplateCount { get; set; }

    }
}