using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AssetTaking.Models;

[PrimaryKey("IdRole", "Username")]
[Table("TBL_M_USER")]
public partial class TblMUser
{
    [Key]
    [Column("ID_Role")]
    public int IdRole { get; set; }

    [Key]
    [StringLength(150)]
    [Unicode(false)]
    public string Username { get; set; } = null!;
}
