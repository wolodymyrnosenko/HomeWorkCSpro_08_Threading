using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HairSalon
{
    public class HairSalon
    {
        private readonly int waitingChairs;
        private readonly Queue<int> queue = new Queue<int>();
        private readonly object lockObj = new object();
        private readonly SemaphoreSlim customerReady = new SemaphoreSlim(0);//signal: at least one customer is ready
        private readonly SemaphoreSlim hairdresserReady = new SemaphoreSlim(0);//signal: hairdresser is ready for a customer

        public HairSalon(int waitingChairs)
        {
            this.waitingChairs = waitingChairs;
        }

        public bool CustomerArrives(int id)
        {
            bool joined = false;

            lock (lockObj)
            {
                if (queue.Count < waitingChairs)
                {
                    queue.Enqueue(id);
                    joined = true;
                    Console.WriteLine($"Відвідувач {id} сів у чергу. Очікують: {queue.Count}");
                    customerReady.Release();//notify hairdresser
                }
                else
                {
                    Console.WriteLine($"Відвідувач {id} пішов, відсутні місця.");
                }
            }

            if (joined)
            {
                hairdresserReady.Wait();//wait until hairdresser takes this customer
            }

            return joined;
        }

        public void HairdresserWork(CancellationToken token)
        {
            while (true)
            {
                try
                {
                    customerReady.Wait(token);//wait for customer or cancellation
                }
                catch (OperationCanceledException)
                {
                    lock (lockObj)
                    {
                        if (queue.Count == 0)
                            break;//stop if no more customers
                    }
                    continue;//still serve remaining customers
                }

                int clientId;
                lock (lockObj)
                {
                    if (queue.Count == 0)
                        continue;

                    clientId = queue.Dequeue();
                }

                Console.WriteLine($"Перукар почав стригти клієнта {clientId}");
                hairdresserReady.Release();//signal the customer it's their turn

                Thread.Sleep(2000);//simulate haircut time
                Console.WriteLine($"Перукар закінчив стрижку клієнта {clientId}");
            }
            
            Console.WriteLine("Салон зачинено. Перукар завершив роботу.");
        }
    }
}
