using Azure.Core;
using Microsoft.EntityFrameworkCore;
using ProfanityFilter.Common;
using ProfanityFilter.DTOs.Response;
using ProfanityFilter.Entities;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ProfanityFilter.Services
{
    public class ProfanityFilterServiceWithScopeFactory : IProfanityFilterService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly object _lockObject = new();
        private HashSet<string> _profanityList = new(StringComparer.OrdinalIgnoreCase);
        private HashSet<string> _exceptionList = new(StringComparer.OrdinalIgnoreCase);
        private Regex _compiledProfanityRegex;

        //private static readonly HashSet<string> exceptionWordList = new(StringComparer.OrdinalIgnoreCase)
        //{

        //   "actitud", "acumulación", "acumulador", "acumular", "altitud", "amputacion", "amputación", "amputado", "amputar",
        //    "amputas", "apetitibilidad", "apetito", "apetitosa", "apetitoso", "aptitud", "artanal", "articulacion", "articulación",
        //    "articulado", "articular", "articulo", "aseo", "asi", "asset", "assimilate", "associate", "assure", "binóculo",
        //    "bipedestación", "bulo", "bípedo", "cacaguate", "cacahuatal", "cacaos", "cacarear", "cacareo", "cada", "calculadora",
        //    "calculando", "calcular", "calculo", "cama", "canal", "capa", "cara", "casa", "caza", "celo", "cenáculo", "chulo",
        //    "cicatriz", "cicatrización", "cicatrizar", "ciempiés", "circulacion", "circular", "circulo", "circuncisión",
        //    "circunferencia", "circunloquio", "circunnavegación", "circunscripción", "circunspecto", "como", "competitivamente",
        //    "competitividad", "competitivo", "computa", "computable", "computacion", "computacional", "computadamente",
        //    "computador", "computadora", "computar", "computarizado", "computismo", "computista", "con", "constitucional",
        //    "constitución", "constituyente", "cuadrúpedo", "cuerda", "culto", "cumbre", "cumpleaños", "cumplidor",
        //    "cumplimentar", "cumplir", "curo", "cuyo", "cúmulo", "deputacion", "deputar", "desarticular", "despedida",
        //    "despedir", "destituir", "desvincular", "diputado", "disputa", "disputable", "disputación", "disputado", "disputador",
        //    "disputar", "disputas", "disreputacion", "documentación", "documental", "documento", "duda", "duro", "ecuménico",
        //    "espectacular", "espectacularidad", "espectaculo", "espera", "exactitud", "expedicionario", "expedición", "expedientar",
        //    "expediente", "expedir", "expeditivo", "expedito", "eyaculación", "eyacular", "fallar", "fisioterapeuta", "frio",
        //    "fruta", "gesticulo", "gratitud", "guta", "hospedador", "hospedaje", "hospedamiento", "hospedar", "idioma",
        //    "impedimento", "impedir", "impeditivo", "imputabilidad", "imputable", "imputacion", "imputación", "imputado",
        //    "imputar", "imputas", "incompetitivo", "incumbir", "indisputable", "indisputablemente", "inexactitud",
        //    "inimputable", "inmaculado", "inocular", "inoculo", "institucional", "institución", "instituible", "instituto",
        //    "izquierda", "jerga", "juventud", "latitud", "locutorio", "longitud", "luta", "maculación", "macular", "marca",
        //    "maria", "mil", "milpiés", "minusculos", "mis", "monóculo", "mulo", "musculatura", "musculo", "musculoso", "naci",
        //    "nube", "nubes", "obstaculo", "oráculo", "pagina", "pais", "pala", "particular", "particularidad", "particularmente",
        //    "passion", "passive", "pata", "pedacito", "pedagogismo", "pedagogía", "pedagógico", "pedalada", "pedalear", "pedaleo",
        //    "pedalera", "pedalero", "pedante", "pedantería", "pedantismo", "pedantón", "pedazo", "pedernal", "pedestre",
        //    "pedestrismo", "pediatría", "pedicelo", "pediculosis", "pedicura", "pedicuro", "pedid", "pedido", "pedidor", "pedidura",
        //    "pedigrí", "pedigüeño", "pedimento", "pedimos", "pedir", "pediátrico", "pedrada", "pedregal", "pedregoso", "pedrera",
        //    "pedrería", "pedreñal", "pedro", "pedrusco", "pedunculado", "pedáculo", "pedúnculo", "pelo", "pena", "pense", "pero",
        //    "perraje", "perrera", "perro", "peta", "pidiendo", "pie", "pierda", "pináculo", "piso", "pita", "plenitud", "por",
        //    "porra", "psicoterapeuta", "pua", "pudo", "puedo", "pula", "pulo", "puma", "punta", "pupa", "pura", "putativo", "puya",
        //    "radioterapeuta", "rectitud", "reculada", "recular", "reiterativamente", "reiterativo", "repedir", "repetitivamente",
        //    "repetitividad", "repetitivo", "reputacion", "reputado", "reputar", "restituir", "rio", "ruta", "se", "sea", "seco",
        //    "sed", "ser", "solicitud", "substituibilidad", "substitutivo", "sucumbir", "suputacion", "suputación", "suputar",
        //    "sustituir", "tentacular", "tentaculo", "terapeuta", "terapéutico", "ti", "tináculo", "tio", "tirita", "tiritar",
        //    "tiriton", "titar", "titerero", "titeresca", "titeretada", "titerismo", "titilación", "titilante", "titilar",
        //    "titiritaina", "titiritar", "titiriteo", "titiritera", "titiritero", "titubea", "titubear", "titubeo", "titulación",
        //    "titular", "titularidad", "titulativo", "titulero", "titulillo", "titulización", "titulizado", "torpedear", "torpedeo",
        //    "torpedero", "torpedo", "títere", "título", "vehicular", "vehiculo", "velocípedo", "venga", "vicisitud", "vinculación",
        //    "vincular", "vinculo", "virtud","reputa", "amputamiento", "crepúsculo", "pedorrear", "pedorreo", "pedorrera", "cacatúa",
        //    "cacao","partícula", "testículo", "folículo", "vesícula", "clavícula", "matrícula", "cutícula", "cuadrícula", "película",
        //    "molécula", "gesticulación", "restitución", "destitución", "sustitución", "circunvalación", "juzgamiento", "entendimiento",
        //    "apetitosamente", "fecal", "funicular", "incumbencia","dice","impenetrables","disputaban","analogo","titula","seguro","analisis","analizando",
        //    "analiza","analizo","veintitantos","perspicuo","tripartita","analanlogia","analogo","analitico","crepusculo","minusculo","ridiculo","dusty","apene",
        //    "clocks","invierno","caia","asu","peso","dame","book","books","ticketpassengers",


        //    "addict", "addiction", "addictive", "amputee", "analects", "analeptic", "analgesia", "analgesic", "anality", "analogous",
        //    "analogue", "analogy", "analysand", "analyser", "analysis", "analyst", "analytically", "appetite", "as", "ask", "asp",
        //    "assassin", "assassinate", "assemble", "assert", "assertion", "assertive", "assess", "assessment", "assessor", "assign",
        //    "assignable", "assignment", "assist", "assistance", "assistant", "assume", "assumption", "attitude", "banal", "banality",
        //    "batch", "beautiful", "benedick", "bit", "born", "botch", "buck", "but", "bypass", "canalization", "canalize", "circumstance",
        //    "circumvent", "class", "classic", "classics", "classify", "classroom", "cockaded", "cockalorum", "cockbilled", "cockering",
        //    "cockfight", "cockle", "cockloft", "cocksure", "cocktail", "cocky", "compass", "constitution", "contradict", "contradiction",
        //    "contradictory", "cook", "cork", "cucumber", "cumbrous", "cummerbund", "cummingtonite", "cumulus", "cup", "deck", "dickens",
        //    "dickey", "dickie", "dickybird", "dictate", "dictation", "dictator", "dictatorial", "dictionary", "dictum", "ditch", "dock",
        //    "duck", "edict", "embarrass", "embarrassment", "entitled", "entity", "fiberglass", "fit", "gamecock", "glass", "grass",
        //    "grasshopper", "gratitude", "haycock", "his", "hit", "hitch", "identity", "indict", "institution", "interdict", "is", "it",
        //    "its", "jurisdiction", "kick", "kiss", "latitude", "like", "lit", "lock", "luck", "malediction", "mass", "massacre", "massive",
        //    "missing", "mit", "mock", "nick", "onto", "pass", "passage", "passenger", "passport", "password", "peacock", "peacocking", "pick",
        //    "pinnacle", "pit", "pitch", "port", "predict", "predictable", "prediction", "predictor", "proof", "puck", "put", "quantity", "rate",
        //    "recall", "rock", "scunthorpe", "seacock", "see", "seem", "seemed", "seen", "seven", "sheet", "shin", "ship", "shirt", "shirting",
        //    "shitake", "shot", "shut", "shuttlecock", "shuttlecocked", "sick", "sit", "slot", "sock", "spatchcock", "spit", "sputtering",
        //    "sputum", "stopcock", "substitute", "subtitles", "such", "suit", "sunglasses", "that", "therapist", "think", "tick", "tithe",
        //    "titillate", "title", "titmouse", "titration", "titter", "titular", "tuck", "turncock", "valediction", "verdict", "walk", "want",
        //    "witch","woodcock", "weathercock", "spotted dick", "assault", "shiitake", "sourpuss", "pussycat","it","dusty","clocks","clock",
        //    "brass","trapping","opened","happened","spin","thomass","itit","passinstead","cliff","scraping","docks","rare","wrapped","held",
        //    "rope","basement","shells","seashells","racing","sat","coin","asa","meticulous","erratic","thumping","penetrated","entities","bone",
        //    "interstitial","vacuum","fortitude","succumbed","trapped","hourglasses","seek","reconstitute","seat","computational","classical",
        //    "dampeners","accumulated","damp","spun","repetitive","hum","dawn","mapping","analytical","assumed","glass","glassy","cybergothicmassive",
        //    "buttresses","egoidentity"
        //};

        public ProfanityFilterServiceWithScopeFactory(IServiceScopeFactory? scope)
        {
            _scopeFactory = scope;
        }


        private static string CleanTextForUser(string userText)
        {
            if (string.IsNullOrEmpty(userText)) return string.Empty;

            string regexPattern = @"[^a-zA-Z0-9áéíóúÁÉÍÓÚñÑ\s.,;:¡!¿?()\-–—\r\n]";



            return Regex.Replace(userText, regexPattern, "");
        }




        private static string CleanText(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            var leetDict = new Dictionary<char, char> {
                { '4', 'a' }, { '@', 'a' }, { '3', 'e' }, { '1', 'i' },
                { '!', 'i' }, { '0', 'o' }, { '5', 's' }, { '$', 's' },
                { '7', 't' }, { '8', 'b' }
            };

            string normalizedText = input.Normalize(System.Text.NormalizationForm.FormD);
            var cleanChars = new List<char>();

            for (int i = 0; i < normalizedText.Length; i++)
            {
                char c = normalizedText[i];


                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                {

                    if (cleanChars.Count > 0 && cleanChars[cleanChars.Count - 1] == 'n')
                    {
                        cleanChars[cleanChars.Count - 1] = 'ñ';
                        continue;
                    }

                    continue;
                }

                char lowerC = char.ToLowerInvariant(c);

                if (leetDict.TryGetValue(lowerC, out char normalText))
                {
                    cleanChars.Add(normalText);
                }

                else if (char.IsAsciiLetterOrDigit(lowerC) || lowerC == 'ñ' || lowerC == ' ')
                {
                    cleanChars.Add(lowerC);
                }
            }

            return new string(cleanChars.ToArray());
        }

        public async Task LoadWordsFromDBAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var scopeService = scope.ServiceProvider.GetRequiredService<InternsDbContext>();

            var dbWords = await scopeService.BannedWordsJdrs
                .Select(w => w.Word)
                .ToListAsync();

            var dbWordsExceptions = await scopeService.ExceptionWordsJdrs
                .Select(f => f.Word)
                .ToListAsync();

            var cleanWords = dbWords.Select(CleanText).Where(w => !string.IsNullOrEmpty(w)).ToList();

            lock (_lockObject)
            {
                _profanityList = new HashSet<string>(cleanWords, StringComparer.OrdinalIgnoreCase);

                _exceptionList = new HashSet<string>(dbWordsExceptions, StringComparer.OrdinalIgnoreCase);

                string combinedPattern = $@"\b[a-zñ]*({string.Join("|", _profanityList.Select(Regex.Escape))})[a-zñ]*\b";

                _compiledProfanityRegex = new Regex(combinedPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }
        }

        public async Task<(Result<CreateWordResponse> Result, double TimeMs)> CreateProfaniyWord(string? word, int languageId)
        {
            var stopwatch = Stopwatch.StartNew();

            if (string.IsNullOrWhiteSpace(word))
            {
                stopwatch.Stop();
                return (Result<CreateWordResponse>.Failure(ProfanityErrors.TheDataIsEmptyOrNull), stopwatch.Elapsed.TotalMilliseconds);
            }
            if (languageId <= 0)
            {
                stopwatch.Stop();
                return (Result<CreateWordResponse>.Failure($"the data {languageId} cannot be less than or equal to 0"), stopwatch.Elapsed.TotalMilliseconds);
            }

            using var scope = _scopeFactory.CreateScope();
            var scopeService = scope.ServiceProvider.GetRequiredService<InternsDbContext>();



            LanguagesJdr? language = await scopeService.LanguagesJdrs
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == languageId);

            if (language == null)
            {
                stopwatch.Stop();
                return (Result<CreateWordResponse>.Failure(ProfanityErrors.LanguageNotFound), stopwatch.Elapsed.TotalMilliseconds);
            }

            bool wordExist = await scopeService.BannedWordsJdrs
                .AsNoTracking()
                .AnyAsync(w => w.Word == word);


            if (wordExist)
            {
                stopwatch.Stop();
                return (Result<CreateWordResponse>.Failure(ProfanityErrors.WordAlreadyExists), stopwatch.Elapsed.TotalMilliseconds);
            }

            BannedWordsJdr newWord = new BannedWordsJdr
            {
                Word = word,
                LanguageId = languageId,
            };

            await scopeService.BannedWordsJdrs.AddAsync(newWord);
            await scopeService.SaveChangesAsync();

            string cleanWord = CleanText(word);

            lock (_lockObject)
            {
                _profanityList.Add(cleanWord);
            }

            CreateWordResponse response = new CreateWordResponse
            {
                Word = word,
                LanguageId = languageId,
                Language = new CreateWordLanguagesResponse
                {
                    Name = language.Name,
                    Code = language.Code,
                }
            };

            stopwatch.Stop();
            return (Result<CreateWordResponse>.Success(response), stopwatch.Elapsed.TotalMilliseconds);
        }

        public async Task<(Result<CreateWordResponse> Result, double TimeMs)> CreateExceptionWord(string? word, int languageId)
        {
            var stopwatch = Stopwatch.StartNew();

            if (string.IsNullOrWhiteSpace(word))
            {
                stopwatch.Stop();
                return (Result<CreateWordResponse>.Failure(ProfanityErrors.TheDataIsEmptyOrNull), stopwatch.Elapsed.TotalMilliseconds);
            }
            if (languageId <= 0)
            {
                stopwatch.Stop();
                return (Result<CreateWordResponse>.Failure($"the data {languageId} cannot be less than or equal to 0"), stopwatch.Elapsed.TotalMilliseconds);
            }

            using var scope = _scopeFactory.CreateScope();
            var scopeService = scope.ServiceProvider.GetRequiredService<InternsDbContext>();



            LanguagesJdr? language = await scopeService.LanguagesJdrs
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == languageId);

            if (language == null)
            {
                stopwatch.Stop();
                return (Result<CreateWordResponse>.Failure(ProfanityErrors.LanguageNotFound), stopwatch.Elapsed.TotalMilliseconds);
            }

            bool wordExist = await scopeService.ExceptionWordsJdrs
                .AsNoTracking()
                .AnyAsync(w => w.Word == word);

            if (wordExist)
            {
                stopwatch.Stop();
                return (Result<CreateWordResponse>.Failure(ProfanityErrors.WordAlreadyExists), stopwatch.Elapsed.TotalMilliseconds);
            }

            ExceptionWordsJdr newWord = new ExceptionWordsJdr
            {
                Word = word,
                LanguageId = languageId,
            };

            scopeService.ExceptionWordsJdrs.Add(newWord);
            await scopeService.SaveChangesAsync();

            string cleanWord = CleanText(word);

            lock (_lockObject)
            {
                _exceptionList.Add(cleanWord);
            }

            CreateWordResponse response = new CreateWordResponse
            {
                Word = word,
                LanguageId = languageId,
                Language = new CreateWordLanguagesResponse
                {
                    Name = language.Name,
                    Code = language.Code,
                }
            };

            stopwatch.Stop();
            return (Result<CreateWordResponse>.Success(response), stopwatch.Elapsed.TotalMilliseconds);
        }





        public async Task<(Result<string> Result, double TimeMs)> HasProfanity(string? userText)
        {
            Regex localRegex;
            HashSet<string> localProfanityList;

            lock (_lockObject)
            {
                localRegex = _compiledProfanityRegex;
                localProfanityList = _profanityList;
            }


            var stopwatch = Stopwatch.StartNew();

            if (string.IsNullOrWhiteSpace(userText))
            {
                stopwatch.Stop();
                return (Result<string>.Failure(ProfanityErrors.TheDataIsEmptyOrNull), stopwatch.Elapsed.TotalMilliseconds);
            }

            string cleanText = CleanText(userText);

            string[] rawWords = cleanText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var normalWords = new List<string>();
            var reconstructedWords = new List<string>();

            string temporaryWord = "";

            foreach (var word in rawWords)
            {

                string singleRawWords = RemoveRepeatedChars(word);


                if (singleRawWords.Length <= 2)
                {
                    temporaryWord += singleRawWords;
                }
                else
                {


                    if (!string.IsNullOrEmpty(temporaryWord))
                    {
                        if (temporaryWord.Length > 3)
                        {
                            reconstructedWords.Add(temporaryWord);

                        }
                        else
                        {
                            normalWords.Add(temporaryWord);
                        }
                        temporaryWord = "";
                    }

                    normalWords.Add(word);
                }
            }

            if (!string.IsNullOrEmpty(temporaryWord))
            {
                if (temporaryWord.Length > 3) reconstructedWords.Add(temporaryWord);
                else normalWords.Add(temporaryWord);
            }


            foreach (var suspectedWord in reconstructedWords)
            {
                string singleCharSuspected = RemoveRepeatedChars(suspectedWord);
                if (_exceptionList.Contains(suspectedWord)) continue;

                if (_profanityList.Any(p => suspectedWord.Contains(p)))
                {
                    stopwatch.Stop();
                    return (Result<string>.Failure($"Profanity evasive pattern detected {suspectedWord}."), stopwatch.Elapsed.TotalMilliseconds);
                }
            }


            foreach (var word in normalWords)
            {

                if (_exceptionList.Contains(word)) continue;


                Match matchNormal = localRegex.Match(word);

                if (!matchNormal.Success)
                {
                    string singleCharWord = RemoveRepeatedChars(word);

                    if (singleCharWord != word)
                    {
                        matchNormal = localRegex.Match(singleCharWord);
                    }
                }

                if (matchNormal.Success)
                {
                    if (_exceptionList.Contains(word) || _exceptionList.Contains(matchNormal.Value)) continue;

                    stopwatch.Stop();
                    return (Result<string>.Failure($"'{word}' is a profanity word."), stopwatch.Elapsed.TotalMilliseconds);
                }
                string cleanWordForLev = RemoveRepeatedChars(word);


                foreach (var profanityWord in localProfanityList)
                {
                    if (Math.Abs(cleanWordForLev.Length - profanityWord.Length) > 1) continue;

                    int distance = CalculateLevenshteinDistance(cleanWordForLev, profanityWord);

                    if (distance <= 1)
                    {
                        stopwatch.Stop();
                        return (Result<string>.Failure($"'{word}' is a profanity word."), stopwatch.Elapsed.TotalMilliseconds);
                    }
                }
            }


            string bestText = CleanTextForUser(userText);
            stopwatch.Stop();

            return (Result<string>.Success(bestText), stopwatch.Elapsed.TotalMilliseconds);
        }


        private static int CalculateLevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s)) return t?.Length ?? 0;
            if (string.IsNullOrEmpty(t)) return s.Length;

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 0; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }

        private static string RemoveRepeatedChars(string word)
        {
            if (string.IsNullOrEmpty(word) || word.Length <= 2) return word;

            var result = new List<char>();
            result.Add(word[0]);
            result.Add(word[1]);

            for (int i = 2; i < word.Length; i++)
            {

                if (word[i] != word[i - 1] || word[i] != word[i - 2])
                {
                    result.Add(word[i]);
                }
            }

            return new string(result.ToArray());
        }


    }
}





// 1000 goodrequests completed in: 2610 ms
/* text: Un texto informativo brinda descripciones y hechos sobre la realidad con el fin de dar a conocer algo de la manera más objetiva posible, es decir, sin incluir emociones, opiniones, puntos de vista o deseos de su emisor. Por ejemplo: una noticia, un manual de instrucciones, una entrada enciclopédica.
 */
//  1000 bad requests completed in: 4329 ms
/* text: Un texto informativo brinda descripciones y hechos sobre la realidad con el fin de dar a conocer algo de la manera más objetiva posible, es decir ass, sin incluir emociones, opiniones, puntos de vista o deseos de su emisor. Por ejemplo: una noticia, un manual de instrucciones, una entrada enciclopédica
*/


