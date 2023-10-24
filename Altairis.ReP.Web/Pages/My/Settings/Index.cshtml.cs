using Altairis.ReP.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Altairis.ReP.Web.Pages.My.Settings;
public class IndexModel : PageModel {
    private readonly UserManager<ApplicationUser> userManager;

    public IndexModel(UserManager<ApplicationUser> userManager) {
        this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public class InputModel {

        public string Language { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        public bool SendNotifications { get; set; }

        public bool SendNews { get; set; }

        [Required, MaxLength(100)]
        public string DisplayName { get; set; }

        public bool ShowInMemberDirectory { get; set; }

    }

    public IEnumerable<SelectListItem> AllLanguages { get; } = new List<SelectListItem>() {
        new SelectListItem(UI.My_Settings_Index_Language_CS, "cs-CZ"),
        new SelectListItem(UI.My_Settings_Index_Language_EN, "en-US")
    };

    public ApplicationUser Me { get; set; }

    public async Task OnGetAsync() {
        Me = await userManager.GetUserAsync(User);
        Input.Language = Me.Language;
        Input.PhoneNumber = Me.PhoneNumber;
        Input.SendNotifications = Me.SendNotifications;
        Input.SendNews = Me.SendNews;
        Input.DisplayName = Me.DisplayName;
        Input.ShowInMemberDirectory = Me.ShowInMemberDirectory;
    }

    public async Task<IActionResult> OnPostAsync() {
        if (!ModelState.IsValid) return Page();
        Me = await userManager.GetUserAsync(User);

        Me.Language = Input.Language;
        Me.PhoneNumber = Input.PhoneNumber;
        Me.SendNews = Input.SendNews;
        Me.SendNotifications = Input.SendNotifications;
        Me.DisplayName= Input.DisplayName;
        Me.ShowInMemberDirectory = Input.ShowInMemberDirectory;
        await userManager.UpdateAsync(Me);

        return RedirectToPage("Index", null, "saved");
    }

}
