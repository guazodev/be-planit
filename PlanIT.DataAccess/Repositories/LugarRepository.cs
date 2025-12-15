using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PlanIT.Domain.Entities;
using PlanIT.DataAccess.Interfaces;

namespace PlanIT.DataAccess.Repositories;

public class LugarRepository : ILugarRepository
{
    private readonly string _connectionString;

    public LugarRepository(IConfiguration config)
    {
            _connectionString = config.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException(
                    "La cadena de conexión 'Postgres' no fue encontrada.");
        }

    public async Task<IEnumerable<Lugar>> ObtenerTodos()
    {
        using var conn = new NpgsqlConnection(_connectionString);

        var sql = @"
            SELECT 
                id,
                nombre,
                provincia,
                presupuesto_min AS Presupuesto_Min,
                presupuesto_max AS Presupuesto_Max,
                tiempo_recomendado_dias AS Tiempo_Recomendado_Dias,
                descripcion
            FROM lugares;
        ";

        return await conn.QueryAsync<Lugar>(sql);
    }
}
