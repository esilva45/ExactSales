using Newtonsoft.Json;
using Npgsql;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace ServiceIntegration {
    class SendMessage {
        public static string urlligacao = null;
        public static string urlservice = null;
        public static string token = null;

        public static void Message() {
            NpgsqlConnection conn = Connection.GetConnection();

            XElement configXml = XElement.Load(System.AppDomain.CurrentDomain.BaseDirectory + @"\config.xml");
            urlligacao = configXml.Element("UrlLigacao").Value.ToString();
            urlservice = configXml.Element("UrlService").Value.ToString();
            token = configXml.Element("TokenExact").Value.ToString();

            CallModel call = new CallModel();
            string call_id = "";
            string result = "";
            int range_start = 0;
            int range_end = 0;
            int extension = 0;

            try {
                try {
                    range_start = Int32.Parse(configXml.Element("ExtensionStart").Value.ToString());
                }
                catch (Exception) {
                    range_start = 0;
                }

                try {
                    range_end = Int32.Parse(configXml.Element("ExtensionEnd").Value.ToString());

                    if (range_end == 0) {
                        range_end = 999999999;
                    }
                }
                catch (Exception) {
                    range_end = 999999999;
                }

                string query = "select myphone_callhistory_v14.call_id, dnowner, " +
                    "party_callerid, to_char(start_time  - interval '3 hours', 'YYYY-MM-DD HH24:MI:SS') as start_time, " +
                    "to_char(end_time  - interval '3 hours', 'YYYY-MM-DD HH24:MI:SS') as end_time, " +
                    "(date_part('second', end_time - start_time) + (date_part('minute', end_time - start_time) * 60)) as tempo, " +
                    "to_char(established_time, 'YYYYMMDDHH24MISS') as established " +
                    "from myphone_callhistory_v14 " +
                    "inner join crm_integration on myphone_callhistory_v14.call_id = crm_integration.call_id " +
                    "where crm_integration.processed = false";

                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                NpgsqlDataReader rd = command.ExecuteReader();

                while (rd.Read()) {
                    try {
                        extension = Int32.Parse(rd["dnowner"].ToString());
                    }
                    catch (Exception) {
                        extension = 0;
                    }

                    if ((extension >= range_start) && (extension <= range_end)) {
                        result = "";
                        call.UrlLigacao = urlligacao + Util.FindDirectory(rd["dnowner"].ToString(), rd["party_dn"].ToString(), rd["established"].ToString());
                        call.OrigemTel = rd["dnowner"].ToString();
                        call.DestinoTel = rd["party_dn"].ToString();
                        call.DtInicioChamada = rd["start_time"].ToString();
                        call.DtFimChamada = rd["end_time"].ToString();
                        call.TempoConversacao = rd["tempo"].ToString();
                        result = Send(JsonConvert.SerializeObject(call), rd["call_id"].ToString());

                        if (result.Equals("OK")) {
                            call_id += rd["call_id"] + " ";
                        }
                    } else {
                        call_id += rd["call_id"] + " ";
                    }
                }

                rd.Close();

                if (call_id != "") {
                    call_id = call_id.Trim().Replace(" ", ",");
                    NpgsqlCommand cmd = new NpgsqlCommand("update crm_integration set processed = true where call_id in (" + call_id + ")", conn);
                    cmd.ExecuteReader();
                }
            }
            catch (Exception ex) {
                Util.Log(ex.ToString());
            }
            finally {
                if (conn != null) {
                    conn.Close();
                }
            }
        }

        private static string Send(string json, string call_id) {
            string code = "ERROR";

            try {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(urlservice);
                httpWebRequest.ContentType = "application/json";

                if (token != null || token != "") {
                    httpWebRequest.Headers["Token_Exact"] = token;
                }

                httpWebRequest.Method = "POST";

                byte[] data = Encoding.UTF8.GetBytes(json);
                httpWebRequest.ContentLength = data.Length;

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream())) {
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                HttpStatusCode respStatusCode = httpResponse.StatusCode;

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream())) {
                    HttpStatusCode statusCode = ((HttpWebResponse)httpResponse).StatusCode;
                    //var result = streamReader.ReadToEnd();
                    code = statusCode.ToString();
                    Util.Log("ID: " + call_id + " code: " + code);
                    Util.Log(json);
                }
            }
            catch (Exception ex) {
                Util.Log(ex.ToString());
            }

            return code;
        }
    }
}
