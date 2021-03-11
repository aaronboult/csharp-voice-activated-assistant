using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Xml;

namespace Control{

    class InformationGrabber{

        static InformationGrabber(){

            ServicePointManager.Expect100Continue = true;
            
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;

        }

        public static void __TestGrabber__(bool debug = false){

            InformationGrabber grabber = new InformationGrabber();

            Console.WriteLine(grabber.SearchForTerm("tennis", debug));

        }

        public string SearchForTerm(string term, bool debug = false){

            string URL = "https://en.wikipedia.org/w/api.php";
            string PARAMETERS = "?format=xml&action=query&prop=extracts&exintro&explaintext&redirects=1&titles=";

            XmlDocument document = new XmlDocument();

            document.LoadXml(GET(URL, $"{PARAMETERS}{term}"));

            string contents = "";

            if (document.GetElementsByTagName("extract")[0] != null){

                contents = document.GetElementsByTagName("extract")[0].InnerText;

            }

            if (contents.ToLower() == $"{term} may refer to:"){

                return "";

            }
            
            return contents;

        }

        private string GET(string url, string parameters){

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + parameters);

            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using(HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using(Stream stream = response.GetResponseStream())
            using(StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }

        }

        private string UrlEncode(Dictionary<string, string> args){

            string encoded = "";

            foreach (KeyValuePair<string, string> pair in args){

                string value = "";

                string ampersand = "";

                if (pair.Value != ""){

                    value = $"={System.Uri.EscapeDataString(pair.Value)}";

                }

                if (encoded == ""){

                    encoded = "?";

                }
                else{

                    ampersand = "&";

                }

                encoded += $"{ampersand}{System.Uri.EscapeDataString(pair.Key)}{value}";

            }

            return encoded;

        }
        
    }

}