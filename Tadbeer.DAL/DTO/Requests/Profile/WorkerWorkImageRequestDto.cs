using Tadbeer.DAL.DTO.Requests.WorkImages;

namespace Tadbeer.DAL.DTO.Requests.Profile;

// Legacy compatibility alias. Prefer CreateWorkImagesRequestDto for new code.
public class WorkerWorkImageRequestDto : CreateWorkImagesRequestDto
{
    public ICollection<WorkerWorkSubImageRequestDto> SubImages { get; set; } = new List<WorkerWorkSubImageRequestDto>();
}

