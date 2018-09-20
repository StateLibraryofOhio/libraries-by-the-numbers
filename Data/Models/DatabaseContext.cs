using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace StateOfOhioLibrary.Data.Models
{
    public class DatabaseContext : DbContext
    {
        public DbSet<UserModel> User { get; set; }
        public DbSet<TemplateModel> Template { get; set; }
        public DbSet<DataUploadHistoryModel> DataUploadHistory { get; set; }
        public DbSet<LibraryDataModel> LibraryData { get; set; }
        public DbSet<CustomTemplateDataModel> CustomTemplate { get; set; }
        public DbSet<CustomColumnDataModel> CustomColumn { get; set; }
    }
}