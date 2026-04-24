using MediatR;

namespace Login.Application.Integration.Commands.Authorize;

public record AuthorizeIntegrationCommand(string ClientId, string ClientSecret) : IRequest<string>;
