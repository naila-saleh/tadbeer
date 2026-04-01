namespace Tadbeer.DAL.Models;

public class Specialty
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; }
    public string Icon { get; set; }

    public ICollection<WorkerSpecialty> WorkerSpecialties { get; set; } = new List<WorkerSpecialty>();
    public ICollection<AIDetection> AIDetections { get; set; } = new List<AIDetection>();
}