using AdminDashboard.Dto;
using MediatR;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Dtos.DtosCategory;

namespace AdminDashboard.Quires
{
    public class GetAllCategoriesQuery : IRequest<IReadOnlyList<CategoryResponseDto>>
    {
    }
}
