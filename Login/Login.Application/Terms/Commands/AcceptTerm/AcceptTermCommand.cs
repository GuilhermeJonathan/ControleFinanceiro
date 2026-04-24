using MediatR;

namespace Login.Application.Terms.Commands.AcceptTerm;

public record AcceptTermCommand(
    string TermName,
    string? IpAddress,
    string? UserAgent
) : IRequest;
