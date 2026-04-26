using HMS.Application.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Application.Features.Permissions
{
    public record GetAllPermissionsQuery : IRequest<List<PermissionDto>>;
}
