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
            getAntraege();
        }

        private static void getAntraege()
        {
            var req = new AntraegeExportRequestType();
            req.Antragsstatus = AntragStatusKey.Bearbeitungbeendet;

            MyWS.getAntraegeExportResponse result = (MyWS.getAntraegeExportResponse) genericWsCall("<SOAPURL>", "<WsClientType>", "<ClientMethod>", req);
            Debug.WriteLine(result.GetAntraegeExportResponse1.ReturnCode.Description);
        }


        private static dynamic genericWsCall(string tspUrl, string genType, string genTaskType,  dynamic paras)
        {
            //client
            var serviceUrl = tspUrl;
            var binding = new BasicHttpBinding();
            binding.Security.Mode = BasicHttpSecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;
            var endpoint = new EndpointAddress(new Uri(serviceUrl));
            var client = new WsClient(binding, endpoint);

            //prepare call
            Type wsType = Type.GetType(genType);
            MethodInfo wsMethod = wsType.GetMethod(genTaskType);
            dynamic result;
    
            try
            {
                client.ClientCredentials.ClientCertificate.SetCertificate(StoreLocation.LocalMachine, StoreName.My, X509FindType.FindByThumbprint, "<Fingerprint>");
                client.OpenAsync().Wait();
                var task = (Task) wsMethod.Invoke(client, new object[] { paras });
                task.Wait();
                result = task.GetType().GetProperty("Result").GetValue(task);
                client.CloseAsync().Wait();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                client.Abort();
                throw ex;
            }
            return result;
        }
    }
}
