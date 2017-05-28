using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using RandomizerSharp.PokemonModel;

namespace RandomizerSharp
{
    public class RomFunctions
    {
        public static ISet<Pokemon> GetBasicOrNoCopyPokemon(IList<Pokemon> validPokemons)
        {
            ISet<Pokemon> dontCopyPokes = new SortedSet<Pokemon>();

            foreach (var pkmn in validPokemons)
            {
                if (pkmn == null)
                    continue;

                if (pkmn.EvolutionsTo.Count != 1)
                {
                    dontCopyPokes.Add(pkmn);
                }
                else
                {
                    var onlyEvo = pkmn.EvolutionsTo[0];
                    if (!onlyEvo.CarryStats)
                        dontCopyPokes.Add(pkmn);
                }
            }
            return dontCopyPokes;
        }

        public static ISet<Pokemon> GetMiddleEvolutions(IList<Pokemon> validPokemons)
        {
            ISet<Pokemon> middleEvolutions = new SortedSet<Pokemon>();
            foreach (var pkmn in validPokemons)
            {
                if (pkmn.EvolutionsTo.Count != 1 || pkmn.EvolutionsFrom.Count <= 0)
                    continue;

                var onlyEvo = pkmn.EvolutionsTo[0];
                if (onlyEvo.CarryStats)
                    middleEvolutions.Add(pkmn);
            }
            return middleEvolutions;
        }

        public static ISet<Pokemon> GetFinalEvolutions(IList<Pokemon> validPokemons)
        {
            ISet<Pokemon> finalEvolutions = new SortedSet<Pokemon>();
            foreach (var pkmn in validPokemons)
            {
                if (pkmn.EvolutionsTo.Count != 1 || pkmn.EvolutionsFrom.Count != 0)
                    continue;

                var onlyEvo = pkmn.EvolutionsTo[0];
                if (onlyEvo.CarryStats)
                    finalEvolutions.Add(pkmn);
            }
            return finalEvolutions;
        }

        public static Move[] GetMovesAtLevel(Pokemon pkmn, int level, Move emptyValue)
        {
            var curMoves = new Move[4];
            curMoves.Populate(emptyValue);

            var moveCount = 0;
            var movepool = pkmn.MovesLearnt;
            foreach (var ml in movepool)
            {
                if (ml.Level > level)
                    break;
                var alreadyKnownMove = false;
                for (var i = 0; i < moveCount; i++)
                {
                    if (curMoves[i] != ml.Move)
                        continue;

                    alreadyKnownMove = true;
                    break;
                }

                if (alreadyKnownMove)
                    continue;
                
                if (moveCount == 4)
                {
                    for (var i = 0; i < 3; i++)
                        curMoves[i] = curMoves[i + 1];

                    curMoves[3] = ml.Move;
                }
                else
                {
                    curMoves[moveCount++] = ml.Move;
                }
            }
            return curMoves;
        }

        public static string CamelCase(string original)
        {
            var @string = original.ToLower().ToCharArray();
            var docap = true;
            for (var j = 0; j < @string.Length; j++)
            {
                var current = @string[j];
                if (docap && char.IsLetter(current))
                {
                    @string[j] = char.ToUpper(current);
                    docap = false;
                }
                else
                {
                    if (!docap && !char.IsLetter(current) && current != '\'')
                        docap = true;
                }
            }
            return new string(@string);
        }

        public static int FreeSpaceFinder(byte[] rom, byte freeSpace, int amount, int offset) => FreeSpaceFinder(
            rom,
            freeSpace,
            amount,
            offset,
            true);

        public static int FreeSpaceFinder(byte[] rom, byte freeSpace, int amount, int offset, bool longAligned)
        {
            if (!longAligned)
            {
                var searchNeedle = new byte[amount + 2];
                for (var i = 0; i < amount + 2; i++)
                    searchNeedle[i] = freeSpace;
                return SearchForFirst(rom, offset, searchNeedle) + 2;
            }
            else
            {
                var searchNeedle = new byte[amount + 5];
                for (var i = 0; i < amount + 5; i++)
                    searchNeedle[i] = freeSpace;
                return (SearchForFirst(rom, offset, searchNeedle) + 5) & ~3;
            }
        }

        public static List<int> Search(IList<byte> haystack, byte[] needle, int beginOffset = 0) => Search(
            haystack,
            beginOffset,
            haystack.Count,
            needle);

        public static List<int> Search(IList<byte> haystack, int beginOffset, int endOffset, byte[] needle)
        {
            var currentMatchStart = beginOffset;
            var currentCharacterPosition = 0;
            var toFillTable = BuildKmpSearchTable(needle);
            var results = new List<int>();
            int currentSum;

            while ((currentSum = currentMatchStart + currentCharacterPosition) < endOffset)
            {
                if (needle[currentCharacterPosition] == haystack[currentSum])
                {
                    currentCharacterPosition++;

                    if (currentCharacterPosition != needle.Length)
                        continue;

                    results.Add(currentMatchStart);
                    currentCharacterPosition = 0;
                    currentMatchStart = currentMatchStart + needle.Length;
                }
                else
                {
                    var toFillEntry = toFillTable[currentCharacterPosition];
                    currentMatchStart = currentMatchStart + currentCharacterPosition - toFillEntry;
                    currentCharacterPosition = Math.Max(toFillEntry, 0);
                }
            }

            return results;
        }

        public static int SearchForFirst(byte[] haystack, int beginOffset, byte[] needle)
        {
            var currentMatchStart = beginOffset;
            var currentCharacterPosition = 0;
            var docSize = haystack.Length;
            var needleSize = needle.Length;
            var toFillTable = BuildKmpSearchTable(needle);
            while (currentMatchStart + currentCharacterPosition < docSize)
            {
                if (needle[currentCharacterPosition] == haystack[currentCharacterPosition + currentMatchStart])
                {
                    currentCharacterPosition = currentCharacterPosition + 1;
                    if (currentCharacterPosition == needleSize)
                        return currentMatchStart;
                }
                else
                {
                    currentMatchStart = currentMatchStart +
                                        currentCharacterPosition -
                                        toFillTable[currentCharacterPosition];
                    currentCharacterPosition = toFillTable[currentCharacterPosition] > -1
                        ? toFillTable[currentCharacterPosition] : 0;
                }
            }
            return -1;
        }

        private static int[] BuildKmpSearchTable(byte[] needle)
        {
            var stable = new int[needle.Length];
            var pos = 2;
            var j = 0;
            stable[0] = -1;
            stable[1] = 0;
            while (pos < needle.Length)
            {
                if (needle[pos - 1] == needle[j])
                {
                    stable[pos] = j + 1;
                    pos++;
                    j++;
                }
                else if (j > 0)
                {
                    j = stable[j];
                }
                else
                {
                    stable[pos] = 0;
                    pos++;
                }
            }
            return stable;
        }

        public static string RewriteDescriptionForNewLineSize(
            string moveDesc,
            string newline,
            int lineSize,
            Func<string, int> ssd)
        {
            moveDesc = moveDesc.Replace("-" + newline, "").Replace(newline, " ");
            moveDesc = moveDesc.Replace("Sp. Atk", "Sp__Atk");
            moveDesc = moveDesc.Replace("Sp. Def", "Sp__Def");
            moveDesc = moveDesc.Replace("SP. ATK", "SP__ATK");
            moveDesc = moveDesc.Replace("SP. DEF", "SP__DEF");
            var words = Regex.Split(moveDesc, " ");
            var fullDesc = new StringBuilder();
            var thisLine = new StringBuilder();
            var currLineWc = 0;
            var currLineCc = 0;
            var linesWritten = 0;
            for (var i = 0; i < words.Length; i++)
            {
                words[i] = words[i].Replace("SP__", "SP. ");
                words[i] = words[i].Replace("Sp__", "Sp. ");
                var reqLength = ssd(words[i]);
                if (currLineWc > 0)
                    reqLength++;
                if (currLineCc + reqLength <= lineSize)
                {
                    if (currLineWc > 0)
                        thisLine.Append(' ');
                    thisLine.Append(words[i]);
                    currLineWc++;
                    currLineCc += reqLength;
                }
                else
                {
                    if (currLineWc > 0)
                    {
                        if (linesWritten > 0)
                            fullDesc.Append(newline);
                        fullDesc.Append(thisLine);
                        linesWritten++;
                        thisLine = new StringBuilder();
                    }
                    thisLine.Append(words[i]);
                    currLineWc = 1;
                    currLineCc = ssd(words[i]);
                }
            }

            if (currLineWc <= 0)
                return fullDesc.ToString();

            if (linesWritten > 0)
                fullDesc.Append(newline);

            fullDesc.Append(thisLine);

            return fullDesc.ToString();
        }

        public static string FormatTextWithReplacements(
            string text,
            IDictionary<string, string> replacements,
            string newline,
            string extraline,
            string newpara,
            int maxLineLength,
            Func<string, int> ssd)
        {
            var endsWithPara = false;
            if (text.EndsWith(newpara, StringComparison.Ordinal))
            {
                endsWithPara = true;
                text = text.Substring(0, text.Length - newpara.Length);
            }
            text = text.Replace(newline, " ").Replace(extraline, " ");
            if (replacements != null)
            {
                var index = 0;
                foreach (var toReplace in replacements)
                {
                    index++;
                    text = text.Replace(toReplace.Key, "<tmpreplace" + index + ">");
                }
                index = 0;
                foreach (var toReplace in replacements)
                {
                    index++;
                    text = text.Replace("<tmpreplace" + index + ">", toReplace.Value);
                }
            }
            var oldParagraphs = Regex.Split(text, newpara.Replace("\\", "\\\\"));
            var finalResult = new StringBuilder();
            var sentenceNewLineSize = Math.Max(10, maxLineLength / 2);
            for (var para = 0; para < oldParagraphs.Length; para++)
            {
                var words = Regex.Split(oldParagraphs[para], " ");
                var fullPara = new StringBuilder();
                var thisLine = new StringBuilder();
                var currLineWc = 0;
                var currLineCc = 0;
                var linesWritten = 0;
                var currLineLastChar = (char) 0;
                foreach (var t in words)
                {
                    var reqLength = ssd(t);
                    if (currLineWc > 0)
                        reqLength++;
                    if (currLineCc + reqLength > maxLineLength ||
                        currLineCc >= sentenceNewLineSize &&
                        (currLineLastChar == '.' ||
                         currLineLastChar == '?' ||
                         currLineLastChar == '!' ||
                         currLineLastChar == '…' ||
                         currLineLastChar == ','))
                    {
                        if (currLineWc > 0)
                        {
                            if (linesWritten > 1)
                                fullPara.Append(extraline);
                            else if (linesWritten == 1)
                                fullPara.Append(newline);
                            fullPara.Append(thisLine);
                            linesWritten++;
                            thisLine = new StringBuilder();
                        }
                        thisLine.Append(t);
                        currLineWc = 1;
                        currLineCc = ssd(t);
                        if (t.Length == 0)
                            currLineLastChar = (char) 0;
                        else
                            currLineLastChar = t[t.Length - 1];
                    }
                    else
                    {
                        if (currLineWc > 0)
                            thisLine.Append(' ');
                        thisLine.Append(t);
                        currLineWc++;
                        currLineCc += reqLength;
                        if (t.Length == 0)
                            currLineLastChar = (char) 0;
                        else
                            currLineLastChar = t[t.Length - 1];
                    }
                }
                if (currLineWc > 0)
                {
                    if (linesWritten > 1)
                        fullPara.Append(extraline);

                    else if (linesWritten == 1)
                        fullPara.Append(newline);

                    fullPara.Append(thisLine);
                }

                if (para > 0)
                    finalResult.Append(newpara);

                finalResult.Append(fullPara);
            }

            if (endsWithPara)
                finalResult.Append(newpara);

            return finalResult.ToString();
        }
    }
}