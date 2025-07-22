using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MyWebAPI.Models;

public partial class Product
{
    public string ProductID { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public decimal Price { get; set; }

    public string? Description { get; set; }

    public string Picture { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string CateID { get; set; } = null!;

    // [JsonIgnore] serialization時忽略OrderDetail屬性(關聯)
    // 避免循環參照
    [JsonIgnore]
    public virtual Category Cate { get; set; } = null!;

    // [JsonIgnore] serialization時忽略OrderDetail屬性
    [JsonIgnore]

    public virtual ICollection<OrderDetail> OrderDetail { get; set; } = new List<OrderDetail>();
}
