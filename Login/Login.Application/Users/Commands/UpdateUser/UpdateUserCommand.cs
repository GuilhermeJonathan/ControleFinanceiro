using MediatR;

namespace Login.Application.Users.Commands.UpdateUser;

public record UpdateUserCommand(
    Guid Id,
    string Name,
    string? Occupation,
    string? Address,
    string? Cellphone,
    string? Phone,
    int? CountryId,
    string? Region
) : IRequest;
