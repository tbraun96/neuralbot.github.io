using NeuralBot.Neural.Networks;
using NeuralBot.Window;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace NeuralBot.Neural
{
    [Serializable]
    public class Brain
    {
        private ArtificialNetwork Network;
        public Brain(ArtificialNetwork Network)
        {
            this.Network = Network;
        }

        public Brain() { }

        public void Train(List<double> Inputs, List<double> ExpectedOutputs)
        {
            Task task = Task.Factory.StartNew(() => this.Network.Train(Inputs, ExpectedOutputs));
            while (!task.IsCompleted)
            {
                    Thread.Sleep(10);
            }
        }

        public async Task<List<double>> Think(List<double> Inputs)
        {
            return await this.Network.Think(Inputs);
        }

        public ArtificialNetwork GetNetwork()
        {
            return this.Network;
        }

        public void SaveNetwork(string Filename, NetworkType Type)
        {
            if (Type == NetworkType.BACK_PROPAGATION)
            {

                /**NetworkLoadSaveData Nlsd = new NetworkLoadSaveData(((BackPropagationNetwork)this.Network).Weights);
                Nlsd.ConstructBiasField(((BackPropagationNetwork)this.Network).Neurons);
                XmlSerializer xmlSel = new XmlSerializer(typeof(NetworkLoadSaveData));
                using (TextWriter txtStream = new StreamWriter(Filename))
                    xmlSel.Serialize(txtStream, Nlsd);*/
                XmlSerializer xmlSel = new XmlSerializer(typeof(BackPropagationNetwork));
                using (TextWriter txtStream = new StreamWriter(Filename))
                    xmlSel.Serialize(txtStream, this.Network);
            }
        }

        public static Brain LoadNetwork(string Filename)
        {
            XmlSerializer xmlSel = new XmlSerializer(typeof(BackPropagationNetwork));
            using (TextReader txtStream = new StreamReader(Filename))
            {
                //return new Brain(BackPropagationNetwork.ConstructFromNlsd((NetworkLoadSaveData) xmlSel.Deserialize(txtStream)));
                Brain ai = new Brain((BackPropagationNetwork)xmlSel.Deserialize(txtStream));
                for (int x = 0; x < ((BackPropagationNetwork)ai.Network).Neurons.Count; x++)
                {
                    for (int y = 0; y < ((BackPropagationNetwork)ai.Network).Neurons[x].Count; y++)
                    {
                        ((BackPropagationNetwork)ai.Network).Neurons[x][y].AINetwork = ai.Network;
                    }
                }
                return ai;
            }
        }
    }

}

