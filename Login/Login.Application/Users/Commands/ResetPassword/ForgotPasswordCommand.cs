using MediatR;

namespace Login.Application.Users.Commands.ResetPassword;

public record ForgotPasswordCommand(string Identificador) : IRequest;

public record ValidateHashCommand(string Document, string Token) : IRequest<bool>;

public record ValidateSecurityCodeCommand(string Document, string Token) : IRequest;

public record ResetPasswordCommand(
    string Document,
    string Password,
    string Token,
    string? TermName
) : IRequest;

public record RedefinePasswordCommand(
    string Document,
    string Password,
    string Token
) : IRequest;
