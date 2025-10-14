using System.Data;
using BaiTap_03_23WebC_Nhom10.Models;
using BaiTap_03_23WebC_Nhom10.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

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

        // GET ALL
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                string query = @"
                    SELECT P.ID, P.PRODUCT_NAME, P.PRICE, P.DISCOUNT, P.IMAGE, P.DESCRIPTION,
                             P.QUALITY, P.CATEGORY_ID, P.TAG_ID, P.VIEWS, P.SELLED, 
                             P.STATUS, C.CATEGORY_NAME, T.TAG_NAME , P.CREATE_AT, P.UPDATE_AT
                      FROM PRODUCTS P
                      JOIN CATEGORY C ON P.CATEGORY_ID = C.ID
                      JOIN TAGS T ON P.TAG_ID = T.ID
                      WHERE P.STATUS = 1";

                DataTable dt = _db.ExecuteQuery(query);
                var products = new List<Product>();

                foreach (DataRow row in dt.Rows)
                    products.Add(MapToProduct(row));

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi khi truy vấn dữ liệu.", detail = ex.Message });
            }
        }

        // GET BY ID
        [HttpGet("{id:int}")]
        public IActionResult GetProductById(int id)
        {
            if (id <= 0)
                return BadRequest(new { error = "ID sản phẩm không hợp lệ." });

            try
            {
                string query = @"
                        SELECT P.ID, P.PRODUCT_NAME, P.PRICE, P.DISCOUNT, P.IMAGE, P.DESCRIPTION,
                             P.QUALITY, P.CATEGORY_ID, P.TAG_ID, P.VIEWS, P.SELLED, 
                             P.STATUS, C.CATEGORY_NAME, T.TAG_NAME , P.CREATE_AT, P.UPDATE_AT
                      FROM PRODUCTS P
                      JOIN CATEGORY C ON P.CATEGORY_ID = C.ID
                      JOIN TAGS T ON P.TAG_ID = T.ID
                        WHERE P.ID = @id";

                var parameters = new SqlParameter[]
                {
                    new("@id", SqlDbType.Int) { Value = id }
                };

                var param = new SqlParameter("@id", id);
                DataTable dt = _db.ExecuteQuery(query, new[] { param });

                if (dt.Rows.Count == 0)
                    return NotFound(new { error = "Không tìm thấy sản phẩm." });

                var product = MapToProduct(dt.Rows[0]);

                // Danh sách ảnh
                product.imageList = string.IsNullOrEmpty(product.image)
                    ? new List<string> { "/img/no-image.jpg" }
                    : product.image.Split(';', StringSplitOptions.RemoveEmptyEntries)
                                   .Select(img => $"/img/{img.Trim()}").ToList();

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi khi truy vấn sản phẩm.", detail = ex.Message });
            }
        }

        // POST - CREATE
        [HttpPost]
        [Consumes("multipart/form-data")]
        public IActionResult Create(IFormCollection form)
        {
            try
            {
 
                string name = form["productName"];
                if (string.IsNullOrWhiteSpace(name))
                    return BadRequest(new { error = "Vui lòng nhập tên sản phẩm." });

                string checkQuery = "SELECT COUNT(*) FROM PRODUCTS WHERE PRODUCT_NAME = @name";
                var checkParam = new SqlParameter("@name", name.Trim());
                int existingCount = Convert.ToInt32(_db.ExecuteScalar(checkQuery, new[] { checkParam }));

                if (existingCount > 0)
                {
                    return BadRequest(new { error = "Tên sản phẩm đã tồn tại trong hệ thống!" });
                }

 
                var imageNames = new List<string>();
                if (form.Files != null && form.Files.Count > 0)
                {
                    if (form.Files.Count > 5)
                        return BadRequest(new { error = "Chỉ được tải tối đa 5 ảnh cho mỗi sản phẩm." });

                    string uploadPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "img");
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    foreach (var file in form.Files)
                    {
                        if (file.Length > 0)
                        {
                            string fileName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
                            using var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create);
                            file.CopyTo(stream);
                            imageNames.Add(fileName);
                        }
                    }
                }

                // Ảnh đầu tiên là ảnh chính
                string imageList = string.Join(";", imageNames);
                string mainImage = imageNames.FirstOrDefault();

                // Tag xử lý
                string tagName = form["tagID"].FirstOrDefault();
                int? tagId = null;
                if (!string.IsNullOrEmpty(tagName))
                {
                    var dt = _db.ExecuteQuery("SELECT ID FROM TAGS WHERE TAG_NAME = @name",
                        new[] { new SqlParameter("@name", tagName) });

                    if (dt.Rows.Count > 0)
                        tagId = Convert.ToInt32(dt.Rows[0]["ID"]);
                    else
                    {
                        var inserted = _db.ExecuteScalar(
                            "INSERT INTO TAGS (TAG_NAME) OUTPUT INSERTED.ID VALUES (@name)",
                            new[] { new SqlParameter("@name", tagName) });

                        if (inserted != null)
                            tagId = Convert.ToInt32(inserted);
                    }
                }

                //  Parse dữ liệu
                decimal price = decimal.TryParse(form["price"], out var p) ? p : 0;
                decimal discount = decimal.TryParse(form["discount"], out var d) ? d / 100 : 0;
                int? quality = int.TryParse(form["quality"], out var q) ? q : null;
                string description = form["description"];
                int categoryId = int.TryParse(form["categoryID"], out var c) ? c : 0;

                string insertQuery = @"
                                        INSERT INTO PRODUCTS (PRODUCT_NAME, PRICE, DISCOUNT, IMAGE, DESCRIPTION,
                                                              QUALITY, CATEGORY_ID, TAG_ID, STATUS, CREATE_AT)
                                        OUTPUT INSERTED.ID
                                        VALUES (@name, @price, @discount, @image, @desc, @quality, @category, @tag, 1, GETDATE())";

                var parameters = new[]
                {
                    new SqlParameter("@name", name.Trim()),
                    new SqlParameter("@price", price),
                    new SqlParameter("@discount", discount),
                    new SqlParameter("@image", (object?)imageList ?? DBNull.Value),
                    new SqlParameter("@desc", (object?)description ?? DBNull.Value),
                    new SqlParameter("@quality", (object?)quality ?? DBNull.Value),
                    new SqlParameter("@category", categoryId),
                    new SqlParameter("@tag", (object?)tagId ?? DBNull.Value)
                };

                var newIdObj = _db.ExecuteScalar(insertQuery, parameters);

                if (newIdObj == null)
                    return StatusCode(500, new { error = "Không thể thêm sản phẩm." });

                int newId = Convert.ToInt32(newIdObj);

                return Ok(new
                {
                    message = "Thêm sản phẩm thành công.",
                    data = new
                    {
                        id = newId,
                        name,
                        mainImage,
                        images = imageNames
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi khi thêm sản phẩm.", detail = ex.Message });
            }
        }

        // Helper mapper
        private Product MapToProduct(DataRow row)
        {
            return new Product
            {
                id = Convert.ToInt32(row["ID"]),
                productName = row["PRODUCT_NAME"].ToString(),
                price = row["PRICE"] == DBNull.Value ? 0 : Convert.ToDecimal(row["PRICE"]),
                discount = row["DISCOUNT"] == DBNull.Value ? 0 : Convert.ToDecimal(row["DISCOUNT"]),
                image = row["IMAGE"]?.ToString(),
                description = row["DESCRIPTION"]?.ToString(),
                quality = row["QUALITY"] == DBNull.Value ? null : Convert.ToInt32(row["QUALITY"]),
                categoryID = Convert.ToInt32(row["CATEGORY_ID"]),
                tagID = row["TAG_ID"] == DBNull.Value ? null : Convert.ToInt32(row["TAG_ID"]),
                views = row["VIEWS"] == DBNull.Value ? 0 : Convert.ToInt32(row["VIEWS"]),
                selled = row["SELLED"] == DBNull.Value ? 0 : Convert.ToInt32(row["SELLED"]),
                status = row["STATUS"] != DBNull.Value && Convert.ToBoolean(row["STATUS"]),
                createAT = row["CREATE_AT"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(row["CREATE_AT"]),
                updateAT = row["UPDATE_AT"] == DBNull.Value ? null : Convert.ToDateTime(row["UPDATE_AT"]),
                categoryName = row["CATEGORY_NAME"]?.ToString(),
                tagName = row["TAG_NAME"]?.ToString()

            };
        }
    }
}
