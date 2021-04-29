using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainScheduleModel
{
    public class Poisson
    {
        private readonly Random _rand;
        private readonly double L;

        public Poisson()
        {
        }

        public Poisson(double x)
        {
            L = Math.Exp(-x);
            _rand = new Random();
        }

        public int Next()
        {
            int K = 0;
            double P = 1;

            while (P > L)
            {
                K++;
                P *= _rand.NextDouble();
            }

            return K;
        }
    }
}
