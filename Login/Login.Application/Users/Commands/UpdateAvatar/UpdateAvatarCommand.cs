using MediatR;

namespace Login.Application.Users.Commands.UpdateAvatar;

public record UpdateAvatarCommand(string? AvatarUrl) : IRequest;
