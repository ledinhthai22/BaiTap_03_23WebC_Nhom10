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

        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                string query = @"
                    SELECT P.ID, P.PRODUCT_NAME, P.PRICE, P.DISCOUNT, P.IMAGE, DESCRIPTION,
                           QUALITY, CATEGORY_ID, TAG_ID, VIEWS, SELLED, P.STATUS, C.CATEGORY_NAME, 
                           CREATE_AT, UPDATE_AT 
                    FROM dbo.PRODUCTS  P JOIN CATEGORY C ON P.CATEGORY_ID = C.ID
                    WHERE P.STATUS = 1";

                DataTable dt = _db.ExecuteQuery(query);
                List<Product> products = new();

                foreach (DataRow row in dt.Rows)
                {
                    products.Add(MapToProduct(row));
                }

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi khi truy vấn cơ sở dữ liệu.", detail = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public IActionResult GetProductById(int id)
        {
            if (id <= 0)
                return BadRequest(new { error = "Invalid product ID." });

            try
            {
                string query = @"
                        SELECT P.ID, P.PRODUCT_NAME, P.PRICE, P.DISCOUNT, P.IMAGE, P.DESCRIPTION,
                               P.QUALITY, P.CATEGORY_ID, P.TAG_ID, P.VIEWS, P.SELLED, P.STATUS, C.CATEGORY_NAME, 
                               P.CREATE_AT, P.UPDATE_AT 
                        FROM dbo.PRODUCTS P 
                        JOIN CATEGORY C ON P.CATEGORY_ID = C.ID
                        WHERE P.ID = @id";

                var parameters = new SqlParameter[]
                {
            new("@id", SqlDbType.Int) { Value = id }
                };

                DataTable dt = _db.ExecuteQuery(query, parameters);

                if (dt.Rows.Count == 0)
                    return NotFound(new { error = "Không tìm thấy sản phẩm." });

                var product = MapToProduct(dt.Rows[0]);

                // Xử lý danh sách ảnh: tách theo dấu ';' và thêm log để debug
                if (!string.IsNullOrEmpty(product.image))
                {
                    product.imageList = product.image.Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select(img => $"/img/{img.Trim()}")
                        .ToList();
                }
                else
                {
                    product.imageList = new List<string> { "/img/no-image.jpg" }; // Mặc định nếu không có ảnh
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi khi truy vấn cơ sở dữ liệu.", detail = ex.Message });
            }
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public IActionResult Create(IFormCollection form)
        {
            try
            {
                string[] productNames = form["productName"]
                    .ToString()
                    .Split(';', StringSplitOptions.RemoveEmptyEntries);

                if (productNames.Length == 0)
                    return BadRequest(new { message = "Vui lòng nhập ít nhất một tên sản phẩm." });

                List<string> imageNames = new();
                if (form.Files?.Count > 0)
                {
                    string uploadPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "img");
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    foreach (var file in form.Files)
                    {
                        if (file.Length > 0)
                        {
                            string fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName);
                            string fullPath = Path.Combine(uploadPath, fileName);
                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }
                            imageNames.Add(fileName);
                        }
                    }
                }

                string imageList = string.Join(";", imageNames);

                string tagName = (form["tagID"].FirstOrDefault() ?? string.Empty).Trim();
                int? tagId = null;

                if (!string.IsNullOrEmpty(tagName))
                {
                    var dtTag = _db.ExecuteQuery("SELECT ID FROM TAGS WHERE TAG_NAME = @name",
                        new SqlParameter[] { new("@name", tagName) });

                    if (dtTag.Rows.Count > 0)
                        tagId = Convert.ToInt32(dtTag.Rows[0]["ID"]);
                    else
                    {
                        object? inserted = _db.ExecuteScalar(
                            "INSERT INTO TAGS (TAG_NAME, STATUS) OUTPUT INSERTED.ID VALUES (@name, 1)",
                            new SqlParameter[] { new("@name", tagName) });

                        if (inserted != null && int.TryParse(inserted.ToString(), out int newTagId))
                            tagId = newTagId;
                    }
                }

                decimal price = decimal.TryParse(form["price"], out var p) ? p : 0;
                decimal discount = decimal.TryParse(form["discount"], out var d) ? d / 100 : 0;
                int? quality = int.TryParse(form["quality"], out var q) ? q : null;
                string description = form["description"];
                int categoryID = int.TryParse(form["categoryID"], out var c) ? c : 0;

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
                        new("@name", name.Trim()),
                        new("@price", price),
                        new("@discount", discount),
                        new("@image", string.IsNullOrEmpty(imageList) ? (object)DBNull.Value : imageList),
                        new("@desc", description ?? (object)DBNull.Value),
                        new("@quality", quality ?? (object)DBNull.Value),
                        new("@category", categoryID != 0 ? (object)categoryID : DBNull.Value),
                        new("@tag", tagId ?? (object)DBNull.Value)
                    };

                    object? newIdObj = _db.ExecuteScalar(query, parameters);

                    if (newIdObj != null && int.TryParse(newIdObj.ToString(), out int newId))
                    {
                        addedProducts.Add(new
                        {
                            id = newId,
                            name = name.Trim(),
                            images = imageNames,
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
                return StatusCode(500, new { error = "Lỗi khi thêm sản phẩm.", detail = ex.Message });
            }
        }

        private Product MapToProduct(DataRow row)
        {
            return new Product
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
                updateAT = row["UPDATE_AT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["UPDATE_AT"]),
                categoryName = row["CATEGORY_NAME"]?.ToString() // Thêm ánh xạ categoryName
            };
        }
    }
}