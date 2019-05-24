using System;
using System.ServiceModel;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;
using System.Threading.Tasks;
using MyWS;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

namespace SoapTest
{
    class Program
    {
        static void Main(string[] args)
        {
            getMyWSAntraegeClosure();
        }

        private static void getSmcbAntraegeClosure()
        {
            var req = new AntraegeExportRequestType();
            req.Antragsstatus = AntragStatusKey.Bearbeitungbeendet;
            getSmcbAntraegeExportResponse result = null;
          
            //closure of wsmethod
            Action<MyWSClient> wsMethod = (MyWSClient client) =>
            {
                var task = client.getAntraegeExportAsync(req);
                task.Wait();
                result = task.Result;
             };
             
            wsCall("<MyWSUrl>", wsMethod);
            Debug.WriteLine(result.GetbAntraegeExportResponse1.ReturnCode.Description);
        }


        

        private static void wsCall(string tspUrl, Action<MyWSClient> wsMethod )
        {
            //client
            var serviceUrl = tspUrl;
            var binding = new BasicHttpBinding();
            binding.Security.Mode = BasicHttpSecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;
            var endpoint = new EndpointAddress(new Uri(serviceUrl));
            var client = new MyWSClient(binding, endpoint);

            try
            {
                client.ClientCredentials.ClientCertificate.SetCertificate(StoreLocation.LocalMachine, StoreName.My, X509FindType.FindByThumbprint, "<fingerprint>");
                client.OpenAsync().Wait();
                wsMethod(client);
                client.CloseAsync().Wait();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                client.Abort();
                throw ex;
            }
        }
    }
}
