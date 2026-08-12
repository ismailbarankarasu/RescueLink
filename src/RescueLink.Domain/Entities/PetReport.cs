using RescueLink.Domain.Common;
using RescueLink.Domain.Enums;

namespace RescueLink.Domain.Entities
{
    public class PetReport : BaseEntity
    {
        public Guid UserId { get; private set; }
        public ReportType ReportType { get; private set; }
        public ReportStatus Status { get; private set; } = ReportStatus.Active;
        public string Title { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public AnimalSpecies Species { get; private set; }
        public AnimalGender Gender { get; private set; }
        public string? PetName { get; private set; }
        public string? Breed { get; private set; }
        public AnimalColor PrimaryColor { get; private set; }
        public AnimalColor? SecondaryColor { get; private set; }
        public DateTimeOffset EventDate { get; private set; }

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
            DateTimeOffset eventDate)
        {
            ValidateCreation(
                userId,
                reportType,
                title,
                description,
                species,
                primaryColor,
                secondaryColor,
                eventDate);

            return new PetReport
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
                EventDate = eventDate
            };
        }
        public void Resolve()
        {
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
            if (Status != ReportStatus.Active)
            {
                throw new InvalidOperationException(
                    "Only active reports can be cancelled.");
            }

            Status = ReportStatus.Cancelled;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
        private static void ValidateCreation(
            Guid userId,
            ReportType reportType,
            string title,
            string description,
            AnimalSpecies species,
            AnimalColor primaryColor,
            AnimalColor? secondaryColor,
            DateTimeOffset eventDate)
        {

            if (userId == Guid.Empty)
            {
                throw new ArgumentException(
                    "User ID cannot be empty.",
                    nameof(userId));
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(title);
            ArgumentException.ThrowIfNullOrWhiteSpace(description);

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

        private static string? NormalizeOptionalText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
       
    }
}