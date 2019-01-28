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
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SearchPage : ContentPage
	{
        public event Action<string> SelectedIPAddress; //Wybrany przez użytkownika adres IP przekazywany do okna rodzica
        Task task; //Zadanie wyszukiwania uług w sieci
        public static ObservableCollection<string> IPAdresses { get; set; }
        public SearchPage ()
		{
			InitializeComponent ();
            OKButton.IsEnabled = false; //Wyłączenie przycisku OK to czasu wybrania adresu IP serwera przez użytkownika

            Text.Text = "Wyszukiwanie serwera..."; //Wyświetlenie komunikatu

            task = ProbeForNetworkServers(); //Uruchomienie zadania wyszukiwania uług w sieci
        }

        public async Task ProbeForNetworkServers() //Asynchroniczna metoda służacą do wyszukiwania usług w sieci lokalnej z wykorzystaniem zeroconf
        {
            CancelButton.IsEnabled = false; //Deaktywacja przycisku Wstecz do czasu zakończenia działania wątku

            if (IPAdresses != null)
                IPAdresses.Clear(); //Czyszczenie listy adresów IP

            IReadOnlyList<IZeroconfHost> results = await
                ZeroconfResolver.ResolveAsync("_pilotServer._tcp.local."); //Wyszukiwanie usług o nazwie "_pilotServer._tcp.local." z wykorzystaniem zeroconf
            IPAdresses = new ObservableCollection<string>();
            foreach (var item in results)
            {
                IPAdresses.Add(item.IPAddress); //Dodawanie znalezionych adresów IP do listy
            }
            if (IPAdresses.Count == 0)
            {
                Text.Text = "Nie odnaleziono serwerów."; //Wyświetlenie komunikatu o nieznalezieniu serwerów w seci lokalnej
            }
            else
            {
                Text.IsVisible = false; //Ukrycie komunikatu o wyszukiwaniu serwerów
                IPAddressesListView.ItemsSource = null;
                IPAddressesListView.ItemsSource = IPAdresses; //Podpięcie nowej listy adresów IP z uruchomioną usługą serwera aplikacji Pilot do kontrolki ListView
                IPAddressesListView.IsVisible = true; //Wyświetlenie wygenerowanej listy
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
            SelectedIPAddress(e.SelectedItem.ToString()); //Przekazanie wybranego przez użytkownika adresu IP do okna rodzica
        }
    }
}