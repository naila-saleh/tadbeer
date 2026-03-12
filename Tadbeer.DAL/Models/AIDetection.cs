namespace Tadbeer.DAL.Models;

public class AIDetection
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string ImagePath { get; set; } = null!;

    public Guid PredictedSpecialtyId { get; set; }
    public Specialty PredictedSpecialty { get; set; } = null!;

    public decimal ConfidenceScore { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}