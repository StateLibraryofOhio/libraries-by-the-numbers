using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StateOfOhioLibrary.Models
{
    public class DashboardModel
    {
        public int NoOfTemplates { get; set; }
        public int NooOfUsers { get; set; }
        public int NoOfDatas { get; set; }
        public int NoOfLibraries { get; set; }

        public DateTime? LibraryDataLastUpdated { get; set; }
    }
}