using System;
using System.Collections.Generic;

namespace AssetTaking.Models;

public partial class TblRAssetPo
{
    public int Id { get; set; }

    public int AssetId { get; set; }

    public string? PoNumber { get; set; }

    public string? PoItem { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public string? ModifiedBy { get; set; }

    public virtual TblTAsset Asset { get; set; } = null!;
}
