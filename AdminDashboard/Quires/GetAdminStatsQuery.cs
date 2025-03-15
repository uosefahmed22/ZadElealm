using AdminDashboard.Commands;
using AdminDashboard.Dto;
using MediatR;

namespace AdminDashboard.Quires
{
    public record GetAdminStatsQuery : IRequest<AdminStatsResult>;

}
