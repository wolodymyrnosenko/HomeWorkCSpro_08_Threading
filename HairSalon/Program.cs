using System.Text;

namespace HairSalon
{
    public class Program
    {
        //Thread-safe random generator (different instance per thread)
        private static readonly ThreadLocal<Random> LocalRnd = new ThreadLocal<Random>(() => new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId)));

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;

            var salon = new HairSalon(waitingChairs: 3);
            var cts = new CancellationTokenSource();

            //Start the hairdresser in a background task
            var hairdresserTask = Task.Run(() => salon.HairdresserWork(cts.Token));

            //Simulation parameters
            int totalCustomers = 20;// how many customers will arrive
            (int minMs, int maxMs) delay = (500, 10000);//random arrival delay range

            //Spawn customer tasks asynchronously
            var customerTasks = Enumerable.Range(1, totalCustomers).Select(async i =>
            {
                await Task.Delay(LocalRnd.Value!.Next(delay.minMs, delay.maxMs));
                salon.CustomerArrives(i);
            }).ToArray();

            //Wait until all customers have arrived and been processed
            Task.WaitAll(customerTasks);

            //Signal that no more customers will arrive
            cts.Cancel();

            //Wait for hairdresser to finish remaining work and exit
            hairdresserTask.Wait();
        }
    }
}
