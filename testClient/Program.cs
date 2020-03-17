using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace testClient
{
    class Program
    {
        static readonly ServerKeyService serverKeyService = new ServerKeyService();
        static readonly SessionService sessionService = new SessionService();
        static readonly UserService userService;

        static readonly(string, Func<Task>) [] Menu = new(string, Func<Task>) []
        {
            ("1. Login", Login),
            ("2. Refresh", Refresh),
            ("3. Register", Register),
        };

        static Program()
        {
            userService = new UserService(sessionService);
        }

        static async Task Main(string[] args)
        {
            var serverPublicKey = await serverKeyService.GetServerPublicKey();
            await sessionService.ExchangeKeyWithServer(serverPublicKey);

            var requestCoordinator = new RequestCoordinator(sessionService);

            while (true)
            {
                foreach(var item in Menu)
                {
                    Console.WriteLine(item.Item1);
                }

                var result = int.Parse(Console.ReadLine());
                var action = Menu[result - 1].Item2;
                await action();
            }
        }

        static async Task Login()
        {
            var login = Console.ReadLine();
            var password = Console.ReadLine();
            try
            {
                await userService.Login(login, password);
                Console.WriteLine("Log in.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static async Task Refresh()
        {
            try
            {
                await userService.RefreshToken();
                Console.WriteLine("Refreshed.");
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static async Task Register()
        {
            var email = Console.ReadLine();
            var login = Console.ReadLine();
            var password = Console.ReadLine();
            try
            {
                await userService.Register(email, login, password);
                Console.WriteLine("Registered.");
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}