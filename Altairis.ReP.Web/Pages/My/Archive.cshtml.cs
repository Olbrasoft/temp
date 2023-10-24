using Altairis.ReP.Data.Dtos.ReservationDtos;
using Altairis.ReP.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Altairis.ReP.Web.Pages.My;
public class ArchiveModel : PageModel
{
    private readonly IReservationService _service;
    private readonly UserManager<ApplicationUser> userManager;

    public ArchiveModel(IReservationService service, UserManager<ApplicationUser> userManager)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    public IEnumerable<UserReservationDto> Reservations { get; set; }

    public async Task OnGetAsync(CancellationToken token)
    {
        var userId = int.Parse(userManager.GetUserId(User));
        Reservations = await _service.GetUserReservationsAsync(userId, token);
    }
}
