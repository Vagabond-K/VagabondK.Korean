using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VagabondK.Korean.Hangul
{
    /// <summary>
    /// 한글 오토마타
    /// </summary>
    public class HangulAutomata : IHangulAutomata
    {
        private readonly static Dictionary<char, HangulSequence> breakMap = new Dictionary<char, HangulSequence>
        {
            ['ㄳ'] = new HangulSequence('ㄱ', 'ㅅ'),
            ['ㄵ'] = new HangulSequence('ㄴ', 'ㅈ'),
            ['ㄶ'] = new HangulSequence('ㄴ', 'ㅎ'),
            ['ㄺ'] = new HangulSequence('ㄹ', 'ㄱ'),
            ['ㄻ'] = new HangulSequence('ㄹ', 'ㅁ'),
            ['ㄼ'] = new HangulSequence('ㄹ', 'ㅂ'),
            ['ㄽ'] = new HangulSequence('ㄹ', 'ㅅ'),
            ['ㄾ'] = new HangulSequence('ㄹ', 'ㅌ'),
            ['ㄿ'] = new HangulSequence('ㄹ', 'ㅍ'),
            ['ㅀ'] = new HangulSequence('ㄹ', 'ㅎ'),
            ['ㅄ'] = new HangulSequence('ㅂ', 'ㅅ'),
            ['ㅘ'] = new HangulSequence('ㅗ', 'ㅏ'),
            ['ㅙ'] = new HangulSequence('ㅗ', 'ㅐ'),
            ['ㅚ'] = new HangulSequence('ㅗ', 'ㅣ'),
            ['ㅝ'] = new HangulSequence('ㅜ', 'ㅓ'),
            ['ㅞ'] = new HangulSequence('ㅜ', 'ㅔ'),
            ['ㅟ'] = new HangulSequence('ㅜ', 'ㅣ'),
            ['ㅢ'] = new HangulSequence('ㅡ', 'ㅣ'),
        };

        private readonly static Dictionary<HangulSequence, char> combineMap = breakMap.ToDictionary(item => item.Value, item => item.Key).Concat(new Dictionary<HangulSequence, char>
        {
            [new HangulSequence(HangulCharacter.Dot, HangulCharacter.Dot)] = HangulCharacter.DoubleDot,
            [new HangulSequence('ㅣ', HangulCharacter.Dot)] = 'ㅏ',
            [new HangulSequence('ㅏ', HangulCharacter.Dot)] = 'ㅑ',
            [new HangulSequence(HangulCharacter.Dot, 'ㅣ')] = 'ㅓ',
            [new HangulSequence(HangulCharacter.DoubleDot, 'ㅣ')] = 'ㅕ',
            [new HangulSequence(HangulCharacter.Dot, 'ㅡ')] = 'ㅗ',
            [new HangulSequence('ㅚ', HangulCharacter.Dot)] = 'ㅘ',
            [new HangulSequence(HangulCharacter.DoubleDot, 'ㅡ')] = 'ㅛ',
            [new HangulSequence('ㅡ', HangulCharacter.Dot)] = 'ㅜ',
            [new HangulSequence('ㅜ', HangulCharacter.Dot)] = 'ㅠ',
            [new HangulSequence('ㅏ', 'ㅣ')] = 'ㅐ',
            [new HangulSequence('ㅑ', 'ㅣ')] = 'ㅒ',
            [new HangulSequence('ㅓ', 'ㅣ')] = 'ㅔ',
            [new HangulSequence('ㅕ', 'ㅣ')] = 'ㅖ',
            [new HangulSequence('ㅘ', 'ㅣ')] = 'ㅙ',
            [new HangulSequence('ㅠ', 'ㅣ')] = 'ㅝ',
            [new HangulSequence('ㅝ', 'ㅣ')] = 'ㅞ',
        }).ToDictionary(item => item.Key, item => item.Value);

        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// 교체 입력(예: ㄱㅋㄲ) 제한시간(밀리초)을 가져오거나 설정합니다. 기본 값은 600밀리초입니다.
        /// </summary>
        public int ReplaceInputTimeout { get; set; } = 600;

        /// <summary>
        /// 문자 입력
        /// </summary>
        /// <param name="editing">편집중인 문자열</param>
        /// <param name="input">입력할 문자의 집합. 문자가 여러 개일 경우 순서대로 교체하여 입력 가능.</param>
        /// <param name="output">문자 입력으로 인해 완성된 문자열</param>
        /// <returns>문자 입력 후의 편집중인 문자열</returns>
        public virtual string InputCharacter(string editing, string input, out string output)
        {
            lock (this)
            {
                if (string.IsNullOrEmpty(input))
                {
                    output = string.Empty;
                    return editing;
                }

                string result = null;
                var inputChar = input?.FirstOrDefault() ?? '\0';

                if (cancellationTokenSource != null && inputChar == ' ')
                {
                    cancellationTokenSource?.Cancel();
                    cancellationTokenSource = null;
                    output = string.Empty;
                    return editing;
                }

                if (inputChar == '\b')
                {
                    output = null;
                    return InputBackspaceCore(editing);
                }

                StringBuilder outputBuilder = new StringBuilder();

                if (inputChar.IsHangul())
                {
                    if (string.IsNullOrEmpty(editing))
                    {
                        result = inputChar.ToString();
                    }
                    else
                    {
                        char[] parts;
                        bool isReplaced = false;
                        if (editing.Length > 1)
                        {
                            if (editing.Length > 2)
                            {
                                outputBuilder.Append(editing.Substring(0, editing.Length - 2));
                                editing = editing.Substring(outputBuilder.Length);
                            }

                            char last = editing[editing.Length - 1];
                            if (last.IsDots()
                                && TryCombine(last, inputChar, out var newHangulVowel))
                            {
                                if (newHangulVowel.IsDots())
                                {
                                    char newHangulInitial = editing[editing.Length - 2];
                                    var newBuilding = HangulCharacter.FromHangulParts(newHangulInitial, newHangulVowel);
                                    if (newBuilding != null)
                                    {
                                        result = newBuilding.ToString();
                                    }
                                    else
                                    {
                                        result = $"{newHangulInitial}{newHangulVowel}";
                                    }
                                }
                                else
                                {
                                    editing = editing[0].ToString();
                                    inputChar = newHangulVowel;
                                }
                            }
                            else if (TryGetReplaceCharacter(last, input, out var replaced) || TryCombine(last, inputChar, out replaced))
                            {
                                isReplaced = true;
                                inputChar = replaced;
                                editing = editing.Remove(editing.Length - 1);
                                var currentChar = editing.LastOrDefault();
                                if (!currentChar.IsHangulSyllable() 
                                    && !(currentChar.CanBeHangulInitial() && inputChar.IsHangulVowel())
                                    && !TryCombine(currentChar, inputChar, out _))
                                {
                                    result = editing + replaced;
                                }
                            }
                            else
                            {
                                outputBuilder.Append(editing[0]);
                                editing = editing.Substring(1);
                            }
                        }

                        if (result == null)
                        {
                            var currentChar = editing.Last();

                            parts = currentChar.IsDots() ? new[] { currentChar } : currentChar.ToHangulParts();

                            if (parts != null)
                            {
                                var lastPart = parts.Last();
                                if (!isReplaced && TryGetReplaceCharacter(lastPart, input, out var replaced) || TryCombine(lastPart, inputChar, out replaced))
                                {
                                    parts[parts.Length - 1] = replaced;
                                    result = HangulCharacter.FromHangulParts(parts).ToString();
                                }
                                else if (lastPart.IsHangulConsonant() && TryBreak(lastPart, out char broken1, out char broken2))
                                {
                                    if (broken2.CanBeHangulInitial() && inputChar.IsHangulVowel())
                                    {
                                        result = HangulCharacter.FromHangulParts(broken2, inputChar).ToString();
                                        parts[parts.Length - 1] = broken1;
                                        outputBuilder.Append(HangulCharacter.FromHangulParts(parts).Value);
                                    }
                                    else if (!isReplaced && TryGetReplaceCharacter(broken2, input, out replaced) || TryCombine(broken2, inputChar, out replaced))
                                    {
                                        if (TryCombine(broken1, replaced, out var newFinal))
                                        {
                                            parts[parts.Length - 1] = newFinal;
                                            result = HangulCharacter.FromHangulParts(parts).ToString();
                                        }
                                        else if (parts.Length == 3)
                                        {
                                            result = replaced.ToString();
                                            parts[parts.Length - 1] = broken1;
                                            outputBuilder.Append(HangulCharacter.FromHangulParts(parts).Value);
                                        }
                                    }
                                }
                                else if (inputChar.IsDots())
                                {
                                    result = $"{currentChar}{inputChar}";
                                }
                                else if (parts.Length > 2 && lastPart.CanBeHangulInitial() && inputChar.IsHangulVowel())
                                {
                                    result = HangulCharacter.FromHangulParts(lastPart, inputChar).ToString();
                                    Array.Resize(ref parts, parts.Length - 1);

                                    outputBuilder.Append(HangulCharacter.FromHangulParts(parts).Value);
                                }

                                if (result == null)
                                {
                                    Array.Resize(ref parts, parts.Length + 1);
                                    parts[parts.Length - 1] = inputChar;
                                    result = HangulCharacter.FromHangulParts(parts).ToString();
                                    if (string.IsNullOrEmpty(result))
                                    {
                                        result = inputChar.ToString();
                                        outputBuilder.Append(currentChar);
                                    }
                                }
                            }
                            else
                            {
                                result = inputChar.ToString();
                                outputBuilder.Append(currentChar);
                            }
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(editing))
                    {
                        var building = editing;

                        result = string.Empty;
                        outputBuilder.Append(building);
                        outputBuilder.Append(inputChar);
                    }
                    else
                    {
                        outputBuilder.Append(inputChar);
                    }
                }

                if (input.Length > 1)
                {
                    cancellationTokenSource?.Cancel();
                    cancellationTokenSource = new CancellationTokenSource();
                    Task.Delay(ReplaceInputTimeout, cancellationTokenSource.Token).ContinueWith(task =>
                    {
                        cancellationTokenSource = null;
                    }, cancellationTokenSource.Token);
                }

                output = outputBuilder.ToString();
                return result;
            }
        }

        /// <summary>
        /// 백스페이스 입력
        /// </summary>
        /// <param name="editing">편집중인 문자열</param>
        /// <returns>백스페이스 입력 후의 편집중인 문자열</returns>
        public string InputBackspace(string editing) => InputCharacter(editing, "\b", out _);

        private string InputBackspaceCore(string editing)
        {
            if (!string.IsNullOrEmpty(editing))
            {
                var character = editing[editing.Length - 1];
                editing = editing.Remove(editing.Length - 1);

                if (character.IsHangulSyllable())
                {
                    var parts = character.ToHangulParts();
                    if (parts.Length == 2)
                    {
                        if (TryBreak(parts[1], out var character1, out _))
                        {
                            if (character1.IsDots())
                                editing += $"{parts[0]}{character1}";
                            else
                                editing += HangulCharacter.FromHangulParts(parts[0], character1);
                        }
                        else
                        {
                            var parts2 = editing.LastOrDefault().ToHangulParts();
                            if (parts2 != null && parts2.Length > 2 && TryCombine(parts2[2], parts[0], out var newFinal) && TryBreak(newFinal, out _, out _))
                            {
                                parts2[2] = newFinal;
                                editing = HangulCharacter.FromHangulParts(parts2).ToString();
                            }
                            else
                            {
                                editing += parts[0];
                            }
                        }
                    }
                    else
                    {
                        if (TryBreak(parts[2], out var character1, out _))
                        {
                            editing += HangulCharacter.FromHangulParts(parts[0], parts[1], character1);
                        }
                        else
                        {
                            editing += HangulCharacter.FromHangulParts(parts[0], parts[1]);
                        }
                    }
                }
                else if (TryBreak(character, out var character1, out _))
                {
                    var parts2 = editing.LastOrDefault().ToHangulParts();
                    if (parts2 != null && parts2.Length > 2)
                    {
                        if (TryCombine(parts2[2], character1, out var newFinal))
                        {
                            if (TryBreak(newFinal, out _, out _))
                            {
                                editing = editing.Remove(editing.Length - 1);
                                parts2[2] = newFinal;
                                editing += $"{HangulCharacter.FromHangulParts(parts2[0], parts2[1], newFinal)}";
                            }
                            else
                            {
                                editing += character1;
                            }
                        }
                        else if (character1.IsHangulVowel())
                        {
                            if (parts2[2].CanBeHangulInitial())
                            {
                                editing = editing.Remove(editing.Length - 1);
                                editing += $"{HangulCharacter.FromHangulParts(parts2[0], parts2[1])}{HangulCharacter.FromHangulParts(parts2[2], character1)}";
                            }
                            else if (TryBreak(parts2[2], out var broken1, out var broken2) && broken2.CanBeHangulInitial())
                            {
                                editing = editing.Remove(editing.Length - 1);
                                editing += $"{HangulCharacter.FromHangulParts(parts2[0], parts2[1], broken1)}{HangulCharacter.FromHangulParts(broken2, character1)}";
                            }
                        }
                    }
                    else
                    {
                        editing += character1;
                    }
                }
            }

            return editing;
        }

        private bool TryGetReplaceCharacter(char editing, string replaceSequence, out char next)
        {
            int index;
            if (cancellationTokenSource != null && replaceSequence != null && replaceSequence.Length > 1 && (index = replaceSequence.IndexOf(editing)) >= 0)
            {
                next = replaceSequence[++index % replaceSequence.Length];
                return true;
            }
            else
            {
                next = '\0';
                return false;
            }
        }

        /// <summary>
        /// 두 개의 한글 문자를 조합합니다.
        /// </summary>
        /// <param name="character1">한글 문자1</param>
        /// <param name="character2">한글 문자2</param>
        /// <param name="combined">조합 결과</param>
        /// <returns>조합 가능 여부</returns>
        public virtual bool TryCombine(char character1, char character2, out char combined)
            => combineMap.TryGetValue(new HangulSequence(character1, character2), out combined);

        /// <summary>
        /// 한글 문자를 분해합니다.
        /// </summary>
        /// <param name="combined">한글 문자</param>
        /// <param name="character1">분해된 한글 문자1</param>
        /// <param name="character2">분해된 한글 문자2</param>
        /// <returns>분해 가능 여부</returns>
        public virtual bool TryBreak(char combined, out char character1, out char character2)
        {
            if (breakMap.TryGetValue(combined, out var hangulTuple))
            {
                character1 = hangulTuple.Character1;
                character2 = hangulTuple.Character2;
                return true;
            }
            else
            {
                character1 = '\0';
                character2 = '\0';
                return false;
            }
        }

        struct HangulSequence
        {
            public HangulSequence(char character1, char character2)
            {
                Character1 = character1;
                Character2 = character2;
            }

            public char Character1 { get; }
            public char Character2 { get; }
        }
    }
}
