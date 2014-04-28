using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicVehicleRoutingProblem
{
    public class Client
    {
        public int visitID;
        public int locationID;
        public double time;
        public double unld; //unload time
        public double size; //size of request

        public static bool operator ==(Client v1, Client v2)
        {
            if (v1 != null && v2 != null)
            {
                if (v1.locationID != v2.locationID)
                    return false;
                else if (v1.visitID != v2.visitID)
                    return false;
                else if (v1.time != v2.time)
                    return false;
                else if (v1.size != v2.size)
                    return false;
                else if (v1.unld != v2.unld)
                    return false;
                else
                    return true;
            }
            else if (v1 == null && v2 == null)
                return true;
            else
                return false;
        }

        public static bool operator !=(Client v1, Client v2)
        {
            if (v1 != null && v2 != null)
            {
                if (v1.locationID != v2.locationID)
                    return true;
                else if (v1.visitID != v2.visitID)
                    return true;
                else if (v1.time != v2.time)
                    return true;
                else if (v1.size != v2.size)
                    return true;
                else if (v1.unld != v2.unld)
                    return true;
                else
                    return false;
            }
            else if (v1 == null && v2 == null)
                return false;
            else
                return true;
        }

        public override string ToString()
        {
            string result = "";
            //VisitID
            result += this.visitID.ToString() + " ";
            //LocationID
            result += this.locationID.ToString() + " ";
            return result;
        }

        public static string ClientsToString(int[][][] subsets)
        {
            StringBuilder result = new StringBuilder();
            result.Append("NUMSETS:");
            result.Append(subsets.Length);
            result.Append("\n");
            for (int i = 0; i <subsets.Length; i++)
            {
                result.Append("SET:");
                result.Append(subsets[i].Length.ToString());
                result.Append("\n");
                for (int j = 0; j < subsets[i].Length; j++)
                {
                    result.Append("PATH:");
                    result.Append(subsets[i][j].Length);
                    result.Append("\n");
                    for (int k = 0; k < subsets[i][j].Length; k++)
                    {
                        result.Append(subsets[i][j][k].ToString());
                        result.Append(" "); 
                    }
                    //result.Append(subsets[i][j].ToArray() +" ";
                    result.Append("\n");
                }
                //result.Append("\n"); 
            }
            string filename = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            using (StreamWriter outfile = new StreamWriter(filename + @"\AllTxtFiles.txt"))
            {
                outfile.Write(result.ToString());
            }
            return result.ToString(); 
        }
    }
}
