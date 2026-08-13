using RescueLink.Domain.Common;

namespace RescueLink.Domain.Entities;

public class PetReportPhoto : BaseEntity
{
    public Guid PetReportId { get; private set; }
    public string StorageKey { get; private set; } = string.Empty;
    public bool IsPrimary { get; private set; }
    public int DisplayOrder { get; private set; }

    private PetReportPhoto()
    {
    }

    internal static PetReportPhoto Create(
        Guid petReportId,
        string storageKey,
        bool isPrimary,
        int displayOrder)
    {
        if (petReportId == Guid.Empty)
        {
            throw new ArgumentException(
                "Pet report ID cannot be empty.",
                nameof(petReportId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(storageKey);

        if (displayOrder < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(displayOrder),
                "Display order cannot be negative.");
        }

        return new PetReportPhoto
        {
            PetReportId = petReportId,
            StorageKey = storageKey.Trim(),
            IsPrimary = isPrimary,
            DisplayOrder = displayOrder
        };
    }

    internal void SetAsPrimary()
    {
        IsPrimary = true;
    }

    internal void RemovePrimaryStatus()
    {
        IsPrimary = false;
    }
    internal void UpdateDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(displayOrder),
                "Display order cannot be negative.");
        }

        DisplayOrder = displayOrder;
    }
}