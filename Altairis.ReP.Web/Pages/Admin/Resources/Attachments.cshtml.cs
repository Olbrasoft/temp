using Altairis.ReP.Data;
using Altairis.ReP.Data.Entities;
using Microsoft.Extensions.Options;

namespace Altairis.ReP.Web.Pages.Admin.Resources
{
    public class AttachmentsModel : PageModel
    {
        private readonly IResourceAttachmentService _attachmentService;
        private readonly IResourceService _resourceService;
        private readonly AttachmentService attachmentProcessor;
        private readonly IOptions<AppSettings> options;

        public AttachmentsModel(IResourceAttachmentService attachmetService, IResourceService resourceService, AttachmentService attachmentProcessor, IOptions<AppSettings> options)
        {
            _attachmentService = attachmetService ?? throw new ArgumentNullException(nameof(attachmetService));
            _resourceService = resourceService ?? throw new ArgumentNullException(nameof(resourceService));
            this.attachmentProcessor = attachmentProcessor;
            this.options = options;
        }

        public string ResourceName { get; set; }

        public IEnumerable<ResourceAttachment> Items { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public class InputModel
        {
            public IFormFile File { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int resourceId, CancellationToken token)
        {
            if (!await Init(resourceId, token)) return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int resourceId, CancellationToken token)
        {
            if (!await Init(resourceId, token)) return NotFound();
            if ((Input?.File?.Length ?? 0) > 0) await attachmentProcessor.CreateAttachment(Input.File, resourceId);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetDeleteAttachment(int attachmentId)
        {
            if (!options.Value.Features.UseAttachments) return NotFound();
            await attachmentProcessor.DeleteAttachment(attachmentId);
            return RedirectToPage();
        }

        private async Task<bool> Init(int resourceId, CancellationToken token)
        {
            if (!options.Value.Features.UseAttachments) return false;
            var res = await _resourceService.GetResourceByIdOrNullAsync(resourceId, token);
            if (res == null) return false;

            ResourceName = res.Name;
            Items = await _attachmentService.GetResourceAttachmentsByAsync(resourceId, token);
            return true;
        }
    }
}
