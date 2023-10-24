using Altairis.ReP.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Altairis.ReP.Web.Pages.Login;
public class IndexModel : PageModel {
    private readonly IUserService _service;
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly UserManager<ApplicationUser> userManager;

    public IndexModel(IUserService service, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager) {
        _service = service;
        this.signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public class InputModel {

        [Required, MaxLength(50)]
        public string UserName { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }

    public async Task<IActionResult> OnGet(CancellationToken token) => await  _service.IsThereAnyUserAsync(token) ? Page() : RedirectToPage("/FirstRun");

    public async Task<IActionResult> OnPostAsync(string returnUrl = "/") {
        if (ModelState.IsValid) {
            var result = await signInManager.PasswordSignInAsync(
                Input.UserName,
                Input.Password,
                Input.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded) {
                return LocalRedirect(returnUrl);
            } else {
                ModelState.AddModelError(string.Empty, Resources.UI.Login_Index_LoginFailed);
            }
        }
        return Page();
    }

}
