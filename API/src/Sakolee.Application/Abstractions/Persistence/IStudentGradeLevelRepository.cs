using Sakolee.Application.Common;
using Sakolee.Domain.Entities;

namespace Sakolee.Application.Abstractions.Persistence;

/// <summary>
/// Repository interface for managing StudentGradeLevel data operations.
/// </summary>
public interface IStudentGradeLevelRepository
{
    /// <summary>
    /// Retrieves a read-only list of all student grade levels asynchronously.
    /// </summary>
    /// <param name="search">The search term filter.</param>
    /// <param name="tenantId">The optional tenant identifier.</param>
    /// <param name="active">Optional active status filter.</param>
    /// <param name="showDeleted">Flag indicating whether to include deleted records.</param>
    /// <param name="sort">Sorting parameters.</param>
    /// <param name="page">Current page number.</param>
    /// <param name="limit">Number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A read-only list of student grade levels and total count.</returns>
    Task<(IReadOnlyList<StudentGradeLevel> Items, int TotalCount)> ListAsync(
         string? search,
         Guid? tenantId,
         bool? active,
         bool? showDeleted,
         SortRequest sort,
         int page,
         int limit,
         CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific student grade level by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the student grade level.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The student grade level if found; otherwise, null.</returns>
    Task<StudentGradeLevel?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a student grade level with the specified name already exists.
    /// </summary>
    /// <param name="name">The name of the student grade level to check.</param>
    /// <param name="tenantId">The optional tenant identifier.</param>
    /// <param name="excludeStudentGradeLevelId">Optional student grade level ID to exclude from the check (useful during updates).</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>True if the name exists; otherwise, false.</returns>
    Task<bool> NameExistsAsync(
        string name,
        Guid? tenantId,
        Guid? excludeStudentGradeLevelId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new student grade level to the repository context.
    /// </summary>
    /// <param name="studentGradeLevel">The student grade level entity to add.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    Task AddAsync(
        StudentGradeLevel studentGradeLevel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing student grade level as updated in the repository context.
    /// </summary>
    /// <param name="studentGradeLevel">The student grade level entity to update.</param>
    void Update(StudentGradeLevel studentGradeLevel);

    /// <summary>
    /// Marks an existing student grade level for removal from the repository context.
    /// </summary>
    /// <param name="studentGradeLevel">The student grade level entity to remove.</param>
    void Remove(StudentGradeLevel studentGradeLevel);
}