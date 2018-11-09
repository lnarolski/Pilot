using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;

namespace Pilot
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
            KeyboardRead.IsVisible = false;
        }

        private void Button_Shortcuts(object sender, EventArgs e)
        {

        }

        private void Show_keyboard(object sender, EventArgs e)
        {
            KeyboardRead.Unfocus();
            Thread.Sleep(500);
            KeyboardRead.Focus();
        }

        private void Button_Config(object sender, EventArgs e)
        {

        }
    }
}
