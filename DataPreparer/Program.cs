using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using fastJSON;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            const string datadirpath = "C:\\Users\\Tata\\Desktop\\visualisierung\\unmodifieddata";
            const string metadatadirpath = "C:\\Users\\Tata\\Desktop\\visualisierung\\unmodifiedmetadata";
            string samecolumns = "samecolumns.txt";
            string missingvalues = "missingvalues.txt";

            string[] dir = Directory.GetFiles(datadirpath);
            File.Delete(samecolumns);
            File.Delete(missingvalues); //records that have "" or null as value
            string[] tsvfile;
            string[,] donetext=new string[1,1]; //need to initialize
            List<string> appendcolumns = new List<string>();
            List<string> appendrows = new List<string>();
            List<int> todeletecolumns = new List<int>();
            List<int> todeleterows = new List<int>();

            foreach (var file in dir)
            {
                tsvfile = File.ReadAllLines(file);
                int columns=0;

                PreparingTextforComparison(ref donetext, tsvfile, ref columns, ref todeleterows);

                DeleteRowsWithMissingValues(todeleterows, ref tsvfile, file, appendrows);

                string filename = file.Split('\\').Last().Split('.').First() + ".json";
                Dictionary<string, object> json = fastJSON.JSON.Parse(File.ReadAllText(metadatadirpath + "\\" + filename)) as Dictionary<string, object>;//this seems to be the best string to json method

                Comparing(columns, file, donetext, appendcolumns, todeletecolumns, json);

                if(todeletecolumns.Count>0)
                {
                    todeletecolumns = todeletecolumns.Distinct().ToList(); //duplicates can mess up the columns that are to be deleted
                    todeletecolumns.Sort();
                    for (int i = 0; i < todeletecolumns.Count; i++)
                    {
                        todeletecolumns[i] = todeletecolumns[i] - i; //this is needed for deleting the correct columns AND JSON ROWS, as all columns will shift when one is deleted AND SO WILL THE JSON ROWS
                    }

                    DeleteJsonRows(todeletecolumns, file, metadatadirpath, json);

                    DeleteColumns(todeletecolumns, ref tsvfile, file);
                }
                todeletecolumns.Clear();
                WritetoTSV(tsvfile, file);
            }
            File.AppendAllLines(samecolumns, appendcolumns);
            File.AppendAllLines(missingvalues, appendrows);
            Console.ReadLine();
        }

        static void DeleteColumns(List<int> todelete, ref string[] tsvfile, string file)
        {

            for (int i = 0; i < tsvfile.Length; i++)
            {
                if(tsvfile[i]!=null)
                {
                    foreach (var samecolumns in todelete)
                    {
                        var allindexes = tsvfile[i].AllIndexesOf("\t");

                        if (samecolumns == allindexes.Count) //if the column to be deleted is the last one
                        {
                            int temp2 = allindexes.Last();
                            tsvfile[i] = tsvfile[i].Remove(temp2, tsvfile[i].Length - temp2);
                        }
                        else if (samecolumns == 0)
                        {
                            tsvfile[i] = tsvfile[i].Remove(0, allindexes.First() + 1);
                        }
                        else
                        {
                            tsvfile[i] = tsvfile[i].Remove(allindexes[samecolumns - 1] + 1, allindexes[samecolumns] - allindexes[samecolumns - 1]);
                        }
                    }
                }
            }
           
        }

        static void PreparingTextforComparison(ref string[,] donetext, string[] tsvfile,ref int columns, ref List<int> todeleterows)
        {
            columns = tsvfile[0].Split('\t').Length;
            donetext = new string[tsvfile.Length, columns];
            string[] temp;
            for (int i = 0; i < tsvfile.Length; i++)
            {
                temp = tsvfile[i].Split('\t');
                for (int j = 0; j < columns; j++)
                {

                    if(temp[j]==null || temp[j]=="")
                    {
                        todeleterows.Add(i);
                    }
                    donetext[i, j] = temp[j];
                }
            }
            todeleterows = todeleterows.Distinct().ToList();
        }
        
        static void Comparing(int columns, string file, string[,] donetext, List<string> append, List<int> todelete, Dictionary<string, object> json)
        {
            for (int i = 0; i < columns - 1; i++)
            {
                for (int j = i + 1; j < columns; j++)
                {
                    bool samecolumns = true;
                    for (int r = 0; r < donetext.GetLength(0); r++)
                    {
                        if (donetext[r, i] != donetext[r, j])
                        {
                            samecolumns = false;
                        }
                    }
                    if (samecolumns == true)
                    {
                        string text = "Columns in " + file + " : " + (json["columns"] as List<object>).ElementAt(i)+" "+i+ " and " + (json["columns"] as List<object>).ElementAt(j) +" "+j+ " are the same";  //need to convert object to list for the indexing
                        Console.WriteLine(text);
                        append.Add(text);
                        todelete.Add(j);
                    }
                }
            }
            
        }

        static void DeleteJsonRows(List<int> todelete, string file, string filepath, Dictionary<string, object> json)
        {
            string filename = file.Split('\\').Last().Split('.').First()+".json";
            foreach (var row in todelete)
            {
                (json["columns"] as List<object>).RemoveAt(row);
                fastJSON.JSON.ToNiceJSON(json);
            }
            File.WriteAllText(filename, fastJSON.JSON.ToNiceJSON(json));
        }

        static void DeleteRowsWithMissingValues(List<int> todeleterows, ref string[] tsvfile, string file, List<string> appendrows)
        {
            foreach (var row in todeleterows)
            {
                string text = "Row "+row+" in "+file+" has missing value"; 
                Console.WriteLine(text);
                appendrows.Add(text);
                tsvfile[row] = null;
            }
            todeleterows.Clear();
        }

        static void WritetoTSV(string[] tsvfile, string file)
        {
            //File.WriteAllLines(file.Split('\\').Last(), tsvfile);
            using (var stream = File.OpenWrite(file.Split('\\').Last()))
            using (StreamWriter writer = new StreamWriter(stream)) //streamwriter acts really weird, sometimes a part of a line might appear in a newline
            {
                for (int i = 0; i < tsvfile.Length; i++)
                {
                    if (tsvfile[i] != null)
                    {
                        writer.WriteLine(tsvfile[i]);
                    }
                }
                writer.Flush();
                writer.Close();
            }
        }
    }
}
