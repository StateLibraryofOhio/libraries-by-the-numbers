using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace StateOfOhioLibrary.Data.Models
{
    [Table("LibraryData")]
    public class LibraryDataModel
    {
        [Key]
        public int LibraryDataId { get; set; }
        public string Status { get; set; }
        public int DataYear { get; set; }
        public string LibraryID { get; set; }
        public string LibraryName { get; set; }
        public string County { get; set; }

        public int InternetPC { get; set; }
        public int AnnualUses { get; set; }
        public int AnnualWirelessSessions { get; set; }
        public int CircPhysicalMaterial { get; set; }
        public int CircDownloadableMaterial { get; set; }
        public int Databases { get; set; }
        public int PrintMaterial { get; set; }
        public int PhysicalVideo { get; set; }
        public int PhysicalAudio { get; set; }
        public int Ebooks { get; set; }
        public int PopulationLegalSvcArea { get; set; }
        public int RegAdults { get; set; }
        public int RegChildren { get; set; }
        public int CentralLibraries { get; set; }
        public int Branches { get; set; }
        public decimal AnnualHrsOpen { get; set; }
        public int CircTotal { get; set; }
        public int CircAdult { get; set; }
        public int CircJuvenile { get; set; }
        public decimal LibraryStaffTotal { get; set; }
        public int PrintMaterials { get; set; }
        public int PrintSubscription { get; set; }
        public int DownloadableVideo { get; set; }
        public int DownloadableAudio { get; set; }
        public int ComputerSoftware { get; set; }
        public int ILLProvided { get; set; }
        public int ILLReceived { get; set; }
        public int AttendanceTypicalWeek { get; set; }
        public int AnnualReferenceTransaction { get; set; }
        public int ChildrenPrograms { get; set; }
        public int YoungAdultPrograms { get; set; }
        public int TotalLibraryPrograms { get; set; }

        public int ChildrenProgramAttendance { get; set; }
        public int YoungAdultProgramAttendance { get; set; }
        public int TotalProgramAttendance { get; set; }
        public int TotalLibraryProgramAttendance { get; set; }
        public int SummerReadingChildren { get; set; }
        public int SummerReadingTeens { get; set; }
        public int SummerReadingAdult { get; set; }
        public int BookMobiles { get; set; }
        public int HomeWorkSessions { get; set; }
        public int MealsSnacksSFSP { get; set; }
        public int AnnualAttendanceLibrary { get; set; }

        public string Notes { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}