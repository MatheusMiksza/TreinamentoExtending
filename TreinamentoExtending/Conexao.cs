using System;
using System.Net;
using System.ServiceModel.Description;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Tooling.Connector;

namespace TreinamentoExtending {
    class Conexao {

        private static CrmServiceClient crmServiceClientDestino;        

        public CrmServiceClient Obter() {
            var connectionStringCRM = @"AuthType=OAuth;
            Username = matheusmiksza@rrsantosteste.onmicrosoft.com;
            Password = Adm12345; SkipDiscovery = True;
            AppId = 51f81489-12ee-4a9e-aaae-a2591f45987d;
            RedirectUri = app://58145B91-0C36-4500-8554-080854F2AC97;
            Url = https://org929a95b8.crm2.dynamics.com/main.aspx;";


            if (crmServiceClientDestino == null) 
            {
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                crmServiceClientDestino = new CrmServiceClient(connectionStringCRM);  
            }
            return crmServiceClientDestino;
        }
    }
}
