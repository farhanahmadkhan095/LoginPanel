    using LoginPanel.Models;

namespace LoginPanel.Services
{
    public interface IInventoryAdjustmentReasonService
    {
        Task<int> AddOrUpdateAsync(InventoryAdjustmentReasonDto dto);
        Task<IEnumerable<InventoryAdjustmentReasonDto>> GetAllAsync();
        Task<InventoryAdjustmentReasonDto?> GetByIdAsync(int id);
        Task ToggleActiveAsync(int id, bool isActive);
        Task ToggleDeletedAsync(int id, bool isDeleted);
    }
}