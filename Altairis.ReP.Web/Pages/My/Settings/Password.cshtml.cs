using Altairis.ReP.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Altairis.ReP.Web.Pages.My.Settings;
public class PasswordModel : PageModel {
    private readonly UserManager<ApplicationUser> userManager;

    public PasswordModel(UserManager<ApplicationUser> userManager) {
        this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public class InputModel {

        [Required, DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required, DataType(DataType.Password)]
        public string NewPassword { get; set; }

    }

    public async Task<IActionResult> OnPostAsync() {
        if (!ModelState.IsValid) return Page();
        var me = await userManager.GetUserAsync(User);
        var result = await userManager.ChangePasswordAsync(me, Input.CurrentPassword, Input.NewPassword);
        return this.IsIdentitySuccess(result) ? RedirectToPage("Index", null, "changepassword") : (IActionResult)Page();
    }

}
