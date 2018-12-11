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

            if (ConnectionClass.connected)
                IP_address_entry.Text = ConnectionClass.ipAddress;
		}

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private void OKButton_Clicked(object sender, EventArgs e)
        {

        }

        private void ConnectButton_Clicked(object sender, EventArgs e)
        {
            if (!ConnectionClass.connected)
            {
                if (ConnectionClass.Connect(IP_address_entry.Text) == ConnectionState.CONNECTION_NOT_ESTABLISHED)
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

        }
    }
}