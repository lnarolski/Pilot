using System;
using System.Collections.Generic;
using System.Text;

namespace Pilot
{
    enum ConnectionState
    {
        CONNECTION_ESTABLISHED,
        CONNECTION_NOT_ESTABLISHED,
    }
    static class ConnectionClass
    {
        public static ConnectionState Connect() {

            return ConnectionState.CONNECTION_NOT_ESTABLISHED;
        }
    };
}
