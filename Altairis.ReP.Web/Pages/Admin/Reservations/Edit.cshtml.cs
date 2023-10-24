using Altairis.ReP.Data.Dtos.ReservationDtos;
using Altairis.Services.Mailing.Templating;
using System.Globalization;

namespace Altairis.ReP.Web.Pages.Admin.Reservations;
public class EditModel : PageModel
{
    private readonly IReservationService _service;

    private readonly ITemplatedMailerService mailer;

    public EditModel(IReservationService service, ITemplatedMailerService mailer)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        this.mailer = mailer ?? throw new ArgumentNullException(nameof(mailer));
    }

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public class InputModel
    {

        public DateTime DateBegin { get; set; }

        public DateTime DateEnd { get; set; }

        public bool System { get; set; }

        public string Comment { get; set; }

    }

    public int ResourceId { get; set; }

    public string ResourceName { get; set; }

    public int UserId { get; set; }

    public string UserName { get; set; }

    public string NotificationEmail { get; set; }

    public CultureInfo NotificationCulture { get; set; }

    public async Task<ReservationEditDto> Init(int reservationId, CancellationToken token)
    {
        var r = await _service.GetReservationForEditOrNullAsync(reservationId, token);
        if (r != null)
        {
            ResourceId = r.ResourceId;
            ResourceName = r.ResourceName;
            UserId = r.UserId;
            UserName = r.UserName;

            if (r.UserSendNotifications && r.UserName != User.Identity.Name)
            {
                NotificationEmail = r.UserEmail;
                NotificationCulture = new CultureInfo(r.UserLanguage);
            }
        }
        return r;
    }

    public async Task<IActionResult> OnGetAsync(int reservationId, CancellationToken token)
    {
        var r = await Init(reservationId, token);
        if (r == null) return NotFound();

        Input = new InputModel
        {
            Comment = r.Comment,
            DateBegin = r.DateBegin,
            DateEnd = r.DateEnd,
            System = r.System
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int reservationId, CancellationToken token)
    {
        var r = await Init(reservationId, token);
        if (r == null) return NotFound();
       
        if (!ModelState.IsValid) return Page();

        var result = await _service.SaveAsync(reservationId, r.ResourceId , Input.DateBegin, Input.DateEnd, Input.System, Input.Comment, token);

        foreach (var item in result.Conflicts)
        {
            ModelState.AddModelError(string.Empty, string.Format(UI.My_Reservations_Err_Conflict, item.UserName, item.DateBegin));
        }

        if (!ModelState.IsValid) return Page();

        // Send notification if time changed
        if ((r.DateBegin != Input.DateBegin || r.DateEnd != Input.DateEnd) && !string.IsNullOrEmpty(NotificationEmail))
        {
            var msg = new TemplatedMailMessageDto("ReservationChanged", NotificationEmail);
            await mailer.SendMessageAsync(msg, new
            {
                resourceName = ResourceName,
                userName = User.Identity.Name,
                oldDateBegin = r.DateBegin,
                oldDateEnd = r.DateEnd,
                dateBegin = Input.DateBegin,
                dateEnd = Input.DateEnd
            }, NotificationCulture, NotificationCulture);
        }
        return RedirectToPage("Index", null, "saved");
    }

    public async Task<IActionResult> OnPostDeleteAsync(int reservationId, CancellationToken token)
    {
        var r = await Init(reservationId, token);
        if (r == null) return NotFound();

        // Send notification
        if (!string.IsNullOrEmpty(NotificationEmail))
        {
            var msg = new TemplatedMailMessageDto("ReservationDeleted", NotificationEmail);
            await mailer.SendMessageAsync(msg, new
            {
                resourceName = ResourceName,
                userName = User.Identity.Name,
                oldDateBegin = r.DateBegin,
                oldDateEnd = r.DateEnd,
            }, NotificationCulture, NotificationCulture);
        }

        // Delete reservation
        if (await _service.DeleteReservationAsync(reservationId, token) == CommandStatus.NotFound) return NotFound();

        return RedirectToPage("Index", null, "deleted");
    }

}
