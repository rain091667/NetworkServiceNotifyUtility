using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkServiceNotifyUtility.ServiceModel
{
    class NotifyMessageHandler
    {
        private static ConcurrentQueue<NotifyMessageContent> _notifyMessageQueue = null;

        public int Count => _notifyMessageQueue.Count;
        public bool IsEmpty => _notifyMessageQueue.IsEmpty;

        public NotifyMessageHandler()
        {
            _notifyMessageQueue = new ConcurrentQueue<NotifyMessageContent>();
        }

        public void QueueNotifyMessage(NotifyMessageContent notifyMsgContent)
        {
            _notifyMessageQueue.Enqueue(notifyMsgContent);
        }

        public NotifyMessageContent DeQueueNotifyMessage()
        {
            if (_notifyMessageQueue.IsEmpty) return null;
            if (_notifyMessageQueue.TryDequeue(out NotifyMessageContent notifyMessage))
                return notifyMessage;
            return null;
        }
    }
}
