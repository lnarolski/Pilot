using Pilot.Resx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

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
            if (Id != -1) //Gdy formularz ma być wykorzystany do edycji skrótu
            {
                ShortcutCell = DatabaseClass.GetShortcutFromDatabase(Id);

                TextEntry.Text = ShortcutCell.Text;
                WWWAddressEntry.Text = ShortcutCell.WWWAddress;

                Button DeleteButton = new Button()
                {
                    Text = AppResources.DeleteShortcutFormPage,
                    BackgroundColor = Colors.Red,
                    TextColor = Colors.White,
                    Margin = new Thickness(50,100),
                };
                DeleteButton.Clicked += DeleteButton_Clicked;
                FormStackLayout.Children.Add(DeleteButton);

                AcceptButton.Text = AppResources.ChangeShortcutFormPage;
            }
		}

        private void DeleteButton_Clicked(object sender, EventArgs e)
        {
            DatabaseClass.DeleteShortcut(ShortcutCell);

            if (DatabaseClass.DatabaseState == DatabaseState.DATABASE_OK)
                Navigation.PopModalAsync();
            else
                DisplayAlert(AppResources.Error, DatabaseClass.exceptionText, AppResources.OK);
        }

        private void BackButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private void AcceptButton_Clicked(object sender, EventArgs e)
        {
            if (TextEntry.Text == AppResources.AddShortcutsPage)
            {
                DisplayAlert(AppResources.Error, AppResources.WrongShortcutNameError, AppResources.OK);
                return;
            }

            ShortcutCell.Text = TextEntry.Text;
            ShortcutCell.WWWAddress = WWWAddressEntry.Text;
            ShortcutCell.Image = "https://www.google.com/s2/favicons?domain=" + ShortcutCell.WWWAddress; //Pobranie adresu favicon'a z wykorzystaniem Google'a

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
                DisplayAlert(AppResources.Error, DatabaseClass.exceptionText, AppResources.OK);
        }
    }
}