// 1) Services/IProductService.cs
using LoginPanel.Models;

namespace LoginPanel.Services
{
    public interface IProductService
    {
        Task<List<ProductModel>> GetProductsAsync();
        Task<ProductModel?> GetProductAsync(int id);
        Task<int> GetStockAsync(int productId);
        Task DeductInventoryAfterPaymentAsync(int orderId);

    }
}
