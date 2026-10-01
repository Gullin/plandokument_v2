using Plandokument.Application.Interfaces.Repositories;
using Plandokument.Domain.Entities;

namespace Plandokument.Infrastructure.Repositories;

public class PlanBasRepository : IPlanBasRepository
{
    private readonly IPlanRegisterBasRepository _registerRepo;
    private readonly IPlanGeometriBasRepository _geometriRepo;

    public PlanBasRepository(IPlanRegisterBasRepository registerRepository, IPlanGeometriBasRepository geometriRepository)
    {
        _registerRepo = registerRepository;
        _geometriRepo = geometriRepository;
    }

    public async Task<IEnumerable<PlanBas>> GetPlanBasAsync(CancellationToken cancellationToken)
    {
        var registerList = await _registerRepo.GetPlanRegisterBasAsync(cancellationToken);
        var geometriList = await _geometriRepo.GetPlanGeometriBasAsync(cancellationToken);

        var joined = registerList
            .Join(
                geometriList,
                register => register.lmakt,
                geometri => geometri.lmakt,
                (register, geometri) => new PlanBas
                {
                    Register = register,
                    Geometri = geometri
                })
            .ToList();

        return joined;
    }
}
