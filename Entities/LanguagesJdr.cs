using System;
using System.Collections.Generic;

namespace ProfanityFilter.Entities;

public partial class LanguagesJdr
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public virtual ICollection<BannedWordsJdr> BannedWordsJdrs { get; set; } = new List<BannedWordsJdr>();

    public virtual ICollection<ExceptionWordsJdr> ExceptionWordsJdrs { get; set; } = new List<ExceptionWordsJdr>();
}
