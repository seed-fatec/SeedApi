using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeedApi.API.Middlewares;
using SeedApi.Application.DTOs.Requests.Classes;
using SeedApi.Application.DTOs.Responses;
using SeedApi.Application.DTOs.Responses.Classes;
using SeedApi.Application.Services;
using SeedApi.Domain.Entities;

namespace SeedApi.API.Controllers;

[ApiController]
[Route("api/courses/{courseId:int}/classes")]
public class CourseClassesController(
    ClassService classService,
    CourseService courseService,
    TeacherService teacherService,
    UserService userService
) : ControllerBase
{
  private readonly ClassService _classService = classService;
  private readonly CourseService _courseService = courseService;
  private readonly TeacherService _teacherService = teacherService;
  private readonly UserService _userService = userService;

  [HttpGet]
  [Authorize]
  [ProducesResponseType<ClassCollectionResponse>(StatusCodes.Status200OK)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> ListClasses(int courseId)
  {
    var course = await _courseService.GetCourseByIdAsync(courseId);
    if (course == null)
      return NotFound(new ErrorResponse { Message = "Curso não encontrado." });

    var classes = await _classService.ListClassesByCourseAsync(courseId);
    var response = new ClassCollectionResponse
    {
      Classes = classes.Select(c => new ClassResponse
      {
        Id = c.Id,
        Name = c.Name,
        Description = c.Description,
        StartTimestamp = c.StartTimestamp,
        DurationMinutes = c.DurationMinutes,
        IsFree = c.IsFree,
        CourseId = c.CourseId
      }).ToList()
    };
    return Ok(response);
  }

  [HttpGet("{classId:int}")]
  [Authorize]
  [ProducesResponseType<ClassResponse>(StatusCodes.Status200OK)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> GetClass(int courseId, int classId)
  {
    var course = await _courseService.GetCourseByIdAsync(courseId);
    if (course == null)
      return NotFound(new ErrorResponse { Message = "Curso não encontrado." });

    var classEntity = await _classService.GetClassByIdAsync(classId);
    if (classEntity == null || classEntity.CourseId != courseId)
      return NotFound(new ErrorResponse { Message = "Aula não encontrada para este curso." });

    return Ok(new ClassResponse
    {
      Id = classEntity.Id,
      Name = classEntity.Name,
      Description = classEntity.Description,
      StartTimestamp = classEntity.StartTimestamp,
      DurationMinutes = classEntity.DurationMinutes,
      IsFree = classEntity.IsFree,
      CourseId = classEntity.CourseId
    });
  }

  [HttpPost]
  [Authorize]
  [RequireRole(UserRole.Teacher)]
  [ProducesResponseType<ClassResponse>(StatusCodes.Status201Created)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
  [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> CreateClass(int courseId, [FromBody] ClassCreateRequest request)
  {
    var user = await _userService.GetAuthenticatedUserAsync(User);
    if (user == null)
      return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });

    var course = await _courseService.GetCourseByIdAsync(courseId);
    if (course == null)
      return NotFound(new ErrorResponse { Message = "Curso não encontrado." });

    var isTeacher = await _teacherService.IsTeacherInCourseAsync(user.Id, courseId);
    if (!isTeacher)
      return Forbid();

    var classStart = request.StartTimestamp;
    var classEnd = request.StartTimestamp.AddMinutes(request.DurationMinutes);
    var courseStart = course.StartDate.ToDateTime(TimeOnly.MinValue);
    var courseEnd = course.EndDate.ToDateTime(TimeOnly.MaxValue);

    if (classStart < courseStart || classEnd > courseEnd)
    {
      return BadRequest(new ErrorResponse { Message = "A aula deve estar dentro do período do curso." });
    }

    // Validação de conflito de horário de aula
    var existingClasses = await _classService.ListClassesByCourseAsync(courseId);
    bool hasConflict = existingClasses.Any(c =>
      (classStart < c.StartTimestamp.AddMinutes(c.DurationMinutes)) &&
      (classEnd > c.StartTimestamp)
    );
    if (hasConflict)
    {
      return BadRequest(new ErrorResponse { Message = "Já existe uma aula nesse intervalo de tempo para este curso." });
    }

    var newClass = new Class
    {
      Name = request.Name,
      Description = request.Description,
      StartTimestamp = request.StartTimestamp,
      DurationMinutes = request.DurationMinutes,
      IsFree = request.Free,
      CourseId = courseId
    };
    var created = await _classService.AddClassAsync(newClass);
    return CreatedAtAction(nameof(GetClass), new { courseId, classId = created.Id }, new ClassResponse
    {
      Id = created.Id,
      Name = created.Name,
      Description = created.Description,
      StartTimestamp = created.StartTimestamp,
      DurationMinutes = created.DurationMinutes,
      IsFree = created.IsFree,
      CourseId = created.CourseId
    });
  }

  [HttpDelete("{classId:int}")]
  [Authorize]
  [RequireRole(UserRole.Teacher)]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> DeleteClass(int courseId, int classId)
  {
    var user = await _userService.GetAuthenticatedUserAsync(User);
    if (user == null)
      return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });

    var course = await _courseService.GetCourseByIdAsync(courseId);
    if (course == null)
      return NotFound(new ErrorResponse { Message = "Curso não encontrado." });

    var isTeacher = await _teacherService.IsTeacherInCourseAsync(user.Id, courseId);
    if (!isTeacher)
      return Forbid();

    var classEntity = await _classService.GetClassByIdAsync(classId);
    if (classEntity == null || classEntity.CourseId != courseId)
      return NotFound(new ErrorResponse { Message = "Aula não encontrada para este curso." });

    await _classService.DeleteClassAsync(classId);
    return NoContent();
  }

  [HttpPut("{classId:int}")]
  [Authorize]
  [RequireRole(UserRole.Teacher)]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
  [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> UpdateClass(int courseId, int classId, [FromBody] ClassUpdateRequest request)
  {
    var user = await _userService.GetAuthenticatedUserAsync(User);
    if (user == null)
      return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });

    var course = await _courseService.GetCourseByIdAsync(courseId);
    if (course == null)
      return NotFound(new ErrorResponse { Message = "Curso não encontrado." });

    var isTeacher = await _teacherService.IsTeacherInCourseAsync(user.Id, courseId);
    if (!isTeacher)
      return Forbid();

    var classEntity = await _classService.GetClassByIdAsync(classId);
    if (classEntity == null || classEntity.CourseId != courseId)
      return NotFound(new ErrorResponse { Message = "Aula não encontrada para este curso." });

    // Validação de datas
    var newStart = request.StartTimestamp;
    var newEnd = request.StartTimestamp.AddMinutes(classEntity.DurationMinutes);
    var courseStart = course.StartDate.ToDateTime(TimeOnly.MinValue);
    var courseEnd = course.EndDate.ToDateTime(TimeOnly.MaxValue);
    if (newStart < courseStart || newEnd > courseEnd)
      return BadRequest(new ErrorResponse { Message = "A aula deve estar dentro do período do curso." });

    // Validação de conflito de horário de aula
    var existingClasses = await _classService.ListClassesByCourseAsync(courseId);
    bool hasConflict = existingClasses.Any(c =>
      c.Id != classId &&
      newStart < c.StartTimestamp.AddMinutes(c.DurationMinutes) &&
      newEnd > c.StartTimestamp
    );
    if (hasConflict)
      return BadRequest(new ErrorResponse { Message = "Já existe uma aula nesse intervalo de tempo para este curso." });

    classEntity.Name = request.Name;
    classEntity.Description = request.Description;
    classEntity.StartTimestamp = request.StartTimestamp;
    await _classService.UpdateClassAsync(classEntity);
    return NoContent();
  }
}
