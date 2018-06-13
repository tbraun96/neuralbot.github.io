using NeuralBot.Neural.Networks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml;
using System.Xml.Serialization;

namespace NeuralBot.Neural
{
    [XmlInclude(typeof(BackPropagationNetwork))]
    [Serializable]
    public abstract class ArtificialNetwork
    {
        //For graphing
        [XmlIgnore]
        public Series MseSeries;
        [XmlIgnore]
        public Series OtherSeries;
        abstract public Task<List<double>> Think(List<double> Inputs);
        abstract public Task Train(List<double> inputs, List<double> ExpectedOutputs);
    }

    public enum NetworkType
    {
        BACK_PROPAGATION
    }

}
