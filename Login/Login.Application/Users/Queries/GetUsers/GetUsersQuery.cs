using Login.Application.Common;
using MediatR;

namespace Login.Application.Users.Queries.GetUsers;

public record GetUsersQuery(
    Guid? HierarchyId,
    Guid? ProfileId,
    int? StatusId,
    string? Email,
    string? Name,
    string? Document,
    int? UserTypeId,
    string? Region,
    string? RequestDateStart,
    string? RequestDateEnd,
    int PageSize = 20,
    int CurrentPage = 1,
    string? ColumnName = null,
    string? Direction = null
) : IRequest<PagedResult<UserDto>>;
