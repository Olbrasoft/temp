using Altairis.Services.DateProvider;
using Microsoft.AspNetCore.Http;
using Storage.Net.Blobs;

namespace Altairis.ReP.Web.Services;
public class AttachmentService
{
    private const string AttachmentPath = "attachments/{0:0000}/{1:yyyyMMddHHmmss}-{2:n}{3}";

    private readonly IBlobStorage _blobStorage;
    private readonly IDateProvider _dateProvider;
    private readonly IResourceAttachmentService _service;

    public AttachmentService(IBlobStorage blobStorage, IDateProvider dateProvider, IResourceAttachmentService service)
    {
        _blobStorage = blobStorage ?? throw new ArgumentNullException(nameof(blobStorage));
        _dateProvider = dateProvider ?? throw new ArgumentNullException(nameof(dateProvider));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async Task<ResourceAttachment> CreateAttachment(IFormFile formFile, int resourceId)
    {

        var created = _dateProvider.Now;
        var storagePath = string.Format(AttachmentPath,
             resourceId,                            // 0
             created,                               // 1
             Guid.NewGuid(),                        // 2
             Path.GetExtension(formFile.FileName)); //3

        // Upload file to storage
        using var stream = formFile.OpenReadStream();
        await _blobStorage.WriteAsync(storagePath, stream);

        // Create attachment
        return await _service.SaveResourceAttachmentAsync(created,
                                                   resourceId,
                                                   Path.GetFileName(formFile.FileName),
                                                   formFile.Length,
                                                   storagePath
                                                   );
    }

    public async Task DeleteAttachment(int resourceAttachmentId)
    {
        // Get attachment info
        var a = await _service.GetAttachmentOrNullByAsync(resourceAttachmentId);
        if (a == null) return; // Already deleted

        // Delete attachment from storage
        await _blobStorage.DeleteAsync(a.StoragePath);

        // Delete attachment from database
        await _service.DeleteResourceAttachmentAsync(resourceAttachmentId);
    }

    public async Task<(byte[], string)> GetAttachment(int resourceAttachmentId)
    {
        // Get attachment info
        var a = await _service.GetAttachmentOrNullByAsync(resourceAttachmentId);
        if (a == null) throw new FileNotFoundException();

        // Get data from storage
        var data = await _blobStorage.ReadBytesAsync(a.StoragePath);

        // Send data
        return new(data, a.FileName);
    }
}
