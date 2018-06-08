using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace NetworkServiceNotifyUtility.ServiceModel
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    class NotifyNetworkService : INotifyNetworkService
    {
        private const string _serverVersionString = "Notify Network Service Version:{0}";
        private static string _serviceVersion = string.Empty;
        private NotifyMessageHandler _notifyMessageHandler = null;
        private List<string> _descriptionInfoLists = null;

        public NotifyNetworkService(NotifyMessageHandler handler)
        {
            string version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            _serviceVersion = String.Format(_serverVersionString, version);
            _notifyMessageHandler = handler;
            _descriptionInfoLists = new List<string>();
        }

        public void AddServerInfoDescription(string description)
        {
            _descriptionInfoLists.Add(description);
        }

        public void ClearServerInfoDescription()
        {
            _descriptionInfoLists.Clear();
        }

        public ServiceNotifyResponse Notify(ServiceNotifyRequest clientRequest)
        {
            ServiceNotifyResponse res = new ServiceNotifyResponse();
            try
            {
                _notifyMessageHandler.QueueNotifyMessage(new NotifyMessageContent()
                {
                    TimeStamp = clientRequest.TimeStamp,
                    NotifyMessage = clientRequest.NotifyMessage
                });
                res.SetTimeSpan(DateTime.Now);
                res.AcceptStatus = true;
            }
            catch (Exception exception)
            {
                res.ErrorMessage = NetworkService.DebugMode ? exception.Message : string.Empty;
            }
            return res;
        }

        public bool ServerActive()
        {
            return true;
        }

        public NotifyServerInfo ServerInfo()
        {
            NotifyServerInfo info = new NotifyServerInfo()
            {
                Version = _serviceVersion,
                Description = new List<string>()
            };
            info.Description.AddRange(_descriptionInfoLists.ToArray());
            return info;
        }
    }
}
