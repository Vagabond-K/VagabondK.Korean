using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VagabondK.Korean.ScreenInput
{
    /// <summary>
    /// 화면 기반 문자키 입력 모델
    /// </summary>
    public interface IScreenKeyInputModel
    {
        /// <summary>
        /// 문자 입력 커맨드
        /// </summary>
        ICommand InputCharacterCommand { get; }

        /// <summary>
        /// 백스페이스 입력 커맨드
        /// </summary>
        ICommand InputBackspaceCommand { get; }
    }
}
