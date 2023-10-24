using Altairis.ReP.Data;
using Altairis.ReP.Data.Entities;
using Altairis.Services.DateProvider;
using Altairis.TagHelpers;
using Altairis.ValidationToolkit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace Altairis.ReP.Web.Pages.My;
public class ReservationsModel : PageModel
{
    private readonly IResourceService _resourceService;
    private readonly IReservationService _reservationService;
    private readonly IResourceAttachmentService _resourceAttachmentService;
    private readonly IDateProvider _dateProvider;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IOpeningHoursService _openingHourseService;
    private readonly AttachmentService attachmentProcessor;
    private readonly AppSettings _options;

    public ReservationsModel(
        IResourceService resourceService,
        IReservationService reservationService,
        IResourceAttachmentService resourceAttachmentService,
        IDateProvider dateProvider,
        UserManager<ApplicationUser> userManager,
        IOpeningHoursService hoursProvider,
        IOptions<AppSettings> optionsAccessor,
        AttachmentService attachmentProcessor)
    {
        _resourceService = resourceService ?? throw new ArgumentNullException(nameof(resourceService));
        _reservationService = reservationService ?? throw new ArgumentNullException(nameof(reservationService));
        _resourceAttachmentService = resourceAttachmentService ?? throw new ArgumentNullException(nameof(resourceAttachmentService));
        _dateProvider = dateProvider ?? throw new ArgumentNullException(nameof(dateProvider));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _openingHourseService = hoursProvider ?? throw new ArgumentNullException(nameof(hoursProvider));
        this.attachmentProcessor = attachmentProcessor;
        _options = optionsAccessor?.Value ?? throw new ArgumentException(nameof(optionsAccessor));
    }

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public class InputModel
    {

        [DataType(DataType.DateTime), DateOffset(0, 1, CompareTime = true)]
        public DateTime DateBegin { get; set; }

        [DataType(DataType.DateTime), GreaterThan(nameof(DateBegin))]
        public DateTime DateEnd { get; set; }

        public bool System { get; set; }

        public string Comment { get; set; }
    }

    public Resource Resource { get; set; }

    public IEnumerable<CalendarEvent> Reservations { get; set; }

    public DateTime CalendarDateBegin { get; set; }

    public DateTime CalendarDateEnd { get; set; }

    public IEnumerable<ResourceAttachment> Attachments { get; set; } = Enumerable.Empty<ResourceAttachment>();

    public bool CanDoReservation { get; set; } = false;

    private async Task<bool> Init(int resourceId)
    {
        // Get resource
        Resource = await _resourceService.GetResourceByIdOrNullAsync(resourceId);

        if (Resource == null) return false;
        CanDoReservation = Resource.Enabled || User.IsPrivilegedUser();

        // Get last Monday as the start date
        CalendarDateBegin = _dateProvider.Today;
        while (CalendarDateBegin.DayOfWeek != CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek) CalendarDateBegin = CalendarDateBegin.AddDays(-1);

        // Get future reservations
        Reservations = (await _reservationService.GetByResourceIdAsync(resourceId, CalendarDateBegin))
               .Select(r => new CalendarEvent
               {
                   Id = "reservation_" + r.Id,
                   BackgroundColor = r.System ? r.ResourceForegroundColor : r.ResourceBackgroundColor,
                   ForegroundColor = r.System ? r.ResourceBackgroundColor : r.ResourceForegroundColor,
                   CssClass = r.System ? "system" : string.Empty,
                   DateBegin = r.DateBegin,
                   DateEnd = r.DateEnd,
                   Name = r.System ? r.Comment : r.UserDisplayName,
                   Description = r.System ? r.UserDisplayName : r.Comment,
                   IsFullDay = false,
               });

        var lastEventEnd = Reservations.Max(x => x.DateEnd);

        CalendarDateEnd = CalendarDateBegin.AddMonths(1);

        if (lastEventEnd.HasValue && lastEventEnd > CalendarDateEnd) CalendarDateEnd = lastEventEnd.Value;

        // Get attachments
        if (_options.Features.UseAttachments)
            Attachments = await _resourceAttachmentService.GetResourceAttachmentsByAsync(resourceId);

        return true;
    }

    public async Task<IActionResult> OnGetAsync(int resourceId)
    {
        if (!await Init(resourceId)) return NotFound();

        var dt = _dateProvider.Now.AddDays(1);
        Input.DateBegin = dt.AddMinutes(-dt.Minute);
        Input.DateEnd = Input.DateBegin.AddHours(1);

        return Page();
    }

    public async Task<IActionResult> OnGetDownload(int attachmentId)
    {
        if (!_options.Features.UseAttachments) return NotFound();
        try
        {
            var result = await attachmentProcessor.GetAttachment(attachmentId);
            return File(result.Item1, "application/octet-stream", result.Item2);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }

    public async Task<IActionResult> OnPostAsync(int resourceId)
    {
        if (!(await Init(resourceId) && (Resource.Enabled || User.IsPrivilegedUser()))) return NotFound();
        if (!ModelState.IsValid) return Page();

        if (!User.IsPrivilegedUser())
        {
            // Check reservation time length
            var resLength = Input.DateEnd.Subtract(Input.DateBegin).TotalMinutes;
            if (Resource.MaximumReservationTime > 0 && resLength > Resource.MaximumReservationTime)
            {
                ModelState.AddModelError(string.Empty, string.Format(UI.My_Reservations_Err_Maxlength, Resource.MaximumReservationTime));
                return Page();
            }

            // Check if it begins and ends in the same day
            if (_options.Features.UseOpeningHours && Input.DateBegin.Date != Input.DateEnd.Date)
            {
                ModelState.AddModelError(string.Empty, UI.My_Reservations_Err_SingleDay);
                return Page();
            }

            // Check against opening times
            if (_options.Features.UseOpeningHours)
            {
                var openTime = await _openingHourseService.GetOpeningHoursAsync(Input.DateBegin);
                if (Input.DateBegin < openTime.AbsoluteOpeningTime || Input.DateEnd > openTime.AbsoluteClosingTime)
                {
                    ModelState.AddModelError(string.Empty, UI.My_Reservations_Err_OpeningHours);
                    return Page();
                }
            }
        }

        if (!ModelState.IsValid) return Page();
                     
        // Create reservation
        var result = await _reservationService.SaveAsync(Input.DateBegin,
                                                        Input.DateEnd,
                                                        int.Parse(_userManager.GetUserId(User)),
                                                        resourceId,
                                                        User.IsPrivilegedUser() && Input.System,
                                                        User.IsPrivilegedUser() ? Input.Comment : null);

        foreach (var conflict in result.Conflicts)
        {
            ModelState.AddModelError(string.Empty, string.Format(UI.My_Reservations_Err_Conflict, conflict.UserName, conflict.DateBegin));
        }

        return !ModelState.IsValid ? Page() : RedirectToPage("Reservations", string.Empty, new { resourceId }, "created");
    }
}

