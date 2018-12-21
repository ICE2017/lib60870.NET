/// <summary>
/// This example client is passively listening to the server's messages
/// </summary>

using System;
using System.Threading;
using lib60870;
using lib60870.CS101;
using lib60870.CS104;
using Polly;

namespace cs104_client3
{
	class MainClass
	{
        private static Connection con;
        //ivate static IBus bus= RabbitHutch.CreateBus("host=localhost");

		public static void Main (string[] args)
		{
			Console.WriteLine ("Using lib60870.NET version " + LibraryCommon.GetLibraryVersionString ());

            try
            {
                var retryTwoTimesPolicy = Policy.Handle<Exception>().Retry(3,
                    (ex, count) => {
                        Console.WriteLine("执行失败! 重试次数 {0}", count);
                        Console.WriteLine("异常来自 {0}", ex.GetType().Name);
                    });
                retryTwoTimesPolicy.Execute(() => {
                    con = new Connection("192.168.216.60", 2410);
                    con.DebugOutput = true;
                    con.SetASDUReceivedHandler((parameter, asdu) => {
                        Console.WriteLine(asdu.ToString());
                        if (asdu.TypeId == TypeID.M_ME_NC_1)
                        {
                            for (int i = 0; i < asdu.NumberOfElements; i++)
                            {
                                var mfv = (MeasuredValueShort)asdu.GetElement(i);
                                Console.WriteLine("  IOA: " + mfv.ObjectAddress + " float value: " + mfv.Value);
                                Console.WriteLine("   " + mfv.Quality.ToString());
                            }
                        }
                        else
                        {
                            Console.WriteLine("Unknown message type!");
                        }
                        return true;
                    }, null);
                    con.SetConnectionHandler((parameter, connectionEvent) => {
                        switch (connectionEvent)
                        {
                            case ConnectionEvent.OPENED:
                                Console.WriteLine("Connected");
                                break;
                            case ConnectionEvent.CLOSED:
                                Console.WriteLine("Connection closed");
                                break;
                            case ConnectionEvent.STARTDT_CON_RECEIVED:
                                Console.WriteLine("STARTDT CON received");
                                break;
                            case ConnectionEvent.STOPDT_CON_RECEIVED:
                                Console.WriteLine("STOPDT CON received");
                                break;
                        }
                    }, null);
                    con.SetReceivedRawMessageHandler(( parameter,message,messageSize) => {
                        Console.WriteLine("====="+BitConverter.ToString(message));
                        return true;
                    }, null);
                    con.Connect();
                });

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            bool running = true;           
            Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e) {
				e.Cancel = true;
				running = false;
			};

			while (running) {
				Thread.Sleep(100);
			}
			con.Close ();
		}
	}
}
