using Microsoft.AspNetCore.Mvc;
using BaiTap_03_23WebC_Nhom10.Models;
using BaiTap_03_23WebC_Nhom10.Service;
using Microsoft.Data.SqlClient;
using System.Data;

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
        public IActionResult Create([FromForm] Product product, IFormFile? ImageFiles) // Sử dụng ImageFiles cho file upload
        {
            if (product == null || string.IsNullOrWhiteSpace(product.productName))
                return BadRequest(new { message = "Tên sản phẩm không được để trống." });

            try
            {
                string imagePath = null;

                // 1. Xử lý lưu ảnh (nếu có)
                if (ImageFiles != null && ImageFiles.Length > 0)
                {
                    // Đảm bảo thư mục wwwroot/uploads tồn tại
                    string uploadPath = Path.Combine(_env.WebRootPath, "wwwroot","img");
                    if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                    // Tạo tên file duy nhất
                    string fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(ImageFiles.FileName);
                    string fullPath = Path.Combine(uploadPath, fileName);

                    // Lưu file vào thư mục
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        ImageFiles.CopyTo(stream);
                    }

                    // Lưu đường dẫn tương đối vào database
                    imagePath = "/uploads/" + fileName;
                    product.image = imagePath;
                }

                // 2. Chèn dữ liệu sản phẩm vào DB
                // Dùng OUTPUT INSERTED.ID để lấy ID mới được tạo
                string query = @"
                INSERT INTO PRODUCTS
                (PRODUCT_NAME, PRICE, DISCOUNT, IMAGE, DESCRIPTION, QUALITY, CATEGORY_ID, TAG_ID, STATUS, CREATE_AT)
                OUTPUT INSERTED.ID
                VALUES (@name, @price, @discount, @image, @desc, @quality, @category, @tag, 1, GETDATE())";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@name", product.productName ?? (object)DBNull.Value),
                    new SqlParameter("@price", product.price),
                    new SqlParameter("@discount", product.discount),
                    new SqlParameter("@image", product.image ?? (object)DBNull.Value),
                    new SqlParameter("@desc", product.description ?? (object)DBNull.Value),
                    new SqlParameter("@quality", product.quality ?? (object)DBNull.Value),
                    new SqlParameter("@category", product.categoryID),
                    new SqlParameter("@tag", product.tagID),
                };

                // ExecuteScalar trả về ID mới (vì có OUTPUT INSERTED.ID)
                object newIdObj = _db.ExecuteNonQuery(query, parameters);
                int newId = Convert.ToInt32(newIdObj);
                product.id = newId;

                // Trả về sản phẩm đã tạo thành công
                return Created($"/api/products/{newId}", new { message = "Thêm sản phẩm thành công", product });
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
