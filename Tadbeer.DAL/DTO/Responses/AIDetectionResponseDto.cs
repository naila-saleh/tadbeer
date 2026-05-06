namespace Tadbeer.DAL.DTO.Responses;

public class AIDetectionResponseDto
{
    public string PredictedLabel { get; set; } = null!;
    public IEnumerable<SpecialtyResponseDto> MatchedSpecialties { get; set; } = [];
    public IEnumerable<WorkerPublicProfileResponseDto> SuggestedWorkers { get; set; } = [];
}
