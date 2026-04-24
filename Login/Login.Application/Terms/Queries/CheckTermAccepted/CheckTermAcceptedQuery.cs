using MediatR;

namespace Login.Application.Terms.Queries.CheckTermAccepted;

public record CheckTermAcceptedQuery(string TermName) : IRequest<bool>;
