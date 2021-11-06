using System;
using VagabondK.Korean.ScreenInput;

namespace ScreenInputSample
{
    public class MainViewModel : NotifyPropertyChangeObject
    {
        public IScreenKeyInputModel InputModel => Get(() => new HangulKeyInputModel());
    }
}
