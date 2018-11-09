using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Pilot
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }

        private void Button_Shortcuts(object sender, EventArgs e)
        {

        }

        private void Show_keyboard(object sender, EventArgs e)
        {
        X: KeyboardRead.Focus();
        }

        private void Button_Config(object sender, EventArgs e)
        {

        }
    }
}
