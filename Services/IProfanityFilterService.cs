using ProfanityFilter.Common;
using ProfanityFilter.DTOs.Response;
using ProfanityFilter.Entities;

namespace ProfanityFilter.Services
{
    public interface IProfanityFilterService
    {
        Task LoadWordsFromDBAsync() => Task.CompletedTask;
        Task<(Result<string> Result, double TimeMs)> HasProfanity(string? userText);
        Task<(Result<CreateWordResponse> Result, double TimeMs)> CreateProfaniyWord(string? word, int languageId);
        Task<(Result<CreateWordResponse> Result, double TimeMs)> CreateExceptionWord(string? word, int languageId);
    }
}
