using RescueLink.Domain.Common;
using RescueLink.Domain.Enums;
using RescueLink.Domain.Events;
using RescueLink.Domain.ValueObjects;

namespace RescueLink.Domain.Entities;
public class PetReport : BaseEntity
{
    public Guid UserId { get; private set; }
    public ReportType ReportType { get; private set; }

    public ReportStatus Status { get; private set; } =
        ReportStatus.Active;

    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public AnimalSpecies Species { get; private set; }
    public AnimalGender Gender { get; private set; }
    public string? PetName { get; private set; }
    public string? Breed { get; private set; }
    public AnimalColor PrimaryColor { get; private set; }
    public AnimalColor? SecondaryColor { get; private set; }
    public DateTimeOffset EventDate { get; private set; }
    public bool IsArchived { get; private set; }

    public DateTimeOffset? ArchivedAt{ get; private set; }
    public void Archive()
    {
        if (IsArchived)
        {
            return;
        }

        var archivedAt = DateTimeOffset.UtcNow;

        IsArchived = true;
        ArchivedAt = archivedAt;
        UpdatedAt = archivedAt;
    }
   
    public GeoLocation Location { get; private set; } = null!;

    public const int MaximumPhotoCount = 5;

    private readonly List<PetReportPhoto> _photos = [];

    public IReadOnlyCollection<PetReportPhoto> Photos =>
        _photos.AsReadOnly();

    public bool CanAddPhoto => !IsArchived && _photos.Count < MaximumPhotoCount;

    private PetReport()
    {
    }

    public static PetReport Create(
        Guid userId,
        ReportType reportType,
        string title,
        string description,
        AnimalSpecies species,
        AnimalGender gender,
        string? petName,
        string? breed,
        AnimalColor primaryColor,
        AnimalColor? secondaryColor,
        DateTimeOffset eventDate,
        GeoLocation location)
    {
        ValidateCreation(
            userId: userId,
            reportType: reportType,
            title: title,
            description: description,
            species: species,
            gender: gender,
            primaryColor: primaryColor,
            secondaryColor: secondaryColor,
            eventDate: eventDate,
            location: location);

        var petReport = new PetReport
        {
            UserId = userId,
            ReportType = reportType,
            Status = ReportStatus.Active,
            Title = title.Trim(),
            Description = description.Trim(),
            Species = species,
            Gender = gender,
            PetName = NormalizeOptionalText(petName),
            Breed = NormalizeOptionalText(breed),
            PrimaryColor = primaryColor,
            SecondaryColor = secondaryColor,
            EventDate = eventDate,
            Location = location,
            IsArchived = false,
            ArchivedAt = null
        };

        petReport.RaiseDomainEvent(
            new PetReportCreatedDomainEvent(
                petReport.Id));

        return petReport;
    }

    public void UpdateDetails(

        string title,
        string description,
        AnimalSpecies species,
        AnimalGender gender,
        string? petName,
        string? breed,
        AnimalColor primaryColor,
        AnimalColor? secondaryColor,
        DateTimeOffset eventDate,
        GeoLocation location)
    {
        EnsureNotArchived();
        if (Status != ReportStatus.Active)
        {
            throw new InvalidOperationException(
                "Only active reports can be updated.");
        }

        ValidateCreation(
            userId: UserId,
            reportType: ReportType,
            title: title,
            description: description,
            species: species,
            gender: gender,
            primaryColor: primaryColor,
            secondaryColor: secondaryColor,
            eventDate: eventDate,
            location: location);

        Title = title.Trim();
        Description = description.Trim();
        Species = species;
        Gender = gender;
        PetName = NormalizeOptionalText(petName);
        Breed = NormalizeOptionalText(breed);
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
        EventDate = eventDate;
        Location = location;
        UpdatedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(
            new PetReportUpdatedDomainEvent(
                PetReportId: Id));
    }

    public void Resolve()
    {
        EnsureNotArchived();
        if (Status != ReportStatus.Active)
        {
            throw new InvalidOperationException(
                "Only active reports can be resolved.");
        }

        Status = ReportStatus.Resolved;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        EnsureNotArchived();
        if (Status != ReportStatus.Active)
        {
            throw new InvalidOperationException(
                "Only active reports can be cancelled.");
        }

        Status = ReportStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AddPhoto(string storageKey)
    {
        EnsureNotArchived();
        ArgumentException.ThrowIfNullOrWhiteSpace(
            storageKey);

        var normalizedStorageKey =
            storageKey.Trim();

        if (_photos.Count >= MaximumPhotoCount)
        {
            throw new InvalidOperationException(
                $"A report can contain at most " +
                $"{MaximumPhotoCount} photos.");
        }

        if (_photos.Any(photo =>
                string.Equals(
                    photo.StorageKey,
                    normalizedStorageKey,
                    StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                "The same photo cannot be added more than once.");
        }

        var photo = PetReportPhoto.Create(
            petReportId: Id,
            storageKey: normalizedStorageKey,
            isPrimary: _photos.Count == 0,
            displayOrder: _photos.Count);

        _photos.Add(photo);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetPrimaryPhoto(Guid photoId)
    {
        EnsureNotArchived();
        var selectedPhoto =
            _photos.SingleOrDefault(
                photo => photo.Id == photoId);

        if (selectedPhoto is null)
        {
            throw new InvalidOperationException(
                "Photo does not belong to this report.");
        }

        foreach (var photo in _photos)
        {
            if (photo.Id == selectedPhoto.Id)
            {
                photo.SetAsPrimary();
            }
            else
            {
                photo.RemovePrimaryStatus();
            }
        }

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemovePhoto(Guid photoId)
    {
        EnsureNotArchived();
        var photoToRemove =
            _photos.SingleOrDefault(
                photo => photo.Id == photoId);

        if (photoToRemove is null)
        {
            throw new InvalidOperationException(
                "Photo does not belong to this report.");
        }

        var wasPrimary =
            photoToRemove.IsPrimary;

        _photos.Remove(photoToRemove);

        for (var index = 0;
             index < _photos.Count;
             index++)
        {
            _photos[index].UpdateDisplayOrder(
                index);
        }

        if (wasPrimary && _photos.Count > 0)
        {
            _photos[0].SetAsPrimary();
        }

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static void ValidateCreation(
        Guid userId,
        ReportType reportType,
        string title,
        string description,
        AnimalSpecies species,
        AnimalGender gender,
        AnimalColor primaryColor,
        AnimalColor? secondaryColor,
        DateTimeOffset eventDate,
        GeoLocation location)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            title);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            description);

        ArgumentNullException.ThrowIfNull(
            location);

        if (!Enum.IsDefined(reportType))
        {
            throw new ArgumentOutOfRangeException(
                nameof(reportType),
                "Report type is invalid.");
        }

        if (!Enum.IsDefined(species))
        {
            throw new ArgumentOutOfRangeException(
                nameof(species),
                "Animal species is invalid.");
        }

        if (!Enum.IsDefined(gender))
        {
            throw new ArgumentOutOfRangeException(
                nameof(gender),
                "Animal gender is invalid.");
        }

        if (!Enum.IsDefined(primaryColor))
        {
            throw new ArgumentOutOfRangeException(
                nameof(primaryColor),
                "Primary color is invalid.");
        }

        if (secondaryColor.HasValue &&
            !Enum.IsDefined(secondaryColor.Value))
        {
            throw new ArgumentOutOfRangeException(
                nameof(secondaryColor),
                "Secondary color is invalid.");
        }

        if (eventDate > DateTimeOffset.UtcNow)
        {
            throw new ArgumentException(
                "Event date cannot be in the future.",
                nameof(eventDate));
        }

        if (secondaryColor.HasValue &&
            primaryColor == secondaryColor.Value)
        {
            throw new ArgumentException(
                "Primary and secondary colors cannot be the same.",
                nameof(secondaryColor));
        }
    }

    private static string? NormalizeOptionalText(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private void EnsureNotArchived()
    {
        if (IsArchived)
        {
            throw new InvalidOperationException(
                "Archived pet reports cannot be modified.");
        }
    }

    public void Restore()
    {
        if (!IsArchived)
        {
            return;
        }

        var restoredAt =
            DateTimeOffset.UtcNow;

        IsArchived = false;
        ArchivedAt = null;
        UpdatedAt = restoredAt;

        if (Status == ReportStatus.Active)
        {
            RaiseDomainEvent(
                new PetReportUpdatedDomainEvent(
                    PetReportId: Id));
        }
    }
}