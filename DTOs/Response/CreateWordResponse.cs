namespace ProfanityFilter.DTOs.Response
{
    public class CreateWordResponse
    {
        public string Word { get; set; } = null!;
        public int LanguageId { get; set; }

        public CreateWordLanguagesResponse Language { get; set; } = null!;
    }
}
