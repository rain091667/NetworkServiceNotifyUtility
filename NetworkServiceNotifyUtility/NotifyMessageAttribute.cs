using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NetworkServiceNotifyUtility
{
    public class NotifyMessageContent
    {
        [DataMember(Name = "TimeStamp")]
        public string TimeStamp { get; set; }

        [DataMember(Name = "NotifyMessage")]
        public string NotifyMessage { get; set; }

        public NotifyMessageContent()
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
}
