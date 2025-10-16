using System;
using System.Collections.Generic;

namespace AssetTaking.Models;

public partial class TblRPriceRange
{
    public int Id { get; set; }

    public int? RangeStart { get; set; }

    public int? RangeEnd { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public string? ModifiedBy { get; set; }
}
