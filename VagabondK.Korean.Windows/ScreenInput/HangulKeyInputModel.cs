using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VagabondK.Korean.Hangul;

namespace VagabondK.Korean.ScreenInput
{
    /// <summary>
    /// 한글 키 입력 모델
    /// </summary>
    public class HangulKeyInputModel : IScreenKeyInputModel, INotifyPropertyChanged
    {
        /// <summary>
        /// 생성자
        /// </summary>
        public HangulKeyInputModel()
        {
            InputCharacterCommand = new InstantCommand<object>(InputKey);
            InputBackspaceCommand = new InstantCommand(InputBackspace);
        }

        private IHangulAutomata automata = new HangulAutomata();

        /// <summary>
        /// 속성 값이 변경될 때 발생합니다.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 문자 입력 커맨드를 가져옵니다.
        /// </summary>
        public ICommand InputCharacterCommand { get; }

        /// <summary>
        /// 백스페이스 입력 커맨드를 가져옵니다.
        /// </summary>
        public ICommand InputBackspaceCommand { get; }

        /// <summary>
        /// 한글 오토마타를 가저오거나 설정합니다.
        /// </summary>
        public IHangulAutomata Automata
        {
            get => automata;
            set
            {
                if (automata != value)
                {
                    automata = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Automata)));
                }
            }
        }

        private void InputKey(object parameter)
        {
            if (Keyboard.FocusedElement is TextBox textBox && parameter is string characters && characters.Length > 0)
            {
                string result = string.Empty;
                if (characters[0] != '\b')
                {
                    var input = characters;

                    if (textBox.SelectionLength > 0)
                        InputBackspace(textBox);

                    var text = GetEditingText(textBox);
                    result = automata.InputCharacter(text, input, out string output);
                    result = output + result;

                    for (int i = 0; i < text.Length; i++)
                        InputBackspace(textBox);
                }
                else if (textBox.SelectionLength == 0)
                {
                    var text = GetEditingText(textBox);
                    result = automata.InputBackspace(text);
                    for (int i = 0; i < text.Length; i++)
                        InputBackspace(textBox);
                }
                InputText(textBox, result);
            }
        }

        private void InputBackspace()
        {
            if (Keyboard.FocusedElement is TextBox textBox)
            {
                string result = string.Empty;
                if (textBox.SelectionLength == 0)
                {
                    var text = GetEditingText(textBox);
                    result = automata.InputBackspace(text);
                    for (int i = 0; i < text.Length; i++)
                        InputBackspace(textBox);
                }
                else
                {
                    InputBackspace(textBox);
                }
                InputText(textBox, result);
            }
        }

        private string GetEditingText(TextBox textBox)
            => textBox.Text.Substring(Math.Max(textBox.CaretIndex - 2, 0), Math.Min(textBox.Text.Length, Math.Min(textBox.CaretIndex, 2)));

        private void InputText(UIElement element, string text)
            => element.RaiseEvent(new TextCompositionEventArgs(InputManager.Current.PrimaryKeyboardDevice, new TextComposition(InputManager.Current, element, text))
            {
                RoutedEvent = TextCompositionManager.TextInputEvent,
            });

        private void InputBackspace(UIElement element)
            => element.RaiseEvent(new KeyEventArgs(InputManager.Current.PrimaryKeyboardDevice, PresentationSource.FromVisual(element), 0, Key.Back)
            {
                RoutedEvent = UIElement.KeyDownEvent
            });
    }
}
