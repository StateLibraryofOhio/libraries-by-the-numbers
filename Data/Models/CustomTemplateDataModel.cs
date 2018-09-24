using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace StateOfOhioLibrary.Data.Models
{

    [Table("CustomTemplate")]
    public class CustomTemplateDataModel
    {
        [Key]
        public int CustomTemplateId { get; set; }
        public string Status { get; set; }
        public int CustomColumnId { get; set; }
        public string FileLocation { get; set; }
        public bool? DynamicTextPosition { get; set; }
        public string MappingColumn { get; set; }
        public string ColumnText { get; set; }
        public string Notes { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

    }

}