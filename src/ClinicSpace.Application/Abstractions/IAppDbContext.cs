using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ClinicSpace.Application.Abstractions;

/// <summary>
/// The application's view of the database. Implemented by the EF Core context in
/// Infrastructure so Application services never depend on a concrete provider.
/// Entity sets are added here as slices introduce them.
/// </summary>
public interface IAppDbContext
{
    DatabaseFacade Database { get; }
}
