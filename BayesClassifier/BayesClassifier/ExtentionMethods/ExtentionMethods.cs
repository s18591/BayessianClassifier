using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace BayesClassifier.ExtentionMethods
{
    public static class ExtentionMethods
    {
        public static string CONNECTION_STRING = "Data Source=db-mssql;Initial Catalog=s18591;Integrated Security=True";

        //ReadingFromFile
        public static List<string> ReadTraining(this string path)
        {
            List<string> list = new List<string>();

            using (var reader = new StreamReader(@path))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    line = line.Trim();
                    line = Regex.Replace(line, @",", ".");
                    line = Regex.Replace(line, @"\s+", "','");
                    line = "'" + line + "'";
                    list.Add(line);
                }
            }
            return list;
        }
        //ReadTest
        public static List<List<string>> ReadTest(this string path)
        {
            List<List<string>> list = new List<List<string>>();

            using (var reader = new StreamReader(@path))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    line = line.Trim();
                    line = Regex.Replace(line, @",", ".");
                    line = Regex.Replace(line, @"\s+", " ");
                    list.Add(new List<string>(line.Split(" ")));
                }
            }
            return list;
        }

        //Creating a table and inserting data to it
        public static void CreateTable(this List<string> trainingList)
        {
            using (var con = new SqlConnection(CONNECTION_STRING))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();
                string s = "CREATE TABLE table1 (";

                string insert = "insert into table1 (";
                for (int i = 0; i < trainingList[0].Split(",").Length - 1; i++)
                {
                    s += $"column{i} varchar(100), ";
                    insert += $"column{i},";
                }
                insert += $"decision)";
                s += $"decision varchar(100));";
                string values = "";
                for (int i = 0; i < trainingList.Count; i++)
                {
                    values += insert + "\n";
                    values += "VALUES (";
                    values += trainingList[i];
                    values += ")\n";
                }
                com.CommandText = s;
                com.ExecuteNonQuery();
                com.CommandText = values;
                com.ExecuteNonQuery();
            }
        }
        //DropTable
        public static void DropTable(this bool des)
        {
            using (var con = new SqlConnection(CONNECTION_STRING))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();
                com.CommandText = "drop table table1";
                com.ExecuteNonQuery();
            }
        }

        //calc couts of decition
        public static string calcCounts(this string counts)
        {
            double dub = 0;
            using (var con = new SqlConnection(CONNECTION_STRING))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();
                com.CommandText = "select count(distinct decision) counts from table1;";
                var drTmp = com.ExecuteReader();
                while (drTmp.Read())
                {
                    counts = drTmp["counts"].ToString();
                }
                drTmp.Close();
            }
            return counts;
        }
        //Create a list of posible decition
        public static List<string> decition(this List<string> listYesNo)
        {
            using (var con = new SqlConnection(CONNECTION_STRING))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();
                com.CommandText = "select distinct decision from table1";
                var dr = com.ExecuteReader();
                while (dr.Read())
                {
                    listYesNo.Add(dr["decision"].ToString());
                }
                dr.Close();
            }
            return listYesNo;
        }


        public static double Calc(this List<string> testString, double num, double total, string des, double counts)
        {
            double res = -1;
            using (var con = new SqlConnection(CONNECTION_STRING))
            using (var com = new SqlCommand())
            {
                //Console.WriteLine("num: " + num);
                //Console.WriteLine("total: " + total);
                Console.WriteLine("des: " + des);
                //testString.ForEach(Console.WriteLine);

                com.Connection = con;
                con.Open();
                double prob = 1;
                double desCount = 0;
                for (int i = 0; i < testString.Count; i++)
                {
                    com.CommandText = $"select count(distinct column{i}) tmp from table1";
                    var drTmp = com.ExecuteReader();
                    while (drTmp.Read())
                    {
                        desCount = Convert.ToDouble(drTmp["tmp"]);
                    }
                    drTmp.Close();

                    com.CommandText = $"select count(1) tmp from table1 where decision = '{des}' and column{i} = '{testString[i]}'";
                    var dr = com.ExecuteReader();
                    while (dr.Read())
                    {
                        double tmp = Convert.ToDouble(dr["tmp"]);

                        prob *= (tmp + 1.0 )/ (num + desCount);
                        Console.WriteLine("probWithoutSmoothing: " +((tmp + 1.0 )/ (num + desCount)));
                        
                        Console.WriteLine("probWithSmoothing: " + prob);
                        Console.WriteLine("=============");
                    }
                    dr.Close();

                }
                prob *= (num + 1) / (total + counts);               
                res = prob;
                Console.WriteLine("prob: " + prob);
                Console.WriteLine("resTotal: " + res);
            }
            return res;
        }
        public static double SumHorizontal( this Dictionary<string, Dictionary<string, int>> hash, string key)
        {
            double res = 0;
                foreach (string k in hash[key].Keys)
                {
                res += hash[key][k];
                }
            

            return res;
        }
        public static double SumVertical(this Dictionary<string,Dictionary<string,int>>hash,string key)
        {
            double res = 0;
            foreach (string k in hash.Keys)
            {
                res += hash[k][key];
            }

            return res;
        }
    }

        }
    



