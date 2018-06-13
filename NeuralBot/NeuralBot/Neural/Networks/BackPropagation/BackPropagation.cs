using NeuralBot.Optics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Serialization;

namespace NeuralBot.Neural.Networks
{
    [Serializable]
    public class BackPropagationNetwork : ArtificialNetwork
    {
        public int HiddenLayers;
        public int NeuronsPerLayer;
        public double LearningRate;
        public double Momentum;
        public List<List<double>> Weights = new List<List<double>>();
        public List<List<BpNeuron>> Neurons = new List<List<BpNeuron>>();
        public List<double> ExpectedOutputs;
        public bool TrainingMode = false;

        

        public BackPropagationNetwork(){
            
        }

        public BackPropagationNetwork(int Layers, int NeuronsPerLayer, double LearningRate, List<List<double>> Weights, double Momentum)
        {
            this.HiddenLayers = Layers;
            this.NeuronsPerLayer = NeuronsPerLayer;
            this.LearningRate = LearningRate;
            this.Momentum = Momentum;
        }

        /**public static BackPropagationNetwork ConstructFromNlsd(NetworkLoadSaveData Data)
        {
            BackPropagationNetwork Network = new BackPropagationNetwork();

            foreach (List<double> Column in Data.Weights)
            {
                List<double> ColumnWeights = new List<double>();
                foreach (double Value in Column)
                {
                    ColumnWeights.Add(Value);
                }
                Network.Weights.Add(ColumnWeights);
            }
            for (int x = 0; x < Data.Bias.Count; x++)
            {
                List<BpNeuron> ColumnNeurons = new List<BpNeuron>();
                for (int y = 0; y < Data.Bias[x].Count; y++)
                {
                    BpNeuron Neuron = new BpNeuron();
                    Neuron.Location = new Vector2(x == Data.Bias.Count -1 ? 0 : x + 1, y);
                    Neuron.Bias = Data.Bias[x][y];
                    Neuron.AINetwork = Network;
                    ColumnNeurons.Add(Neuron);
                }
                Network.Neurons.Add(ColumnNeurons);
            }
            return Network;

        }*/

        private List<double> Normalize(List<double> Input)
        {
            List<double> NormalizedValues = new List<double>();

            double Smallest = Double.MaxValue;
            double Largest = Double.MinValue;
            for (int i = 0; i < Input.Count; i++)
            {
                if (Input[i] < Smallest)
                {
                    Smallest = Input[i];
                }
                if (Input[i] > Largest)
                {
                    Largest = Input[i];
                }
            }

            for (int i = 0; i < Input.Count; i++)
            {
                NormalizedValues.Add((Input[i] - Smallest)/(Largest - Smallest));
                Console.WriteLine(Input[i] + " => " + NormalizedValues[i]);
            }
            return NormalizedValues;
        }

        public async override Task Train(List<double> Inputs, List<double> ExpectedOutputs)
        {
            this.TrainingMode = true;
            NetworkState State = NetworkState.FORWARD_PROPAGATION;
            Console.WriteLine("Setting up network...");
            double LastErrorRate = 0;
            //Inputs = Normalize(Inputs);
            //Console.WriteLine("Done normalizing outputs");
            if (this.Weights.Count == 0 || this.Neurons.Count == 0)
            {
                SetupNetwork(Inputs.Count, ExpectedOutputs.Count, Inputs);
            }
            Program.WriteLine("SizeOf Inputs = " + Inputs.Count + ", SizeOf ExpectedOutputs: " + ExpectedOutputs.Count);
            Console.SetOut(Program.ConsoleOut);
            Console.WriteLine();
            Console.WriteLine("|Initialized Weights size = | " + Weights[0].Count  + "," + Weights[1].Count);
            this.ExpectedOutputs = ExpectedOutputs;
            bool TrainingIsComplete = false;
            List<double> TempOutputs = new List<double>();
            Stopwatch watch = new Stopwatch();
            for (int Iterations = 0; Iterations < 1000 || TrainingIsComplete; Iterations++)
            {
                if (State == NetworkState.FORWARD_PROPAGATION)
                {
                    for (int x = 0; x <= this.HiddenLayers; x++)
                    {
                        //propagate to output
                        if (x == HiddenLayers)
                        {
                            Program.WriteLine("Propagating from final hidden layer to output layer");
                            List<Task<double>> Calcs = new List<Task<double>>();
                            for (int i = 0; i < ExpectedOutputs.Count; i++)
                            {
                                Calcs.Add(this.Neurons[x][i].Fire(TempOutputs, State));
                            }
                            TempOutputs.Clear();
                            for (int i = 0; i < ExpectedOutputs.Count; i++)
                            {
                                TempOutputs.Insert(i, await Calcs[i]);
                            }

                            //calculate error
                            double Error = NeuralFunctions.ComputeError(TempOutputs.ToArray(), ExpectedOutputs.ToArray());
                            Console.SetOut(Program.ConsoleOut);
                            Console.WriteLine("****************************Iteration " + Iterations + " Error: " + Error + "****************************");
                            if (Error == LastErrorRate)
                            {
                                throw new BrainFireException("The last few epochs have flatlined. Restarting the network should fix the problem");
                                Program.WriteLine("Retrying");
                                this.Weights = null;
                                this.Neurons = null;
                                await this.Train(Inputs, ExpectedOutputs);
                                this.LearningRate = .1;
                                return;
                            }
                            if (this.MseSeries != null)
                            {
                                DataPoint p = new DataPoint(ImageAI.CURRENT_EPOCH_VIS_POS++, Error);
                                if (Error > 0 && Error < 1000)
                                {
                                    this.MseSeries.Points.Add(p);
                                }
                                
                               
                            }
                            //Refresh Training rate
                            watch.Stop();
                            if (Iterations > 0)
                            {
                                Program.WriteLine("time elapsed: " + watch.Elapsed.TotalMilliseconds);
                                //double DeltaT = NeuralFunctions.ComputeTimeDependantTrainingRate((Error - LastErrorRate), watch.Elapsed.TotalMilliseconds, this.LearningRate, Iterations);
                                if (this.OtherSeries != null)
                                {
                                    //DataPoint p = new DataPoint(Iterations, DeltaT);
                                    //this.OtherSeries.Points.Add(p);
                                }
                                //this.LearningRate += DeltaT;
                                //Program.WriteLine("New training rate: " + this.LearningRate);
                            }
                            watch.Reset();
                            watch.Start();
                            LastErrorRate = Error;
                            Console.SetOut(Program.Debug);
                            if (Double.IsNaN(Error))
                            {
                                throw new BrainFireException("Invalid Error Rate Calculated");
                            }

                            

                            if (Error < .01)
                            {
                                //Trained successfully, return to exit
                                DataPoint pz = new DataPoint(ImageAI.CURRENT_CYCLE++, Iterations);
                                this.OtherSeries.Points.Add(pz);
                                return;
                            }
                            //Set up TempOutputs for BackPropagationNetwork (Give DeltaK values)
                            TempOutputs.Clear();
                            for (int i = 0; i < ExpectedOutputs.Count; i++)
                            {
                                TempOutputs.Add(this.Neurons[x][i].DeltaKOutput);
                            }
                            State = NetworkState.BACK_PROPAGATION;
                            Program.WriteLine("Setting STATE TO BackProp");
                            break;
                        }
                        //propagate input to first neurons
                        else if (x == 0)
                        {
                            Program.WriteLine("Propagating from input layer to initial hidden layer");
                            List<Task<double>> Calcs = new List<Task<double>>();
                            for (int i = 0; i < this.NeuronsPerLayer; i++)
                            {
                                Calcs.Add(this.Neurons[x][i].Fire(Inputs, State));
                            }
                            TempOutputs.Clear();
                            for (int i = 0; i < this.NeuronsPerLayer; i++)
                            {
                                TempOutputs.Insert(i, await Calcs[i]);
                            }
                        }
                            //normal hidden layer
                        else
                        {
                            Program.WriteLine("Propagating from hidden layer to hidden layer");
                            List<Task<double>> Calcs = new List<Task<double>>();
                            for (int i = 0; i < this.NeuronsPerLayer; i++)
                            {
                                Calcs.Add(this.Neurons[x][i].Fire(TempOutputs, State));
                            }
                            TempOutputs.Clear();
                            for (int i = 0; i < this.NeuronsPerLayer; i++)
                            {
                                TempOutputs.Insert(i, await Calcs[i]);
                            }
                        }
                    }
                    State = NetworkState.BACK_PROPAGATION;
                }

                if (State == NetworkState.BACK_PROPAGATION)
                {
                    for (int x = this.HiddenLayers; x >= 0; x--)
                    {

                        if (x >= 1)
                        {
                            Program.WriteLine("BackPropagating from Layer to Layer");
                            List<Task<double>> Calcs = new List<Task<double>>();
                            for (int i = 0; i < this.NeuronsPerLayer; i++)
                            {
                                Calcs.Add(this.Neurons[x - 1][i].Fire(TempOutputs, State));
                            }
                            TempOutputs.Clear();
                            for (int i = 0; i < this.NeuronsPerLayer; i++)
                            {
                                TempOutputs.Insert(i, await Calcs[i]);
                            }
                        }
                            //input neurons
                        else
                        {
                            Program.WriteLine("BackPropagating from Layer to Input");
                            List<Task<double>> Calcs = new List<Task<double>>();
                            for (int i = 0; i < Inputs.Count; i++)
                            {
                                Calcs.Add(this.Neurons[this.Neurons.Count - 1][i].Fire(TempOutputs, State));
                            }
                            TempOutputs.Clear();
                            for (int i = 0; i < Inputs.Count; i++)
                            {
                                await Calcs[i];
                            }
                        }
                    }
                    
                }
                State = NetworkState.FORWARD_PROPAGATION;

            }
        }
        public const double MIN = -1; //-.6 for squash
        public const double MAX = 1; //.6 for squash
        public void SetupNetwork(int NumberOfInputs, int NumberOfOutputs, List<double> Inputs)
        {
            if (this.Weights == null)
            {
                this.Weights = new List<List<double>>();
            }
            if (this.Neurons == null)
            {
                this.Neurons = new List<List<BpNeuron>>();
            }
            for (int i = 0; i <= this.HiddenLayers + 1; i++)
            {
                //input layer
                if (i == 0)
                {
                    // add weights
                        List<double> Weights = new List<double>();
                        Program.WriteLine("# of inputs: " + NumberOfInputs);
                        Program.WriteLine("# of NPL" + this.NeuronsPerLayer);
                        for (int x = 0; x < NumberOfInputs; x++)
                        {
                           
                            for (int y = 0; y < this.NeuronsPerLayer; y++)
                            {
                                
                                Weights.Add(NeuralFunctions.NextDouble(MIN,MAX));
                            }
                        }
                        Program.WriteLine("SizeOf Weights: " + Weights.Count);
                        this.Weights.Add(Weights);
                }

                //output layer
                else if (i == this.HiddenLayers + 1)
                {
                    //Add output neurons to final layer
                    List<BpNeuron> NeuronLayer = new List<BpNeuron>();
                    for (int y = 0; y < NumberOfOutputs; y++)
                    {
                        NeuronLayer.Add(new BpNeuron(i, y, NeuronClass.OUTPUT, this));
                    }
                    this.Neurons.Add(NeuronLayer);
                }

                //hidden layer
                else
                {
                    //Add neurons to layer
                    List<BpNeuron> NeuronLayer = new List<BpNeuron>();
                    for (int y = 0; y < this.NeuronsPerLayer; y++)
                    {
                        NeuronLayer.Add(new BpNeuron(i, y, NeuronClass.HIDDEN, this));
                    }
                    this.Neurons.Add(NeuronLayer);

                    //add weights inbound to next layer
                        if (i < this.HiddenLayers)
                        {
                            List<double> Weights = new List<double>();
                            for (int x = 0; x < this.NeuronsPerLayer; x++)
                            {
                                for (int y = 0; y < this.NeuronsPerLayer; y++)
                                {
                                    Weights.Add(NeuralFunctions.NextDouble(MIN, MAX));
                                }
                            }
                            this.Weights.Add(Weights);
                        }
                    else
                    {
                            List<double> Weights = new List<double>();
                            for (int x = 0; x < this.NeuronsPerLayer; x++)
                            {
                                for (int y = 0; y < NumberOfOutputs; y++)
                                {
                                    Weights.Add(NeuralFunctions.NextDouble(MIN, MAX));
                                }
                            }
                            this.Weights.Add(Weights);
                    }
                }

            }

            //finally, add input neurons to the END of the array
            List<BpNeuron> InputNeurons = new List<BpNeuron>();
            for (int i = 0; i < NumberOfInputs; i++)
            {
                InputNeurons.Add(new BpNeuron(0, i, NeuronClass.INPUT, this));
                InputNeurons[i].InputNeuronInput = Inputs[i]; 
            }
            this.Neurons.Add(InputNeurons);
        }

        public async override Task<List<double>> Think(List<double> Inputs)
        {
            this.TrainingMode = false;
            this.HiddenLayers = this.Neurons.Count - 2;
            NetworkState State = NetworkState.FORWARD_PROPAGATION;
            Program.WriteLine("Number of Inputs:" + Inputs.Count);
            Program.WriteLine("Neurons Per Layer(1): " + NeuronsPerLayer);
            Program.WriteLine("Neurons Per Layer(2): " + this.NeuronsPerLayer);
            List<double> TempOutputs = new List<double>();
          
                    for (int x = 0; x <= this.HiddenLayers; x++)
                    {
                        if (x == HiddenLayers)
                        {
                            Program.WriteLine("Propagating from final hidden layer to output layer, x = " + x);
                            List<Task<double>> Calcs = new List<Task<double>>();
                            for (int i = 0; i < this.ExpectedOutputs.Count; i++)
                            {
                                Calcs.Add(this.Neurons[x][i].Fire(TempOutputs, State));
                            }
                            TempOutputs.Clear();
                            for (int i = 0; i < this.ExpectedOutputs.Count; i++)
                            {
                                TempOutputs.Insert(i, await Calcs[i]);
                            }
                            return TempOutputs;
                        }
                        //propagate input to first neurons
                        else if (x == 0)
                        {
                            Program.WriteLine("Propagating from input layer to initial hidden layer");
                            List<Task<double>> Calcs = new List<Task<double>>();
                            for (int i = 0; i < this.NeuronsPerLayer; i++)
                            {
                                Calcs.Add(this.Neurons[x][i].Fire(Inputs, State));
                            }
                            TempOutputs.Clear();
                            for (int i = 0; i < this.NeuronsPerLayer; i++)
                            {
                                TempOutputs.Insert(i, await Calcs[i]);
                            }
                        }
                        //normal hidden layer
                        else
                        {
                            Program.WriteLine("Propagating from hidden layer to hidden layer");
                            List<Task<double>> Calcs = new List<Task<double>>();
                            for (int i = 0; i < this.NeuronsPerLayer; i++)
                            {
                                Calcs.Add(this.Neurons[x][i].Fire(TempOutputs, State));
                            }
                            TempOutputs.Clear();
                            for (int i = 0; i < this.NeuronsPerLayer; i++)
                            {
                                TempOutputs.Insert(i, await Calcs[i]);
                            }
                        }
                    }
                
                return null;
            }
       

        [Serializable]
        public sealed class BpNeuron : Neural.Neuron
        {

            public Vector2 Location;
            public double Bias;
            public double Output;
            public double DeltaKOutput;
            protected double DeltaBiasOutput;
            protected NeuronClass NeuronType;
            private double DeltaW;
            public double InputNeuronInput;
            [XmlIgnore]
            public ArtificialNetwork AINetwork;

            public BpNeuron() { }

            public BpNeuron(int Layer, int NeuronNumber, NeuronClass NeuronType, ArtificialNetwork AINetwork)
            {
                this.Location = new Vector2(Layer, NeuronNumber);
                this.NeuronType = NeuronType;
                this.Bias = NeuralFunctions.NextDouble(MIN, MAX);
                this.AINetwork = AINetwork;
            }

            private List<double> GetWeights(int NumberOfInputs, NetworkState State)
            {
                int NeuronsPerLayer = ((BackPropagationNetwork) this.AINetwork).NeuronsPerLayer;
                int NumberOfOutputs = ((BackPropagationNetwork)this.AINetwork).ExpectedOutputs.Count;
                if (State == NetworkState.FORWARD_PROPAGATION)
                {
                    List<double> WeightsInbound = new List<double>();
                    //Get weights from input to first hidden layer (1)
                    if (this.Location.X == 1)
                    {
                        for (int i = 0; i < NumberOfInputs; i++)
                        {
                                WeightsInbound.Add(((BackPropagationNetwork) this.AINetwork).Weights[0][this.Location.Y + (NeuronsPerLayer * i)]);
                        }
                    }
                    //Get weights from hidden to hidden, or hidden to output
                    else
                    {
                        for (int i = 0; i < ((BackPropagationNetwork)this.AINetwork).NeuronsPerLayer; i++)
                        {
                            if (((BackPropagationNetwork)this.AINetwork).ExpectedOutputs.Count > 1)
                            {
                                WeightsInbound.Add(((BackPropagationNetwork)this.AINetwork).Weights[this.Location.X - 1][this.Location.Y + (NumberOfOutputs * i)]);
                            }
                                //one neuron only
                            else
                            {
                                WeightsInbound.Add(((BackPropagationNetwork)this.AINetwork).Weights[this.Location.X - 1][i]);

                            }
                        }
                    }
                    return WeightsInbound;
                }

                else if (State == NetworkState.BACK_PROPAGATION)
                {
                    // last input layer
                    List<double> WeightsInbound = new List<double>();
                    if (this.Location.X == ((BackPropagationNetwork)this.AINetwork).HiddenLayers)
                    {
                        //number of inputs is equal to the number of outputs inbound to this layer
                        for (int i = 0; i < NumberOfInputs; i++)
                        {
                            WeightsInbound.Add(((BackPropagationNetwork)this.AINetwork).Weights[this.Location.X][i + (this.Location.Y * NumberOfInputs)]);
                        }
                    }

                        //From hidden to hidden, or hidden to initial
                    else
                    {
                        if (this.Location.X > 0)
                        {
                            for (int i = 0; i < NumberOfInputs; i++)
                            {
                                WeightsInbound.Add(((BackPropagationNetwork)this.AINetwork).Weights[this.Location.X - 1][i + (this.Location.Y * NumberOfInputs)]);
                            }
                        }
                            //input layer
                        else
                        {
                            for (int i = 0; i < NumberOfInputs; i++)
                            {
                                WeightsInbound.Add(((BackPropagationNetwork)this.AINetwork).Weights[0][i + (this.Location.Y * NumberOfInputs)]);
                            }
                        }
                    }
                    return WeightsInbound;
                }
                return null;
            }

            public unsafe override async Task<double> Fire(List<double> Inputsf, NetworkState State)
            {
                if (!((BackPropagationNetwork)this.AINetwork).TrainingMode)
                {
                    if (((BackPropagationNetwork)this.AINetwork).HiddenLayers + 1 == this.GetLocation().X)
                    {
                        Program.WriteLine("NeuronType: " + this.ToString());
                        Program.WriteLine("Editing neuron class from HIDDEN to OUTPUT");
                        this.NeuronType = NeuronClass.OUTPUT;
                    }
                }
                //Console.SetOut(Program.Debug);
                //Console.WriteLine("Firing Neuron " + this.ToString());
                List<double> Weights = GetWeights(Inputsf.Count, State);
                //Console.WriteLine("Received Weights, count = " + Weights.Count);
                List<double> Inputs = Inputsf.ToList();
                if (Inputs.Count != Weights.Count)
                {
                    throw new BrainFireException("The number of inputs != the number of weights. Aborting");
                }

                if (State == NetworkState.FORWARD_PROPAGATION)
                {
                    if (NeuronType == NeuronClass.HIDDEN)
                    {
                        //simply multiply and sum. Then run through Sigmoid function
                        double value = 0.0;
                        for (int i = 0; i < Inputs.Count; i++)
                        {
                            value += Inputs[i] * Weights[i];
                        }
                        //Console.WriteLine("Total Sum: " + value);
                        value += this.Bias;
                        //Console.WriteLine("Total Sum (Bias) : " + value);
                        value = NeuralFunctions.SimpleSquash(value);
                        //Console.WriteLine("Sigmoid: " + value);
                        this.Output = value;
                    }
                    else if (NeuronType == NeuronClass.OUTPUT)
                    {
                        //multiply and sum, then calculate this neurons error. Then, update the bias term
                        double value = 0.0;
                        for (int i = 0; i < Inputs.Count; i++)
                        {
                            value += Inputs[i] * Weights[i];
                        }
                        value += this.Bias;
                        //Program.WriteLine("Total Sum: " + value);
                        Console.SetOut(Program.ConsoleOut);
                        this.Output = NeuralFunctions.SimpleSquash(value);
                        Console.SetOut(Program.Debug);
                        //NeuralFunctions.TanSigmoid(value, &value);
                        if (((BackPropagationNetwork)this.AINetwork).TrainingMode)
                        {
                            //Console.WriteLine("Sigmoid: " + value);
                            double T = ((BackPropagationNetwork)this.AINetwork).ExpectedOutputs[this.GetLocation().Y];
                            this.DeltaKOutput = this.Output * (1 - this.Output) * (T - this.Output);
                            //Console.WriteLine("DeltaK: " + this.DeltaKOutput);
                            //Update bias
                            double DeltaBias = this.DeltaKOutput * ((BackPropagationNetwork)this.AINetwork).LearningRate;
                            //Console.WriteLine("Delta Bias: " + DeltaBias);
                            this.DeltaBiasOutput = DeltaBias;
                            this.Bias += this.DeltaBiasOutput;
                            //Console.WriteLine("New Bias: " + Bias);
                        }
                        //this.Output = this.DeltaKOutput;
                    }
                }
                else if (State == NetworkState.BACK_PROPAGATION)
                {
                    if (NeuronType == NeuronClass.HIDDEN)
                    {
                        //primary role of the neuron in this state is to update weights and biases
                        //required Inputs[] are DeltaK values from output neuron 
                        double DeltaX = 0.0;
                        for (int i = 0; i < Inputs.Count; i++)
                        {
                            //Console.WriteLine("Operation[+]: " + Inputs[i] + " x " + Weights[i]);
                            DeltaX += Weights[i] * Inputs[i];
                        }
                        //Console.WriteLine("Total Sum: " + DeltaX);
                        //get previous output
                        DeltaX = DeltaX * (1 - this.Output) * this.Output;
                        //Console.WriteLine("DeltaX: " + DeltaX);
                        double DeltaBias = DeltaX * ((BackPropagationNetwork)this.AINetwork).LearningRate;
                        //Console.WriteLine("Delta Bias: " + DeltaBias);
                        //Console.WriteLine("Old Bias: " + this.Bias);
                        if (this.DeltaW == null)
                        {
                            this.DeltaW = DeltaX * ((BackPropagationNetwork)this.AINetwork).LearningRate * this.Output;
                            for (int i = 0; i < Inputs.Count; i++)
                            {
                                ((BackPropagationNetwork)this.AINetwork).Weights[this.GetLocation().X][i + (this.Location.Y * Inputs.Count)] += this.DeltaW;
                            }
                        }
                        else
                        {
                            double DeltaWCur = DeltaX * ((BackPropagationNetwork)this.AINetwork).LearningRate * this.Output;
                            this.DeltaW = DeltaWCur + (((BackPropagationNetwork)this.AINetwork).Momentum * this.DeltaW); //Momentum term
                            for (int i = 0; i < Inputs.Count; i++)
                            {
                                ((BackPropagationNetwork)this.AINetwork).Weights[this.GetLocation().X][i + (this.Location.Y * Inputs.Count)] += this.DeltaW;
                            }
                        }
                        this.Bias += DeltaBias;
                        //Console.WriteLine("New Bias: " + this.Bias);
                        this.Output = DeltaX;
                    }
                    else if (NeuronType == NeuronClass.INPUT)
                    {
                        for (int i = 0; i < Inputs.Count; i++)
                        {
                            double WeightDelta = Inputs[i] * ((BackPropagationNetwork)this.AINetwork).LearningRate * this.InputNeuronInput;
                            //Console.WriteLine("Weight Before: " + ((BackPropagationNetwork)this.AINetwork).Weights[this.GetLocation().X][i + (this.Location.Y * Inputs.Count)]);
                            //Console.WriteLine("Weight Delta: " + WeightDelta);
                            ((BackPropagationNetwork)this.AINetwork).Weights[this.GetLocation().X][i + (this.Location.Y * Inputs.Count)] += WeightDelta;
                            //Console.WriteLine("New Weight: " + ((BackPropagationNetwork)this.AINetwork).Weights[this.GetLocation().X][i + (this.Location.Y * Inputs.Count)]);
                        }

                    }
                }
                Console.SetOut(Program.Debug);
                Console.WriteLine("Output: " + this.Output);
                return this.Output;
            }

            public override Vector2 GetLocation()
            {
                return this.Location;
            }

            public override string ToString()
            {
                return this.GetLocation().ToString() + " => TypeOf " + this.NeuronType;
            }

        }

        
        [Serializable]
        public enum NeuronClass
        {
            HIDDEN, OUTPUT, INPUT
        }

    }

    [Serializable]
    public enum NetworkState
    {
        FORWARD_PROPAGATION, BACK_PROPAGATION
    }

}
