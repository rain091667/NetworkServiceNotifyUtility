using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NetworkServiceNotifyUtility.ServiceModel
{
    [DataContract]
    class ServiceNotifyRequest
    {
        [DataMember(Name = "TimeStamp")] public string TimeStamp { get; set; }

        [DataMember(Name = "NotifyMessage")] public string NotifyMessage { get; set; }

        public ServiceNotifyRequest()
        {
            TimeStamp = NotifyMessage = string.Empty;
        }

        public void SetTimeSpan(TimeSpan time)
        {
            TimeStamp = time.Ticks.ToString();
        }

        public void SetTimeSpan(DateTime time)
        {
            TimeStamp = TimeSpan.FromTicks(time.Ticks).Ticks.ToString();
        }

        public TimeSpan? TimeSpanInstance
        {
            get
            {
                if (long.TryParse(TimeStamp, out long timeTickStr))
                    return TimeSpan.FromTicks(timeTickStr);
                return null;
            }
        }

        public DateTime? DateTimeInstance
        {
            get
            {
                if (long.TryParse(TimeStamp, out long timeTickStr))
                    return DateTime.FromFileTimeUtc(timeTickStr);
                return null;
            }
        }
    }

    [DataContract]
    public class ServiceNotifyResponse
    {
        [DataMember(Name = "TimeStamp")] public string TimeStamp { get; set; }

        [DataMember(Name = "AcceptStatus")] public bool AcceptStatus { get; set; }

        [DataMember(Name = "ErrorMessage")] public string ErrorMessage { get; set; }

        public ServiceNotifyResponse()
        {
            ErrorMessage = TimeStamp = string.Empty;
            AcceptStatus = false;
        }

        public void SetTimeSpan(TimeSpan time)
        {
            TimeStamp = time.Ticks.ToString();
        }

        public void SetTimeSpan(DateTime time)
        {
            TimeStamp = TimeSpan.FromTicks(time.Ticks).Ticks.ToString();
        }

        public TimeSpan? TimeSpanInstance
        {
            get
            {
                if (long.TryParse(TimeStamp, out long timeTickStr))
                    return TimeSpan.FromTicks(timeTickStr);
                return null;
            }
        }

        public DateTime? DateTimeInstance
        {
            get
            {
                if (long.TryParse(TimeStamp, out long timeTickStr))
                    return DateTime.FromFileTimeUtc(timeTickStr);
                return null;
            }
        }
    }

    [DataContract]
    public class NotifyServerInfo
    {
        [DataMember(Name = "Version")] public string Version { get; set; }
        [DataMember(Name = "Description")] public List<string> Description { get; set; }

        public NotifyServerInfo()
        {
            Version = String.Empty;
            Description = new List<string>();
        }
    }
}
