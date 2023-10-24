using Altairis.ReP.Data;
using Altairis.Services.DateProvider;
using Altairis.TagHelpers;
using MapsterMapper;
using Microsoft.Extensions.Options;

namespace Altairis.ReP.Web.Pages.My;

public partial class CalendarModel : PageModel
{
    private readonly IReservationService _reservationService;
    private readonly IResourceService _resourceService;
    private readonly ICalendarEntryService _calendarEntryService;
    private readonly IMapper _mapper;

    private readonly IDateProvider dateProvider;
    private readonly IOptions<AppSettings> options;

    public CalendarModel(IReservationService reservationService, IResourceService resourceService, ICalendarEntryService calendarEntryService, IMapper mapper, IDateProvider dateProvider, IOptions<AppSettings> options)
    {
        _reservationService = reservationService ?? throw new ArgumentNullException(nameof(reservationService));
        _resourceService = resourceService ?? throw new ArgumentNullException(nameof(resourceService));
        _calendarEntryService = calendarEntryService ?? throw new ArgumentNullException(nameof(calendarEntryService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.dateProvider = dateProvider ?? throw new ArgumentNullException(nameof(dateProvider));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public bool CanManageEntries => options.Value.Features.UseCalendarEntries && User.IsPrivilegedUser();

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public class InputModel
    {

        [Required, DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today.AddDays(1);

        [Required, MaxLength(50)]
        public string Title { get; set; }

        [DataType("Markdown")]
        public string Comment { get; set; }

    }

    public IEnumerable<CalendarEvent> Reservations { get; set; }

    public IEnumerable<ResourceTag> Resources { get; set; }

    public IEnumerable<CalendarEntryInfo> CalendarEntries { get; set; }

    public class CalendarEntryInfo
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public string Comment { get; set; }
    }

    public class ResourceTag
    {
        public string Name { get; set; } = String.Empty;
        public string ForegroundColor { get; set; } = String.Empty;
        public string BackgroundColor { get; set; } = String.Empty;
        public string GetStyle() => $"color:{ForegroundColor};background-color:{BackgroundColor};";
    }

    public DateTime DateBegin { get; set; }

    public DateTime DateEnd { get; set; }

    public DateTime DatePrev { get; set; }

    public DateTime DateNext { get; set; }

    public async Task<IActionResult> OnGetAsync(int? year, int? month, CancellationToken token)
    {
        // Redirect to current month
        if (!year.HasValue || !month.HasValue) return RedirectToPage(new { dateProvider.Today.Year, dateProvider.Today.Month });

        // Initialize data
        await Init(year, month, token);
        return Page();
    }

    public async Task<IActionResult> OnGetDeleteAsync(int? year, int? month, int entryId, CancellationToken token)
    {
        // Initialize data
        await Init(year, month, token);

        // Delete entry
        if (CanManageEntries)
        {
            await _calendarEntryService.DeleteCalendarEntryByIdAsync(entryId, token);
        }

        return RedirectToPage(pageName: null, pageHandler: null, fragment: string.Empty);
    }

    public async Task<IActionResult> OnPostAsync(int? year, int? month, CancellationToken token)
    {
        // Initialize data
        await Init(year, month,token);

        // Validate arguments
        if (!ModelState.IsValid || !CanManageEntries) return Page();

        // Create new entry
        await _calendarEntryService.SaveAsync(Input.Date, Input.Title, Input.Comment, token);


        return RedirectToPage(pageName: null, pageHandler: null, fragment: string.Empty);
    }

    private async Task Init(int? year, int? month, CancellationToken token)
    {
        // Get month name for display
        DateBegin = new DateTime(year.Value, month.Value, 1);
        DateEnd = DateBegin.AddMonths(1).AddDays(-1);
        DatePrev = DateBegin.AddMonths(-1);
        DateNext = DateBegin.AddMonths(+1);

        // Get all resources for tags
        Resources = _mapper.Map<IEnumerable<ResourceTag>>(await _resourceService.GetResourceTagsAsync(token));

        var ri = (await _reservationService.GetBetweenDatesAsync(DateBegin.AddDays(-6), DateEnd.AddDays(6),token))
              .Select(rwdid => new CalendarEvent
              {
                  Id = "reservation_" + rwdid.Id,
                  BackgroundColor = rwdid.System ? rwdid.ResourceForegroundColor : rwdid.ResourceBackgroundColor,
                  ForegroundColor = rwdid.System ? rwdid.ResourceBackgroundColor : rwdid.ResourceForegroundColor,
                  CssClass = rwdid.System ? "system" : string.Empty,
                  DateBegin = rwdid.DateBegin,
                  DateEnd = rwdid.DateEnd,
                  Name = rwdid.System ? rwdid.Comment : rwdid.UserDisplayName,
                  Description = rwdid.System ? rwdid.UserDisplayName : rwdid.Comment,
                  IsFullDay = false
              });

        var calendarEntries = await _calendarEntryService.GetBetweenDatesAsync(DateBegin.AddDays(-6), DateEnd.AddDays(6),token);

        var ei = calendarEntries.Select(ce => new CalendarEvent
        {
            Id = "event_" + ce.Id,
            BackgroundColor = options.Value.Design.CalendarEntryBgColor,
            ForegroundColor = options.Value.Design.CalendarEntryFgColor,
            DateBegin = ce.Date,
            DateEnd = ce.Date,
            Name = ce.Title,
            IsFullDay = true,
            Href = "#event_detail_" + ce.Id,
        });

        Reservations = ei.Concat(ri);

        CalendarEntries = calendarEntries.Select(ce => new CalendarEntryInfo
        {
            Id = ce.Id,
            Date = ce.Date,
            Title = ce.Title,
            Comment = ce.Comment
        });
    }

}
