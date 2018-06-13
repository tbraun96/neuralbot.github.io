using NeuralBot.Neural;
using NeuralBot.Neural.Networks;
using NeuralBot.Optics.Image;
using NeuralBot.Window;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace NeuralBot.Optics
{
    public class ImageAI
    {

        public const int NUMBER_OF_LAYERS = 1;
        public const double TRAINING_RATE = .01;
        public const double MOMENTUM = .9;
        public const int NUMBER_OF_OUTPUTS = 1;
        public const int RATIO_TOP = 2;
        public const int RATIO_BOTTOM = 3;

        public static Brain CreateAIFromImageFiles(string[] ImageFileNames, double[] ExpectedValues)
        {
            List<Bitmap> bmps = new List<Bitmap>();
            List<double> ExpectedOutputs = new List<double>();
            int i = 0;
            foreach (string file in ImageFileNames)
            {
                bmps.Add(new Bitmap(file));
                ExpectedOutputs.Add(ExpectedValues[i]);
                i++;
            }
            Program.WriteLine("Number of bmps: " + bmps.Count);

            double NEURONS_PER_LAYER_TOTAL = 0;
            int k = 0;
            foreach (Bitmap bmp in bmps.ToArray())
            {
                double NEURONS_PER_LAYER = 1;
                if (bmp.Width < 64 || bmp.Height < 64)
                {
                    NEURONS_PER_LAYER *= (bmp.Width * bmp.Height);
                    NEURONS_PER_LAYER *= (int)RATIO_TOP;
                    NEURONS_PER_LAYER /= (int) RATIO_BOTTOM;
                    NEURONS_PER_LAYER += NUMBER_OF_OUTPUTS;
                }
                else
                {
                    NEURONS_PER_LAYER *= (64 * 64);
                    NEURONS_PER_LAYER *= (int)RATIO_TOP;
                    NEURONS_PER_LAYER /= (int)RATIO_BOTTOM;
                    NEURONS_PER_LAYER += NUMBER_OF_OUTPUTS;
                }
                if (bmp.Width > 64 || bmp.Height > 64)
                {
                    bmps[k] = ImageUtilities.ResizeImage(bmp, new Size(64, 64));
                }
                NEURONS_PER_LAYER_TOTAL += NEURONS_PER_LAYER;
                k++;
            }
            
            Program.WriteLine(NEURONS_PER_LAYER_TOTAL + " = ImageAI's NPL");
            List<double> Inputs = new List<double>();
            foreach (Bitmap bmp in bmps.ToArray())
            {
                for (int y = 0; y < bmp.Height - 1; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        Color oc = bmp.GetPixel(x, y);
                        Inputs.Add(oc.GetHue() + oc.GetSaturation() + oc.GetBrightness());
                    }
                }
            }
            Thread.Sleep(2000);
            Brain brain = new Brain(new BackPropagationNetwork(NUMBER_OF_LAYERS, (int) NEURONS_PER_LAYER_TOTAL, TRAINING_RATE, null, MOMENTUM));
            brain.Train(Inputs, ExpectedOutputs);
            return brain;
        }


        public static Brain CreateAIFromImageFile(string ImageFileName, int ExpectedOutput)
        {
            Bitmap bmp = new Bitmap(ImageFileName);
            List<double> ExpectedOutputs = new List<double>();
            ExpectedOutputs.Add(ExpectedOutput);
            
            int NEURONS_PER_LAYER = 1;
            if (bmp.Width < 64 || bmp.Height < 64)
            {
                NEURONS_PER_LAYER *= (bmp.Width * bmp.Height);
                NEURONS_PER_LAYER *= (int)RATIO_TOP;
                NEURONS_PER_LAYER /= (int)RATIO_BOTTOM;
                NEURONS_PER_LAYER += NUMBER_OF_OUTPUTS;
            }
            else
            {
                NEURONS_PER_LAYER *= (64 * 64);
                NEURONS_PER_LAYER *= (int)RATIO_TOP;
                    NEURONS_PER_LAYER /= (int) RATIO_BOTTOM;
                NEURONS_PER_LAYER += NUMBER_OF_OUTPUTS;
            }
            if (bmp.Width > 64 || bmp.Height > 64)
            {
                bmp = ImageUtilities.ResizeImage(bmp, new Size(64, 64));
            }
            Program.WriteLine(NEURONS_PER_LAYER + " = ImagAI's NPL");
            List<double> Inputs = new List<double>();
            for (int y = 0; y < bmp.Height - 1; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color oc = bmp.GetPixel(x, y);
                    Inputs.Add(oc.GetHue() + oc.GetSaturation() + oc.GetBrightness());
                }
            }
            Brain brain = new Brain(new BackPropagationNetwork(NUMBER_OF_LAYERS, NEURONS_PER_LAYER, TRAINING_RATE, null, MOMENTUM));
            Program.window.HookToBrain(brain);
            brain.Train(Inputs, ExpectedOutputs);
            return brain;
        }

        public static bool ProcessImage(Brain brain, Bitmap bmp)
        {
            bmp = ImageUtilities.ResizeImage(bmp, new Size(64, 64));
            List<double> Inputs = new List<double>();
            for (int y = 0; y < bmp.Height - 1; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color oc = bmp.GetPixel(x, y);
                    Inputs.Add(oc.GetHue() + oc.GetSaturation() + oc.GetBrightness());
                }
            }
            int NeuronsPerLayer = Inputs.Count * ((int)RATIO_TOP / RATIO_BOTTOM);
            NeuronsPerLayer += 1;
            List<double> Values = brain.Think(Inputs).Result;

            if (Values != null)
            {
                double Value = Values[0];
                Console.WriteLine("Value: " + Value);
                return Value > .5 ? true : false;
            }
            else
            {
                throw new BrainFireException("Null return value.");
            }
            return false;
        }

        /*
         * Essentially, this method scans through all the files in a directory and
         * Classifies them
         * 
         **/

        public const int POSITIVE = 1;
        public const int NEGATIVE = 0;
        public static int CURRENT_EPOCH_VIS_POS = 0;
        public static int CURRENT_CYCLE = 0;
        public static Brain CreateCompoundAI(string PositiveDirectory, string NegativeDirectory)
        {
            string[] Pos = Directory.GetFiles(PositiveDirectory);
            string[] Neg = Directory.GetFiles(NegativeDirectory);
            Brain brain = null;
            for (int k = 0; k < 100; k++)
            {
                for (int i = 0; i < Pos.Length; i++)
                {
                    Program.WriteLine("Training Image " + Pos[i]);
                    if (i == 0)
                    {
                        brain = CreateAIFromImageFile(Pos[i], POSITIVE);
                    }
                    else
                    {
                        TrainBrain(brain, Pos[i], POSITIVE);
                    }
                    CURRENT_EPOCH_VIS_POS = 0;
                    brain.GetNetwork().MseSeries.Points.Clear();
                }
                
            }
            Program.WriteLine("***Training NEGATIVES.***");
            for (int i = 0; i < Neg.Length; i++)
            {
                    TrainBrain(brain, Neg[i], NEGATIVE);
            }

            return brain;
        }


        public static void TrainBrain(Brain brain, string Filename, int ExpectedOutput)
        {
            Bitmap bmp = new Bitmap(Filename);
            if (bmp.Width > 64 || bmp.Height > 64)
            {
                bmp = ImageUtilities.ResizeImage(bmp, new Size(64, 64));
            }
            List<double> Inputs = new List<double>();
            double[] ExpectedOutputs = new double[1];
            ExpectedOutputs[0] = ExpectedOutput;
            for (int y = 0; y < bmp.Height - 1; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color oc = bmp.GetPixel(x, y);
                    Inputs.Add(oc.GetHue() + oc.GetSaturation() + oc.GetBrightness());
                }
            }
            brain.Train(Inputs.ToList(),ExpectedOutputs.ToList());
        }

        public static bool ProcessImageFromFile(Brain brain, string Filename)
        {
            Bitmap bmp = new Bitmap(Filename);
            bmp = ImageUtilities.ResizeImage(bmp, new Size(64, 64));
            List<double> Inputs = new List<double>();
            for (int y = 0; y < bmp.Height - 1; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color oc = bmp.GetPixel(x, y);
                    Inputs.Add(oc.GetHue() + oc.GetSaturation() + oc.GetBrightness());
                }
            }
            double NeuronsPerLayer = Inputs.Count;
            NeuronsPerLayer *= RATIO_TOP;
            NeuronsPerLayer /= RATIO_BOTTOM;
            NeuronsPerLayer += 1;
            List<double> Values = brain.Think(Inputs).Result;

            if (Values != null)
            {
                double Value = Values[0];
                for (int i = 0; i < Values.Count; i++)
                {
                    Program.WriteLine("FINAL RESULT[" + i  + "]: " + Values[i]);
                }
                return Value > .5 ? true : false;
            }
            else
            {
                throw new BrainFireException("Null return value.");
            }
            return false;
        }

        public static bool ProcessImageFromFile(string BrainFile, string Filename)
        {
            Program.WriteLine("Loading brain...");
            Brain brain = Brain.LoadNetwork(BrainFile);
            Program.WriteLine("Done loading brain");
            Bitmap bmp = new Bitmap(Filename);
            bmp = ImageUtilities.ResizeImage(bmp, new Size(64, 64));
            List<double> Inputs = new List<double>();
            for (int y = 0; y < bmp.Height - 1; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color oc = bmp.GetPixel(x, y);
                    Inputs.Add(oc.GetHue() + oc.GetSaturation() + oc.GetBrightness());
                }
            }
            int NeuronsPerLayer = Inputs.Count * ((int)RATIO_TOP / RATIO_BOTTOM);
            NeuronsPerLayer += 1;
                List<double> Values = brain.Think(Inputs).Result;

                if (Values != null)
                {
                    double Value = Values[0];
                    Console.WriteLine("Value: " + Value);
                    return Value > .5 ? true : false;
                }
                else
                {
                    throw new BrainFireException("Null return value.");
                }
            return false;
        }

    }
}
