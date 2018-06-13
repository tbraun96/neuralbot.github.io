using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralBot.Neural
{
    [Serializable]
    public class NeuralFunctions
    {

        private static Random Rand = new Random();

        public static double Gaussian(double x, double sigma)
        {
            return Math.Exp(-1 * ((x * x)/(2 * (sigma * sigma))));
        }

        public static double Sigmoid(double x)
        {
            return 1 / (1 + Math.Exp(-x));
        }


        public static double SimpleSquash(double x)
        {
            return 1 / (.25 + Math.Abs(x));
        }

        public static double TanSigmoid(double x)
        {
            if (x < -20.0) return 0.0;
            else if (x > 20.0) return 1.0;
            return (Math.Exp(x) - Math.Exp(-x)) / (Math.Exp(x) + Math.Exp(-x));
        }

        public static int Factorial(int Input)
        {
            int Value = 1;
            for (int i = Input; i > 0; i--)
            {
                Value *= i;
            }
            return Value;
        }

        //t = training rate
        //DeltaE = final - initial of Error calculation
        private static double ComputeNewTrainingRate(double t, double DeltaE)
        {
            double DeltaT = 1 / (1 - (t * DeltaE));
            DeltaT = 1 - DeltaT;
            return DeltaT;
            //return t;
        }

        //z = time (sec)
        //t = current training rate
        //k = iteration count

        private static double LastDelta = Double.NaN;

        public static double ComputeTimeDependantTrainingRate(double DeltaE, double z, double t, double k)
        {
            Program.WriteLine("DeltaE: " + DeltaE);
            z = z / 1000; //convert msec to seconds
            double DeltaT = ComputeNewTrainingRate(t, DeltaE);
            Program.WriteLine("Delta (non time depend): " + DeltaT);
            DeltaT += Math.Pow(z,1) * DeltaE;
            Program.WriteLine("Delta (time depend, non scaled): " + DeltaT);
            DeltaT /= Math.Exp((k* k)/(1000*z));
            Program.WriteLine("Delta (time depend final): " + DeltaT);
            if (Double.IsNaN(LastDelta))
            {
                LastDelta = DeltaT;
            }
            else
            {
                if (DeltaT - LastDelta > 0)
                {
                    //positive slope
                    Program.WriteLine("Positive Slope (" + (DeltaT - LastDelta) + ")");
                    LastDelta = DeltaT;
                    return -DeltaT;
                }
                else
                {
                    Program.WriteLine("Negative Slope (" + (DeltaT - LastDelta) + ")");
                    LastDelta = DeltaT;
                    return 2 * DeltaT;
                }
            }
            return -DeltaT;
        }

        public static double ComputeError(double[] Inputs, double[] ValuesExpected)
        {
            if (Inputs.Length != ValuesExpected.Length)
            {
                throw new BrainFireException("NeuralFunctions.ComputeError requires the number of Inputs equal to the number of Expected values");
            }
            double value = 0.0;
            for (int i = 0; i < Inputs.Length; i++)
            {
                Program.WriteLine(Inputs[i] + "," + ValuesExpected[i]);
                value += (Inputs[i] - ValuesExpected[i]) * (Inputs[i] - ValuesExpected[i]);
            }
            return (value / Inputs.Length);
        }

        public static double NextDouble(double minimum, double maximum)
        {
            return Rand.NextDouble() * (maximum - minimum) + minimum;
        }

    }
}
