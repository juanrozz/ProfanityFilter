//using Microsoft.EntityFrameworkCore;
//using ProfanityFilter.Common;
//using ProfanityFilter.DTOs.Request;
//using ProfanityFilter.DTOs.Response;
//using ProfanityFilter.Entities;
//using System.Diagnostics;
//using System.Globalization;
//using System.Text.RegularExpressions;

//namespace ProfanityFilter.Services
//{
//    public class ProfanityFilterServiceWithDisponse : IProfanityFilterService, IDisposable
//    {
//        private readonly InternsDbContext _dbContext;
//        private readonly object _lockObject = new();
//        private HashSet<string> _profanityList = new(StringComparer.OrdinalIgnoreCase);
//        private bool _disposed = false;
//        private HashSet<string> _exceptionList = new(StringComparer.OrdinalIgnoreCase);
//        private Regex _compiledProfanityRegex;
//        public ProfanityFilterServiceWithDisponse(InternsDbContext dbContext)
//        {
//            _dbContext = dbContext;
//        }

//        private static string CleanTextForUser(string userText)
//        {
//            if (string.IsNullOrEmpty(userText)) return string.Empty;

//            string regexPattern = @"[^a-zA-Z0-9áéíóúÁÉÍÓÚñÑ\s.,;:¡!¿?()\-–—\r\n]";



//            return Regex.Replace(userText, regexPattern, "");
//        }


//        private static string CleanText(string input)
//        {
//            if (string.IsNullOrEmpty(input)) return string.Empty;

//            var leetDict = new Dictionary<char, char> {
//                { '4', 'a' }, { '@', 'a' }, { '3', 'e' }, { '1', 'i' },
//                { '!', 'i' }, { '0', 'o' }, { '5', 's' }, { '$', 's' },
//                { '7', 't' }, { '8', 'b' }
//            };

//            string normalizedText = input.Normalize(System.Text.NormalizationForm.FormD);
//            var cleanChars = new List<char>();

//            for (int i = 0; i < normalizedText.Length; i++)
//            {
//                char c = normalizedText[i];


//                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
//                {

//                    if (cleanChars.Count > 0 && cleanChars[cleanChars.Count - 1] == 'n')
//                    {
//                        cleanChars[cleanChars.Count - 1] = 'ñ';
//                        continue;
//                    }

//                    continue;
//                }

//                char lowerC = char.ToLowerInvariant(c);

//                if (leetDict.TryGetValue(lowerC, out char normalText))
//                {
//                    cleanChars.Add(normalText);
//                }

//                else if (char.IsAsciiLetterOrDigit(lowerC) || lowerC == 'ñ' || lowerC == ' ')
//                {
//                    cleanChars.Add(lowerC);
//                }
//            }

//            return new string(cleanChars.ToArray());
//        }

//        public async Task<(HashSet<string> ProfanityList, HashSet<string> ExceptionList, Regex CompiledRegex)> LoadWordsFromDBAsync()
//        {

//            var dbWords = await _dbContext.BannedWordsJdrs
//                .Select(w => w.Word)
//                .ToListAsync();

//            var dbWordsExceptions = await _dbContext.ExceptionWordsJdrs
//                .Select(f => f.Word)
//                .ToListAsync();

//            var cleanWords = dbWords.Select(CleanText).Where(w => !string.IsNullOrEmpty(w)).ToList();

//            var profanityList = new HashSet<string>(cleanWords, StringComparer.OrdinalIgnoreCase);
//            var exceptionList = new HashSet<string>(dbWordsExceptions, StringComparer.OrdinalIgnoreCase);

//            string combinedPattern = $@"\b[a-zñ]*({string.Join("|", profanityList.Select(Regex.Escape))})[a-zñ]*\b";

//            var compiledProfanityRegex = new Regex(combinedPattern, RegexOptions.IgnoreCase);

//            return (profanityList, exceptionList, compiledProfanityRegex);
//        }


//        public async Task<(Result<CreateWordResponse> Result, double TimeMs)> CreateProfaniyWord(string? word, int languageId)
//        {
//            var stopwatch = Stopwatch.StartNew();

//            if (string.IsNullOrWhiteSpace(word))
//            {
//                stopwatch.Stop();
//                return (Result<CreateWordResponse>.Failure(ProfanityErrors.TheDataIsEmptyOrNull), stopwatch.Elapsed.TotalMilliseconds);
//            }

//            if (languageId <= 0)
//            {
//                stopwatch.Stop();
//                return (Result<CreateWordResponse>.Failure($"the data {languageId} cannot be less than or equal to 0"), stopwatch.Elapsed.TotalMilliseconds);
//            }


//            bool wordExist = await _dbContext.BannedWordsJdrs
//                .AsNoTracking()
//                .AnyAsync(w => w.Word == word);


//            if (wordExist)
//            {
//                stopwatch.Stop();
//                return (Result<CreateWordResponse>.Failure(ProfanityErrors.WordAlreadyExists), stopwatch.Elapsed.TotalMilliseconds);
//            }

//            LanguagesJdr? lenguage = await _dbContext.LanguagesJdrs
//                .AsNoTracking()
//                .FirstOrDefaultAsync(l => l.Id == languageId);

//            if (lenguage == null)
//            {
//                stopwatch.Stop();
//                return (Result<CreateWordResponse>.Failure(ProfanityErrors.LanguageNotFound), stopwatch.Elapsed.TotalMilliseconds);
//            }


//            //CreateProfanityWordResponse
//            BannedWordsJdr newWord = new BannedWordsJdr
//            {
//                Word = word,
//                LanguageId = languageId,
//            };

//            await _dbContext.BannedWordsJdrs.AddAsync(newWord);
//            await _dbContext.SaveChangesAsync();

//            lock (_lockObject)
//            {
//                _profanityList.Add(CleanText(word));
//            }

//            CreateWordResponse response = new CreateWordResponse
//            {
//                Word = word,
//                LanguageId = languageId,
//                Language = new CreateWordLanguagesResponse
//                {
//                    Name = lenguage.Name,
//                    Code = lenguage.Code,
//                }
//            };

//            stopwatch.Stop();
//            return (Result<CreateWordResponse>.Success(response), stopwatch.Elapsed.TotalMilliseconds);
//        }


//        public async Task<(Result<CreateWordResponse> Result, double TimeMs)> CreateExceptionWord(string? word, int languageId)
//        {
//            var stopwatch = Stopwatch.StartNew();

//            if (string.IsNullOrWhiteSpace(word))
//            {
//                stopwatch.Stop();
//                return (Result<CreateWordResponse>.Failure(ProfanityErrors.TheDataIsEmptyOrNull), stopwatch.Elapsed.TotalMilliseconds);
//            }
//            if (languageId <= 0)
//            {
//                stopwatch.Stop();
//                return (Result<CreateWordResponse>.Failure($"the data {languageId} cannot be less than or equal to 0"), stopwatch.Elapsed.TotalMilliseconds);
//            }


//            LanguagesJdr? language = await _dbContext.LanguagesJdrs
//                .AsNoTracking()
//                .FirstOrDefaultAsync(l => l.Id == languageId);

//            if (language == null)
//            {
//                stopwatch.Stop();
//                return (Result<CreateWordResponse>.Failure(ProfanityErrors.LanguageNotFound), stopwatch.Elapsed.TotalMilliseconds);
//            }

//            bool wordExist = await _dbContext.ExceptionWordsJdrs
//                .AsNoTracking()
//                .AnyAsync(w => w.Word == word);

//            if (wordExist)
//            {
//                stopwatch.Stop();
//                return (Result<CreateWordResponse>.Failure(ProfanityErrors.WordAlreadyExists), stopwatch.Elapsed.TotalMilliseconds);
//            }

//            ExceptionWordsJdr newWord = new ExceptionWordsJdr
//            {
//                Word = word,
//                LanguageId = languageId,
//            };

//            _dbContext.ExceptionWordsJdrs.Add(newWord);
//            await _dbContext.SaveChangesAsync();

//            string cleanWord = CleanText(word);

//            lock (_lockObject)
//            {
//                _exceptionList.Add(cleanWord);
//            }

//            CreateWordResponse response = new CreateWordResponse
//            {
//                Word = word,
//                LanguageId = languageId,
//                Language = new CreateWordLanguagesResponse
//                {
//                    Name = language.Name,
//                    Code = language.Code,
//                }
//            };

//            stopwatch.Stop();
//            return (Result<CreateWordResponse>.Success(response), stopwatch.Elapsed.TotalMilliseconds);
//        }


//        public async Task<(Result<string> Result, double TimeMs)> HasProfanity(string? userText)
//        {
//            var stopwatch = Stopwatch.StartNew();

//            if (string.IsNullOrWhiteSpace(userText))
//            {
//                stopwatch.Stop();
//                return (Result<string>.Failure(ProfanityErrors.TheDataIsEmptyOrNull), stopwatch.Elapsed.TotalMilliseconds);
//            }

//            var (localProfanityList, localExceptionList, localRegex) = await LoadWordsFromDBAsync();

//            string cleanText = CleanText(userText);
//            string[] rawWords = cleanText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

//            var normalWords = new List<string>();
//            var reconstructedWords = new List<string>();
//            string temporaryWord = "";

//            foreach (var word in rawWords)
//            {
//                string singleRawWords = RemoveRepeatedChars(word);

//                if (singleRawWords.Length <= 2)
//                {
//                    temporaryWord += singleRawWords;
//                }
//                else
//                {
//                    if (!string.IsNullOrEmpty(temporaryWord))
//                    {
//                        if (temporaryWord.Length > 3) reconstructedWords.Add(temporaryWord);
//                        else normalWords.Add(temporaryWord);
//                        temporaryWord = "";
//                    }
//                    normalWords.Add(word);
//                }
//            }

//            if (!string.IsNullOrEmpty(temporaryWord))
//            {
//                if (temporaryWord.Length > 3) reconstructedWords.Add(temporaryWord);
//                else normalWords.Add(temporaryWord);
//            }

//            foreach (var suspectedWord in reconstructedWords)
//            {
//                string singleCharSuspected = RemoveRepeatedChars(suspectedWord);
//                if (localExceptionList.Contains(suspectedWord)) continue;

//                if (localProfanityList.Any(p => suspectedWord.Contains(p)))
//                {
//                    stopwatch.Stop();
//                    return (Result<string>.Failure($"Profanity evasive pattern detected {suspectedWord}."), stopwatch.Elapsed.TotalMilliseconds);
//                }
//            }


//            foreach (var word in normalWords)
//            {
//                if (localExceptionList.Contains(word)) continue;

//                Match matchNormal = localRegex.Match(word);

//                if (!matchNormal.Success)
//                {
//                    string singleCharWord = RemoveRepeatedChars(word);

//                    if (singleCharWord != word)
//                    {
//                        matchNormal = localRegex.Match(singleCharWord);
//                    }
//                }

//                if (matchNormal.Success)
//                {
//                    if (localExceptionList.Contains(word) || localExceptionList.Contains(matchNormal.Value)) continue;

//                    stopwatch.Stop();
//                    return (Result<string>.Failure($"'{word}' is a profanity word."), stopwatch.Elapsed.TotalMilliseconds);
//                }

//                string cleanWordForLev = RemoveRepeatedChars(word);


//                foreach (var profanityWord in localProfanityList)
//                {
//                    if (Math.Abs(cleanWordForLev.Length - profanityWord.Length) > 1) continue;

//                    int distance = CalculateLevenshteinDistance(cleanWordForLev, profanityWord);

//                    if (distance <= 1)
//                    {
//                        stopwatch.Stop();
//                        return (Result<string>.Failure($"'{word}' is a profanity word."), stopwatch.Elapsed.TotalMilliseconds);
//                    }
//                }
//            }

//            string bestText = CleanTextForUser(userText);
//            stopwatch.Stop();

//            return (Result<string>.Success(bestText), stopwatch.Elapsed.TotalMilliseconds);
//        }



//        private static int CalculateLevenshteinDistance(string s, string t)
//        {
//            if (string.IsNullOrEmpty(s)) return t?.Length ?? 0;
//            if (string.IsNullOrEmpty(t)) return s.Length;

//            int n = s.Length;
//            int m = t.Length;
//            int[,] d = new int[n + 1, m + 1];

//            for (int i = 0; i <= n; d[i, 0] = i++) ;
//            for (int j = 0; j <= m; d[0, j] = j++) ;

//            for (int i = 1; i <= n; i++)
//            {
//                for (int j = 1; j <= m; j++)
//                {
//                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
//                    d[i, j] = Math.Min(
//                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
//                        d[i - 1, j - 1] + cost);
//                }
//            }
//            return d[n, m];
//        }


//        private static string RemoveRepeatedChars(string word)
//        {
//            if (string.IsNullOrEmpty(word) || word.Length <= 2) return word;

//            var result = new List<char>();
//            result.Add(word[0]);
//            result.Add(word[1]);

//            for (int i = 2; i < word.Length; i++)
//            {

//                if (word[i] != word[i - 1] || word[i] != word[i - 2])
//                {
//                    result.Add(word[i]);
//                }
//            }

//            return new string(result.ToArray());
//        }

//        public void Dispose()
//        {
//            Dispose(true);
//            GC.SuppressFinalize(this);
//        }

//        protected virtual void Dispose(bool disposing)
//        {
//            if (!_disposed)
//            {
//                if (disposing)
//                {
//                    _dbContext.Dispose();
//                }
//                _disposed = true;
//            }
//        }
//    }
//}

//// 1000 goodrequests completed in: 25507 ms
///* text: Un texto informativo brinda descripciones y hechos sobre la realidad con el fin de dar a conocer algo de la manera más objetiva posible, es decir, sin incluir emociones, opiniones, puntos de vista o deseos de su emisor. Por ejemplo: una noticia, un manual de instrucciones, una entrada enciclopédica.
// */
////  1000 badrequests completed in: 24630 ms
///* text: Un texto informativo brinda descripciones y hechos sobre la realidad con el fin de dar a conocer algo de la manera más objetiva posible, es decir ass, sin incluir emociones, opiniones, puntos de vista o deseos de su emisor. Por ejemplo: una noticia, un manual de instrucciones, una entrada enciclopédica
//*/