using Greeting;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GreetingAppClient
{
    public class Program
    {
        static readonly int PORT = 5001;

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Press any key to start...");
            Console.ReadKey();

            var channel = new Channel("localhost", PORT, ChannelCredentials.Insecure);
            var client = new Greeting.GreeterSvc.GreeterSvcClient(channel);

            CancellationToken ct = CancellationToken.None;
            var cl = new CallOptions(cancellationToken: ct, deadline: DateTime.UtcNow.AddMinutes(2));

            for (int i = 0; i < 2 * 100 * 100; i++)
            {
                HelloRequest rq = new HelloRequest
                {
                    Name = $"hello{i}"
                };
                var rs = await client.SayHelloAsync(rq, cl);
                Log($"SayHelloAsync {rs.Message}");
            }

            StringBuilder sbRs = new StringBuilder();
            var listRq = new DumbRequest();
            using (var call = client.ListGreetings(listRq, cl))
            {
                var stream = call.ResponseStream;

                StringBuilder sb = new StringBuilder();

                while (await stream.MoveNext(ct))
                {
                    GreetingData g = stream.Current;
                    sbRs.AppendLine($"name {g.Name} time ${g.Time}");
                }
            }

            Log($"ListGreetings {sbRs.ToString()}");

            channel.ShutdownAsync().Wait();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }


        static void Log(string s, params object[] args)
        {
            Console.WriteLine(string.Format(s, args));
        }

        static void Log(string s)
        {
            Console.WriteLine(s);
        }
    }
}
