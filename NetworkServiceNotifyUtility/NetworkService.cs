using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using NetworkServiceNotifyUtility.ServiceModel;
using NetworkServiceNotifyUtility.Utility;

namespace NetworkServiceNotifyUtility
{
    /// <summary>
    /// NetworkService
    /// </summary>
    public class NetworkService
    {
        private static bool _debugMode = false;
        private readonly ServiceHost _serviceHost = null;
        private readonly string _serverIpaddress = string.Empty;
        private readonly int _serverPort = 38000;
        private readonly NetworkInfoUtility _networkInfos = null;
        private readonly NotifyNetworkService _serviceInstance = null;
        private readonly NotifyMessageHandler _notifyMessageHandler = null;

        /// <summary>
        /// NotifyMessageCount
        /// </summary>
        public int NotifyMessageCount => _notifyMessageHandler.Count;

        /// <summary>
        /// NotifyMessageIsEmpty
        /// </summary>
        public bool NotifyMessageIsEmpty => _notifyMessageHandler.IsEmpty;

        /// <summary>
        /// DebugMode
        /// </summary>
        public static bool DebugMode { get => _debugMode; set => _debugMode = value; }

        /// <summary>
        /// ServiceEndpointUrl
        /// </summary>
        public string ServiceEndpointUrl
        {
            get
            {
                string url = string.Empty;
                try
                {
                    url = this._serviceHost.Description.Endpoints[0].ListenUri.AbsoluteUri;
                }
                catch (Exception)
                {
                    url = string.Empty;
                }
                return url;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipaddress"></param>
        /// <param name="port"></param>
        public NetworkService(string ipaddress, ushort port = 38000)
        {
            if (!IsAdministrator())
            {
                throw new SecurityException("Inorder to register service binding. Must run as a user with local Administrator privileges.");
            }

            _networkInfos = new NetworkInfoUtility();
            if (!_networkInfos.CheckIPaddress(ipaddress))
            {
                throw new FormatException("Ipaddress illegal.");
            }
            if (_networkInfos.IsUsedIPEndPoint(port))
            {
                throw new Exception("Port is in used or illegal port number.");
            }

            this._notifyMessageHandler = new NotifyMessageHandler();
            this._serverIpaddress = ipaddress;
            this._serverPort = port;
            this._serviceInstance = new NotifyNetworkService(_notifyMessageHandler);

            // http
            _serviceHost = new WebServiceHost(_serviceInstance, new Uri[] { new Uri("http://" + _serverIpaddress + ":" + _serverPort.ToString()) });
            WebHttpBinding binding = new WebHttpBinding();
            binding.MaxReceivedMessageSize = 2147483647;
            _serviceHost.AddServiceEndpoint(typeof(INotifyNetworkService), binding, "NotifyNetworkService");
            var smb = new ServiceMetadataBehavior();
            smb.HttpGetEnabled = true;
            smb.HttpsGetEnabled = false;
            _serviceHost.Description.Behaviors.Add(smb);
        }

        private bool IsAdministrator()
        {
            WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            return pricipal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public void Start()
        {
            try
            {
                _serviceHost.Open();
            }
            catch (CommunicationException ce)
            {
                if (_debugMode)
                    Console.WriteLine("CommunicationException error: {0}", ce.Message);
            }
            catch (Exception exc)
            {
                if (_debugMode)
                    Console.WriteLine("Exception error: {0}", exc.Message);
            }
        }

        public void Stop()
        {
            try
            {
                _serviceHost.Close();
            }
            catch (CommunicationException ce)
            {
                if (_debugMode)
                    Console.WriteLine("CommunicationException error: {0}", ce.Message);
            }
            catch (Exception exc)
            {
                if (_debugMode)
                    Console.WriteLine("Exception error: {0}", exc.Message);
            }
        }

        public NotifyMessageContent NextNotifyMessage()
        {
            return _notifyMessageHandler.DeQueueNotifyMessage();
        }

        public void AddServerInfoDescription(string description)
        {
            _serviceInstance.AddServerInfoDescription(description);
        }

        public void ClearServerInfoDescription()
        {
            _serviceInstance.ClearServerInfoDescription();
        }

        public static ServiceNotifyResponse Notify(string notifyMessage, string ipaddress, ushort port = 38000, int waitTimeMsUnit = 10000)
        {
            ServiceNotifyResponse notifyResponse = null;
            try
            {
                IPHostEndPoint targetEndPoint = new IPHostEndPoint(ipaddress, port);
                ServiceNotifyRequest request = new ServiceNotifyRequest()
                {
                    NotifyMessage = notifyMessage
                };
                request.SetTimeSpan(DateTime.Now);
                string subscriptionData = SerializeUtility.Serialize(request);
                string res = Web_Request("http://" + targetEndPoint.IPaddress.ToString() + ":" + targetEndPoint.Port + "/NotifyNetworkService/Notify", subscriptionData, waitTimeMsUnit);
                if (!string.IsNullOrWhiteSpace(res))
                {
                    notifyResponse = SerializeUtility.DeSerialize<ServiceNotifyResponse>(res);
                }
            }
            catch (Exception ex)
            {
                notifyResponse = null;
            }
            return notifyResponse;
        }

        public static async Task<ServiceNotifyResponse> NotifyAsync(string notifyMessage, string ipaddress, ushort port = 38000, int waitTimeMsUnit = 10000)
        {
            ServiceNotifyResponse notifyResponse = null;
            await Task.Run(() =>
            {
                try
                {
                    IPHostEndPoint targetEndPoint = new IPHostEndPoint(ipaddress, port);
                    ServiceNotifyRequest request = new ServiceNotifyRequest()
                    {
                        NotifyMessage = notifyMessage
                    };
                    request.SetTimeSpan(DateTime.Now);
                    string subscriptionData = SerializeUtility.Serialize(request);
                    string res = Web_Request("http://" + targetEndPoint.IPaddress.ToString() + ":" + targetEndPoint.Port + "/NotifyNetworkService/Notify", subscriptionData, waitTimeMsUnit);
                    if (!string.IsNullOrWhiteSpace(res))
                    {
                        notifyResponse = SerializeUtility.DeSerialize<ServiceNotifyResponse>(res);
                    }
                }
                catch (Exception ex)
                {
                    notifyResponse = null;
                }
            });
            return notifyResponse;
        }

        public static bool IsServerActive(string ipaddress, ushort port = 38000, int waitTimeMsUnit = 10000)
        {
            bool isServerActive = false;
            try
            {
                IPHostEndPoint targetEndPoint = new IPHostEndPoint(ipaddress, port);
                isServerActive = Web_Request_IsServerActive("http://" + targetEndPoint.IPaddress.ToString() + ":" +
                                                            targetEndPoint.Port +
                                                            "/NotifyNetworkService/ServerActive", waitTimeMsUnit);
            }
            catch (Exception ex)
            {
                isServerActive = false;
            }
            return isServerActive;
        }

        public static async Task<bool> IsServerActiveAsync(string ipaddress, ushort port = 38000, int waitTimeMsUnit = 10000)
        {
            bool isServerActive = false;
            await Task.Run(() =>
            {
                try
                {
                    IPHostEndPoint targetEndPoint = new IPHostEndPoint(ipaddress, port);
                    isServerActive = Web_Request_IsServerActive("http://" + targetEndPoint.IPaddress.ToString() + ":" + targetEndPoint.Port +
                                                                "/NotifyNetworkService/ServerActive", waitTimeMsUnit);
                }
                catch (Exception ex)
                {
                    isServerActive = false;
                }
            });
            return isServerActive;
        }

        public static NotifyServerInfo GetServerInfo(string ipaddress, ushort port = 38000, int waitTimeMsUnit = 10000)
        {
            NotifyServerInfo notifyServerInfo = null;
            try
            {
                IPHostEndPoint targetEndPoint = new IPHostEndPoint(ipaddress, port);
                string res = Web_Request_ServerInfo("http://" + targetEndPoint.IPaddress.ToString() + ":" +
                                                            targetEndPoint.Port +
                                                            "/NotifyNetworkService/ServerInfo", waitTimeMsUnit);
                if (!string.IsNullOrWhiteSpace(res))
                {
                    notifyServerInfo = SerializeUtility.DeSerialize<NotifyServerInfo>(res);
                }
            }
            catch (Exception ex)
            {
                notifyServerInfo = null;
            }
            return notifyServerInfo;
        }

        public static async Task<NotifyServerInfo> GetServerInfoAsync(string ipaddress, ushort port = 38000, int waitTimeMsUnit = 10000)
        {
            NotifyServerInfo notifyServerInfo = null;
            await Task.Run(() =>
            {
                try
                {
                    IPHostEndPoint targetEndPoint = new IPHostEndPoint(ipaddress, port);
                    string res = Web_Request_ServerInfo("http://" + targetEndPoint.IPaddress.ToString() + ":" +
                                                        targetEndPoint.Port +
                                                        "/NotifyNetworkService/ServerInfo", waitTimeMsUnit);
                    if (!string.IsNullOrWhiteSpace(res))
                    {
                        notifyServerInfo = SerializeUtility.DeSerialize<NotifyServerInfo>(res);
                    }
                }
                catch (Exception ex)
                {
                    notifyServerInfo = null;
                }
            });
            return notifyServerInfo;
        }

        private static string Web_Request(string url, string reqData, int ms)
        {
            string responseData;
            try
            {
                byte[] sendData = Encoding.UTF8.GetBytes(reqData);
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Timeout = 10 * 1000;
                req.Method = "POST";
                req.ContentType = "application/json";
                req.ContentLength = sendData.Length;
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(sendData, 0, sendData.Length);
                }

                using (WebResponse wr = req.GetResponse())
                {
                    StreamReader reader = new StreamReader(wr.GetResponseStream());
                    responseData = reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                responseData = string.Empty;
            }
            return responseData;
        }

        private static bool Web_Request_IsServerActive(string url, int ms)
        {
            bool isServerActive = false;
            try
            {
                string responseData = string.Empty;
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Timeout = ms;
                req.Method = "GET";
                req.ContentType = "application/json";
                using (WebResponse wr = req.GetResponse())
                {
                    StreamReader reader = new StreamReader(wr.GetResponseStream());
                    responseData = reader.ReadToEnd();
                    if (bool.TryParse(responseData, out bool tServerResult))
                    {
                        isServerActive = tServerResult;
                    }
                }
            }
            catch (Exception)
            {
                isServerActive = false;
            }
            return isServerActive;
        }

        private static string Web_Request_ServerInfo(string url, int ms)
        {
            string responseData = string.Empty;
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Timeout = ms;
                req.Method = "GET";
                req.ContentType = "application/json";
                using (WebResponse wr = req.GetResponse())
                {
                    StreamReader reader = new StreamReader(wr.GetResponseStream());
                    responseData = reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                responseData = string.Empty;
            }
            return responseData;
        }
    }
}