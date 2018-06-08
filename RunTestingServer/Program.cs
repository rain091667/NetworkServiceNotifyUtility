using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetworkServiceNotifyUtility;
using NetworkServiceNotifyUtility.ServiceModel;
using NetworkServiceNotifyUtility.Utility;

namespace RunTestingServer
{
    class Program
    {
        private static NetworkService mNetworkService = null;
        private static string UserName = "User1";
        static void Main(string[] args)
        {
            mNetworkService = new NetworkService("127.0.0.1");
            mNetworkService.Start();
            Console.WriteLine(mNetworkService.ServiceEndpointUrl);

            Console.WriteLine("Wait Start...");
            Console.ReadKey();

            Console.WriteLine("Testing Server Active....");
            Console.WriteLine("Res: " + NetworkService.IsServerActive("127.0.0.1", waitTimeMsUnit: 1000));

            mNetworkService.AddServerInfoDescription("訊息伺服器");

            NotifyServerInfo serverInfo = NetworkService.GetServerInfo("127.0.0.1", waitTimeMsUnit: 1000);
            Console.WriteLine("Server Info: " + serverInfo.Version + ", Description: " + String.Join(", ", serverInfo.Description.ToArray()));

            Console.WriteLine();
            Console.WriteLine("Enter your \"Name\": ");
            UserName = Console.ReadLine();
            Console.WriteLine("Start Send Message, Input Message to Send:");
            Console.WriteLine("Message:");

            //Console.ReadKey();

            //return;
            Task.Run(async() =>
            {
                NotifyMessageContent content = null;
                DateTime msgTime;
                testing2 obj;
                while (true)
                {
                    if (!mNetworkService.NotifyMessageIsEmpty)
                    {
                        content = mNetworkService.NextNotifyMessage();
                        if (content.DateTimeInstance.HasValue)
                        {
                            //msgTime = DateTime.FromFileTimeUtc(long.Parse(content.TimeStamp));
                            //Console.WriteLine(" ==> " + msgTime.ToString("yyyy-MM-dd HH:mm:ss.fff") + ": " + content.NotifyMessage);

                            obj = SerializeUtility.DeSerialize<testing2>(content.NotifyMessage);
                            Console.WriteLine("[" + obj.Name + "] " + content.DateTimeInstance?.ToString("yyyy-MM-dd HH:mm:ss.fff") + ": " + obj.Message);
                        }
                    }
                    await Task.Delay(1);
                }
            });

            Random rands = new Random(Environment.TickCount);
            string sendString = String.Empty;
            testing2 sendObj = null;
            while (true)
            {
                sendString = Console.ReadLine();
                sendObj = new testing2()
                {
                    Name = UserName,
                    Message = sendString
                };
                NetworkService.Notify(SerializeUtility.Serialize(sendObj), "127.0.0.1");

                //mNetworkService.Notify(rands.NextDouble().ToString("N5"), "192.168.1.183");

                /*mNetworkService.Notify(
                    SerializeUtility.Serialize(new testing()
                    {
                        aaa = 10,
                        bbb = "Hello World",
                        ccc = 0.123456896,
                        ddd = "AAAAAAAAAAAAAAAAAAAAAAA",
                        eee = "CCCCCCCCCCCCCCCCCC"
                    }), "192.168.1.183");
                Thread.Sleep(500);*/
            }
        }

        [DataContract]
        class testing
        {
            [DataMember(Name = "aaa")] public int aaa { get; set; }
            [DataMember(Name = "bbb")] public string bbb { get; set; }
            [DataMember(Name = "ccc")] public double ccc { get; set; }
            [DataMember(Name = "ddd")] public string ddd { get; set; }
            [DataMember(Name = "eee")] public string eee { get; set; }

            public string ToString()
            {
                return "" + aaa.ToString() + ", " + bbb.ToString() + ", " + ccc.ToString() + ", " + ddd.ToString() +
                       ", " + eee.ToString();
            }
        }

        [DataContract]
        class testing2
        {
            [DataMember(Name = "Name")] public string Name { get; set; }
            [DataMember(Name = "Message")] public string Message { get; set; }

            public string ToString()
            {
                return "Name: " + Name + ", " + "Message: " + Message;
            }
        }
    }
}
