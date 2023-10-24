using Altairis.ReP.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Altairis.ReP.Web.Pages.Login;
public class ActivateModel : PageModel {
    private readonly UserManager<ApplicationUser> userManager;

    public ActivateModel(UserManager<ApplicationUser> userManager) {
        this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public string NewUserName { get; set; }

    public class InputModel {

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

    }

    public async Task<IActionResult> OnGetAsync(int userId, string token) {
        // Get user
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null || user.EmailConfirmed) return RedirectToPage("Index", null, "afail");
        NewUserName = user.UserName;

        // Try to confirm e-mail address
        var result = await userManager.ConfirmEmailAsync(user, token);
        if (!this.IsIdentitySuccess(result)) return RedirectToPage("Index", null, "afail");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int userId) {
        if (!ModelState.IsValid) return Page();

        // Get user
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null) return NotFound();
        NewUserName = user.UserName;


        // Try to set password
        var result = await userManager.AddPasswordAsync(user, Input.Password);
        if (!this.IsIdentitySuccess(result)) return Page();

        return RedirectToPage("Index", null, "adone");
    }

}
