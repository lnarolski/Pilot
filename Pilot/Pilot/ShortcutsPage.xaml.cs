using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pilot
{
    public struct ShortcutCell
    {
        public int Id { get; set; }
        public string Image { get; set; }
        public string Text { get; set; }
        public string WWWAddress { get; set; }
        public bool ButtonVisible { get; set; }
    }
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ShortcutsPage : ContentPage
	{
        public static ObservableCollection<ShortcutCell> Shortcuts { get; set; }
        private ConnectionState state;
        public ShortcutsPage ()
		{
			InitializeComponent ();

            Shortcuts = new ObservableCollection<ShortcutCell>();
            
            RefreshShortcutsList();
		}

        private void RefreshShortcutsList() //Metoda odświeżająca listę skrótów
        {
            Shortcuts.Clear();

            DatabaseClass.CreateShortcutsList(Shortcuts);

            Shortcuts.Add(new ShortcutCell()
            {
                Image = "plus.png",
                Text = "Dodaj nowy skrót",
                WWWAddress = "",
                ButtonVisible = false,
            });

            ShortcutsListView.ItemsSource = null;
            ShortcutsListView.ItemsSource = Shortcuts; //Podpięcie nowej listy skrótów do kontrolki ListView
        }

        private void ShowAlert(ConnectionState connectionState)
        {
            switch (connectionState)
            {
                case ConnectionState.CONNECTION_NOT_ESTABLISHED:
                    DisplayAlert("Błąd", "Brak połączenia z komputerem", "OK");
                    break;
                case ConnectionState.SEND_NOT_SUCCESS:
                    DisplayAlert("Błąd", "Brak połączenia z komputerem\n" + ConnectionClass.exceptionText, "OK");
                    break;
                default:
                    break;
            }
        }

        private void BackButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private void PreviousButton_Clicked(object sender, EventArgs e)
        {
            if ((state = ConnectionClass.Send(Commands.SEND_PREVIOUS)) != ConnectionState.SEND_SUCCESS)
                ShowAlert(state);
        }

        private void PlayStopButton_Clicked(object sender, EventArgs e)
        {
            if ((state = ConnectionClass.Send(Commands.SEND_PLAYSTOP)) != ConnectionState.SEND_SUCCESS)
                ShowAlert(state);
        }

        private void NextButton_Clicked(object sender, EventArgs e)
        {
            if ((state = ConnectionClass.Send(Commands.SEND_NEXT)) != ConnectionState.SEND_SUCCESS)
                ShowAlert(state);
        }

        private void VolDownButton_Clicked(object sender, EventArgs e)
        {
            if ((state = ConnectionClass.Send(Commands.SEND_VOLDOWN)) != ConnectionState.SEND_SUCCESS)
                ShowAlert(state);
        }

        private void StopButton_Clicked(object sender, EventArgs e)
        {
            if ((state = ConnectionClass.Send(Commands.SEND_STOP)) != ConnectionState.SEND_SUCCESS)
                ShowAlert(state);
        }

        private void VolUpButton_Clicked(object sender, EventArgs e)
        {
            if ((state = ConnectionClass.Send(Commands.SEND_VOLUP)) != ConnectionState.SEND_SUCCESS)
                ShowAlert(state);
        }

        private void ShortcutsListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (((ShortcutCell) e.Item).Text == "Dodaj nowy skrót") //Otwarcie formularza dodawania skrótu, gdy treść klikniętej komórki to "Dodaj nowy skrót"
            {
                ShortcutFormPage shortcutFormPage = new ShortcutFormPage();
                shortcutFormPage.Disappearing += ShortcutFormPage_Disappearing;
                Navigation.PushModalAsync(shortcutFormPage);
            }
            else //Wysyłanie polecenia związanego z klikniętą komórką w innym wypadku
            {
                byte[] WWWAddressByte = Encoding.ASCII.GetBytes(((ShortcutCell)e.Item).WWWAddress);
                ConnectionClass.Send(Commands.SEND_OPEN_WEBPAGE, WWWAddressByte);
            }
        }

        private void ShortcutFormPage_Disappearing(object sender, EventArgs e)
        {
            RefreshShortcutsList();
        }

        private void EditButton_Clicked(object sender, EventArgs e) //Otwarcie formularza w trybie edycji komórki powiązanej z numerem Id
        {
            Button ClickedButton = (Button) sender;
            StackLayout listViewItem = (StackLayout) ClickedButton.Parent;
            Label label = (Label) listViewItem.Children[0];
            int Id = int.Parse(label.Text);

            if (Id > 0)
            {
                ShortcutFormPage shortcutFormPage = new ShortcutFormPage(Id);
                shortcutFormPage.Disappearing += ShortcutFormPage_Disappearing;
                Navigation.PushModalAsync(shortcutFormPage);
            }
        }
    }
}