using Greeting;
using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Greeting.GreeterSvc;

namespace GreetingAppServer
{
    public class Program
    {
        private const int PORT = 5001;

        static Task Main(string[] args)
        {
            Server svc = new Server
            {
                Services = { GreeterSvc.BindService(new GreeterSvcImpl()) },
                Ports = { new ServerPort("localhost", PORT, ServerCredentials.Insecure) }
            };

            svc.Start();

            Console.WriteLine($"Server listening on port {PORT}");
            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();
            return svc.ShutdownAsync();
        }
    }

    class GreeterSvcImpl : GreeterSvcBase
    {
        static ConcurrentBag<GreetingData> _bagColl = new ConcurrentBag<GreetingData>();

        public async override Task ListGreetings(DumbRequest request, IServerStreamWriter<GreetingData> responseStream, ServerCallContext context)
        {
            if (!_bagColl.IsEmpty)
            {
                foreach (var item in _bagColl)
                {
                    await responseStream.WriteAsync(item);
                }
            }
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            long epochTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var greeting = new GreetingData
            {
                Name = request.Name,
                Time = epochTime
            };
            _bagColl.Add(greeting);

            var reply = new HelloReply
            {
                Message = $"Hello back {request.Name} - My time is {epochTime}"
            };

            return Task.FromResult(reply);
        }
    }
}
