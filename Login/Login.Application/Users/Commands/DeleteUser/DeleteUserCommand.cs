using MediatR;

namespace Login.Application.Users.Commands.DeleteUser;

public record DeleteUserCommand(Guid Id) : IRequest;
