using System;
using System.Collections.Generic;

namespace AssetTaking.Models;

public partial class TblTAsset
{
    public int Id { get; set; }

    public int AssetId { get; set; }

    public int? AssetInId { get; set; }

    public int? AssetOutId { get; set; }

    public string? NamaBarang { get; set; }

    public DateTime? TanggalMasuk { get; set; }

    public string? NomorAsset { get; set; }

    public string? KodeBarang { get; set; }

    public string? KategoriBarang { get; set; }

    public int? Qty { get; set; }

    public string? DstrctIn { get; set; }

    public string? DstrctOut { get; set; }

    public int? Status { get; set; }

    public string? PoNumber { get; set; }

    public string? Foto { get; set; }

    public DateTime? SentAt { get; set; }

    public string? SentBy { get; set; }

    public DateTime? AcceptedAt { get; set; }

    public string? AcceptedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public string? ModifiedBy { get; set; }

    public virtual ICollection<TblRAssetPo> TblRAssetPos { get; set; } = new List<TblRAssetPo>();

    public virtual ICollection<TblRAssetSerial> TblRAssetSerials { get; set; } = new List<TblRAssetSerial>();
}
