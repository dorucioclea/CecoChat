using CecoChat.Contracts.Bff;
using CecoChat.Data;
using FluentValidation;

namespace CecoChat.Server.Bff.Endpoints;

public sealed class GetChatsRequestValidator : AbstractValidator<GetChatsRequest>
{
    public GetChatsRequestValidator()
    {
        RuleFor(x => x.NewerThan)
            .ValidNewerThanDateTime();
    }
}
