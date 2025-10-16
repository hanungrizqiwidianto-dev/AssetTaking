using System;
using System.Collections.Generic;

namespace AssetTaking.Models;

public partial class TblMAssetCategory
{
    public int Id { get; set; }

    public string? KategoriBarang { get; set; }

    public int? PriceRange { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public string? ModifiedBy { get; set; }
}
