namespace Tadbeer.DAL.Models;

public class Specialty
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Icon { get; set; } = null!;

    public ICollection<WorkerSpecialty> WorkerSpecialties { get; set; } = new List<WorkerSpecialty>();
    public ICollection<AIDetection> AIDetections { get; set; } = new List<AIDetection>();
}