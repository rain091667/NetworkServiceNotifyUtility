using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetworkServiceNotifyUtility.Utility
{
    public class IPHostEndPoint
    {
        private IPAddress _IPaddress = null;
        private ushort? _Port = null;

        public IPHostEndPoint(string ipAddress, ushort port)
        {
            if (!IPAddress.TryParse(ipAddress, out _IPaddress))
            {
                throw new FormatException("Error IP Address.");
            }
            _Port = port;
        }

        public IPHostEndPoint(IPAddress ipAddress, ushort port)
        {
            _IPaddress = ipAddress;
            _Port = port;
        }

        public IPAddress IPaddress { get { return _IPaddress; } }
        public ushort? Port { get { return _Port; } }
    }
}
