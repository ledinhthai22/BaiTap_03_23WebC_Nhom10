using Microsoft.AspNetCore.Mvc;
using BaiTap_03_23WebC_Nhom10.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BaiTap_03_23WebC_Nhom10.Areas.Admin.Controllers;
namespace BaiTap_03_23WebC_Nhom10.Views.Shared.Components;

public class SingleShopProduct : ViewComponent
{
    private readonly HttpClient _httpClient;
    public SingleShopProduct(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri("https://localhost:7048/");
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var products = await _httpClient.GetFromJsonAsync<List<Product>>("/api/products");
        return View(products);
    }

}
