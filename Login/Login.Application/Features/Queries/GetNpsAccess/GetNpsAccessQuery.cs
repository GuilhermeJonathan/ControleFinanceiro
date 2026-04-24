using MediatR;

namespace Login.Application.Features.Queries.GetNpsAccess;

public record GetNpsAccessQuery : IRequest<NpsAccessDto>;
public record NpsAccessDto(string Email, bool HasAccess);
