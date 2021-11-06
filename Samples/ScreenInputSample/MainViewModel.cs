using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using VagabondK.Korean;
using VagabondK.Korean.Hangul;
using VagabondK.Korean.ScreenInput;

namespace ScreenInputSample
{
    public class MainViewModel : NotifyPropertyChangeObject
    {
        public IScreenKeyInputModel InputModel => Get(() => new HangulKeyInputModel());
    }
}
