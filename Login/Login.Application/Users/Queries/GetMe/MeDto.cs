namespace Login.Application.Users.Queries.GetMe;

public record MeDto(
    Guid Id,
    string Name,
    string Email,
    string Document,
    string? Cellphone,
    string? AvatarUrl
);
