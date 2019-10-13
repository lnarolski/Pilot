using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pilot
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ShortcutsPage : ContentPage
	{
		public ShortcutsPage ()
		{
			InitializeComponent ();

            ShortcutsButtonsGrid.Children.Add(new Button()
            {
                Text = "Dodaj nowy skrót",
                Image = "Pilot.img.plus.png",
                ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 20),
            });
		}

        private void BackButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }
    }
}