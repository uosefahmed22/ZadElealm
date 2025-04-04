﻿using MediatR;

namespace AdminDashboard.Commands.CategoryCommand
{
    public class DeleteCategoryCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public DeleteCategoryCommand(int id)
        {
            Id = id;
        }
    }
}
