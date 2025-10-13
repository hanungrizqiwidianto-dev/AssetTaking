using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AssetTaking.Models;

[Table("TBL_M_ASSET_CATEGORY")]
public partial class TblMAssetCategory
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("kategori_barang")]
    [StringLength(50)]
    [Unicode(false)]
    public string? KategoriBarang { get; set; }

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
