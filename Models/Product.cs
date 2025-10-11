using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;

namespace BaiTap_03_23WebC_Nhom10.Models
{
    public class Product
    {
        public int id { get; set; }

        [Required]
        public string? productName { get; set; }

        [Range(0, (double)decimal.MaxValue)]
        public decimal price { get; set; }

        [Range(0, 1)]
        public decimal discount { get; set; }

        public string? image { get; set; }

        public string? description { get; set; }

        public int? quality { get; set; }

        public int categoryID { get; set; }

        public int tagID { get; set; }

        public int? views { get; set; }

        public int? selled { get; set; }

        public bool? status { get; set; }
        public string? categoryName { get; set; }
        public DateTime? createAT { get; set; }

        public DateTime? updateAT { get; set; }
        public List<string>? imageList { get; set; }
    }
}