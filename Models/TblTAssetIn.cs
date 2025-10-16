using System;
using System.Collections.Generic;

namespace AssetTaking.Models;

public partial class TblTAssetIn
{
    public int Id { get; set; }

    public string? NamaBarang { get; set; }

    public string? NomorAsset { get; set; }

    public string? KodeBarang { get; set; }

    public string? KategoriBarang { get; set; }

    public string? DstrctIn { get; set; }

    public string? Foto { get; set; }

    public int? Qty { get; set; }

    public string? State { get; set; }

    public DateTime? SentAt { get; set; }

    public string? SentBy { get; set; }

    public DateTime? AcceptedAt { get; set; }

    public string? AcceptedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public string? ModifiedBy { get; set; }
}
