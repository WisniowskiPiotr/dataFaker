using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dataFaker
{
    interface IFakerInterface
    {
        Task SendQueryAsync();
    }

    class FakerManager
    {
        DateTime startDate = new DateTime(2018, 02, 22);
        DateTime endDate = new DateTime(2018, 05, 30);
        int minSearches = 10;
        int maxSearches = 100;
        double variation = 0.4;
        
        object[] fakers;

        public FakerManager(Type fakerType,int requiredArgsNb, string[] randomArgList)
        {
            Random random = new Random();
            int fakersNb = GetNumberOfFakes();
            fakers = new object[fakersNb];
            for (int i = 0; i < fakersNb; i++)
            {
                List<string> args = new List<string>();
                for (int j = 0; j < requiredArgsNb; j++)
                {
                    string arg;
                    do
                    {
                        arg = randomArgList[random.Next(0, randomArgList.Length - 1)];
                    } while (args.Contains(arg));
                    args.Add(arg);
                }
                fakers[i]=Activator.CreateInstance(fakerType, args.ToArray());
            }
        }

        public void StartFakingData()
        {
            foreach (object faker in fakers)
            {
                Console.Write("Start Faking \n");
                Task task = ((IFakerInterface)faker).SendQueryAsync();
                task.Wait();
                Random rand = new Random();
                int maxSleep = 82800 / fakers.Length;
                int sleep = rand.Next(0, maxSleep);
                Console.Write("Wait " + sleep + " s");
                Thread.Sleep(1000* sleep);
            }
        }

        private int GetNumberOfFakes()
        {
            DateTime today = DateTime.Today;
            double portion;
            if (today < startDate)
                portion = 0.0;
            else if (today > endDate)
                portion = 1.0;
            else
                portion = (1.0 * (today.Ticks - startDate.Ticks)) / (1.0 * (endDate.Ticks - startDate.Ticks));
            return Convert.ToInt32( variation * (minSearches + portion * (maxSearches - minSearches)));
        }
    }
}
