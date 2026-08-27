using MediatR;
using ZadElealm.Core.Models;

namespace AdminDashboard.Quires.CategoryQuery
{
    public class GetCategoryByIdQuery : IRequest<Category>
    {
        public int Id { get; set; }
    }
}
