using MediatR;

namespace Login.Application.Profiles.Commands.DeleteProfile;

public record DeleteProfileCommand(Guid Id) : IRequest;
