using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AssetTaking.Models;

[Table("TBL_R_MENU")]
public partial class TblRMenu
{
    [Key]
    [Column("ID_Menu")]
    public int IdMenu { get; set; }

    [Column("Name_Menu")]
    [StringLength(50)]
    public string? NameMenu { get; set; }

    [Column("Sub_Menu")]
    public int? SubMenu { get; set; }

    [Column("Icon_Menu")]
    [StringLength(150)]
    public string? IconMenu { get; set; }

    [Column("Link_Menu")]
    [StringLength(250)]
    public string? LinkMenu { get; set; }

    public int? Order { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Akses { get; set; }
}
