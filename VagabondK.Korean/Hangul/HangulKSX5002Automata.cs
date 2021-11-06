namespace VagabondK.Korean.Hangul
{
    /// <summary>
    /// 두 벌식 한글 키보드(KS X 5002) 입력 오토마타.
    /// </summary>
    public class HangulKSX5002Automata : HangulAutomata
    {
        /// <summary>
        /// 두 개의 한글 문자를 조합합니다.
        /// </summary>
        /// <param name="character1">한글 문자1</param>
        /// <param name="character2">한글 문자2</param>
        /// <param name="combined">조합 결과</param>
        /// <returns>조합 가능 여부</returns>
        public override bool TryCombine(char character1, char character2, out char combined)
        {
            if (character1 == HangulCharacter.Dot || character1 == HangulCharacter.DoubleDot
                || character2 == HangulCharacter.Dot || character2 == HangulCharacter.DoubleDot
                || character2 == 'ㅣ'
                && (character1 == 'ㅏ' 
                || character1 == 'ㅑ'
                || character1 == 'ㅓ'
                || character1 == 'ㅕ'
                || character1 == 'ㅘ'
                || character1 == 'ㅠ'
                || character1 == 'ㅝ'))
            {
                combined = '\0';
                return false;
            }

            return base.TryCombine(character1, character2, out combined);
        }
    }
}
