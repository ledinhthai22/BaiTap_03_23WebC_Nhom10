using System.Data;
using BaiTap_03_23WebC_Nhom10.Models;
using BaiTap_03_23WebC_Nhom10.Service;
using Microsoft.AspNetCore.Mvc;

namespace BaiTap_03_23WebC_Nhom10.Areas.API.Controllers
{
    [Area("API")]
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

            try
            {
                string query = @"
                    SELECT 
                        ID, PRODUCT_NAME, PRICE, DISCOUNT, IMAGE, DESCRIPTION,
                        QUALITY, CATEGORY_ID, TAG_ID, VIEWS, SELLED, STATUS, 
                        CREATE_AT, UPDATE_AT 
                    FROM dbo.PRODUCTS 
                    ORDER BY CREATE_AT DESC";

                DataTable dt = _dbHelper.ExecuteQuery(query);

                foreach (DataRow row in dt.Rows)
                {
                    products.Add(new Product
                    {
                        // SỬA: Ánh xạ từ cột SQL (UPPERCASE) sang thuộc tính Model (camelCase)
                        id = Convert.ToInt32(row["ID"]),
                        productName = row["PRODUCT_NAME"].ToString(),
                        price = row["PRICE"] == DBNull.Value ? 0 : Convert.ToDecimal(row["PRICE"]),
                        // DISCOUNT giờ là decimal
                        discount = row["DISCOUNT"] == DBNull.Value ? 0 : Convert.ToDecimal(row["DISCOUNT"]),
                        image = row["IMAGE"].ToString(),
                        description = row["DESCRIPTION"].ToString(),
                        quality = row["QUALITY"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["QUALITY"]),
                        categoryID = row["CATEGORY_ID"] == DBNull.Value ? 0 : Convert.ToInt32(row["CATEGORY_ID"]), // categoryID không nullable
                        tagID = row["TAG_ID"] == DBNull.Value ? 0 : Convert.ToInt32(row["TAG_ID"]), // tagID không nullable
                        views = row["VIEWS"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["VIEWS"]),
                        selled = row["SELLED"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["SELLED"]),
                        status = row["STATUS"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(row["STATUS"]),
                        createAT = row["CREATE_AT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["CREATE_AT"]),
                        updateAT = row["UPDATE_AT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["UPDATE_AT"])
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi truy vấn cơ sở dữ liệu: " + ex.Message);
            }

            return Json(products);
        }
    }
}