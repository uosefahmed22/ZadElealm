﻿using MediatR;
using ZadElealm.Apis.Commands;
using ZadElealm.Apis.Errors;

namespace AdminDashboard.Commands
{
    public class HandleReportCommand : IRequest<ApiDataResponse> 
    {
        public int ReportId { get; set; }
        public string AdminResponse { get; set; }
    }
}
