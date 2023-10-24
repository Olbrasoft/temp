using Altairis.ReP.Data.Entities;
using Microsoft.AspNetCore.Identity;
using System.Globalization;

namespace Altairis.ReP.Web.Pages;
public class FirstRunModel : PageModel
{
    private readonly IUserService _service;
    private readonly UserManager<ApplicationUser> userManager;

    public FirstRunModel(IUserService service, UserManager<ApplicationUser> userManager)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public class InputModel
    {
        [Required, MaxLength(50)]
        public string UserName { get; set; }

        [Required, MaxLength(100)]
        public string DisplayName { get; set; }

        [Required, MaxLength(50), EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public async Task<IActionResult> OnGet(CancellationToken token)
    {
        if (await _service.IsThereAnyUserAsync(token)) return NotFound();
        Input.Password = SecurityHelper.GenerateRandomPassword();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken token)
    {
        if (await _service.IsThereAnyUserAsync(token)) return NotFound();
        if (!ModelState.IsValid) return Page();

        // Create user
        var user = new ApplicationUser
        {
            UserName = Input.UserName,
            DisplayName = Input.DisplayName,
            Email = Input.Email,
            EmailConfirmed = true,
            Language = CultureInfo.CurrentUICulture.Name,
            Enabled = true
        };
        if (!this.IsIdentitySuccess(await userManager.CreateAsync(user, Input.Password))) return Page();

        // Assign Administrator role
        if (!this.IsIdentitySuccess(await userManager.AddToRoleAsync(user, ApplicationRole.Administrator))) return Page();

        // Redirect to home page
        return RedirectToPage("Index");
    }
}
