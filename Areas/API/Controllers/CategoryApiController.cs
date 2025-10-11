using Microsoft.AspNetCore.Mvc;
using BaiTap_03_23WebC_Nhom10.Models;
using BaiTap_03_23WebC_Nhom10.Service;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BaiTap_03_23WebC_Nhom10.API.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoryApiController : ControllerBase
    {
        private readonly DatabaseHelper _db;
        private readonly IConfiguration _configuration;

        public CategoryApiController(DatabaseHelper db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        // GET: api/categories
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Category> categories = new List<Category>();
            string query = @"SELECT ID, CATEGORY_NAME, STATUS FROM dbo.CATEGORY";

            try
            {
                DataTable dt = _db.ExecuteQuery(query);
                foreach (DataRow row in dt.Rows)
                {
                    categories.Add(new Category
                    {
                        id = Convert.ToInt32(row["ID"]),
                        categoryName = row["CATEGORY_NAME"].ToString(),
                        status = row["STATUS"] == DBNull.Value ? false : (Convert.ToInt32(row["STATUS"]) == 1)
                    });
                }
                return Ok(categories);
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

        // POST: api/categories
        [HttpPost]
        public IActionResult Create([FromBody] Category category)
        {
            if (category == null || string.IsNullOrEmpty(category.categoryName))
                return BadRequest(new { message = "Tên danh mục không được để trống." });
            if(category.categoryName.Length > 100)
                return BadRequest(new { message = "Tên danh mục không được vượt quá 100 ký tự." });
            if (System.Text.RegularExpressions.Regex.IsMatch(category.categoryName, @"\d"))
                return BadRequest(new { message = "Tên danh mục không được chứa số." });
            var connStr = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (var conn = new SqlConnection(connStr))
                {
                    var sql = "INSERT INTO CATEGORY (CATEGORY_NAME, STATUS) OUTPUT INSERTED.ID VALUES (@Name, 1)";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", category.categoryName);
                        conn.Open();

                        // Lấy ID vừa chèn
                        var newId = (int)cmd.ExecuteScalar();
                        category.id = newId;
                        category.status = true;
                    }
                }

                return Ok(category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Lỗi khi thêm danh mục.",
                    detail = ex.Message
                });
            }
        }
    }
}
