using System;
using System.IO;
using System.Xml.Linq;
using Npgsql;

namespace ServiceIntegration {
    class Util {
        public static string FindRecording(string call_id) {
            NpgsqlConnection conn = Connection.GetConnection();
            string recording = "/file%20not%20found";
            string file_name = null;

            try {
                string query = "SELECT recording_url FROM cl_participants where call_id = '" + call_id + "' and recording_url is not null";

                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                NpgsqlDataReader rd = command.ExecuteReader();

                while (rd.Read()) {
                    file_name = rd["recording_url"].ToString().Replace(" ", "%20");
                    recording = "/" + file_name.Replace("%3A", "%253A");
                }

                rd.Close();
            }
            catch (Exception ex) {
                Util.Log(ex.ToString());
            }
            finally {
                if (conn != null) {
                    conn.Close();
                }
            }

            return recording;
        }

        public static string FindDirectory(string dnowner, string party_dn, string start_time) {
            XElement configXml = XElement.Load(System.AppDomain.CurrentDomain.BaseDirectory + @"\config.xml");
            string patch = configXml.Element("PathFile").Value.ToString();
            string result = dnowner + "/file%20not%20found";
            string file_name = null;

            try {
                DirectoryInfo hdDirectoryInWhichToSearch = new DirectoryInfo(patch + dnowner);
                FileInfo[] filesInDir = hdDirectoryInWhichToSearch.GetFiles("*" + dnowner + "*" + party_dn + "*" + start_time + "*.wav");

                foreach (FileInfo foundFile in filesInDir) {
                    file_name = foundFile.Name.Replace(" ", "%20");
                    result = dnowner + "/" + file_name.Replace("%3A", "%253A");
                }
            }
            catch (Exception ex) {
                Log(ex.ToString());
            }

            return result;
        }

        public static void VerifyDir(string path) {
            try {
                DirectoryInfo dir = new DirectoryInfo(path);

                if (!dir.Exists) {
                    dir.Create();
                }
            }
            catch { }
        }

        public static void Log(string lines) {
            XElement configXml = XElement.Load(System.AppDomain.CurrentDomain.BaseDirectory + @"\config.xml");
            string patch = configXml.Element("PathLog").Value.ToString();
            VerifyDir(patch);

            string fileName = DateTime.Now.Day.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Year.ToString() + "_Logs.txt";

            try {
                StreamWriter file = new StreamWriter(patch + fileName, true);
                file.WriteLine(DateTime.Now.ToString() + ": " + lines);
                file.Close();
            }
            catch (Exception) { }
        }
    }
}
