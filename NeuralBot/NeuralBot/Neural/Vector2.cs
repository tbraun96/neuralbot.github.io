using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeuralBot.Neural.Networks
{
    [Serializable]
    public struct Vector2
    {

        public int X;
        public int Y;

        public Vector2(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public string ToString()
        {
            return "Vector2[" + X + "," + Y + "]";
        }

    }
}
