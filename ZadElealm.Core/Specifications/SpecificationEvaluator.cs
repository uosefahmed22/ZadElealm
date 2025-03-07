using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Core.Specifications
{
    public class SpecificationEvaluator<T> where T : BaseEntity
    {
        // How to create a query from a specification
        public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> specification)
        {
            // 1. Check if the specification is null and throw an exception if it is
            if (specification == null)
            {
                throw new ArgumentNullException(nameof(specification));
            }

            // 2. Start with the input query
            var query = inputQuery;

            // 3. Apply the criteria from the specification if it exists
            if (specification.Criteria != null)
            {
                query = query.Where(specification.Criteria);
            }

            // Apply regular includes
            query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));

            // Apply then includes
            query = specification.ThenIncludes.Aggregate(query, (current, include) => include(current));
            // 4. If order by is specified apply it
            if (specification.OrderBy != null)
            {
                query = query.OrderBy(specification.OrderBy);
            }

            // 5. If order by descending is specified apply it
            if (specification.OrderByDescending != null)
            {
                query = query.OrderByDescending(specification.OrderByDescending);
            }

            //6. Add pagination
            if (specification.IsPagingEnabled)
            {
                query = query.Skip(specification.Skip).Take(specification.Take);
            }

            // 7. Apply the includes from the specification for eager loading
            query = specification.Includes.Aggregate(query, (currentExp, includeExp)
                => currentExp.Include(includeExp));

            // 8. Return the modified query
            return query;
        }
    }
}
