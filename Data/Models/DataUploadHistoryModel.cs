using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace StateOfOhioLibrary.Data.Models
{
    [Table("DataUploadHistory")]
    public class DataUploadHistoryModel
    {
        [Key]
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