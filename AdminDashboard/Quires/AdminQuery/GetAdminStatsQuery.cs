using AdminDashboard.Commands;
using AdminDashboard.Dto;
using MediatR;

namespace AdminDashboard.Quires.AdminQuery
{
    public record GetAdminStatsQuery : IRequest<AdminStatsResult>;

}
