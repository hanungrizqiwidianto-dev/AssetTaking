using System;
using System.Collections.Generic;

namespace AssetTaking.Models;

public partial class TblRAssetSerial
{
    public int SerialId { get; set; }

    public int AssetId { get; set; }

    public string SerialNumber { get; set; } = null!;

    public string? State { get; set; }

    public int? Status { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public string? ModifiedBy { get; set; }

    public virtual TblTAsset Asset { get; set; } = null!;
}
