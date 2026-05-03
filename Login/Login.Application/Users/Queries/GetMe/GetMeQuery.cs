using MediatR;

namespace Login.Application.Users.Queries.GetMe;

public record GetMeQuery : IRequest<MeDto>;
