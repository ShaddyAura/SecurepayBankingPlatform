using Dapper;

namespace Infrastructure.Interfaces;

public interface IGenericRepository
{
    Task ExecuteAsync(string storedProcedure, DynamicParameters? parameters = null);

    Task<IEnumerable<T>> QueryAsync<T>(
        string storedProcedure,
        DynamicParameters? parameters = null);

    Task<T?> QuerySingleOrDefaultAsync<T>(
        string storedProcedure,
        DynamicParameters? parameters = null);

    Task<T> QueryMultipleAsync<T>(
        string storedProcedure,
        DynamicParameters parameters,
        Func<SqlMapper.GridReader, Task<T>> map);
}
