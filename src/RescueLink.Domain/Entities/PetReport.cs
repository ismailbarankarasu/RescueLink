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
            var report = new PetReport
            {

                UserId = userId,
                ReportType = reportType,
                Status = ReportStatus.Active,
                Title = title.Trim(),
                Description = description,
                Species = species,
                Gender = gender,
                PetName = petName,
                Breed = breed,
                PrimaryColor = primaryColor,
                SecondaryColor = secondaryColor,
                EventDate = eventDate
            };
            if (userId == Guid.Empty)
            {
                throw new ArgumentException(
                    "User ID cannot be empty.",
                    nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(
                    "Title cannot be empty.",
                    nameof(title));
            }
            return report;
        }
        private PetReport()
        {
        }

    }
}
