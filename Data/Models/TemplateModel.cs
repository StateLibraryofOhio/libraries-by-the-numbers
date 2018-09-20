using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace StateOfOhioLibrary.Data.Models
{
    [Table("Template")]
    public class TemplateModel
    {
        [Key]
        public int TemplateId { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }

        public string FileLocation { get; set; }

        public string Notes { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

    }
}