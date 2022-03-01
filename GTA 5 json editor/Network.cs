using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PS3Lib;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading;

namespace GTA_5_json_editor
{
    class Network
    {
        private static HttpClient Client = new HttpClient();

        public static PS3API PS3 = mainForm.PS3;
        private static uint
            urlAddress = 0x020050A9,
            hmacBypassAddress = 0x00D5CF20;

        public static void switchServer(string server)
        {
            if (server == "Rockstar")
            {
                PS3.Extension.WriteUInt32(hmacBypassAddress, 0x62830000);
                PS3.Extension.WriteString(urlAddress, "ros.rockstargames.com");
            }
            else if (server == "rosser")
            {
                PS3.Extension.WriteUInt32(hmacBypassAddress, 0x38600001);
                PS3.Extension.WriteString(urlAddress, "ros.aaronrosser.xyz");
            }
        }

        public static string addImageToServer(string jobId, string imageUrl)
        {
            jobId = jobId.Replace("https", "http");
            if (new Ping().Send("aaronrosser.xyz").Status == IPStatus.Success)
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        var reqparm = new System.Collections.Specialized.NameValueCollection();
                        reqparm.Add("jobId", jobId);
                        reqparm.Add("imageURL", imageUrl);
                        client.Headers.Add("User-Agent", Variables.version.ToString());
                        byte[] responsebytes = client.UploadValues("http://prod.ros.aaronrosser.xyz/addImageToFile.php", "POST", reqparm);
                        string responsebody = Encoding.UTF8.GetString(responsebytes);
                        Dictionary<string, object> jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(responsebody);
                        if (jsonData.ContainsKey("response"))
                        {
                            return jsonData["response"].ToString();
                        }
                        return "Response not found";
                    }
                }
                catch
                {
                    return "Request failed";
                }
            }
            else
            {
                return "Could not connect to server";
            }
        }

        public static string getJsonFromImage(string imageURL)
        {
            Client.DefaultRequestHeaders.Add("User-Agent", Variables.version.ToString());
            imageURL = imageURL.Replace("https", "http");
            if (imageURL.Contains(".jpg") && imageURL.Contains("prod.cloud.rockstargames.com/ugc/gta5mission/"))
            {
                string baseURL = imageURL.Substring(0, imageURL.Length - 7) + "0_";
                string[] languages = { "en", "de", "fr", "it", "es", "pt", "pl", "ru", "es-mx" };

                // Check if server is online
                if (new Ping().Send("prod.cloud.rockstargames.com").Status == IPStatus.Success)
                {
                    for (int j = 1; j <= 2; j++)
                    {
                        foreach (string language in languages)
                        {
                            for (int i = 0; i <= 10*j; i++)
                            {

                                try
                                {
                                    string url = baseURL + i + "_" + language + ".json";
                                    var result = Client.GetAsync(url).Result;
                                    if (result.StatusCode == HttpStatusCode.OK) return url;
                                }
                                catch
                                {
                                    return "Request failed";
                                }
                            }
                        }
                    }
                }
                else
                {
                    return "Couldn't connect to prod.cloud.rockstargames.com";
                }
            }
            return "The url needs to be the direct image url ending with .jpg";
        }

        public static bool loadJsonfileFromServer(string id, bool customServer)
        {
            if (RPC.Call(Natives.PRELOAD_FIND) != 0)
            {
                RPC.Call(Natives.DATAFILE_DELETE);
            }

            switchServer(customServer ? "rosser" : "Rockstar");

            int number = RPC.Call(Natives.NETWORK_REQUEST_JSON_0x38FC2EEB, "gta5mission", id, 0, 0, 1);
            RPC.Call(Natives.DATAFILE_0x621388FF, number);
            Thread.Sleep(5000);

            switchServer("Rockstar");

            if (RPC.Call(Natives.DATAFILE_0xB41064A4, number) == 1) return true;
            Thread.Sleep(2000);
            return RPC.Call(Natives.DATAFILE_0xB41064A4, number) == 1;
        }

        public static string uploadJsonToServer(string json)
        {
            // No clue what was doing here some sort of obfuscation of the upload url to be combined with obfuscar

            // Generate random string
            byte[] serverBytes = new byte[]
			{
				64, 161, 233, 128, 134, 212, 248, 3, 219, 231, 17, 74, 181, 114, 96, 52, 144, 178, 106, 251, 201, 11,
				83, 222, 189, 246, 145, 236, 183, 178, 143, 185, 223, 110, 160, 130, 5, 68, 237, 126, 157, 208, 160,
				61, 129, 25, 215, 233, 147, 184, 94, 66, 226, 139, 207, 189, 116, 5, 95, 156, 68, 94, 208, 250, 29, 111,
				28, 39, 172, 125, 183, 112, 199, 112, 32, 179, 133, 138, 250, 78, 96, 9, 74, 21, 198, 147, 217, 64, 29,
				79, 216, 136, 99, 191, 23
			};
			byte[] random = new byte[]
			{
				40, 213, 157, 240, 188, 251, 215, 115, 169, 136, 117, 100, 199, 29, 19, 26, 241, 211, 24, 148, 167, 121,
				60, 173, 206, 147, 227, 194, 207, 203, 245, 150, 168, 11, 210, 235, 125, 38, 222, 27, 235, 160, 195, 9,
				251, 33, 189, 132, 249, 210, 102, 52, 133, 238, 249, 200, 31, 99, 43, 244, 46, 44, 186, 206, 47, 5, 109,
				83, 222, 20, 131, 7, 160, 4, 22, 198, 183, 179, 148, 61, 10, 59, 121, 45, 167, 228, 234, 43, 122, 41,
				173, 254, 81, 213, 56
			};

			char[] c = new char[95];
			for (int i = 0; i < serverBytes.Length; i++)
				c[i] = (char)(serverBytes[i] ^ random[i]);

			Random rnd = new Random();
			Byte[] b = new Byte[87];
			rnd.NextBytes(b);
			b[b.Length - 1] = 0;
			int sum = b.Sum(x => x);
			int remainder = sum % random[8];
			b[b.Length - 1] = Convert.ToByte(random[90] - 4 - remainder);

			int test = b.Sum(x => x) % 169;

			Console.WriteLine();
			for (int i = 0; i < b.Length; i++)
			{
				Console.Write(b[i]);
				b[i] ^= random[i];
			}
			Console.WriteLine();

			string b64 = Convert.ToBase64String(b);
			b64 = b64.Replace('/', '-').Replace('+', '_');
			string path = new string(c) + b64;

			if (new Ping().Send(path.Substring(16, 17)).Status == IPStatus.Success)
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        var reqparm = new System.Collections.Specialized.NameValueCollection();
                        reqparm.Add("json", json);
                        client.Headers.Add("User-Agent", Variables.version.ToString() + " " + Ver.a);
                        byte[] responsebytes = client.UploadValues(path, "POST", reqparm);
                        string responsebody = Encoding.UTF8.GetString(responsebytes);
                        Dictionary<string, object> jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(responsebody);
                        if (jsonData.ContainsKey("id"))
                        {
                            return jsonData["id"].ToString();
                        }
                        return null;
                    }
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public static void disableCaching(bool disabled)
        {
            if (disabled)
            {
                PS3.Extension.WriteInt32(0x00D36058, 0x60000000);
            } else
            {
                PS3.Extension.WriteInt32(0x00D36058, 0x4BFE6009);
            }
        }

        public static string getImageRedirectURL(string jobId)
        {
            if (new Ping().Send("prod.cloud.rockstargames.com").Status == IPStatus.Success)
            {
                try
                {
                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://prod.ros.aaronrosser.xyz/cloud/11/cloudservices/ugc/gta5mission/" + jobId + "/1_0.jpg");
                    httpWebRequest.UserAgent = Variables.version.ToString();
                    httpWebRequest.AllowAutoRedirect = false;
                    using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                    {
                        if (httpWebResponse.StatusCode == HttpStatusCode.MovedPermanently)
                        {
                            return httpWebResponse.Headers["Location"];
                        } else
                        {
                            return "No current redirect";
                        }
                    }
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        var response = ex.Response as HttpWebResponse;
                        if (response != null) return response.ToString();
                        else return "Request failed";
                    }
                    else
                    {
                        return "Request failed";
                    }
                }
            }
            else
            {
                return "Couldn't connect to aaronrosser.xyz";
            }
        }
    }
}
