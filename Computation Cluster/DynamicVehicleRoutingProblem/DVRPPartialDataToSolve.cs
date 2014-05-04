using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamicVehicleRoutingProblem
{
    public class DVRPPartialDataToSolve
    {
        public int[][] partial;
        public int NodeNumber;
        public DVRPPartialDataToSolve() { }

        public static string[] SplitText(string text)
        {
            string[] splitedText = text.Split(new Char[] { ' ', ':', '\t' });
            char[] charsToTrim = { '\r', ' ' };
            List<string> splitList = new List<string>();
            foreach (var s in splitedText)
            {
                string result = s.Trim(charsToTrim);
                if (result != "")
                    splitList.Add(result);
            }
            return splitList.ToArray();
        }

        public static DVRPPartialDataToSolve ParsePartialProblemData(byte[] data)
        {
            DVRPPartialDataToSolve pd2s = new DVRPPartialDataToSolve();

            pd2s.partial = new int[0][];
            string text = Communication_Library.CommunicationModule.ConvertDataToString(data, data.Length);
            string[] lines = text.Split(new[] { '\n' });

            int set = 0;
            int indeks = 0;
            for (int i = 0; i < lines.Length - 1; i++)
            {
                string[] split = DVRPHelper.SplitText(lines[i]);

                switch (split[0])
                {
                    case "NUMSETS":
                        pd2s.partial = new int[int.Parse(split[1])][];
                        pd2s.NodeNumber = int.Parse(split[2]);
                        break;
                    case "SET":
                        set = int.Parse(split[1]);
                        pd2s.partial[set] = new int[int.Parse(split[2])];
                        //set++;
                        break;
                    default:
                        for (int j = 0; j < split.Length; j++)
                        {
                            pd2s.partial[set][j] = int.Parse(split[j]);
                        }
                        break;
                }

            }
            return pd2s;
        }
    }
}
