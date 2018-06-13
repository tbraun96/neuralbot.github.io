using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralBot.Neural
{
    [Serializable]
    class BrainFireException : System.ApplicationException
    {

        public BrainFireException(string message)
        {
            Console.WriteLine("[Neural.BrainFireException] " + message);
        }

    }
}
