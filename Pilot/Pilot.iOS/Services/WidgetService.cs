using System;
using Xamarin.Forms;

[assembly: Dependency(typeof(Pilot.iOS.Services.WidgetService))]
namespace Pilot.iOS.Services
{
    public class WidgetService : IWidgetService
    {
        public WidgetService()
        {
        }

        public void CreateWidget()
        {
            
        }

        public void RemoveWidget()
        {
            
        }

        public void SendCommandToServer(CommandsFromClient command)
        {
            
        }

        public void UpdateWidget(string artist, string title, byte[] thumbnail)
        {
            
        }
    }
}
