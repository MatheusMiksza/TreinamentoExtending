using Microsoft.Xrm.Sdk;
using System;



namespace AccountPreValidation
{   //plugin é uma forma de execuar um codigo atraves de açoes  
    //plugin sincrono espera o retorno para prosseguir          
    //plugin assincrono não fica esperando uma resposta imediata
    public class AccountValidation : IPlugin  //obriga que a classe faça o metodo a baixo 
    {
        public void Execute(IServiceProvider serviceProvider) //recebe informaçoas do serviceProvider
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));  //GetService pega as informaçoes do IPluginExecutionContext/typeof pega o tipo da classe

            // var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IPluginExecutionContext));

            // var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            Entity entidadeContexto = null;

            if (context.InputParameters.Contains("Target"))
                entidadeContexto = (Entity)context.InputParameters["Target"];//verifiva se na propriedade InputParameters tem o objeto Target que é a informação do 


            else
                return;


            if (!entidadeContexto.Contains("firstname"))//se nao tiver o primarycontactid faça
                throw new InvalidPluginExecutionException("Nome obrigatorio!");// é um disparo de exeção
        }

    }

}
