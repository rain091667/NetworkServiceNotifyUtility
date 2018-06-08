using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace NetworkServiceNotifyUtility.Utility
{
    /// <summary>
    /// Detect Local Network Information
    /// </summary>
    public class NetworkInfoUtility
    {
        /// <summary>
        /// Function: getLocalIpAddress();
        /// <para>Example: string myip = getLocalIpAddress();</para>
        /// </summary>
        /// <returns>local ip address (First Ip Address)</returns>
        public string getLocalIpAddress()
        {
            string strHostName = Dns.GetHostName();
            string myLocalIpAddress;
            IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);
            foreach (IPAddress ipaddress in iphostentry.AddressList)
            {
                if (ipaddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    myLocalIpAddress = ipaddress.ToString();
                    return myLocalIpAddress;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Function: getLocalIpAddressListIpv4();
        /// <para>Example: string[] myip = getLocalIpAddressListIpv4();</para>
        /// </summary>
        /// <returns>local ip address lists (IPv4)</returns>
        public List<string> getLocalIpAddressListIpv4()
        {
            string strHostName = Dns.GetHostName();
            List<string> myLocalIpAddress = new List<string>();
            IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);
            foreach (IPAddress ipaddress in iphostentry.AddressList)
            {
                if (ipaddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    myLocalIpAddress.Add(ipaddress.ToString());
                }
            }
            return myLocalIpAddress;
        }

        /// <summary>
        /// Function: getLocalIpAddressListIpv6();
        /// <para>Example: string[] myip = getLocalIpAddressListIpv6();</para>
        /// </summary>
        /// <returns>local ip address lists (IPv6)</returns>
        public List<string> getLocalIpAddressListIpv6()
        {
            string strHostName = Dns.GetHostName();
            List<string> myLocalIpAddress = new List<string>();
            IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);
            foreach (IPAddress ipaddress in iphostentry.AddressList)
            {
                if (ipaddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    myLocalIpAddress.Add(ipaddress.ToString());
                }
            }
            return myLocalIpAddress;
        }

        /// <summary>
        /// Function: getLocalIpAddressList();
        /// <para>Example: string[] myip = getLocalIpAddressList();</para>
        /// </summary>
        /// <returns>local ip address lists</returns>
        public List<string> getLocalIpAddressList()
        {
            string strHostName = Dns.GetHostName();
            List<string> myLocalIpAddress = new List<string>();
            IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);
            foreach (IPAddress ipaddress in iphostentry.AddressList)
                myLocalIpAddress.Add(ipaddress.ToString());
            return myLocalIpAddress;
        }

        private IPEndPoint[] GetUsedIPEndPoint()
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPointTCP = ipGlobalProperties.GetActiveTcpListeners();
            IPEndPoint[] ipEndPointUDP = ipGlobalProperties.GetActiveUdpListeners();
            TcpConnectionInformation[] tcpConnectionInformation = ipGlobalProperties.GetActiveTcpConnections();

            List<IPEndPoint> allIPEndPoint = new List<IPEndPoint>();
            foreach (IPEndPoint iep in ipEndPointTCP) allIPEndPoint.Add(iep);
            foreach (IPEndPoint iep in ipEndPointUDP) allIPEndPoint.Add(iep);
            foreach (TcpConnectionInformation tci in tcpConnectionInformation) allIPEndPoint.Add(tci.LocalEndPoint);

            return allIPEndPoint.ToArray();
        }

        /// <summary>
        /// Function: IsUsedIPEndPoint(int port);
        /// <para>Example: bool CanUse = IsUsedIPEndPoint(13000);</para>
        /// </summary>
        /// <param name="port">port number to detect (Max:65535)</param>
        /// <returns>true: used false: not.</returns>
        public bool IsUsedIPEndPoint(ushort port)
        {
            IPEndPoint[] UsedIPEndPointList = GetUsedIPEndPoint();
            foreach (IPEndPoint iep in UsedIPEndPointList)
            {
                if (iep.Port == port)
                {
                    return true;
                }
            }
            return false;
        }

        // 判断指定的网络端点（判断IP和端口）是否被使用  
        private bool IsUsedIPEndPoint(string ip, int port)
        {
            foreach (IPEndPoint iep in GetUsedIPEndPoint())
            {
                if (iep.Address.ToString() == ip && iep.Port == port)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Function: CheckIPaddress(string ipaddress);
        /// <para>Example: bool CanUse = CheckIPaddress("127.0.0.1");</para>
        /// </summary>
        /// <param name="ipaddress">Need check address.</param>
        /// <returns>true: can use false: not.</returns>
        public bool CheckIPaddress(string ipaddress)
        {
            IPAddress Results = null;
            return IPAddress.TryParse(ipaddress, out Results);
        }
    }
}
