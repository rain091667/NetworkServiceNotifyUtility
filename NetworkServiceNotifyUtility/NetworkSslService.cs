using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
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
    /// NetworkSslService
    /// </summary>
    public class NetworkSslService
    {
        public static bool _DebugMode = false;
        private readonly ServiceHost _serviceHost = null;
        private readonly string _serverIpaddress = string.Empty;
        private readonly int _serverPort = 38000;
        private readonly NetworkInfoUtility _networkInfos = null;
        private readonly NotifyNetworkService _serviceInstance = null;
        private readonly NotifyMessageHandler _notifyMessageHandler = null;

        public int NotifyMessageCount => _notifyMessageHandler.Count;
        public bool NotifyMessageIsEmpty => _notifyMessageHandler.IsEmpty;

        public bool DebugMode { get => _DebugMode; set => _DebugMode = value; }
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

        public NetworkSslService(string ipaddress, ushort port = 38000)
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

            // https
            _serviceHost = new WebServiceHost(_serviceInstance, new Uri[] { new Uri("https://" + _serverIpaddress + ":" + _serverPort.ToString()) });
            WebHttpBinding binding = new WebHttpBinding();
            binding.MaxReceivedMessageSize = 2147483647;
            binding.Security.Mode = WebHttpSecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;
            _serviceHost.Credentials.ClientCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.Custom;
            _serviceHost.Credentials.ClientCertificate.Authentication.CustomCertificateValidator = new Auth();
            _serviceHost.AddServiceEndpoint(typeof(INotifyNetworkService), binding, "NotifyNetworkService");
            _serviceHost.Credentials.ServiceCertificate.SetCertificate(
                StoreLocation.LocalMachine,
                StoreName.My,
                X509FindType.FindByThumbprint,
                "30 3b 4a db 5a eb 17 ee ac 00 d8 57 66 93 a9 08 c0 1e 0b 71");
            var smb = new ServiceMetadataBehavior();
            smb.HttpGetEnabled = false;
            smb.HttpsGetEnabled = true;
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
                if (_DebugMode)
                    Console.WriteLine("CommunicationException error: {0}", ce.Message);
            }
            catch (Exception exc)
            {
                if (_DebugMode)
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
                if (_DebugMode)
                    Console.WriteLine("CommunicationException error: {0}", ce.Message);
            }
            catch (Exception exc)
            {
                if (_DebugMode)
                    Console.WriteLine("Exception error: {0}", exc.Message);
            }
        }

        public NotifyMessageContent NextNotifyMessage()
        {
            return _notifyMessageHandler.DeQueueNotifyMessage();
        }

        public ServiceNotifyResponse Notify(string notifyMessage, string ipaddress, ushort port = 38000)
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
                string res = Web_Request("http://" + targetEndPoint.IPaddress.ToString() + ":" + targetEndPoint.Port + "/NotifyNetworkService/Notify", subscriptionData);
                notifyResponse = SerializeUtility.DeSerialize<ServiceNotifyResponse>(res);
            }
            catch (Exception ex)
            {
                notifyResponse = null;
            }
            return notifyResponse;
        }

        public async Task<ServiceNotifyResponse> NotifyAsync(string notifyMessage, string ipaddress, ushort port = 38000)
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
                    string res = Web_Request("http://" + targetEndPoint.IPaddress.ToString() + ":" + targetEndPoint.Port + "/NotifyNetworkService/Notify", subscriptionData);
                    notifyResponse = SerializeUtility.DeSerialize<ServiceNotifyResponse>(res);
                }
                catch (Exception ex)
                {
                    notifyResponse = null;
                }
            });
            return notifyResponse;
        }

        private static string Web_Request(string url, string reqData)
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

        public class Auth : X509CertificateValidator
        {
            public override void Validate(System.Security.Cryptography.X509Certificates.X509Certificate2 certificate)
            {
                return;
            }
        }
    }
}
