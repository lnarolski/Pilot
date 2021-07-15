using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(Pilot.UWP.Services.WidgetService))]
namespace Pilot.UWP.Services
{
    class WidgetService : IWidgetService
    {
        public void CreateWidget()
        {
            
        }

        public void RemoveWidget()
        {
            
        }

        public void SendCommandToServer(CommandsFromClient commandsFromClient)
        {
            
        }

        public void UpdateWidget(string artist, string title, byte[] thumbnail)
        {

        }
    }
}
