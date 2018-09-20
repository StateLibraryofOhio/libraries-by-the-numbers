using System;
using System.Collections.Generic;

namespace StateOfOhioLibrary.Models
{
    public class PagedList<T>
    {
        public List<T> Content { get; set; }

        public Int32 CurrentPage { get; set; }
        public Int32 PageSize { get; set; }
        public int TotalRecords { get; set; }

        public int TotalPages
        {
            get { return (int)Math.Ceiling((decimal)TotalRecords / PageSize); }
        }
    }
}