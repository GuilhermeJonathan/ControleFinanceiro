using Login.Application.Users.Queries.GetUsers;
using MediatR;

namespace Login.Application.Users.Queries.GetUserById;

public record GetUserByIdQuery(Guid Id) : IRequest<UserDto?>;
