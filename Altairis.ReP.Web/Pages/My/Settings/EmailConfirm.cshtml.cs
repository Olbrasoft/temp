using Altairis.ReP.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Altairis.ReP.Web.Pages.My.Settings;
public class EmailConfirmModel : PageModel {
    private readonly UserManager<ApplicationUser> userManager;

    public EmailConfirmModel(UserManager<ApplicationUser> userManager) {
        this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    public async Task<IActionResult> OnGetAsync(string newEmail, string token) {
        var me = await userManager.GetUserAsync(User);
        if (me.Email.Equals(newEmail, StringComparison.OrdinalIgnoreCase)) return RedirectToPage("Index", null, "changeemaildone");

        var result = await userManager.ChangeEmailAsync(me, newEmail, token);
        return result.Succeeded ? RedirectToPage("Index", null, "changeemaildone") : (IActionResult)Page();
    }
}
