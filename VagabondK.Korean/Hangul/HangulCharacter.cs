using System.Collections.Generic;

namespace VagabondK.Korean.Hangul
{
    /// <summary>
    /// 한글 문자 관련 기능을 제공합니다.
    /// </summary>
    public static class HangulCharacter
    {
        /// <summary>
        /// 천지인 중의 천을 나타냅니다. 즉 아래아(ᆞ) 문자를 나타냅니다.
        /// </summary>
        public const char Dot = 'ᆞ';
        /// <summary>
        /// 천지인 중의 천이 두 개 나열된 문자를 나타냅니다.
        /// </summary>
        public const char DoubleDot = 'ᆢ';

        private const char characterStart = '가';
        private const char characterEnd = '힣';
        private const char consonantStart = 'ㄱ';
        private const char consonantEnd = 'ㅎ';
        private const char middleStart = 'ㅏ';
        private const char middleEnd = 'ㅣ';
        private const string initials = "ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎ";
        private const string finals = "\0ㄱㄲㄳㄴㄵㄶㄷㄹㄺㄻㄼㄽㄾㄿㅀㅁㅂㅄㅅㅆㅇㅈㅊㅋㅌㅍㅎ";

        private static readonly int middlesCount = middleEnd - middleStart + 1;
        private static readonly int initialUnit = middlesCount * finals.Length;
        private static readonly Dictionary<char, byte> initialIndeces = CharsToIndexDictionary(initials);
        private static readonly Dictionary<char, byte> finalIndeces = CharsToIndexDictionary(finals);

        private static Dictionary<char, byte> CharsToIndexDictionary(string chars)
        {
            var result = new Dictionary<char, byte>();

            for (int i = 0; i < chars.Length; i++)
                result[chars[i]] = (byte)i;

            return result;
        }

        /// <summary>
        /// 한글 음절 문자를 초성, 중성, 종성 등의 파트로 분해합니다.
        /// </summary>
        /// <param name="character">한글 음절 문자</param>
        /// <returns>초성, 중성, 종성 등의 파트로 구성된 배열. 
        /// 입력된 문자가 한글 문자이면서 음절 문자가 아니라면 해당 한글 문자만 포함된 배열을 반환합니다.</returns>
        public static char[] ToHangulParts(this char character)
        {
            if (character.IsHangulSyllable())
            {
                int code = character - characterStart;
                char initial = initials[code / initialUnit];

                code %= initialUnit;
                char middle = (char)(middleStart + code / finals.Length);

                code %= finals.Length;

                return code != 0 ? new[] { initial, middle, finals[code] } : new[] { initial, middle };
            }
            else if (character.IsHangulConsonant() || character.IsHangulVowel() || character.IsDots())
                return new[] { character };

            return null;
        }

        /// <summary>
        /// 문자가 한글 문자인지 여부를 반환합니다.
        /// </summary>
        /// <param name="character">문자</param>
        /// <returns>한글 문자인지 여부</returns>
        public static bool IsHangul(this char character) => character.IsHangulConsonant() || character.IsHangulVowel() || character.IsHangulSyllable() || character.IsDots();
        
        /// <summary>
        /// 문자가 한글 자음(ㄱㄴㄷㄹ...)을 나타내는지 여부를 반환합니다.
        /// </summary>
        /// <param name="character">문자</param>
        /// <returns>한글 자음인지 여부</returns>
        public static bool IsHangulConsonant(this char character) => character >= consonantStart && character <= consonantEnd;

        /// <summary>
        /// 문자가 한글 모음(ㅏㅑㅓㅕ...)을 나타내는지 여부를 반환합니다.
        /// </summary>
        /// <param name="character">문자</param>
        /// <returns>한글 모음인지 여부</returns>
        public static bool IsHangulVowel(this char character) => character >= middleStart && character <= middleEnd;

        /// <summary>
        /// 문자가 아래아(ᆞ)나 두 개의 아래아(ᆢ)를 나타내는지 여부를 반환합니다.
        /// </summary>
        /// <param name="character">문자</param>
        /// <returns>아래아(ᆞ)나 두 개의 아래아(ᆢ)를 나타내는지 여부</returns>
        public static bool IsDots(this char character) => character == Dot || character == DoubleDot;

        /// <summary>
        /// 문자가 한글 초성이 될 수 있는지 여부를 반환합니다.
        /// </summary>
        /// <param name="character">문자</param>
        /// <returns>한글 초성이 될 수 있는지 여부</returns>
        public static bool CanBeHangulInitial(this char character) => initialIndeces.ContainsKey(character);

        /// <summary>
        /// 문자가 한글 종성(받침)이 될 수 있는지 여부를 반환합니다.
        /// </summary>
        /// <param name="character">문자</param>
        /// <returns>한글 종성(받침)이 될 수 있는지 여부</returns>
        public static bool CanBeHangulFinal(this char character) => finalIndeces.ContainsKey(character);

        /// <summary>
        /// 문자가 한글 음절 문자인지 여부를 반환합니다.
        /// </summary>
        /// <param name="character">문자</param>
        /// <returns>한글 음절 문자인지 여부</returns>
        public static bool IsHangulSyllable(this char character) => character >= characterStart && character <= characterEnd;

        /// <summary>
        /// 일반적인 초성, 중성, 종성의 순으로 입력된 문자들을 결합합니다. 단, 중성 조합은 동작하지 않습니다.
        /// </summary>
        /// <param name="chars">초성, 중성, 종성 입력</param>
        /// <returns>결합된 한글 문자</returns>
        public static char? FromHangulParts(params char[] chars)
            => chars.Length > 1 && chars.Length <= 3 && chars[0].CanBeHangulInitial() && chars[1].IsHangulVowel() && (chars.Length < 3 || chars[2].CanBeHangulFinal())
            ? (char?)(characterStart + finals.Length * (middlesCount * initialIndeces[chars[0]] + (chars[1] - middleStart)) + (chars.Length > 2 ? finalIndeces[chars[2]] : 0))
            : (chars.Length == 1 ? (char?)chars[0] : null);

    }

}
