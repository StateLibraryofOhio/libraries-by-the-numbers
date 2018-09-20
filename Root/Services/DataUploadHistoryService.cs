using StateOfOhioLibrary.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;

namespace StateOfOhioLibrary.Services
{
    public class DataUploadHistoryService
    {
        #region Global Declarations
        DatabaseContext dbContext = new DatabaseContext();
        CommonService commonService = new CommonService();
        #endregion
        /// <summary>
        /// Add Library Data Upload History
        /// </summary>
        /// <param name="dataUploadHistoryModel"></param>
        /// <returns></returns>
        public DataUploadHistoryModel AddLibraryDataUploadHistory(DataUploadHistoryModel dataUploadHistoryModel)
        {
            try
            {
                this.dbContext.DataUploadHistory.Add(dataUploadHistoryModel);
                this.dbContext.SaveChanges();
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }

            return dataUploadHistoryModel;
        }


    }
}