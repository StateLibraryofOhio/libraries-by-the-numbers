
using StateOfOhioLibrary.Data.Models;
using StateOfOhioLibrary.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Linq.Expressions;
namespace StateOfOhioLibrary.Services
{
    public class LibraryDataService
    {
        #region Global Declarations
        DatabaseContext dbContext = new DatabaseContext();
        CommonService commonService = new CommonService();
        public string className = MethodBase.GetCurrentMethod().DeclaringType.Name;
        public string errorLogFilePath = Convert.ToString(WebConfigurationManager.AppSettings["LogFilePath"]);
        public enum EnumStatus
        {
            [Description("Active")]
            Active,
            [Description("Deleted")]
            Deleted
        }
        #endregion

        #region Methods
        /// <summary>
        /// Get library data details
        /// </summary>
        /// <returns></returns>
        public DataModel GetLibraryDataUploadHistory(int year, string sortOrder, string sortdir, int page, int pageSize)
        {
            DataModel dataModel = new DataModel();
            try
            {
                dataModel.TotalNoOfData = dbContext.DataUploadHistory.Where(x => x.Status == EnumStatus.Active.ToString()).ToList().Count;

                dataModel.DataList = new PagedList<DataUploadHistoryModel>();
                IQueryable<DataUploadHistoryModel> query = dbContext.DataUploadHistory;
                if (year != 0)
                { query = query.Where(x => x.DataYear == year).AsQueryable(); }

                var param = Expression.Parameter(typeof(DataUploadHistoryModel), "i");
                Expression conversion = Expression.Convert(Expression.Property(param, sortOrder), typeof(object));
                var mySortExpression = Expression.Lambda<Func<DataUploadHistoryModel, object>>(conversion, param);

                dataModel.DataList.Content = query
                                             .OrderByDescending(x => x.UploadDate)
                                             .Skip((page - 1) * pageSize)
                                             .Take(pageSize)
                                             .ToList();                
                dataModel.DataList.TotalRecords = query.Count();
                dataModel.DataList.CurrentPage = page;
                dataModel.DataList.PageSize = pageSize;
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }
            return dataModel;
        }

        /// <summary>
        /// Add LibraryData
        /// </summary>
        /// <param name="libraryData"></param>
        /// <returns></returns>
        public LibraryDataModel AddLibraryData(LibraryDataModel libraryData)
        {
            try
            {
                this.dbContext.LibraryData.Add(libraryData);
                this.dbContext.SaveChanges();
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }
            return libraryData;
        }
        /// <summary>
        /// Update LibraryData
        /// </summary>
        /// <param name="libraryData"></param>
        /// <returns></returns>
        public LibraryDataModel UpdateLibraryData(LibraryDataModel libraryData)
        {
            try
            {
                this.dbContext.SaveChanges();
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }
            return libraryData;
        }
        /// <summary>
        /// Get library data by name & year
        /// </summary>
        /// <param name="name"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public LibraryDataModel GetLibraryDataByName(string name, int year)
        {
            LibraryDataModel libraryData = new LibraryDataModel();
            try
            {
                libraryData = dbContext.LibraryData.Where(x => x.LibraryName.ToLower() == name && x.DataYear == year).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return libraryData;
        }

        /// <summary>
        /// Get list of library data by name and start year - end year
        /// </summary>
        /// <param name="name"></param>
        /// <param name="startYear"></param>
        /// <param name="endYear"></param>
        /// <returns></returns>
        public List<LibraryDataModel> GetLibraryDataByNameAndYearRange(string name, int startYear, int endYear)
        {
            List<LibraryDataModel> libraryData = new List<LibraryDataModel>();
            try
            {
                libraryData = dbContext.LibraryData.Where(x => x.LibraryName.ToLower() == name && x.DataYear >= startYear && x.DataYear <= endYear).OrderBy(x => x.DataYear).ToList();
            }
            catch (Exception ex)
            {

            }
            return libraryData;
        }

        /// <summary>
        /// Get library data by Id & year
        /// </summary>
        /// <param name="name"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public LibraryDataModel GetLibraryDataByIdAndYear(string libraryId, int year)
        {
            LibraryDataModel libraryDetails = new LibraryDataModel();
            try
            {
                libraryDetails = dbContext.LibraryData.Where(x => x.LibraryID == libraryId && x.DataYear == year).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return libraryDetails;
        }


        /// <summary>
        /// Get library data list by library name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<LibraryDataModel> GetLibraryDataListByName(string name)
        {
            var libraryData = dbContext.LibraryData.AsQueryable();
            var result = libraryData.Where(x => x.LibraryName == name).ToList();
            return result;
        }

        /// <summary>
        /// Get all library data
        /// </summary>
        /// <returns></returns>
        public List<LibraryDataModel> GetAllLibraryData()
        {
            var libraryData = dbContext.LibraryData.ToList();
            return libraryData;
        }


        /// <summary>
        /// Delete Data By Year
        /// </summary>
        /// <param name="year"></param>
        public void DeleteDataByYear(int year)
        {
            try
            {
                dbContext.LibraryData.RemoveRange(dbContext.LibraryData.Where(x => x.DataYear == year));
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        #endregion
    }
}