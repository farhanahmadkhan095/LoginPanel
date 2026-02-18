using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Microsoft.Data.SqlClient;
using LoginPanel.Models;


public class VendorProductPriceService : IVendorProductPriceService
{
    private readonly string _connectionString;

    public VendorProductPriceService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    public List<VendorModel> GetActiveVendors()
    {
        using var con = new SqlConnection(_connectionString);
        return con.Query<VendorModel>("Proc_GetActiveVendors", commandType: CommandType.StoredProcedure).ToList();
    }

    public List<ProductModel> GetActiveProducts()
    {
        using var con = new SqlConnection(_connectionString);
        return con.Query<ProductModel>("Proc_GetActiveProducts", commandType: CommandType.StoredProcedure).ToList();
    }

    public void SaveVendorProductPrice(VendorProductPricingReq req)
    {
        using var con = new SqlConnection(_connectionString);
        con.Execute("Proc_SaveVendorProductPricing", new { req.VendorId, req.ProductId, req.Price }, commandType: CommandType.StoredProcedure);
    }

    public List<VendorProductPriceDisplay> GetAllVendorProductPrices()
    {
        using var con = new SqlConnection(_connectionString);
        string sql = @"
            SELECT vpp.VendorProductPriceId,
                 vpp.VendorId,
                 vpp.ProductId,
                   vm.VendorName,
                   p.Name AS ProductName,
                   p.Price AS MasterPrice,
                   vpp.Price AS VendorPrice
            FROM VendorProductPricing vpp
            INNER JOIN VendorMaster vm ON vm.VendorId = vpp.VendorId
            INNER JOIN Product p ON p.Id = vpp.ProductId";

        return con.Query<VendorProductPriceDisplay>(sql).ToList();
    }
    public List<VendorProductPricingHistoryModel> GetVendorProductPriceHistory(int vendorId, int productId)
    {
        using var con = new SqlConnection(_connectionString);

        string sql = @"
	       SELECT 
	           h.HistoryId,
	           v.VendorName,
	           p.Name AS ProductName,
	           h.OldPrice,
	           h.NewPrice,
	           h.ChangedOn
	       FROM VendorProductPricingHistory h
	       INNER JOIN VendorMaster v ON v.VendorId = h.VendorId
	       INNER JOIN Product p ON p.Id = h.ProductId
           WHERE h.VendorId = @VendorId AND h.ProductId = @ProductId
	       ORDER BY h.ChangedOn DESC";

        return con.Query<VendorProductPricingHistoryModel>(sql, new { VendorId = vendorId, ProductId = productId }).ToList();
    }
}
