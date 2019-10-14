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
        public string Image { get; set; }
        public string Text { get; set; }
        public string WWWAddress { get; set; }
    }
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ShortcutsPage : ContentPage
	{
        public static ObservableCollection<ShortcutCell> Shortcuts { get; set; }
        public ShortcutsPage ()
		{
			InitializeComponent ();

            Shortcuts = new ObservableCollection<ShortcutCell>();

            RefreshShortcutsList();
		}

        private void RefreshShortcutsList()
        {
            Shortcuts.Clear();

            Shortcuts.Add(new ShortcutCell()
            {
                Image = "plus.png",
                Text = "Dodaj nowy skrót",
                WWWAddress = "",
            });

            ShortcutsListView.ItemsSource = null;
            ShortcutsListView.ItemsSource = Shortcuts; //Podpięcie nowej listy skrótów do kontrolki ListView
        }

        private void BackButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private void ShortcutsListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {

        }
    }
}