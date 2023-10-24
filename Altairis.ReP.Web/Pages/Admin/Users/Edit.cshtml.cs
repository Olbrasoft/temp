using Altairis.ReP.Data;
using Altairis.ReP.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace Altairis.ReP.Web.Pages.Admin.Users;
public class EditModel : PageModel {
    private readonly UserManager<ApplicationUser> userManager;
    private readonly AppSettings options;

    public EditModel(UserManager<ApplicationUser> userManager, IOptions<AppSettings> optionsAccessor) {
        this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        options = optionsAccessor?.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));
    }

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public class InputModel {

        [Required, MaxLength(50)]
        public string UserName { get; set; }

        [Required, MaxLength(50), EmailAddress]
        public string Email { get; set; }

        public string Language { get; set; }

        [MaxLength(20), Phone]
        public string PhoneNumber { get; set; }

        public bool IsMaster { get; set; }

        public bool IsAdministrator { get; set; }

        public bool UserEnabled { get; set; }

        [Required, MaxLength(100)]
        public string DisplayName { get; set; }

        public bool ShowInMemberDirectory { get; set; }

    }

    public IEnumerable<SelectListItem> AllLanguages { get; } = new List<SelectListItem>() {
        new SelectListItem(UI.My_Settings_Index_Language_CS, "cs-CZ"),
        new SelectListItem(UI.My_Settings_Index_Language_EN, "en-US")
    };

    public async Task<IActionResult> OnGetAsync(int userId) {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null) return NotFound();

        Input = new InputModel {
            Email = user.Email,
            UserEnabled = user.Enabled,
            Language = user.Language,
            PhoneNumber = user.PhoneNumber,
            UserName = user.UserName,
            IsAdministrator = await userManager.IsInRoleAsync(user, ApplicationRole.Administrator),
            IsMaster = await userManager.IsInRoleAsync(user, ApplicationRole.Master),
            DisplayName = user.DisplayName,
            ShowInMemberDirectory = user.ShowInMemberDirectory
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int userId) {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null) return NotFound();

        if (!ModelState.IsValid) return Page();

        user.Email = Input.Email;
        user.Enabled = Input.UserEnabled;
        user.PhoneNumber = Input.PhoneNumber;
        user.UserName = Input.UserName;
        user.Language = Input.Language;
        user.DisplayName = Input.DisplayName;
        user.ShowInMemberDirectory = options.Features.UseMemberDirectory && Input.ShowInMemberDirectory;

        var result = await userManager.UpdateAsync(user);
        if (!this.IsIdentitySuccess(result)) return Page();

        Task<IdentityResult> SetUserMembership(ApplicationUser user, string role, bool status) => status ? userManager.AddToRoleAsync(user, role) : userManager.RemoveFromRoleAsync(user, role);

        await SetUserMembership(user, ApplicationRole.Administrator, Input.IsAdministrator);
        await SetUserMembership(user, ApplicationRole.Master, Input.IsMaster);

        return RedirectToPage("Index", null, "saved");
    }

    public async Task<IActionResult> OnPostDeleteAsync(int userId) {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null) return NotFound();

        await userManager.DeleteAsync(user);

        return RedirectToPage("Index", null, "deleted");
    }
}
