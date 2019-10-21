using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SQLite;

namespace Pilot
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ConfigPage : ContentPage
	{
        public string SelectedIPAddress;
        public ConfigPage ()
		{
			InitializeComponent();

            if (ConnectionClass.connected)
            {
                ConnectButton.Text = "Rozłącz";
            }
            else
            {
                ConnectButton.Text = "Połącz";
            }
            
            IP_address_entry.Text = DatabaseClass.GetLastIPAddress();
            Port_entry.Text = DatabaseClass.GetLastPort();
            Password_entry.Text = DatabaseClass.GetLastPassword();
        }

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private void ConnectButton_Clicked(object sender, EventArgs e) //próba połączenia z serwerem
        {
            if (!ConnectionClass.connected)
            {
                if (ConnectionClass.Connect(IP_address_entry.Text, Port_entry.Text, Password_entry.Text) == ConnectionState.CONNECTION_NOT_ESTABLISHED)
                    DisplayAlert("Błąd", "Brak połączenia z komputerem\n" + ConnectionClass.exceptionText, "OK");
                else
                {
                    ConnectButton.Text = "Rozłącz";
                    ConnectionClass.Disconnect();
                }
            }
            else
            {
                if (ConnectionClass.Disconnect() == ConnectionState.DISCONECT_NOT_SUCCESS)
                    DisplayAlert("Błąd", "Brak połączenia z komputerem\n" + ConnectionClass.exceptionText, "OK");
                else
                {
                    ConnectButton.Text = "Połącz";
                    ConnectionClass.connected = false;
                }
            }
        }

        private void SearchButton_Clicked(object sender, EventArgs e)
        {
            SearchPage searchPage = new SearchPage();
            searchPage.SelectedIPAddress += value => SelectedIPAddress = value;
            searchPage.Disappearing += SearchPage_Disappearing;
            Navigation.PushModalAsync(searchPage);
        }

        private void SearchPage_Disappearing(object sender, EventArgs e)
        {
            if (SelectedIPAddress != null)
            {
                ConnectButton_Clicked(null, null);
                IP_address_entry.Text = SelectedIPAddress;
                SelectedIPAddress = null;
            }
        }
    }
}