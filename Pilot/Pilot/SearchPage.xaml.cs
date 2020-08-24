using Pilot.Resx;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Zeroconf;

namespace Pilot
{
    public struct FoundServer
    {
        public string IPAddress { get; set; }
        public short port { get; set; }
        public string name { get; set; }
    }

	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SearchPage : ContentPage
	{
        public event Action<string> SelectedIPAddress; //Wybrany przez użytkownika adres IP przekazywany do okna rodzica
        public event Action<short> SelectedPort; //Wybrany przez użytkownika port adresu IP przekazywany do okna rodzica
        Task task; //Zadanie wyszukiwania uług w sieci
        public static ObservableCollection<FoundServer> IPAdresses { get; set; }
        public SearchPage ()
		{
			InitializeComponent ();
            OKButton.IsEnabled = false; //Wyłączenie przycisku OK to czasu wybrania adresu IP serwera przez użytkownika

            Text.Text = AppResources.SearchingSearchPage; //Wyświetlenie komunikatu

            task = ProbeForNetworkServers(); //Uruchomienie zadania wyszukiwania uług w sieci
        }

        public async Task ProbeForNetworkServers() //Asynchroniczna metoda służacą do wyszukiwania usług w sieci lokalnej z wykorzystaniem zeroconf
        {
            CancelButton.IsEnabled = false; //Deaktywacja przycisku Wstecz do czasu zakończenia działania wątku

            if (IPAdresses != null)
                IPAdresses.Clear(); //Czyszczenie listy adresów IP

            IReadOnlyList<IZeroconfHost> results = await
                ZeroconfResolver.ResolveAsync("_pilotServer._tcp.local."); //Wyszukiwanie usług o nazwie "_pilotServer._tcp.local." z wykorzystaniem zeroconf
            IPAdresses = new ObservableCollection<FoundServer>();
            foreach (var item in results)
            {
                IPAdresses.Add(new FoundServer() {
                    IPAddress = item.IPAddress,
                    port = (short) item.Services["_pilotServer._tcp.local."].Port,
                    name = item.DisplayName
                }); //Dodawanie znalezionych adresów IP do listy
            }
            if (IPAdresses.Count == 0)
            {
                Text.Text = AppResources.NotFoundSearchPage; //Wyświetlenie komunikatu o nieznalezieniu serwerów w seci lokalnej
            }
            else
            {
                Text.IsVisible = false; //Ukrycie komunikatu o wyszukiwaniu serwerów
                IPAddressesListView.IsVisible = true; //Wyświetlenie listy
                IPAddressesListView.ItemsSource = null;
                IPAddressesListView.ItemsSource = IPAdresses; //Podpięcie nowej listy adresów IP z uruchomioną usługą serwera aplikacji Pilot do kontrolki ListView
            }

            CancelButton.IsEnabled = true; //Aktywacja przycisku Wstecz
        }


        private void OKButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync(); //Zamknięcie okna wyszukiwania serwerów
        }

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            SelectedIPAddress(null); //Reset wartości wybranego adresu IP, która przekazywana jest do okna rodzica
            Navigation.PopModalAsync(); //Zamknięcie okna wyszukiwania serwerów
        }

        private void OnSelection(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) //W przypadku błędu wyboru adresu IP zakończenie metody
            {
                return;
            }
            OKButton.IsEnabled = true; //Aktywacja przycisku OK
            SelectedIPAddress(((FoundServer) (e.SelectedItem)).IPAddress.ToString()); //Przekazanie wybranego przez użytkownika adresu IP do okna rodzica
            SelectedPort(((FoundServer) (e.SelectedItem)).port); //Przekazanie wybranego przez użytkownika portu adresu IP do okna rodzica
        }
    }
}