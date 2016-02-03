using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AuthentificationWR
{
    static public class CommunicationManager
    {
        /// <summary>
        /// Parse an object to a Json string
        /// </summary>
        /// <param name="objectToSerialize">Object to parse on Json</param>
        /// <returns>Json string</returns>
        static public string GetJsonFromObject(object objectToSerialize)
        {
            return new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(objectToSerialize);
        }

        /// <summary>
        /// Parse a Json string to an object
        /// </summary>
        /// <param name="json">Json string</param>
        /// <param name="resultType">Object result for deserialization</param>
        /// <returns>Object result</returns>
        static public object GetObjectFromJson(string json, Type resultType)
        {
            return new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize(json, resultType);
        }

        /// <summary>
        /// Send a Json request and get it result
        /// </summary>
        /// <param name="serverUri">Web Service Server URI</param>
        /// <param name="jsonRequest">Json request</param>
        /// <returns>Json result</returns>
        static public string SendJsonRequest(Uri serverUri, string jsonRequest)
        {
            ///Initialise the http connection information
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(serverUri);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Accept = "application/json";
            httpWebRequest.Method = "POST";

            ///Send the Json Request
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(jsonRequest);
                streamWriter.Flush();
                streamWriter.Close();
            }

            ///Receive the Json answer
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                string result = streamReader.ReadToEnd();
                streamReader.Close();
                return result;
            }
        }


        static public HttpWebRequest CreateHttpRequest(Uri serverUri, string requestMethod, Dictionary<string, string> requestPropertyDictionary)
        {
            ///Initialise the http connection information
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(serverUri);
            //httpWebRequest.ContentType = "application/text";
            //httpWebRequest.Accept = "application/text";
            httpWebRequest.Method = requestMethod;

            foreach (KeyValuePair<string, string> pair in requestPropertyDictionary)
            {
                //TODO
            }
            
            return httpWebRequest;
        }

        static public void SendHttpRequest(HttpWebRequest httpWebRequest, char[] buffer)
        {
            ///Send the Json Request
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(buffer);
                streamWriter.Flush();
                streamWriter.Close();
            }
        }

        static public StreamReader GetHttpResponse(HttpWebRequest httpWebRequest)
        {
            ///Receive the answer
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                return streamReader;
            }
        }

    }
}
