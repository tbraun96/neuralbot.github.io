using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralBot.Neural
{
    [Serializable]
    public struct NetworkLoadSaveData
    {

        public List<List<double>> Weights;
        public List<List<double>> Bias;

        public NetworkLoadSaveData(List<List<double>> Weights)
        {
            this.Weights = Weights;
            this.Bias = new List<List<double>>();
        }

        public void ConstructBiasField(List<List<NeuralBot.Neural.Networks.BackPropagationNetwork.BpNeuron>> Neurons)
        {
            for (int x = 0; x < Neurons.Count; x++)
            {
                List<double> Column = new List<double>();
                for (int y = 0; y < Neurons[x].Count; y++)
                {
                    Column.Add(Neurons[x][y].Bias);
                }
                Bias.Add(Column);
            }
        }

    }
}
