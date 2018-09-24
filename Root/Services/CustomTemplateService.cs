using System;
using System.Web;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Web.Configuration;
using System.Linq.Expressions;
using StateOfOhioLibrary.Models;
using System.Collections.Generic;
using StateOfOhioLibrary.Data.Models;


namespace StateOfOhioLibrary.Services
{
    public class CustomTemplateService
    {
        #region Global Declaration
        DatabaseContext dbContext = new DatabaseContext();
        SecurityService securityService = new SecurityService();
        CommonService commonService = new CommonService();
        #endregion
        /// <summary>
        /// Add Template
        /// </summary>
        /// <param name="newTemplate"></param>
        /// <returns></returns>
        public CustomTemplateDataModel AddcustomTemplate(CustomTemplateDataModel customTemplate)
        {
            try
            {
                if (customTemplate == null)
                    throw new ArgumentNullException("Template");

                this.dbContext.CustomTemplate.Add(customTemplate);
                this.dbContext.SaveChanges();
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }

            return customTemplate;
        }
        /// <summary>
        /// Get all custom templates for grid
        /// </summary>
        /// <param name="sortOrder"></param>
        /// <param name="sortdir"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public CustomTemplateModel GetCustomTemplates(string sortOrder, string sortdir, int page, int pageSize)
        {

            CustomTemplateModel model = new CustomTemplateModel();
            try
            {
                model.CustomTemplate = new PagedList<CustomTemplateModel>();
                IQueryable<CustomTemplateDataModel> query = dbContext.CustomTemplate;
                model.CustomTemplateCount = query.Where(x => x.Status == EnumStatus.Active.ToString()).Count();

                var param = Expression.Parameter(typeof(CustomTemplateDataModel), "i");
                Expression conversion = Expression.Convert(Expression.Property(param, sortOrder), typeof(object));
                var mySortExpression = Expression.Lambda<Func<CustomTemplateDataModel, object>>(conversion, param);
                var queryValues = (from customTemplate in dbContext.CustomTemplate
                                   where customTemplate.Status != EnumStatus.Deleted.ToString()
                                   join customColumn in dbContext.CustomColumn
                                   on customTemplate.CustomColumnId equals customColumn.CustomColumnId
                                   select new CustomTemplateModel
                                   {
                                       CustomTemplateId = customTemplate.CustomTemplateId,
                                       SectionName = customColumn.Name,
                                       CustomColumnId = customTemplate.CustomColumnId,
                                       FileLocation = customTemplate.FileLocation,
                                       Notes = customTemplate.Notes,
                                       ColumnText = customTemplate.ColumnText,
                                       CreatedAt = customTemplate.CreatedAt,
                                       CreatedBy = customTemplate.CreatedBy
                                   }).ToList();
                model.CustomTemplate.Content = queryValues.OrderBy(x => x.CustomTemplateId + " " + sortdir)
                                   .Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToList();

                model.CustomTemplate.TotalRecords = query.Count();
                model.CustomTemplate.CurrentPage = page;
                model.CustomTemplate.PageSize = pageSize;
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }

            return model;
        }

        /// <summary>
        /// Get Custom Template by CustomColumnId
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CustomTemplateDataModel GetCustomTemplateById(int id)
        {

            var customTemplate = new CustomTemplateDataModel();
            try
            {
                var query = dbContext.CustomTemplate.AsQueryable();
                customTemplate = query.Where(x => x.CustomColumnId == id && x.Status != CommonService.EnumStatus.Deleted.ToString()).FirstOrDefault();
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }

            return customTemplate;
        }

        /// <summary>
        /// Get Custom Template by CustomTemplateId
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CustomTemplateDataModel GetCustomTemplateByCustomTemplateId(int id)
        {
            var customTemplate = new CustomTemplateDataModel();
            try
            {
                var query = dbContext.CustomTemplate.AsQueryable();
                customTemplate = query.Where(x => x.CustomTemplateId == id && x.Status != CommonService.EnumStatus.Deleted.ToString()).FirstOrDefault();
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }


            return customTemplate;
        }

        /// <summary>
        /// Update Custom Template
        /// </summary>
        /// <param name="newCustomTemplate"></param>
        /// <returns></returns>
        public CustomTemplateDataModel UpdateCustomTemplate(CustomTemplateDataModel newCustomTemplate)
        {
            try
            {
                if (newCustomTemplate == null)
                    throw new ArgumentNullException("CustomTemplate");

                this.dbContext.SaveChanges();
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }

            return newCustomTemplate;
        }

    }
}