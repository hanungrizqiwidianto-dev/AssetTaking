using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AssetTaking.Models;

[Table("TBL_M_ROLE")]
public partial class TblMRole
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(150)]
    public string? RoleName { get; set; }
}
