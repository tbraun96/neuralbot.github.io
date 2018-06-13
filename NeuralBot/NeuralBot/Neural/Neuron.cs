using NeuralBot.Neural.Networks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NeuralBot.Neural
{
    [XmlInclude(typeof(NeuralBot.Neural.Networks.BackPropagationNetwork.BpNeuron))]
    [Serializable]
    public abstract class Neuron
    {
        abstract public Task<double> Fire(List<double> inputs, NeuralBot.Neural.Networks.NetworkState State);
        abstract public Vector2 GetLocation();
    }
}
