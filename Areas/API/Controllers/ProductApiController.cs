using System.Data;
using BaiTap_03_23WebC_Nhom10.Models;
using BaiTap_03_23WebC_Nhom10.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace BaiTap_03_23WebC_Nhom10.Controllers.API
{
    [Route("api/products")]
    [ApiController]
    public class ProductApiController : ControllerBase
    {
        private readonly DatabaseHelper _db;
        private readonly IWebHostEnvironment _env;
        public ProductApiController(DatabaseHelper db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }


        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> products = new List<Product>();
            string query = @"
            SELECT 
                ID, PRODUCT_NAME, PRICE, DISCOUNT, IMAGE, DESCRIPTION,
                QUALITY, CATEGORY_ID, TAG_ID, VIEWS, SELLED, STATUS, 
                CREATE_AT, UPDATE_AT 
            FROM dbo.PRODUCTS 
            ORDER BY CREATE_AT DESC";

            try
            {
                DataTable dt = _db.ExecuteQuery(query);

                foreach (DataRow row in dt.Rows)
                {
                    products.Add(new Product
                    {
                        id = Convert.ToInt32(row["ID"]),
                        productName = row["PRODUCT_NAME"].ToString(),
                        price = row["PRICE"] == DBNull.Value ? 0 : Convert.ToDecimal(row["PRICE"]),
                        discount = row["DISCOUNT"] == DBNull.Value ? 0 : Convert.ToDecimal(row["DISCOUNT"]),
                        image = row["IMAGE"].ToString(),
                        description = row["DESCRIPTION"].ToString(),
                        quality = row["QUALITY"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["QUALITY"]),
                        categoryID = row["CATEGORY_ID"] == DBNull.Value ? 0 : Convert.ToInt32(row["CATEGORY_ID"]),
                        tagID = row["TAG_ID"] == DBNull.Value ? 0 : Convert.ToInt32(row["TAG_ID"]),
                        views = row["VIEWS"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["VIEWS"]),
                        selled = row["SELLED"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["SELLED"]),
                        status = row["STATUS"] == DBNull.Value ? (bool?)null : (Convert.ToInt32(row["STATUS"]) == 1),
                        createAT = row["CREATE_AT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["CREATE_AT"]),
                        updateAT = row["UPDATE_AT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["UPDATE_AT"])
                    });
                }
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Lỗi khi truy vấn cơ sở dữ liệu.",
                    detail = ex.Message
                });
            }
        }

        [HttpGet("{id:int}")]
        public IActionResult GetProductById(int id)
        {
            Product? product = null;

            if (id <= 0)
            {
                return BadRequest(new { error = "Invalid product ID." });
            }

            try
            {
                string query = @"
                SELECT 
                    ID, PRODUCT_NAME, PRICE, DISCOUNT, IMAGE, DESCRIPTION,
                    QUALITY, CATEGORY_ID, TAG_ID, VIEWS, SELLED, STATUS, 
                    CREATE_AT, UPDATE_AT 
                FROM dbo.PRODUCTS 
                WHERE ID = @id";

                var parameters = new Microsoft.Data.SqlClient.SqlParameter[]
                {
                    new Microsoft.Data.SqlClient.SqlParameter("@id", System.Data.SqlDbType.Int) { Value = id }
                };

                DataTable dt = _db.ExecuteQuery(query, parameters);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    // Ánh xạ DataRow đầu tiên sang Model Product
                    product = new Product
                    {
                        id = Convert.ToInt32(row["ID"]),
                        productName = row["PRODUCT_NAME"].ToString(),
                        price = row["PRICE"] == DBNull.Value ? 0 : Convert.ToDecimal(row["PRICE"]),
                        discount = row["DISCOUNT"] == DBNull.Value ? 0 : Convert.ToDecimal(row["DISCOUNT"]),
                        image = row["IMAGE"].ToString(),
                        description = row["DESCRIPTION"].ToString(),
                        quality = row["QUALITY"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["QUALITY"]),
                        categoryID = row["CATEGORY_ID"] == DBNull.Value ? 0 : Convert.ToInt32(row["CATEGORY_ID"]),
                        tagID = row["TAG_ID"] == DBNull.Value ? 0 : Convert.ToInt32(row["TAG_ID"]),
                        views = row["VIEWS"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["VIEWS"]),
                        selled = row["SELLED"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["SELLED"]),
                        status = row["STATUS"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(row["STATUS"]),
                        createAT = row["CREATE_AT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["CREATE_AT"]),
                        updateAT = row["UPDATE_AT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["UPDATE_AT"])
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi truy vấn cơ sở dữ liệu: " + ex.Message);
                return StatusCode(500, new { error = "Internal server error." });
            }
            return Ok(product);
        }
        [HttpPost]
        [Consumes("multipart/form-data")]
        public IActionResult Create(IFormCollection form, IFormFile? imageFile)
        {
            try
            {
                // --- 1) Xử lý nhiều sản phẩm ngăn cách bằng dấu ; ---
                string[] productNames = form["productName"]
                    .ToString()
                    .Split(';', StringSplitOptions.RemoveEmptyEntries);

                if (productNames.Length == 0)
                    return BadRequest(new { message = "Vui lòng nhập ít nhất một tên sản phẩm." });

                // --- 2) Lưu ảnh nếu có ---
                string? imagePath = null;
                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "img", "uploads");
                    if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                    string fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(imageFile.FileName);
                    string fullPath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }

                    imagePath = "/img/uploads/" + fileName;
                }

                // --- 3) Xử lý tag ---
                string tagName = (form["tagID"].FirstOrDefault() ?? string.Empty).Trim();
                int? tagId = null;

                if (!string.IsNullOrEmpty(tagName))
                {
                    var dtTag = _db.ExecuteQuery("SELECT ID FROM TAGS WHERE TAG_NAME = @name",
                        new SqlParameter[] { new SqlParameter("@name", tagName) });

                    if (dtTag.Rows.Count > 0)
                    {
                        tagId = Convert.ToInt32(dtTag.Rows[0]["ID"]);
                    }
                    else
                    {
                        object? inserted = _db.ExecuteScalar(
                            "INSERT INTO TAGS (TAG_NAME, STATUS) OUTPUT INSERTED.ID VALUES (@name, 1)",
                            new SqlParameter[] { new SqlParameter("@name", tagName) });

                        if (inserted != null && int.TryParse(inserted.ToString(), out int parsedTagId))
                            tagId = parsedTagId;
                    }
                }

                // --- 4) Lấy các giá trị khác ---
                decimal price = decimal.TryParse(form["price"], out var p) ? p : 0;
                decimal discount = decimal.TryParse(form["discount"], out var d) ? d / 100 : 0; // 20 => 0.2
                int? quality = int.TryParse(form["quality"], out var q) ? q : null;
                string description = form["description"];
                int categoryID = int.TryParse(form["categoryID"], out var c) ? c : 0;

                // --- 5) Thêm từng sản phẩm ---
                List<object> addedProducts = new();

                foreach (var name in productNames)
                {
                    string query = @"
                INSERT INTO PRODUCTS
                (PRODUCT_NAME, PRICE, DISCOUNT, IMAGE, DESCRIPTION, QUALITY, CATEGORY_ID, TAG_ID, STATUS, CREATE_AT)
                OUTPUT INSERTED.ID
                VALUES (@name, @price, @discount, @image, @desc, @quality, @category, @tag, 1, GETDATE())";

                    var parameters = new SqlParameter[]
                    {
                new SqlParameter("@name", name.Trim()),
                new SqlParameter("@price", price),
                new SqlParameter("@discount", discount),
                new SqlParameter("@image", imagePath ?? (object)DBNull.Value),
                new SqlParameter("@desc", description ?? (object)DBNull.Value),
                new SqlParameter("@quality", quality.HasValue ? (object)quality.Value : (object)DBNull.Value),
                new SqlParameter("@category", categoryID != 0 ? (object)categoryID : (object)DBNull.Value),
                new SqlParameter("@tag", tagId.HasValue ? (object)tagId.Value : (object)DBNull.Value)
                    };

                    object? newIdObj = _db.ExecuteScalar(query, parameters);

                    if (newIdObj != null && int.TryParse(newIdObj.ToString(), out int newId))
                    {
                        addedProducts.Add(new
                        {
                            id = newId,
                            name = name.Trim(),
                            image = imagePath,
                            message = "Thêm sản phẩm thành công"
                        });
                    }
                }

                return Ok(new
                {
                    message = $"Đã thêm {addedProducts.Count} sản phẩm thành công.",
                    data = addedProducts
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Lỗi khi thêm sản phẩm.",
                    detail = ex.Message
                });
            }
        }
    }
}
