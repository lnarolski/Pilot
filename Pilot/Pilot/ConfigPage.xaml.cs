using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SQLite;
using Pilot.Resx;

namespace Pilot
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ConfigPage : ContentPage
	{
        public string SelectedIPAddress;
        public short SelectedPort;
        public ConfigPage ()
		{
			InitializeComponent();

            if (ConnectionClass.connected)
            {
                ConnectButton.Text = AppResources.Disconnect;
            }
            else
            {
                ConnectButton.Text = AppResources.Connect;
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
                try
                {
                    if (short.Parse(Port_entry.Text) < 1)
                    {
                        DisplayAlert(AppResources.Error, AppResources.WrongPortNumberError, AppResources.OK);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    DisplayAlert(AppResources.WrongPortNumberError, ex.ToString(), AppResources.OK);
                    return;
                }

                if (ConnectionClass.Connect(IP_address_entry.Text, Port_entry.Text, Password_entry.Text) == ConnectionState.CONNECTION_NOT_ESTABLISHED)
                    DisplayAlert(AppResources.Error, AppResources.NoConnectionError + "\n" + ConnectionClass.exceptionText, AppResources.OK);
                else
                {
                    ConnectButton.Text = AppResources.Disconnect;
                }
            }
            else
            {
                if (ConnectionClass.Disconnect() == ConnectionState.DISCONECT_NOT_SUCCESS)
                    DisplayAlert(AppResources.Error, AppResources.NoConnectionError + "\n" + ConnectionClass.exceptionText, AppResources.OK);
                else
                {
                    ConnectButton.Text = AppResources.Connect;
                }
            }
        }

        private void SearchButton_Clicked(object sender, EventArgs e)
        {
            SearchPage searchPage = new SearchPage();
            searchPage.SelectedIPAddress += value => SelectedIPAddress = value;
            searchPage.SelectedPort += value => SelectedPort = value;
            searchPage.Disappearing += SearchPage_Disappearing;
            Navigation.PushModalAsync(searchPage);
        }

        private void SearchPage_Disappearing(object sender, EventArgs e)
        {
            if (SelectedIPAddress != null)
            {
                IP_address_entry.Text = SelectedIPAddress;
                Port_entry.Text = SelectedPort.ToString();
                //ConnectButton_Clicked(null, null);
                SelectedIPAddress = null;
            }
        }
    }
}