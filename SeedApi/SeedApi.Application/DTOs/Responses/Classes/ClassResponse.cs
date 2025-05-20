namespace SeedApi.Application.DTOs.Responses.Classes;

public class ClassResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime StartTimestamp { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsFree { get; set; }
    public int CourseId { get; set; }
}
