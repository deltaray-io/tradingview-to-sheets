using System.Globalization;
using System.Net;
using GoogleSheetsWrapper;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace tradingview_to_sheets
{
    public class TVHook
    {
        private readonly ILogger _logger;
        
        // FIXME: Hard-coding service accounts, credentials and document IDs are not advised,
        //        but for the sake of simplicity, we'll do it here
        private readonly string _serviceAccount = "YOUR-USER@YOUR-PROJECT.iam.gserviceaccount.com";
        private readonly string _documentId = "YOUR-DOCUMENT-ID";

        private string _jsonCreds = """
           {
             "type": "service_account",
             "project_id": "YOUR-PROJECT",
             "private_key_id": "YOUR-KEY-ID",
             "private_key": "YOUR-PRIVATE-KEY",
             "client_email": "YOUR-USER@YOUR-PROJECT.iam.gserviceaccount.com",
             "client_id": "YOUR-CLIENT-ID",
             "auth_uri": "https://accounts.google.com/o/oauth2/auth",
             "token_uri": "https://oauth2.googleapis.com/token",
             "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
             "client_x509_cert_url": "YOUR-CERT-URL",
             "universe_domain": "googleapis.com"
           }
        """;

        public TVHook(ILoggerFactory loggerFactory) {
            _logger = loggerFactory.CreateLogger<TVHook>();
        }

        private void AppendRaw(string sheet, string content) {
            var sheetHelper = new SheetHelper(_documentId, _serviceAccount, "raw");
            sheetHelper.Init(_jsonCreds);
            var appender = new SheetAppender(sheetHelper);
            
            appender.AppendRows(new List<List<string>>() {
                new List<string>(){DateTime.UtcNow.ToString(CultureInfo.InvariantCulture), sheet, content},
            });
        }

        private void AppendParsed(string sheet, string content) {
            var lines = content.Split("\n");
            int startLine = 0;
            if (lines.Length > 1 && lines[0].StartsWith("date")) {
                // ignore header row
                startLine = 1;
            }

            var sheetHelper = new SheetHelper(_documentId, _serviceAccount, sheet);
            sheetHelper.Init(_jsonCreds);
            var appender = new SheetAppender(sheetHelper);
            
            for (var i = startLine; i < lines.Length; i++) {
                var fields = lines[i].Split(",").ToList();
                appender.AppendRow(fields);
            }
        }


        [Function("TVHook")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req) {
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            var sheet = req.Query["sheet"];
            
            var content = await new StreamReader(req.Body).ReadToEndAsync();
            Exception? failExc = null;
            try {
                AppendRaw(sheet, content);
            }
            catch (Exception e) {
                failExc = e;
            }

            try {
                AppendParsed(sheet, content);
            } 
            catch (Exception e) {
                failExc = e;
            }

            if (failExc != null)
                throw failExc;

            return response;
        }
    }
}
