using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZadElealm.Core.Specifications.Course
{
    public class CoursesWithFiltration : BaseSpecification<Core.Models.Course>
    {
        //public CoursesWithFiltration(CourseSpecParams specParams) : base(p =>
        //    (specParams.CategoryId == 0 || p.CategoryId == specParams.CategoryId) &&
        //    (string.IsNullOrEmpty(specParams.Search) ||
        //        p.Name.ToLower().Contains(specParams.Search) ||
        //        p.Description.ToLower().Contains(specParams.Search)) &&
        //    (!specParams.MinRating.HasValue || p.AverageRating >= specParams.MinRating) &&
        //    (!specParams.MaxRating.HasValue || p.AverageRating <= specParams.MaxRating) &&
        //    (string.IsNullOrEmpty(specParams.Author) || p.Author.ToLower() == specParams.Author.ToLower()))
        //{
        //    if (!string.IsNullOrEmpty(specParams.SortBy))
        //    {
        //        switch (specParams.SortBy.ToLower())
        //        {
        //            case "name":
        //                OrderBy(x => x.Name);
        //                break;
        //            case "price":
        //                AddOrderBy(x => x.Price);
        //                break;
        //            case "rating":
        //                AddOrderBy(x => x.AverageRating);
        //                break;
        //            default:
        //                AddOrderBy(x => x.CreatedAt);
        //                break;
        //        }
        //    }

        //    ApplyPaging(specParams.PageSize * (specParams.PageNumber - 1), specParams.PageSize);
        //}
    }
}
