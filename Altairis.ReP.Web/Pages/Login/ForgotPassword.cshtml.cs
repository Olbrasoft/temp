using Altairis.ReP.Data.Entities;
using Altairis.Services.Mailing.Templating;
using Microsoft.AspNetCore.Identity;

namespace Altairis.ReP.Web.Pages.Login;
public class ForgotPasswordModel : PageModel {
    private readonly UserManager<ApplicationUser> userManager;
    private readonly ITemplatedMailerService mailerService;

    public ForgotPasswordModel(UserManager<ApplicationUser> userManager, ITemplatedMailerService mailerService) {
        this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        this.mailerService = mailerService ?? throw new ArgumentNullException(nameof(mailerService));
    }

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public class InputModel {

        [Required]
        public string UserName { get; set; }

    }

    public async Task<IActionResult> OnPostAsync(string locale) {
        if (!ModelState.IsValid) return Page();

        // Try to find user by name
        var user = await userManager.FindByNameAsync(Input.UserName).ConfigureAwait(false);
        if (user == null) {
            ModelState.AddModelError(nameof(Input.UserName), UI.Login_ForgotPassword_UserNotFound);
            return Page();
        }

        // Get password reset token
        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        // Get password reset URL
        var passwordResetUrl = Url.Page("/Login/ResetPassword",
            pageHandler: null,
            values: new { userId = user.Id, token = token },
            protocol: Request.Scheme);

        // Send password reset mail
        var msg = new TemplatedMailMessageDto("PasswordReset", user.Email);
        await mailerService.SendMessageAsync(msg, new {
            userName = user.UserName,
            url = passwordResetUrl
        }).ConfigureAwait(false);

        return RedirectToPage("Index", null, "sent");
    }

}
