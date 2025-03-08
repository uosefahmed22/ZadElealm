using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;
using ZadElealm.Core.Specifications.Course;

namespace ZadElealm.Core.Specifications
{
    public class CategoryWithCoursesSpecification : BaseSpecification<Core.Models.Course>
    {
        public CategoryWithCoursesSpecification(CourseSpecParams specParams)
            : base(x => x.CategoryId == specParams.CategoryId &&
                (string.IsNullOrEmpty(specParams.Search) ||
                x.Name.ToLower().Contains(specParams.Search) ||
                x.Description.ToLower().Contains(specParams.Search))
                && (string.IsNullOrEmpty(specParams.Author) ||
                x.Author.ToLower().Contains(specParams.Author))
                && (string.IsNullOrEmpty(specParams.Language) ||
                x.CourseLanguage.ToLower().Contains(specParams.Language))
                && (!specParams.MinRating.HasValue || x.rating >= specParams.MinRating)
                && (!specParams.MaxRating.HasValue || x.rating <= specParams.MaxRating)
                && (!specParams.FromDate.HasValue || x.CreatedAt >= specParams.FromDate)
                && (!specParams.ToDate.HasValue || x.CreatedAt <= specParams.ToDate))
        {
            Includes.Add(x => x.Category);
            ApplyOrdering(specParams);
            ApplyPagination((specParams.PageNumber - 1) * specParams.PageSize, specParams.PageSize);
        }

        private void ApplyOrdering(CourseSpecParams specParams)
        {
            switch (specParams.SortBy?.ToLower())
            {
                case "rating":
                    if (specParams.SortDirection?.ToLower() == "asc")
                        OrderBy = x => x.rating;
                    else
                        OrderByDescending = x => x.rating;
                    break;

                case "date":
                    if (specParams.SortDirection?.ToLower() == "asc")
                        OrderBy = x => x.CreatedAt;
                    else
                        OrderByDescending = x => x.CreatedAt;
                    break;

                case "name":
                    if (specParams.SortDirection?.ToLower() == "asc")
                        OrderBy = x => x.Name;
                    else
                        OrderByDescending = x => x.Name;
                    break;

                case "author":
                    if (specParams.SortDirection?.ToLower() == "asc")
                        OrderBy = x => x.Author;
                    else
                        OrderByDescending = x => x.Author;
                    break;

                default:
                    OrderByDescending = x => x.rating; // ترتيب افتراضي تنازلي حسب التقييم
                    break;
            }
        }
    }
}
