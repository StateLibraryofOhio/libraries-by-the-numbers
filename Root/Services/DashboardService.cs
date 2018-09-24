using StateOfOhioLibrary.Data.Models;
using StateOfOhioLibrary.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;

namespace StateOfOhioLibrary.Services
{
    public class DashboardService
    {
        #region Global Declarations
        DatabaseContext dbContext = new DatabaseContext();
        CommonService commonService = new CommonService();
        public string className = MethodBase.GetCurrentMethod().DeclaringType.Name;
        public string errorLogFilePath = Convert.ToString(WebConfigurationManager.AppSettings["LogFilePath"]);
        #endregion

        public enum EnumStatus
        {
            [Description("Active")]
            Active,
            [Description("Deleted")]
            Deleted

        }
        #region Methods
        /// <summary>
        /// Get dashboard data
        /// </summary>
        /// <returns></returns>
        public DashboardModel GetDashboardData()
        {
            DashboardModel dashboardData = new DashboardModel();
            try
            {
                dashboardData.NoOfTemplates = dbContext.Template.Where(x => x.Status == EnumStatus.Active.ToString()).ToList().Count;
                dashboardData.NooOfUsers = dbContext.User.Where(x => x.Status == EnumStatus.Active.ToString()).ToList().Count;
                dashboardData.NoOfDatas = dbContext.DataUploadHistory.Where(x => x.Status == EnumStatus.Active.ToString()).ToList().Count;
                dashboardData.LibraryDataLastUpdated = dbContext.DataUploadHistory.OrderByDescending(x => x.CreatedAt).FirstOrDefault().CreatedAt;

                var libraryData = dbContext.LibraryData.Where(x => x.Status == EnumStatus.Active.ToString()).ToList();
                dashboardData.NoOfLibraries = libraryData.GroupBy(x => x.LibraryName).Select(y => y.Distinct()).ToList().Count;
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }

            return dashboardData;
        }

        #endregion
    }
}