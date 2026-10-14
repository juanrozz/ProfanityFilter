using System;
using System.Collections.Generic;

namespace ProfanityFilter.Entities;

public partial class BannedWordsJdr
{
    public int Id { get; set; }

    public string Word { get; set; } = null!;

    public int LanguageId { get; set; }

    public virtual LanguagesJdr Language { get; set; } = null!;
}
