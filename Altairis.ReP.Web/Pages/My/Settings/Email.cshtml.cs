using Altairis.ReP.Data.Entities;
using Altairis.Services.Mailing.Templating;
using Microsoft.AspNetCore.Identity;

namespace Altairis.ReP.Web.Pages.My.Settings;
public class EmailModel : PageModel {
    private readonly UserManager<ApplicationUser> userManager;
    private readonly ITemplatedMailerService mailerService;

    public EmailModel(UserManager<ApplicationUser> userManager, ITemplatedMailerService mailerService) {
        this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        this.mailerService = mailerService ?? throw new ArgumentNullException(nameof(mailerService));
    }

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public class InputModel {

        [Required, EmailAddress]
        public string Email { get; set; }


        [Required, DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

    }

    public async Task OnGetAsync() {
        var me = await userManager.GetUserAsync(User);
        Input.Email = me.Email;
    }

    public async Task<IActionResult> OnPostAsync() {
        if (!ModelState.IsValid) return Page();

        // Check if the address is really changed
        var me = await userManager.GetUserAsync(User);
        if (me.Email.Equals(Input.Email, StringComparison.OrdinalIgnoreCase)) return RedirectToPage("Index");

        // Check password
        var passwordCorrect = await userManager.CheckPasswordAsync(me, Input.CurrentPassword);
        if (!passwordCorrect) {
            ModelState.AddModelError(nameof(Input.CurrentPassword), UI.My_Settings_Email_InvalidPassword);
            return Page();
        }

        // Get email change token
        var token = await userManager.GenerateChangeEmailTokenAsync(me, Input.Email);

        // Get email change confirmation URL
        var url = Url.Page("/My/Settings/EmailConfirm",
            pageHandler: null,
            values: new {
                newEmail = Input.Email,
                token = token
            },
            protocol: Request.Scheme);

        // Send message
        var msg = new TemplatedMailMessageDto("EmailConfirm", Input.Email);
        await mailerService.SendMessageAsync(msg, new {
            userName = me.UserName,
            url = url
        });

        return RedirectToPage("Index", null, "changeemail");
    }
}
