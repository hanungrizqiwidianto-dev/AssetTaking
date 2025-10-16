using System;
using System.Collections.Generic;

namespace AssetTaking.Models;

public partial class TblMStateCategory
{
    public int Id { get; set; }

    public string? State { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public string? ModifiedBy { get; set; }

    public virtual ICollection<TblRAssetSerial> TblRAssetSerials { get; set; } = new List<TblRAssetSerial>();
}
