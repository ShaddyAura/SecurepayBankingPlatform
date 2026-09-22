using Dapper;
using Infrastructure.Dapper;
using Infrastructure.Interfaces;
using System.Data;

namespace Infrastructure.Repository;

public class GenericRepository : IGenericRepository
{
    private readonly DapperContext _context;

    public GenericRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task ExecuteAsync(string storedProcedure, DynamicParameters? parameters = null)
    {
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string storedProcedure, DynamicParameters? parameters = null)
    {
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<T>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task<T?> QuerySingleOrDefaultAsync<T>(string storedProcedure, DynamicParameters? parameters = null)
    {
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<T>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task<T> QueryMultipleAsync<T>(
        string storedProcedure,
        DynamicParameters parameters,
        Func<SqlMapper.GridReader, Task<T>> map)
    {
        using var connection = _context.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
        return await map(multi);
    }
}
