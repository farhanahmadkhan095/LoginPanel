using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using ExcelDataReader;
using LoginPanel.Enums;
using LoginPanel.Models;
using LoginPanel.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

using System.Data;
using System.Linq;

namespace LoginPanel.Controllers
{
    public class LoginPanelController : Controller
    {

        private string connectionString = "Server=localhost;Database=ProductMaster;Trusted_Connection=True;TrustServerCertificate=True";

        [HttpGet]
        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RegistrationUser(RegistrationModel users)
        {
            try
            {
                if (string.IsNullOrEmpty(users.UserName))
                {
                    ViewBag.Error = "Enter Username!";
                    return View("Registration");
                }
                if (string.IsNullOrEmpty(users.Email))
                {
                    ViewBag.Error = "Enter your Email";
                    return View("Registration");
                }
                if (string.IsNullOrEmpty(users.MobileNo))
                {
                    ViewBag.Error = "Enter your Mobile No.";
                    return View("Registration");
                }
                if (string.IsNullOrEmpty(users.Password))
                {
                    ViewBag.Error = "Enter password!";
                    return View("Registration");
                }
                if (string.IsNullOrEmpty(users.ConfirmPassword))
                {
                    ViewBag.Error = " Enter Confirm Password !";
                    return View("Registration");
                }
                if (users.Password != users.ConfirmPassword)
                {
                    ViewBag.Error = "Match your password";
                    return View("Registration");
                }
                if (users.CountryId == 0)
                {
                    ViewBag.Error = " Select Country!";
                    return View("Registration");
                }
                if (users.StateId == 0)
                {
                    ViewBag.Error = " Select State ";
                    return View("Registration");
                }
                if (users.CityId == 0)
                {
                    ViewBag.Error = " select City ";
                    return View("Registration");
                }

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("Register", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@UserName", users.UserName);
                    cmd.Parameters.AddWithValue("@Email", users.Email);
                    cmd.Parameters.AddWithValue("@MobileNo", users.MobileNo);
                    cmd.Parameters.AddWithValue("@Password", users.Password);
                    cmd.Parameters.AddWithValue("@CountryId", users.CountryId);
                    cmd.Parameters.AddWithValue("@StateId", users.StateId);
                    cmd.Parameters.AddWithValue("@CityId", users.CityId);

                    con.Open();
                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        TempData["Success"] = "Registration Successful!";
                        return RedirectToAction("Login");
                    }
                    else
                    {
                        TempData["Error"] = "Something went wrong!";
                        return View("Registration");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error: " + ex.Message;
                return View("Registration");
            }
        }

        [HttpGet]
        public IActionResult Login(string errorMsg = "")
        {
            ViewBag.Error = errorMsg;
            return View();
        }

        [HttpPost]
        public IActionResult LoginUser(string LoginId, string Password, string returnUrl = "")
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("UserLogin", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Email", LoginId);
                cmd.Parameters.AddWithValue("@Password", Password);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (!dr.Read())
                {
                    return Json(new { success = false, message = "Invalid login" });
                }

                int status = Convert.ToInt32(dr["Status"]);
                string message = dr["Message"].ToString();

                if (status == 0)
                {
                    return Json(new { success = false, message });
                }

                string roleName = dr["RoleName"].ToString();

                // ✅ SESSION SET
                HttpContext.Session.SetString("UserName", dr["UserName"].ToString());
                HttpContext.Session.SetString("RoleName", roleName);
                HttpContext.Session.SetInt32("RoleId", Convert.ToInt32(dr["RoleId"]));
                HttpContext.Session.SetInt32("UserId", Convert.ToInt32(dr["UserId"]));
                HttpContext.Session.SetString("UserSession", "ValidUser");

                string redirectUrl;

                if (roleName.ToUpper() == RoleMaster.ADMIN.ToString())
                    redirectUrl = Url.Action("AdminDashboard");
                else
                    redirectUrl = !string.IsNullOrEmpty(returnUrl)
                        ? returnUrl
                        : Url.Action("Index", "Shop");

                return Json(new
                {
                    success = true,
                    message = "Login successful",
                    redirectUrl
                });
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index","Shop");
        }
        public IActionResult Dashboard()
        {
            ViewBag.Name = HttpContext.Session.GetString("UserName");
            return View();
        }

        public IActionResult AdminDashboard()
        {
            if (HttpContext.Session.GetString("UserSession") == null)
                return RedirectToAction("Login");

            int totalProducts = 0;
            int activeProducts = 0;
            int inactiveProducts = 0;
            int totalVendors = 0;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                totalProducts = (int)new SqlCommand("SELECT COUNT(*) FROM Product", con).ExecuteScalar();
                activeProducts = (int)new SqlCommand("SELECT COUNT(*) FROM Product WHERE IsActive = 1", con).ExecuteScalar();
                inactiveProducts = (int)new SqlCommand("SELECT COUNT(*) FROM Product WHERE IsActive = 0", con).ExecuteScalar();
                totalVendors = (int)new SqlCommand("SELECT COUNT(*) FROM VendorMaster WHERE IsDeleted = 0", con).ExecuteScalar();
            }

            ViewBag.TotalProducts = totalProducts;
            ViewBag.ActiveProducts = activeProducts;
            ViewBag.InactiveProducts = inactiveProducts;
            ViewBag.TotalVendors = totalVendors; 

            return View();
        }


        //public IActionResult AdminDashboard()
        //{
        //    if (HttpContext.Session.GetString("UserSession") == null)
        //    {
        //        return RedirectToAction("Login");
        //    }
        //    return View();
        //}


        public IActionResult UserDashboard()
        {
            return View();
        }

        // ---------------- COUNTRY ----------------
        public JsonResult GetCountryList()
        {
            List<dynamic> countryList = new List<dynamic>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("GetCountryMaster", con);
                cmd.CommandType = CommandType.StoredProcedure;

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    countryList.Add(new
                    {
                        CountryId = dr["CountryId"],
                        CountryName = dr["CountryName"]
                    });
                }
            }
            return Json(countryList);
        }

        // ---------------- STATE ----------------
        public JsonResult GetStateList(int countryId)
        {
            List<dynamic> stateList = new List<dynamic>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("GetStateMasterByCountryId", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CountryId", countryId);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    stateList.Add(new
                    {
                        StateId = dr["StateId"],
                        StateName = dr["StateName"]
                    });
                }
            }
            return Json(stateList);
        }

        // ---------------- CITY ----------------
        public JsonResult GetCityList(int stateId)
        {
            List<dynamic> cityList = new List<dynamic>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("GetCityMasterByStateId", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@StateId", stateId);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    cityList.Add(new
                    {
                        CityId = dr["CityId"],
                        CityName = dr["CityName"]
                    });
                }
            }
            return Json(cityList);
        }


        // GET ALL PRODUCTS 

        [HttpGet]
        public IActionResult PrtGetProductList()
        {
            try
            {
                List<ProductModel> productList = new List<ProductModel>();

                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("GetAllProduct", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        productList.Add(new ProductModel
                        {
                            ProductId = dr["Id"] == DBNull.Value ? 0 : Convert.ToInt32(dr["Id"]),
                            ProductName = dr["Name"]?.ToString(),
                            ProductPrice = dr["Price"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["Price"]),
                            ProductDescription = dr["Description"] == DBNull.Value ? "" : dr["Description"].ToString(),
                            IsActive = dr["IsActive"] == DBNull.Value ? false : Convert.ToBoolean(dr["IsActive"]),
                            EntryBy = dr["EntryBy"] == DBNull.Value ? 0 : Convert.ToInt32(dr["EntryBy"]),
                            CreatedDate = dr["CreatedDate"] == DBNull.Value ? null : dr["CreatedDate"].ToString(),
                            ModifiedDate = dr["ModifiedDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["ModifiedDate"])
                        });
                    }
                }
                return PartialView(productList);
            }
            catch (Exception ex)
            {
                return PartialView("");
            }
        }



        // ---------------- ADD OR UPDATE PRODUCT ----------------
        [HttpPost]
        public JsonResult AddOrUpdateProduct(ProductModel p)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("AddorUpdate", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@ProductId", p.ProductId);
                cmd.Parameters.AddWithValue("@ProductName", p.ProductName);
                cmd.Parameters.AddWithValue("@ProductPrice", p.ProductPrice);
                cmd.Parameters.AddWithValue("@ProductDescription", p.ProductDescription);
                cmd.Parameters.AddWithValue("@ProductStatus", p.IsActive);
                cmd.Parameters.AddWithValue("@EntryBy", HttpContext.Session.GetInt32("UserId"));

                con.Open();
                var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    int status = Convert.ToInt32(dr["StatusCode"]);
                    string msg = dr["ResponseText"].ToString();

                    return Json(new { status = status, message = msg });
                }
            }
            return Json("Success");
        }

        // ---------------- DELETE PRODUCT ----------------
        [HttpPost]
        public JsonResult DeleteProduct(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("DeletProduct", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", id);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        int status = Convert.ToInt32(dr["StatusCode"]);
                        string msg = dr["ResponseText"].ToString();

                        return Json(new { status = status, message = msg });
                    }
                }
                return Json(new { status = 0, message = "Unknown error" });
            }
            catch (Exception ex)
            {
                return Json(new { status = 0, message = ex.Message });
            }
        }

        // ---------------- GET PRODUCT BY ID ----------------
        [HttpGet]
        public JsonResult GetProductById(int id)
        {
            try
            {
                ProductModel p = null;

                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("GetProductById", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", id);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        int status = Convert.ToInt32(dr["StatusCode"]);

                        if (status == 1)
                        {
                            p = new ProductModel
                            {
                                ProductId = Convert.ToInt32(dr["Id"]),
                                ProductName = dr["Name"].ToString(),
                                ProductPrice = Convert.ToDecimal(dr["Price"]),
                                ProductDescription = dr["Description"].ToString(),
                                IsActive = Convert.ToBoolean(dr["IsActive"])
                            };
                        }
                        else
                        {
                            return Json(new
                            {
                                status = status,
                                message = dr["ResponseText"].ToString()
                            });
                        }
                    }
                }

                return Json(p);
            }
            catch (Exception ex)
            {
                return Json(new { status = 0, message = ex.Message });
            }
        }

        // ---------------- UPDATE PRODUCT ----------------
        [HttpPost]
        public JsonResult UpdateProduct(ProductModel p)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("UpdateProduct", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@ProductId", p.ProductId);
                    cmd.Parameters.AddWithValue("@ProductName", p.ProductName);
                    cmd.Parameters.AddWithValue("@ProductPrice", p.ProductPrice);
                    cmd.Parameters.AddWithValue("@ProductDescription", p.ProductDescription);
                    cmd.Parameters.AddWithValue("@ProductStatus", p.IsActive);
                    cmd.Parameters.AddWithValue("@EntryBy", HttpContext.Session.GetInt32("UserId"));

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                return Json(new
                {
                    status = 1,
                    message = "Updated Successfully!",
                    modifiedDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                return Json(new { status = 0, message = ex.Message });
            }
        }

        // ---------------- UPLOAD EXCEL ----------------
        [HttpPost]
        public IActionResult UploadExcel(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
                return Json("Please upload a valid Excel file");

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            DataTable dt = new DataTable();

            using (var stream = excelFile.OpenReadStream())
            {
                var conf = new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = true
                    }
                };

                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet(conf);
                    dt = result.Tables[0];
                }
            }

            dt.Columns.Add("CreatedDate", typeof(DateTime));
            dt.Columns.Add("EntryBy", typeof(int));

            foreach (DataRow row in dt.Rows)
            {
                row["CreatedDate"] = DateTime.Now;
                row["EntryBy"] = HttpContext.Session.GetInt32("UserId");
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                using (SqlBulkCopy bulk = new SqlBulkCopy(con))
                {
                    bulk.DestinationTableName = "Product";


                    bulk.ColumnMappings.Add("ProductName", "Name");
                    bulk.ColumnMappings.Add("Price", "Price");
                    bulk.ColumnMappings.Add("Description", "Description");
                    bulk.ColumnMappings.Add("IsActive", "IsActive");
                    bulk.ColumnMappings.Add("CreatedDate", "CreatedDate");
                    bulk.ColumnMappings.Add("EntryBy", "EntryBy");

                    bulk.WriteToServer(dt);
                }
            }

            return Json("Excel Uploaded Successfully!");
        }

        // ---------------- UPDATE DESCRIPTION ----------------
        [HttpPost]
        public JsonResult UpdateDescription(int ProductId, string ProductDescription)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("UpdateProductDescription", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@ProductId", ProductId);
                    cmd.Parameters.AddWithValue("@ProductDescription", ProductDescription);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                return Json(new
                {
                    status = 1,
                    message = "Description Updated Successfully!",
                    modifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                return Json(new { status = 0, message = ex.Message });
            }
        }

        // ---------------- UPDATE PRICE ----------------
        [HttpPost]
        public JsonResult UpdateProductPrice(int ProductId, decimal ProductPrice)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("UpdateProductPrice", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProductId", ProductId);
                    cmd.Parameters.AddWithValue("@ProductPrice", ProductPrice);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        int status = Convert.ToInt32(dr["StatusCode"]);
                        string msg = dr["ResponseText"].ToString();
                        string ModifiedDate = dr["ModifiedDate"].ToString();

                        return Json(new { status = status, message = msg, modifiedDate = ModifiedDate });
                    }
                }
                return Json(new
                {
                    status = 1,
                    message = "Price Updated Successfully!",
                    modifiedDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                return Json(new { status = 0, message = ex.Message });
            }
        }


        // ---------------- CHANGE STATUS ----------------
        [HttpPost]
        public JsonResult ChangeStatus(int ProductId, bool ProductStatus)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(
                    @"UPDATE Product 
                      SET IsActive = @IsActive, ModifiedDate = GETDATE() 
                      WHERE Id = @Id", con))
                {
                    cmd.Parameters.AddWithValue("@Id", ProductId);
                    cmd.Parameters.AddWithValue("@IsActive", ProductStatus);

                    con.Open();
                    int rows = cmd.ExecuteNonQuery();

                    if (rows > 0)

                        return Json(new { status = 1, message = "Status updated", modifiedDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") });
                    else
                        return Json(new { status = 0, message = "No row updated" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = 0, message = ex.Message });
            }
        }

        // ---------------- TEMPLATE DOWNLOAD ----------------
        public IActionResult DownloadTemplate()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Template");

                worksheet.Cell(1, 1).Value = "Name";
                worksheet.Cell(1, 2).Value = "Price";
                worksheet.Cell(1, 3).Value = "Description";

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "ProductTemplate.xlsx"
                    );
                }
            }
        }
        [HttpPost]
        public JsonResult UpdateStatus(int ProductId, bool IsActive)
        {
            string modifiedDate = "";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("UpdateProductStatus", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@ProductId", ProductId);
                cmd.Parameters.AddWithValue("@IsActive", IsActive);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    modifiedDate = dr["ModifiedDate"].ToString();
                }
            }

            return Json(new
            {
                message = "Status Updated Successfully",
                modifiedDate = modifiedDate
            });
        }
        public IActionResult Vendor()
        {
            return View();
        }
        public IActionResult VendorList()
        {
            List<VendorModel> list = new List<VendorModel>();

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("GetAllVendors", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    list.Add(new VendorModel
                    {
                        VendorId = Convert.ToInt32(dr["VendorId"]),
                        VendorName = dr["VendorName"].ToString(),
                        ContactNumber = dr["ContactNumber"].ToString(),
                        Email = dr["Email"].ToString(),
                        Address = dr["Address"].ToString(),
                        IsActive = Convert.ToBoolean(dr["IsActive"]),
                        CreatedDate = dr["CreatedDate"].ToString(),
                        ModifiedDate = dr["ModifiedDate"].ToString()
                    });
                }
            }

            return View(list);
        }
        [HttpGet]
        public JsonResult GetVendorById(int id)
        {
            VendorModel model = new VendorModel();

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("GetVendorById", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@VendorId", id);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    model.VendorId = Convert.ToInt32(dr["VendorId"]);
                    model.VendorName = dr["VendorName"].ToString();
                    model.ContactNumber = dr["ContactNumber"].ToString();
                    model.Email = dr["Email"].ToString();
                    model.Address = dr["Address"].ToString();
                    model.IsActive = Convert.ToBoolean(dr["IsActive"]);
                }
            }

            return Json(model);
        }

        [HttpGet]
        public IActionResult AddVendor()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AddOrEditVendor(int VendorId = 0)
        {
            VendorModel model = new VendorModel();

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("GetVendorById", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@VendorId", VendorId);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    model.VendorId = Convert.ToInt32(dr["VendorId"]);
                    model.VendorName = dr["VendorName"].ToString();
                    model.ContactNumber = dr["ContactNumber"].ToString();
                    model.Email = dr["Email"].ToString();
                    model.Address = dr["Address"].ToString();
                    model.IsActive = Convert.ToBoolean(dr["IsActive"]);
                }
                return PartialView(model);
            }
        }

        [HttpPost]
        public IActionResult AddVendor([FromBody] VendorModel model)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("AddOrUpdateVendor", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@VendorId", model.VendorId);
                cmd.Parameters.AddWithValue("@VendorName", model.VendorName);
                cmd.Parameters.AddWithValue("@ContactNumber", model.ContactNumber);
                cmd.Parameters.AddWithValue("@Email", model.Email);
                cmd.Parameters.AddWithValue("@Address", model.Address);
                cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

                con.Open();
                cmd.ExecuteReader();
            }

            return Json(new { success = true, message = "Vendor Added Successfully" });
        }

        [HttpGet]
        public IActionResult EditVendor(int id)
        {
            VendorModel model = new VendorModel();

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("GetVendorById", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@VendorId", id);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    model.VendorId = Convert.ToInt32(dr["VendorId"]);
                    model.VendorName = dr["VendorName"].ToString();
                    model.ContactNumber = dr["ContactNumber"].ToString();
                    model.Email = dr["Email"].ToString();
                    model.Address = dr["Address"].ToString();
                    model.IsActive = Convert.ToBoolean(dr["IsActive"]);
                }
            }

            return View(model);
        }
        [HttpPost]
        public IActionResult EditVendor(VendorModel model)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("UpdateVendor", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@VendorId", model.VendorId);
                cmd.Parameters.AddWithValue("@VendorName", model.VendorName);
                cmd.Parameters.AddWithValue("@ContactNumber", model.ContactNumber);
                cmd.Parameters.AddWithValue("@Email", model.Email);
                cmd.Parameters.AddWithValue("@Address", model.Address);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["msg"] = "Vendor Updated Successfully!";
            return RedirectToAction("VendorList");
        }
        [HttpPost]
        public JsonResult DeleteVendor(int id)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("DeleteVendor", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@VendorId", id);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return Json(new { status = 1, message = "Vendor Deleted!" });
        }
        [HttpPost]
        public JsonResult ToggleVendorStatus(int id, bool isActive)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("ToggleVendorStatus", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@VendorId", id);
                cmd.Parameters.AddWithValue("@IsActive", isActive);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return Json(new { status = 1, message = "Status Updated!" });
        }
        public IActionResult VendorPRTView()
        {
            List<VendorModel> list = new List<VendorModel>();

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("GetAllVendors", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    list.Add(new VendorModel
                    {
                        VendorId = Convert.ToInt32(dr["VendorId"]),
                        VendorName = dr["VendorName"].ToString(),
                        ContactNumber = dr["ContactNumber"].ToString(),
                        Email = dr["Email"].ToString(),
                        Address = dr["Address"].ToString(),
                        IsActive = Convert.ToBoolean(dr["IsActive"]),
                        CreatedDate = dr["CreatedDate"].ToString(),
                        ModifiedDate = dr["ModifiedDate"]?.ToString(),
                        IsDeleted = Convert.ToBoolean(dr["IsDeleted"]),
                    });
                }
            }

            return PartialView(list);
        }

        public JsonResult ToggleVendorDelete(int id, bool isDelete)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("ToggleVendorDelete", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VendorId", id);
                    cmd.Parameters.AddWithValue("@IsDeleted", isDelete);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                return Json(new
                {
                    status = 1,
                    message = isDelete ? "Vendor Soft Deleted!" : "Vendor Restored!"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = 0,
                    message = "Error: " + ex.Message
                });
            }
        }
        public IActionResult VendorReport()
        {
            return View();
        }
        public IActionResult GetVendorReport(string name, string email, string contact, string status)
        {
            List<VendorModel> list = new List<VendorModel>();

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("GetVendorReport", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Name", string.IsNullOrEmpty(name) ? (object)DBNull.Value : name);
                cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);
                cmd.Parameters.AddWithValue("@Contact", string.IsNullOrEmpty(contact) ? (object)DBNull.Value : contact);
                cmd.Parameters.AddWithValue("@Status", string.IsNullOrEmpty(status) ? (object)DBNull.Value : status);  // NEW

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    list.Add(new VendorModel
                    {
                        VendorId = Convert.ToInt32(dr["VendorId"]),
                        VendorName = dr["VendorName"].ToString(),
                        ContactNumber = dr["ContactNumber"].ToString(),
                        Email = dr["Email"].ToString(),
                        Address = dr["Address"].ToString(),
                        IsActive = Convert.ToBoolean(dr["IsActive"]),
                        CreatedDate = dr["CreatedDate"].ToString(),
                        ModifiedDate = dr["ModifiedDate"].ToString(),
                        IsDeleted = Convert.ToBoolean(dr["IsDeleted"])
                    });
                }
            }

            return PartialView(list);
        }

        public IActionResult DownloadVendorExcel(string name, string email, string contact, string status)
        {
            List<VendorModel> list = new List<VendorModel>();

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("GetVendorReport", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // EXACT procedure parameters
                cmd.Parameters.AddWithValue("@Name", string.IsNullOrEmpty(name) ? (object)DBNull.Value : name);
                cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);
                cmd.Parameters.AddWithValue("@Contact", string.IsNullOrEmpty(contact) ? (object)DBNull.Value : contact);
                cmd.Parameters.AddWithValue("@Status", string.IsNullOrEmpty(status) ? (object)DBNull.Value : status);

                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new VendorModel
                    {
                        VendorId = Convert.ToInt32(dr["VendorId"]),
                        VendorName = dr["VendorName"].ToString(),
                        ContactNumber = dr["ContactNumber"].ToString(),
                        Email = dr["Email"].ToString(),
                        Address = dr["Address"].ToString(),
                        IsActive = Convert.ToBoolean(dr["IsActive"]),
                        IsDeleted = Convert.ToBoolean(dr["IsDeleted"]),
                        CreatedDate = dr["CreatedDate"].ToString(),
                        ModifiedDate = dr["ModifiedDate"].ToString()
                    });
                }
            }

            // EXCEL (ClosedXML)
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("VendorReport");

                // HEADER
                ws.Cell(1, 1).Value = "VendorId";
                ws.Cell(1, 2).Value = "VendorName";
                ws.Cell(1, 3).Value = "ContactNumber";
                ws.Cell(1, 4).Value = "Email";
                ws.Cell(1, 5).Value = "Address";
                ws.Cell(1, 6).Value = "IsActive";
                ws.Cell(1, 7).Value = "IsDeleted";
                ws.Cell(1, 8).Value = "CreatedDate";
                ws.Cell(1, 9).Value = "ModifiedDate";

                int row = 2;

                foreach (var item in list)
                {
                    ws.Cell(row, 1).Value = item.VendorId;
                    ws.Cell(row, 2).Value = item.VendorName;
                    ws.Cell(row, 3).Value = item.ContactNumber;
                    ws.Cell(row, 4).Value = item.Email;
                    ws.Cell(row, 5).Value = item.Address;
                    ws.Cell(row, 6).Value = item.IsActive ? "Active" : "Inactive";
                    ws.Cell(row, 7).Value = item.IsDeleted ? "Deleted" : "No";
                    ws.Cell(row, 8).Value = item.CreatedDate;
                    ws.Cell(row, 9).Value = item.ModifiedDate;
                    row++;
                }

                ws.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "VendorReport.xlsx"
                    );
                }
            }
        }


        public IActionResult DownloadVendorPdf(string name, string email, string contact, string status)
        {
            List<VendorModel> list = new List<VendorModel>();

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("GetVendorReport", con))
            {

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Name", name ?? "");
                cmd.Parameters.AddWithValue("@Email", email ?? "");
                cmd.Parameters.AddWithValue("@Contact", contact ?? "");
                cmd.Parameters.AddWithValue("@Status", status ?? "");

                con.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(new VendorModel
                    {
                        VendorId = Convert.ToInt32(dr["VendorId"]),
                        VendorName = dr["VendorName"].ToString(),
                        Email = dr["Email"].ToString(),
                        ContactNumber = dr["ContactNumber"].ToString(),
                        Address = dr["Address"].ToString(),
                        IsActive = Convert.ToBoolean(dr["IsActive"])
                    });
                }
            }

            // ---------------- PDF GENERATION ----------------

            QuestPDF.Settings.License = LicenseType.Community;

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Header()
                        .Text("Vendor Report")
                        .FontSize(20)
                        .SemiBold()
                        .AlignCenter();

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);   // SR
                            columns.RelativeColumn();     // Name
                            columns.RelativeColumn();     // Contact
                            columns.RelativeColumn();     // Email
                            columns.RelativeColumn();     // Address
                            columns.ConstantColumn(60);   // Status
                        });

                        // HEADER
                        table.Header(header =>
                        {
                            header.Cell().Text("ID").SemiBold();
                            header.Cell().Text("Name").SemiBold();
                            header.Cell().Text("Contact").SemiBold();
                            header.Cell().Text("Email").SemiBold();
                            header.Cell().Text("Address").SemiBold();
                            header.Cell().Text("Status").SemiBold();
                        });

                        // DATA ROWS
                        foreach (var v in list)
                        {
                            table.Cell().Text(v.VendorId.ToString());
                            table.Cell().Text(v.VendorName);
                            table.Cell().Text(v.ContactNumber);
                            table.Cell().Text(v.Email);
                            table.Cell().Text(v.Address);
                            table.Cell().Text(v.IsActive ? "Active" : "Inactive");
                        }
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text("Generated on: " + DateTime.Now.ToString("dd-MM-yyyy HH:mm"));
                });
            }).GeneratePdf();

            return File(pdf, "application/pdf", "VendorReport.pdf");
        }
    }
}