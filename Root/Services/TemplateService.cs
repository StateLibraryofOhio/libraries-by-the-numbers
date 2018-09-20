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
    public class TemplateService
    {
        #region Global Declaration
        DatabaseContext dbContext = new DatabaseContext();
        SecurityService securityService = new SecurityService();
        CommonService commonService = new CommonService();
        public enum EnumStatus
        {
            [Description("Active")]
            Active,
            [Description("Inactive")]
            Inactive,
            [Description("Deleted")]
            Deleted
        }

        #endregion

        /// <summary>
        /// Get all templates
        /// </summary>
        /// <returns></returns>
        public List<TemplateModel> GetAllTemplate()
        {
            var templates = dbContext.Template.ToList();

            return templates;
        }

        /// <summary>
        /// Get all templates for grid
        /// </summary>
        /// <param name="sortOrder"></param>
        /// <param name="sortdir"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public TemplateViewModel GetTemplates(string sortOrder, string sortdir, int page, int pageSize)
        {

            TemplateViewModel model = new TemplateViewModel();
            try
            {
                model.Template = new PagedList<TemplateModel>();
                IQueryable<TemplateModel> query = dbContext.Template;
                model.TemplateCount = query.Where(x => x.Status == EnumStatus.Active.ToString()).Count();

                var param = Expression.Parameter(typeof(TemplateModel), "i");
                Expression conversion = Expression.Convert(Expression.Property(param, sortOrder), typeof(object));
                var mySortExpression = Expression.Lambda<Func<TemplateModel, object>>(conversion, param);

                model.Template.Content = query
                                             .OrderBy(x => x.TemplateId + " " + sortdir)
                                             .Skip((page - 1) * pageSize)
                                             .Take(pageSize)
                                             .ToList();
                // Count
                model.Template.TotalRecords = query.Count();
                model.Template.CurrentPage = page;
                model.Template.PageSize = pageSize;
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }

            return model;
        }

        /// <summary>
        /// Get template by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TemplateModel GetTemplateById(int id)
        {
            var template = new TemplateModel();
            try
            {
                var query = dbContext.Template.AsQueryable();
                template = query.Where(x => x.TemplateId == id).FirstOrDefault();
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }

            return template;
        }

        /// <summary>
        /// Add template
        /// </summary>
        /// <param name="newTemplate"></param>
        /// <returns></returns>
        public TemplateModel AddTemplate(TemplateModel newTemplate)
        {
            try
            {
                if (newTemplate == null)
                    throw new ArgumentNullException("Template");

                this.dbContext.Template.Add(newTemplate);
                this.dbContext.SaveChanges();
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }

            return newTemplate;
        }

        /// <summary>
        /// Update template
        /// </summary>
        /// <param name="newTemplate"></param>
        /// <returns></returns>
        public TemplateModel UpdateTemplate(TemplateModel newTemplate)
        {
            try
            {
                if (newTemplate == null)
                    throw new ArgumentNullException("Template");

                this.dbContext.SaveChanges();
            }
            catch (Exception exception)
            {
                commonService.LogException(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, exception);
            }

            return newTemplate;
        }

    }
}