using DailyReport.Models;
using Microsoft.EntityFrameworkCore;

namespace DailyReport.Services.Reports
{
    public static class DepartmentServices
    {
        public static void AddDepartment(Department department, ApplicationContext context)
        {
            department.Settings = new DepartmentSettings { DepartmentId = department.Id };
            context.Departments.Add(department);
            context.SaveChanges();
        }

        public static void DeleteDepartment(int id, ApplicationContext context)
        {
            Department? department = (from d in context.Departments
                                      where d.Id == id
                                      select d).FirstOrDefault();
            if (department != null) context.Departments.Remove(department);
            context.SaveChanges();
        }

        public static void UpdateDepartment(Department department, ApplicationContext context)
        {
            Department? _department = (from d in context.Departments
                                       where d.Id == department.Id
                                       select d).AsNoTracking().FirstOrDefault();
            if (_department != null)
            {
                context.Departments.Update(department);
                context.SaveChanges();
            }
        }

        public static List<Department> GetWDepartments(ApplicationContext context)
        {
            List<Department> departments = (from d in context.Departments
                                            where d.Number != 90 & d.Number != 91 && d.Number != 8 & d.Number != 11
                                            orderby d.Number
                                            select d).ToList();
            return departments;
        }

        public static List<Department> GetSortedDepartments(ApplicationContext context)
        {
            List<Department> departments = (from d in context.Departments
                                            orderby d.ShowOrder
                                            select d).ToList();
            return departments;
        }

        public static List<Department> GetSortedDepartments(ApplicationContext context, List<int> excludedDeps)
        {
            List<Department> _departments = (from d in context.Departments
                                             orderby d.ShowOrder
                                             select d).ToList();
            List<Department> actualDepartments = new List<Department>();
            foreach (Department dep in _departments)
            {
                if (!excludedDeps.Contains(dep.Allias)) actualDepartments.Add(dep);
            }

            return actualDepartments;
        }

        /// <summary>
        /// Отделения для главной страницы (сводка отделений). Отбор ведётся по флагу
        /// DepartmentSettings.ShowOnMainPage вместо списков исключений конфигурации.
        /// </summary>
        public static List<Department> GetMainPageDepartments(ApplicationContext context)
        {
            return (from d in context.Departments
                    where d.Settings != null && d.Settings.ShowOnMainPage
                    orderby d.ShowOrder
                    select d).ToList();
        }

        /// <summary>Отделение по идентификатору вместе с его настройками</summary>
        public static Department? GetById(int id, ApplicationContext context)
        {
            return (from d in context.Departments
                    where d.Id == id
                    select d).Include(d => d.Settings).FirstOrDefault();
        }
    }
}