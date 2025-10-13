using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AssetTaking.Models;

[PrimaryKey("IdMenu", "IdRole")]
[Table("TBL_M_AKSES")]
public partial class TblMAkse
{
    [Key]
    [Column("ID_Menu")]
    public int IdMenu { get; set; }

    [Key]
    [Column("ID_Role")]
    public int IdRole { get; set; }

    [Column("IS_ALLOW")]
    public bool? IsAllow { get; set; }
}
