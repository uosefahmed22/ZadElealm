﻿using AdminDashboard.Middlwares;
using MediatR;
using ZadElealm.Apis.Errors;

namespace AdminDashboard.Commands
{
    public class UpdateRoleCommand : IRequest<ApiResponse>
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
