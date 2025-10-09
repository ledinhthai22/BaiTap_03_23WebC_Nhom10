using System.Data;
using BaiTap_03_23WebC_Nhom10.Models;
using BaiTap_03_23WebC_Nhom10.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BaiTap_03_23WebC_Nhom10.Areas.API.Controllers
{
    [Area("API")]
    [Route("[area]/[controller]")]
    public class APIController : Controller
    {
        private readonly DatabaseHelper _dbHelper;

        public APIController(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }


        [HttpGet]
        public IActionResult GetProducts()
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
                DataTable dt = _dbHelper.ExecuteQuery(query);

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
                return Json(products);
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

        [HttpGet("{id}")]
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

                DataTable dt = _dbHelper.ExecuteQuery(query, parameters);

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
            return Json(product);
        }
        [HttpGet("categories")]
        public IActionResult getCategory()
        {
            List<Category> categories = new List<Category>();
            string query = @"SELECT ID, CATEGORY_NAME, STATUS FROM dbo.CATEGORY";
            try
            {
                DataTable dt = _dbHelper.ExecuteQuery(query);
                foreach (DataRow row in dt.Rows)
                {
                    categories.Add(new Category
                    {
                        id = Convert.ToInt32(row["ID"]),
                        categoryName = row["CATEGORY_NAME"].ToString(),
                        status = row["STATUS"] == DBNull.Value ? (bool?)null : (Convert.ToInt32(row["STATUS"]) == 1)
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Lỗi khi truy vấn cơ sở dữ liệu.",
                    detail = ex.Message
                });

            }
            return Json(categories);
        }
    }
}