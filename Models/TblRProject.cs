using System;
using System.Collections.Generic;

namespace AssetTaking.Models;

public partial class TblRProject
{
    public int Id { get; set; }

    public string? Project { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public string? ModifiedBy { get; set; }
}
