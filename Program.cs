// Program.cs  (REPLACE AS-IT-IS)
using System.Data;
using Microsoft.Data.SqlClient;
using LoginPanel.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC + runtime compilation
//builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddControllersWithViews();

// Session
builder.Services.AddSession();

// ? DAPPER DB CONNECTION (ADD)
builder.Services.AddScoped<IDbConnection>(sp =>
    new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

// ? PRODUCT SERVICE (ADD)
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IInventoryAdjustmentReasonService, InventoryAdjustmentReasonService>();
builder.Services.AddScoped<IVendorProductPriceService, VendorProductPriceService>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
builder.Services.AddScoped<IReceivedPurchaseService, ReceivedPurchaseService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // ? fixed (removed wrong line)
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Shop}/{action=Index}/{id?}");

app.Run();



