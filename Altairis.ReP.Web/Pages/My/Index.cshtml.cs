using Altairis.ReP.Data;
using Altairis.ReP.Data.Dtos.ReservationDtos;
using Altairis.ReP.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Olbrasoft.ReP.Business;

namespace Altairis.ReP.Web.Pages.My;
public partial class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IOpeningHoursService hoursProvider;
    private readonly IResourceService _resourceService;
    private readonly IReservationService _reservationService;
    private readonly INewsMessageService _newMesaageService;

    private int UserId => int.Parse(userManager.GetUserId(User));

    public IndexModel(UserManager<ApplicationUser> userManager,
                      IOpeningHoursService hoursProvider,
                      IResourceService resourceService,
                      IReservationService reservationService,
                      INewsMessageService newMesaageService)
    {
        this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        this.hoursProvider = hoursProvider ?? throw new ArgumentNullException(nameof(hoursProvider));
        _resourceService = resourceService ?? throw new ArgumentNullException(nameof(resourceService));
        _reservationService = reservationService ?? throw new ArgumentNullException(nameof(reservationService));
        _newMesaageService = newMesaageService ?? throw new ArgumentNullException(nameof(newMesaageService));
    }

    public IEnumerable<Resource> Resources { get; set; }

    public IEnumerable<ReservationInfoDto> Reservations { get; set; }

    public OpeningHoursInfo OpenToday { get; set; }

    public OpeningHoursInfo OpenTomorrow { get; set; }

    public DateTime LastNewsDate { get; set; }

    public string LastNewsTitle { get; set; }

    public string LastNewsText { get; set; }

    public async Task OnGetAsync(CancellationToken token)
    {
        // Get operning hours
        OpenToday = await hoursProvider.GetOpeningHoursAsync(0);
        OpenTomorrow = await hoursProvider.GetOpeningHoursAsync(1);

        // Get latest news message
        var latestNews = await _newMesaageService.GetFirstNewsMessageOrNullAsync(token);
        if (latestNews != null)
        {
            LastNewsDate = latestNews.Date;
            LastNewsTitle = latestNews.Title;
            LastNewsText = latestNews.Text;
        }

        // Get resources accessible to user
        Resources = await _resourceService.GetResourcesAsync(User.IsPrivilegedUser(), token);

        Reservations = await _reservationService.GetReservationInfosAsync(UserId, token);
    }

    public async Task<IActionResult> OnGetDeleteAsync(int reservationId, CancellationToken token)
    {
        return (await _reservationService.DeleteReservationAsync(reservationId, UserId, token)) == CommandStatus.NotFound
           ? NotFound()
           : RedirectToPage("Index", null, "reservationdeleted");
    }
}
