namespace Altairis.ReP.Web.Pages.Admin.DirectoryEntries;

public class EditModel : PageModel
{
    private readonly IDirectoryEntryService _service;

    public EditModel(IDirectoryEntryService service) 
        => _service = service ?? throw new ArgumentNullException(nameof(service));

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public class InputModel
    {

        [Required, MaxLength(100)]
        public string DisplayName { get; set; }

        [MaxLength(100), EmailAddress]
        public string Email { get; set; }

        [MaxLength(50), Phone]
        public string PhoneNumber { get; set; }

    }

    public async Task<IActionResult> OnGetAsync(int directoryEntryId, CancellationToken token)
    {
        var de = await _service.GetDirectoryEntryOrNullAsync(directoryEntryId, token);
        
        if (de is null) return NotFound();

        Input = new InputModel
        {
            DisplayName = de.DisplayName,
            Email = de.Email,
            PhoneNumber = de.PhoneNumber
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int directoryEntryId, CancellationToken token)
    {
        if (!ModelState.IsValid) return Page();

        return await _service.SaveAsync(directoryEntryId, Input.DisplayName, Input.Email, Input.PhoneNumber, token) == CommandStatus.NotFound
            ? NotFound()
            : RedirectToPage("Index", null, "saved");
    }

    public async Task<IActionResult> OnPostDeleteAsync(int directoryEntryId, CancellationToken token)
    {
        return await _service.DeleteAsync(directoryEntryId, token) == CommandStatus.NotFound
            ? NotFound()
            : RedirectToPage("Index", null, "deleted");
    }

}
