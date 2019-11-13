using System;
using Milvaneth.Utilities;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Milvaneth
{
    /// <summary>
    /// Interaction logic for Initializing.xaml
    /// </summary>
    public partial class Initializing : Window
    {
        public Initializing()
        {
            InitializeComponent();
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth - windowWidth) / 2;
            this.Top = (screenHeight - windowHeight) / 2;

            this.Topmost = true;
            this.Topmost = false;
        }
    }
}
