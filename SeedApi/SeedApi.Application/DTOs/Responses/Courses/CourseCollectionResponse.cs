namespace SeedApi.Application.DTOs.Responses.Courses;

public record CourseCollectionResponse
{
  public List<CourseResponse> Courses { get; init; } = [];
}
