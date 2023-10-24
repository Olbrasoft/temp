using Altairis.ReP.Data.Dtos.NewsMessageDtos;

namespace Altairis.ReP.Web.Pages.Admin.NewsMessages;
public class EditModel : PageModel
{
    private readonly INewsMessageService _service;

    public EditModel(INewsMessageService service) 
        => _service = service ?? throw new ArgumentNullException(nameof(service));

    [BindProperty]
    public NewsMessageDto Input { get; set; } = new NewsMessageDto();

    public async Task<IActionResult> OnGetAsync(int newsMessageId)
    {
        var newsMessageDto = await _service.GetNewsMessageOrNullByAsync(newsMessageId);
        if (newsMessageDto == null) return NotFound();

        Input = newsMessageDto;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int newsMessageId, CancellationToken token) 
        =>  ModelState.IsValid
            ? await _service.SaveAsync(newsMessageId, Input.Title, Input.Text, token) == CommandStatus.NotFound
            ? NotFound()
            : RedirectToPage("Index", null, "saved")
            : Page();

    public async Task<IActionResult> OnPostDeleteAsync(int newsMessageId, CancellationToken token) 
        => await _service.DeleteAsync(newsMessageId, token) == CommandStatus.NotFound
            ? NotFound()
            : RedirectToPage("Index", null, "deleted");
}
