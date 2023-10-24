using Altairis.ReP.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Altairis.ReP.Web.Pages.Login;
public class LogoutModel : PageModel {
    private readonly SignInManager<ApplicationUser> signInManager;

    public LogoutModel(SignInManager<ApplicationUser> signInManager) {
        this.signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
    }

    public async Task<IActionResult> OnGetAsync() {
        await signInManager.SignOutAsync();
        return RedirectToPage("Index", null, "logout");
    }
}

