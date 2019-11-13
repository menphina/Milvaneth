using System.Windows;
using Milvaneth.Service;

namespace Milvaneth.Overlay
{
    public abstract class OverlayBase : Window
    {
        public OverlayBase() { }
        public OverlayBase(BindingRouter br) { }
        public abstract void SetClickthrough(bool toggle);
        public abstract void SetVisibility(bool toggle);
    }
}
