﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Pilot
{
    public interface IWidgetService
    {
        void CreateWidget();
        void RemoveWidget();
        void UpdateWidget(string artist, string title, byte[] thumbnail);
        void SendCommandToServer(CommandsFromClient command);
    }
}
