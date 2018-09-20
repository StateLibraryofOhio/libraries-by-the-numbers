using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
namespace StateOfOhioLibrary.Models
{
    public class ChartDataModel
    {
        public List<ChartData> ChartDataList { get; set; }
    }
    public class ChartData
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public Color Color { get; set; }
    }
}