using AdminDashboard.Models;
using MediatR;

namespace AdminDashboard.Queries.HomeQuery;

public sealed record GetDashboardOverviewQuery : IRequest<DashboardHomeViewModel>;
