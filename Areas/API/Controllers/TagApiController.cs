using System.Data;
using BaiTap_03_23WebC_Nhom10.Models;
using BaiTap_03_23WebC_Nhom10.Service;
using Microsoft.AspNetCore.Mvc;

namespace BaiTap_03_23WebC_Nhom10.Areas.API.Controllers
{
    [Route("api/tags")]
    [ApiController]
    public class TagApiController : Controller
    {
        private readonly DatabaseHelper _db;

        public TagApiController(DatabaseHelper db)
        {
            _db = db;
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
    }
}
