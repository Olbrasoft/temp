namespace Altairis.ReP.Web.Pages;
public class IndexModel : PageModel {
    public IActionResult OnGet() => RedirectToPage("/My/Index");
}
