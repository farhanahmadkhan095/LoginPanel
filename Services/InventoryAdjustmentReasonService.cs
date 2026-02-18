using Dapper;
using System.Data;
using Microsoft.Data.SqlClient;
using LoginPanel.Models;

namespace LoginPanel.Services
{
    public class InventoryAdjustmentReasonService : IInventoryAdjustmentReasonService
    {
        private readonly string _connectionString;

        public InventoryAdjustmentReasonService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<int> AddOrUpdateAsync(InventoryAdjustmentReasonDto dto)
        {
            using var conn = CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Id", dto.Id);
            parameters.Add("@Reason", dto.Reason);
            parameters.Add("@IsActive", dto.IsActive);
            parameters.Add("@IsDeleted", dto.IsDeleted);


            var result = await conn.QueryFirstAsync<int>(
                "usp_InventoryAdjustmentReason_AddOrUpdate",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<IEnumerable<InventoryAdjustmentReasonDto>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<InventoryAdjustmentReasonDto>(
                "usp_InventoryAdjustmentReason_GetAll",
                commandType: CommandType.StoredProcedure);
        }

        public async Task<InventoryAdjustmentReasonDto?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<InventoryAdjustmentReasonDto>(
                "usp_InventoryAdjustmentReason_GetById",
                new { Id = id },
                commandType: CommandType.StoredProcedure);
        }

        public async Task ToggleActiveAsync(int id, bool isActive)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(
                "usp_InventoryAdjustmentReason_ToggleActive",
                new { Id = id, IsActive = isActive },
                commandType: CommandType.StoredProcedure);
        }

        public async Task ToggleDeletedAsync(int id, bool isDeleted)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(
                "usp_InventoryAdjustmentReason_ToggleDeleted",
                new { Id = id, IsDeleted = isDeleted },
                commandType: CommandType.StoredProcedure);
        }
    }
}
