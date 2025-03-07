using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Specifications
{
    public class CategoryWithCoursesSpecification : BaseSpecification<Category>
    {
        public CategoryWithCoursesSpecification(int categoryId) : base(x => x.Id == categoryId)
        {
            Includes.Add(x => x.Courses);
        }
    }
}
