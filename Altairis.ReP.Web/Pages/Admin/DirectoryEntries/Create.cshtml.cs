namespace Altairis.ReP.Web.Pages.Admin.DirectoryEntries;

public class CreateModel : PageModel {
    private readonly IDirectoryEntryService _service;


    public CreateModel(IDirectoryEntryService service) 
        => _service = service ?? throw new ArgumentNullException(nameof(service));

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public class InputModel {

        [Required, MaxLength(100)]
        public string DisplayName { get; set; }

        [MaxLength(100), EmailAddress]
        public string Email { get; set; }

        [MaxLength(50), Phone]
        public string PhoneNumber { get; set; }
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken token)
    {
        if (!ModelState.IsValid) return Page();
               
        await _service.SaveAsync(Input.DisplayName, Input.Email, Input.PhoneNumber, token);

        return RedirectToPage("Index", null, "created");
    }
}
