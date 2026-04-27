using MediatR;

namespace Login.Application.Users.Commands.BlockUser;

public record BlockUserCommand(Guid Id, bool Block) : IRequest;
