using MediatR;

namespace Login.Application.Users.Commands.Refresh;

public record RefreshCommand(string RefreshToken) : IRequest<RefreshResult>;

public record RefreshResult(string AccessToken, string RefreshToken);
