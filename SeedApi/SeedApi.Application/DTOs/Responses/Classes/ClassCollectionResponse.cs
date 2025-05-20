using System.Collections.Generic;

namespace SeedApi.Application.DTOs.Responses.Classes;

public class ClassCollectionResponse
{
    public List<ClassResponse> Classes { get; set; } = new();
}
