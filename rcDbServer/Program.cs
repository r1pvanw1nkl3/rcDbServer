using System.Threading.Tasks;
using System.Net;
using System.Reflection.Metadata;
using System.Text;

namespace rcDbServer
{    class rcDbServer
    {
        public const string URL = "http://localhost:4000/";
        public static HttpListener listener;
        public static Dictionary<string, string> database = [];
        public const string htmlResponse =
            "<!DOCTYPE html>" +
            "<html>" +
            "    <style>" +
            "        table, th, td {{" +
            "        border:1px solid black;" +
            "        border-collapse: collapse;" +
            "        text-align: center;" +
            "        padding-top: 2px;" +
            "        padding-bottom: 2px;" +
            "        }}" +
            "    </style>" +
            "    <title>Database Server</title>" +
            "    <body>" +
            "        <h3>{0}</h3>" +
            "        <table style=\"width:30%\">" +
            "            <tr>" +
            "                <th>key</th>" +
            "                <th>value</th>" +
            "            </tr>" +
            "            {1}" +
            "        </table>" +
            "   </body>" +
            "</html>";

        public const string tableRow =
        "<tr>" +
        "   <td>{0}</td>" +
        "   <td>{1}</td>" +
        "</tr>";

        public const string errorResponse =
            "<!DOCTYPE html>" +
            "<html>" +
            "    <title>Database Server</title>" +
            "    <body>" +
            "        <h3>Error: {0}</h3>" +
            "   </body>" +
            "</html>";

        public static async Task HandleRequests()
        {
            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();

                HttpListenerRequest req = context.Request;
                HttpListenerResponse resp = context.Response;

                //get path - handle retrieviing data from the dictionary
                if (req.HttpMethod == "GET" && req.Url.AbsolutePath == "/get")
                {
                    var queryString = req.QueryString;
                    
                    if (queryString["key"] == null)
                    {
                        var respData = Encoding.UTF8.GetBytes(String.Format(errorResponse, "400 - Invalid Request. Please include key in query string."));
                        resp.StatusCode = 400;
                        resp.ContentLength64 = respData.LongLength;
                        resp.ContentType = "text/html";
                        await resp.OutputStream.WriteAsync(respData, 0, respData.Length);
                    }
                    else
                    {
                        if (!database.ContainsKey(queryString["key"]))
                        {
                            var respData = Encoding.UTF8.GetBytes(String.Format(errorResponse, "404 - Key not found in database."));
                            resp.StatusCode = 404;
                            resp.ContentLength64 = respData.LongLength;
                            resp.ContentType = "text/html";
                            await resp.OutputStream.WriteAsync(respData, 0, respData.Length);
                        }
                        else
                        {
                            var rows = String.Format(tableRow, queryString["key"], database[queryString["key"]]);
                            var respData = Encoding.UTF8.GetBytes(String.Format(htmlResponse, "Query Results:", rows));
                            resp.StatusCode = 200;
                            resp.ContentLength64 = respData.LongLength;
                            resp.ContentType = "text/html";
                            await resp.OutputStream.WriteAsync(respData, 0, respData.Length);
                        }
                    } 
                }
                else if (req.HttpMethod == "GET" && req.Url.AbsolutePath == "/set")
                {
                    var queryString = req.QueryString;
                    if (queryString.Count == 0)
                    {
                            var respData = Encoding.UTF8.GetBytes(String.Format(errorResponse, "400 - No data included in query string."));
                            resp.StatusCode = 400;
                            resp.ContentLength64 = respData.LongLength;
                            resp.ContentType = "text/html";
                            await resp.OutputStream.WriteAsync(respData, 0, respData.Length);
                    }
                    else
                    {
                        string rows = "";
                        foreach (String key in queryString)
                        {
                            if (key != null && queryString[key] != null)
                            {
                                Console.WriteLine($"Key: {key} Value: {queryString[key]}");
                                database[key] = queryString[key];
                                rows += String.Format(tableRow, key, database[key]);
                            }
                        }
                        if (rows.Length == 0)
                        {
                            var respData = Encoding.UTF8.GetBytes(String.Format(errorResponse, "400 - Null values found for all keys."));
                            resp.StatusCode = 400;
                            resp.ContentLength64 = respData.LongLength;
                            resp.ContentType = "text/html";
                            await resp.OutputStream.WriteAsync(respData, 0, respData.Length);
                        }
                        else
                        {
                            var respData = Encoding.UTF8.GetBytes(String.Format(htmlResponse, "Data stored in DB:", rows));
                            resp.StatusCode = 200;
                            resp.ContentLength64 = respData.LongLength;
                            resp.ContentType = "text/html";
                            await resp.OutputStream.WriteAsync(respData, 0, respData.Length);
                        }
                    }
                 }
                else
                {
                    var respData = Encoding.UTF8.GetBytes(String.Format(errorResponse, "400 - Invalid Request."));
                    resp.StatusCode = 400;
                    resp.ContentLength64 = respData.LongLength;
                    resp.ContentType = "text/html";
                    await resp.OutputStream.WriteAsync(respData, 0, respData.Length);
                }
                resp.Close();
            }
        }

        public static void Main(string[] args)
        {
            listener = new HttpListener();
            listener.Prefixes.Add(URL);
            listener.Start();
            Task handleReqTask = HandleRequests();
            handleReqTask.GetAwaiter().GetResult();
            listener.Close();
        }
    }

}