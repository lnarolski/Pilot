using System;
using System.Collections.Generic;
using System.Text;

namespace Pilot
{
    public interface ILockScreenWidgetService
    {
        void CreateLockScreenWidget();
        void RemoveLockScreenWidget();
        void UpdateLockeScreenWidget();
        void SendCommandToServer();
    }
}
