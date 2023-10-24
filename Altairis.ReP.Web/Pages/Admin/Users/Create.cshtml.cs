using System.Globalization;
using Altairis.ReP.Data;
using Altairis.ReP.Data.Entities;
using Altairis.Services.Mailing.Templating;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace Altairis.ReP.Web.Pages.Admin.Users;
public class CreateModel : PageModel {
    private readonly UserManager<ApplicationUser> userManager;
    private readonly ITemplatedMailerService mailerService;
    private readonly AppSettings options;

    public CreateModel(UserManager<ApplicationUser> userManager, ITemplatedMailerService mailerService, IOptions<AppSettings> optionsAccessor) {
        this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        this.mailerService = mailerService ?? throw new ArgumentNullException(nameof(mailerService));
        options = optionsAccessor?.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));
    }

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public class InputModel {

        [Required, MaxLength(50)]
        public string UserName { get; set; }

        [Required, MaxLength(100)]
        public string DisplayName { get; set; }

        public bool ShowInMemberDirectory { get; set; }

        [Required, MaxLength(50), EmailAddress]
        public string Email { get; set; }

        [MaxLength(20), Phone]
        public string PhoneNumber { get; set; }

        public bool IsMaster { get; set; }

        public bool IsAdministrator { get; set; }

        public string Language { get; set; } = "cs-CZ";

    }

    public IEnumerable<SelectListItem> AllLanguages { get; } = new List<SelectListItem>() {
        new SelectListItem(UI.My_Settings_Index_Language_CS, "cs-CZ"),
        new SelectListItem(UI.My_Settings_Index_Language_EN, "en-US")
    };

    public void OnGet() {
        Input.ShowInMemberDirectory = options.Features.UseMemberDirectory;
    }

    public async Task<IActionResult> OnPostAsync() {
        if (!ModelState.IsValid) return Page();

        // Create new user
        var newUser = new ApplicationUser {
            UserName = Input.UserName,
            Email = Input.Email,
            PhoneNumber = Input.PhoneNumber,
            Language = Input.Language,
            DisplayName = Input.DisplayName,
            ShowInMemberDirectory = options.Features.UseMemberDirectory && Input.ShowInMemberDirectory
        };
        var result = await userManager.CreateAsync(newUser);
        if (!this.IsIdentitySuccess(result)) return Page();

        // Assign roles
        if (Input.IsMaster) await userManager.AddToRoleAsync(newUser, ApplicationRole.Master);
        if (Input.IsAdministrator) await userManager.AddToRoleAsync(newUser, ApplicationRole.Administrator);

        // Get e-mail confirmation URL
        var token = await userManager.GenerateEmailConfirmationTokenAsync(newUser);
        var activationUrl = Url.Page("/Login/Activate", pageHandler: null, values: new { UserId = newUser.Id, Token = token }, protocol: Request.Scheme);

        // Send welcome mail
        var culture = new CultureInfo(Input.Language);
        var msg = new TemplatedMailMessageDto("Activation", newUser.Email);
        await mailerService.SendMessageAsync(msg, new {
            userName = newUser.UserName,
            url = activationUrl
        }, culture, culture);

        return RedirectToPage("Index", null, "created");
    }

}
