using Altairis.ReP.Data;
using Altairis.ReP.Data.Entities;
using Altairis.ValidationToolkit;

namespace Altairis.ReP.Web.Pages.Admin;
public class OpeningHoursModel : PageModel
{
    

    private readonly IOpeningHoursService _openingHoursService;

    public OpeningHoursModel( IOpeningHoursService hoursProvider)
    {
       
        _openingHoursService = hoursProvider ?? throw new ArgumentNullException(nameof(hoursProvider));
    }

    public IEnumerable<OpeningHoursInfo> StandardOpeningHours => _openingHoursService.GetStandardOpeningHours();

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public IEnumerable<OpeningHoursChange> OpeningHoursChanges { get; set; }

    public class InputModel
    {

        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today.AddDays(1);

        [DataType(DataType.Time), Range(typeof(TimeSpan), "00:00:00", "23:59:59")]
        public TimeSpan OpeningTime { get; set; } = TimeSpan.Zero;

        [DataType(DataType.Time), Range(typeof(TimeSpan), "00:00:00", "23:59:59"), GreaterThan(nameof(OpeningTime), AllowEqual = true)]
        public TimeSpan ClosingTime { get; set; } = TimeSpan.Zero;

    }

    public async Task OnGetAsync(CancellationToken token) => OpeningHoursChanges = await _openingHoursService.GetOpeningHoursChangesAsync(token);

    public async Task<IActionResult> OnPostAsync(CancellationToken token)
    {
        if (!ModelState.IsValid) return Page();

        await _openingHoursService.SaveOpeningHoursChangeAsync(Input.Date, Input.OpeningTime, Input.ClosingTime, token);

        return RedirectToPage(string.Empty, null, "created");
    }

    public async Task<IActionResult> OnGetDeleteAsync(int ohchId, CancellationToken token)
    {
        if (await _openingHoursService.DeleteOpeningHoursChangeAsync(ohchId, token) == CommandStatus.NotFound) return NotFound();

        return RedirectToPage(string.Empty, null, "deleted");
    }

}
