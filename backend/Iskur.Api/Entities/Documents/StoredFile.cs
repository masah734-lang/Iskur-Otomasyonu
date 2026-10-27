using Iskur.Api.Entities.Common;
using Iskur.Api.Entities.Identity;

namespace Iskur.Api.Entities.Documents;

public class StoredFile : BaseEntity
{
    public string OriginalName { get; set; } = string.Empty;

    public string StorageKey { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public Guid UploadedById { get; set; }

    public ApplicationUser UploadedBy { get; set; } = null!;
}