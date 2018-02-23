using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace dataFaker
{
    public class Program
    {
        public static void Main()
        {
            string[] randomTowns = ICCFaker.Options;
            FakerManager iccFakerManager = new FakerManager(typeof(ICCFaker), 2, randomTowns);
            iccFakerManager.StartFakingData();
        }
    }
}