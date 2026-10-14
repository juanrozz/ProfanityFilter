namespace ProfanityFilter.DTOs.Request
{
    public class CreateWordRequest
    {
        public string Word { get; set; } = null!;
        public int LanguageId { get; set; }
    }
}
