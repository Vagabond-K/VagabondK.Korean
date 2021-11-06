namespace VagabondK.Korean.Hangul
{
    /// <summary>
    /// 한글 오토마타 인터페이스
    /// </summary>
    public interface IHangulAutomata
    {
        /// <summary>
        /// 문자 입력
        /// </summary>
        /// <param name="editing">편집중인 문자열</param>
        /// <param name="input">입력할 문자의 집합. 문자가 여러 개일 경우 순서대로 교체하여 입력 가능.</param>
        /// <param name="output">문자 입력으로 인해 완성된 문자열</param>
        /// <returns>문자 입력 후의 편집중인 문자열</returns>
        string InputCharacter(string editing, string input, out string output);

        /// <summary>
        /// 백스페이스 입력
        /// </summary>
        /// <param name="editing">편집중인 문자열</param>
        /// <returns>백스페이스 입력 후의 편집중인 문자열</returns>
        string InputBackspace(string editing);
    }
}
