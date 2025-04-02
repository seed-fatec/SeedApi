using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SeedApi.Responses.Courses;

public record CourseCollectionResponse
{
  public List<CourseResponse> Courses { get; init; } = [];
}
