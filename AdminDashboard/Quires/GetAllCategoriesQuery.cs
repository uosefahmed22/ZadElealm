using AdminDashboard.Dto;
using MediatR;
using ZadElealm.Apis.Dtos;

namespace AdminDashboard.Quires
{
    public class GetAllCategoriesQuery : IRequest<IReadOnlyList<CategoryResponseDto>>
    {
    }
}
