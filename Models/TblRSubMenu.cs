using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AssetTaking.Models;

[Table("TBL_R_SUB_MENU")]
public partial class TblRSubMenu
{
    [Key]
    [Column("ID_Sub_Menu")]
    public int IdSubMenu { get; set; }

    [Column("ID_Menu")]
    public int? IdMenu { get; set; }

    [Column("Sub_Menu_Description")]
    [StringLength(150)]
    public string? SubMenuDescription { get; set; }

    [Column("Link_Sub_Menu")]
    [StringLength(250)]
    public string? LinkSubMenu { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Akses { get; set; }
}
