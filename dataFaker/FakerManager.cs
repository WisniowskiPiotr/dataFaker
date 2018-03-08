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
        int maxSearches = 500;
        double variation = 0.4;

        Type FakerType;
        int RequiredArgsNb;
        string[] RandomArgList;

        public FakerManager(Type fakerType,int requiredArgsNb, string[] randomArgList)
        {
            FakerType = fakerType;
            RequiredArgsNb = requiredArgsNb;
            RandomArgList = randomArgList;
        }

        public void StartFakingData()
        {
            while (true)
            {
                string[] args = GenerateRandomArgs();
                var faker = Activator.CreateInstance(FakerType, args);

                Console.Write("Start Faking \n");
                Task task = ((IFakerInterface)faker).SendQueryAsync();
                task.Wait();
                Random rand = new Random();
                int maxSleep = 82800 / GetNumberOfFakes();
                int sleep = rand.Next(0, maxSleep);
                Console.Write("Wait " + sleep + " s");
                Thread.Sleep(1000 * sleep);
            }
        }

        private string[] GenerateRandomArgs()
        {
            List<string> args = new List<string>();
            for (int j = 0; j < RequiredArgsNb; j++)
            {
                string arg;
                do
                {
                    arg = RandomArgList[(new Random()).Next(0, RandomArgList.Length - 1)];
                } while (args.Contains(arg));
                args.Add(arg);
            }
            return args.ToArray();
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
