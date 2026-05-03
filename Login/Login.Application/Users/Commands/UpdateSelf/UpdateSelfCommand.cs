using MediatR;

namespace Login.Application.Users.Commands.UpdateSelf;

public record UpdateSelfCommand(string Name, string? Cellphone, string? Document) : IRequest;
