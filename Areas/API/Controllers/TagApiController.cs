using System.Data;
using BaiTap_03_23WebC_Nhom10.Models;
using BaiTap_03_23WebC_Nhom10.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BaiTap_03_23WebC_Nhom10.Areas.API.Controllers
{
    [Route("api/tags")]
    [ApiController]
    public class TagApiController : ControllerBase
    {
        private readonly DatabaseHelper _db;
        private readonly IConfiguration _configuration;

        public TagApiController(DatabaseHelper db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetAllTags()
        {
            List<Tag> tags = new List<Tag>();
            string query = "SELECT ID, TAG_NAME FROM dbo.TAGS ORDER BY TAG_NAME";

            try
            {
                DataTable dt = _db.ExecuteQuery(query);

                foreach (DataRow row in dt.Rows)
                {
                    tags.Add(new Tag
                    {
                        id = Convert.ToInt32(row["ID"]),
                        tagName = row["TAG_NAME"].ToString(),
                    });
                }
                return Ok(tags);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi tải danh sách tags: " + ex.Message);
                return StatusCode(500, new
                {
                    error = "Lỗi khi truy vấn cơ sở dữ liệu để lấy tags.",
                    detail = ex.Message
                });
            }
        }

        [HttpPost]
        public IActionResult Create([FromBody] Tag tag)
        {
            if (tag == null || string.IsNullOrEmpty(tag.tagName))
                return BadRequest(new { message = "Tên thẻ không được để trống." });

            if (tag.tagName.Length > 100)
                return BadRequest(new { message = "Tên thẻ không được vượt quá 100 ký tự." });

            if (System.Text.RegularExpressions.Regex.IsMatch(tag.tagName, @"\d"))
                return BadRequest(new { message = "Tên thẻ không được chứa số." });

            var connStr = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();

               
                    var checkSql = "SELECT COUNT(*) FROM TAGS WHERE TAG_NAME = @Name";
                    using (var checkCmd = new SqlCommand(checkSql, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Name", tag.tagName);
                        int count = (int)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            return BadRequest(new { message = "Thẻ này đã tồn tại." });
                        }
                    }

                    // 2. Thêm tag mới nếu chưa tồn tại
                    var insertSql = "INSERT INTO TAGS (TAG_NAME) OUTPUT INSERTED.ID VALUES (@Name)";
                    using (var insertCmd = new SqlCommand(insertSql, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@Name", tag.tagName);
                        var newId = (int)insertCmd.ExecuteScalar();
                        tag.id = newId;
                    }
                }

                return Ok(tag);
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
