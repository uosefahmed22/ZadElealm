using MediatR;

namespace AdminDashboard.Commands
{
    public class CreateCategoryCommand : IRequest<bool>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public IFormFile ImageUrl { get; set; }
    }
}
