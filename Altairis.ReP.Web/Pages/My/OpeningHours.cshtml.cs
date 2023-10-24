using Altairis.ReP.Data;
using Olbrasoft.ReP.Business;

namespace Altairis.ReP.Web.Pages.My;
public class OpeningHoursPageModel : PageModel {
    private readonly IOpeningHoursService _service;

    public OpeningHoursPageModel(IOpeningHoursService hoursProvider) {
        _service = hoursProvider ?? throw new ArgumentNullException(nameof(hoursProvider));
    }



    public IEnumerable<OpeningHoursInfo> OpeningHours
    {
        get
        {
            return _service.GetOpeningHours(0, 14);
        }
    }

    public async Task<string> GetString()
    {
        return await Task.FromResult(string.Empty);
    }
}
