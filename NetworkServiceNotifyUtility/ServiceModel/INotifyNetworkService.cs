using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace NetworkServiceNotifyUtility.ServiceModel
{
    [ServiceContract]
    interface INotifyNetworkService
    {
        [OperationContract]
        [WebInvoke(
            Method = "POST",
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "Notify")]
        ServiceNotifyResponse Notify(ServiceNotifyRequest clientRequest);

        [OperationContract]
        [WebInvoke(
            Method = "GET",
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "ServerActive")]
        bool ServerActive();

        [OperationContract]
        [WebInvoke(
            Method = "GET",
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "ServerInfo")]
        NotifyServerInfo ServerInfo();
    }
}
