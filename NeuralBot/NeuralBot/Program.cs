using NeuralBot.Neural;
using NeuralBot.Neural.Networks;
using NeuralBot.Optics;
using NeuralBot.Window;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeuralBot
{
    class Program
    {
        public static TextWriter ConsoleOut;
        public static TextWriter Debug;
        public const double Version = 1.0;

        public static void WriteLine(string input)
        {
            Console.SetOut(ConsoleOut);
            Console.WriteLine(input);
            Console.SetOut(Debug);
        }

        static void Main(string[] args)
        {
            Stopwatch watch = new Stopwatch();
            Console.WriteLine("NeuralBot version " + Version + "\nPress [enter] to start");
            Console.ReadLine();
            FileStream ostrm;
            ConsoleOut = Console.Out;
            try
            {
                ostrm = new FileStream("./Redirect.txt", FileMode.Create, FileAccess.Write);
                Debug = new StreamWriter(ostrm);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error opening write stream");
                Console.WriteLine(e.Message);
                Console.ReadLine();
                return;
            }
            Thread Analyzer = new Thread(() => NetworkAnalyzer());
            Analyzer.Start();
            //var t = new Thread(MainFrameThread);
            //t.SetApartmentState(ApartmentState.STA);
            //t.Start();
            Brain brain = ImageAI.CreateCompoundAI("RealBank", "FakeBank");
            brain.SaveNetwork("./A.xml", NetworkType.BACK_PROPAGATION);
            Debug.Close();
            ostrm.Close();
            Console.SetOut(ConsoleOut);
            Console.WriteLine("Execution complete");
            Console.ReadLine();
        }

        public static MainFrame frame;
        private static void MainFrameThread()
        {
            frame = new MainFrame();
            Application.Run(frame);
        }

        public static GraphWindow window = new GraphWindow();
        private static void NetworkAnalyzer()
        {
            while (true)
            {
                try
                {
                    Application.Run(window);
                }
                catch (InvalidOperationException e)
                {

                }
                catch (ArgumentOutOfRangeException)
                {

                }
            }
        }

    }
}
