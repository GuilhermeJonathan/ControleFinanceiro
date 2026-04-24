using Login.Domain.Common;
using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Profiles.Commands.DeleteProfile;

public class DeleteProfileCommandHandler : IRequestHandler<DeleteProfileCommand>
{
    private readonly IProfileRepository _profileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProfileCommandHandler(IProfileRepository profileRepository, IUnitOfWork unitOfWork)
    {
        _profileRepository = profileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _profileRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Perfil {request.Id} não encontrado.");

        profile.Deactivate();
        _profileRepository.Update(profile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
