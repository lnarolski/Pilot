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
	public partial class ShortcutFormPage : ContentPage
	{
        private int Id;
        private ShortcutCell ShortcutCell;
		public ShortcutFormPage (int id = -1)
		{
			InitializeComponent ();

            Id = id;
            if (Id != -1)
            {
                ShortcutCell = DatabaseClass.GetShortcutFromDatabase(Id);

                TextEntry.Text = ShortcutCell.Text;
                WWWAddressEntry.Text = ShortcutCell.WWWAddress;

                Button DeleteButton = new Button()
                {
                    Text = "USUŃ SKRÓT",
                    BackgroundColor = Color.Red,
                    TextColor = Color.White,
                    Margin = new Thickness(50,100),
                };
                DeleteButton.Clicked += DeleteButton_Clicked;
                FormStackLayout.Children.Add(DeleteButton);

                AcceptButton.Text = "Zmień";
            }
		}

        private void DeleteButton_Clicked(object sender, EventArgs e)
        {
            DatabaseClass.DeleteShortcut(ShortcutCell);

            if (DatabaseClass.DatabaseState == DatabaseState.DATABASE_OK)
                Navigation.PopModalAsync();
            else
                DisplayAlert("Wystąpił błąd", DatabaseClass.exceptionText, "OK");
        }

        private void BackButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private void AcceptButton_Clicked(object sender, EventArgs e)
        {
            if (TextEntry.Text == "Dodaj nowy skrót")
            {
                DisplayAlert("Wystąpił błąd", "Nieprawidłowa nazwa skrótu", "OK");
                return;
            }

            ShortcutCell.Text = TextEntry.Text;
            ShortcutCell.WWWAddress = WWWAddressEntry.Text;
            ShortcutCell.Image = "https://www.google.com/s2/favicons?domain=" + ShortcutCell.WWWAddress;

            if (Id != -1)
            {
                DatabaseClass.EditShortcut(ShortcutCell);
            }
            else
            {
                DatabaseClass.AddNewShortcut(ShortcutCell);
            }

            if (DatabaseClass.DatabaseState == DatabaseState.DATABASE_OK)
                Navigation.PopModalAsync();
            else
                DisplayAlert("Wystąpił błąd", DatabaseClass.exceptionText, "OK");
        }
    }
}