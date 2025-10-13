using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AssetTaking.Models;

[Table("TBL_T_ASSET_IN")]
public partial class TblTAssetIn
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("nama_barang")]
    [Unicode(false)]
    public string? NamaBarang { get; set; }

    [Column("nomor_asset")]
    [Unicode(false)]
    public string? NomorAsset { get; set; }

    [Column("kode_barang")]
    [StringLength(50)]
    [Unicode(false)]
    public string? KodeBarang { get; set; }

    [Column("kategori_barang")]
    [StringLength(50)]
    [Unicode(false)]
    public string? KategoriBarang { get; set; }

    [Column("foto")]
    [Unicode(false)]
    public string? Foto { get; set; }

    [Column("qty")]
    public int? Qty { get; set; }

    [Column("created_at", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column("created_by")]
    [StringLength(50)]
    [Unicode(false)]
    public string? CreatedBy { get; set; }

    [Column("modified_at", TypeName = "datetime")]
    public DateTime? ModifiedAt { get; set; }

    [Column("modified_by")]
    [StringLength(50)]
    [Unicode(false)]
    public string? ModifiedBy { get; set; }
}
