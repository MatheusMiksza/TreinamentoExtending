using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Description;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System.Net;
using Microsoft.Xrm.Tooling.Connector;
//using ConexaoAlternativaExtending;

namespace TreinamentoExtending
{
    class Program
    {
        static void Main(string[] args)
        {
            //Descoberta();
            var serviceproxy = new Conexao().Obter(); // essa linha cria a conexão com o dynamics
            //MeuCreate(serviceproxy); // essa executa o metodo desejado 
            //MeuUpdate(serviceproxy);
            //FetchXMLAgrefate(serviceproxy);
            AlterarConta(serviceproxy);
            //RetornarMultiplo(serviceproxy);
            //RetornarMultipliComLink(serviceproxy);
            //TesteCreate(serviceproxy);
            //TesteCreate(serviceproxy);

            Console.ReadKey();
        }
        #region Descoberta
        static void Descoberta()
        {
            Uri local = new Uri("https://disco.crm2.dynamics.com/XRMServices/2011/Discovery.svc");

            ClientCredentials clientcred = new ClientCredentials();   //class variavel = atribuir um valor, o comando new eu coloco se eu não sei qual o valor da variavel
            clientcred.UserName.UserName = "matheus.miksza@treinoDEV.onmicrosoft.com";   // atribuindo valor as propriedades 
            clientcred.UserName.Password = "7055550141/Pats";
            DiscoveryServiceProxy ponteCrm = new DiscoveryServiceProxy(local, null, clientcred, null);   // DiscoveryServiceProxy é uma class para se conectar com o Dynamics com os paremetro endereço e os dados do usuario para logar no CRM
            ponteCrm.Authenticate();   //.Authenticate pede para o dynamics validar o usuario
            RetrieveOrganizationsRequest perguntaCrm = new RetrieveOrganizationsRequest();   //RetrieveOrganizationsRequest é uma classe que faz a pergunta para o Dynamics
            perguntaCrm.AccessType = EndpointAccessType.Default;
            perguntaCrm.Release = OrganizationRelease.Current;   //quero pegar a organização mais recente dos dados

            RetrieveOrganizationsResponse respostaCrm = (RetrieveOrganizationsResponse)ponteCrm.Execute(perguntaCrm);    //a respostaCrm esta recbendo o valor da perginta que a ponteCrm fez
            foreach (var item in respostaCrm.Details)   // esta é a resposta
            {
                Console.WriteLine("Unique " + item.UniqueName);
                Console.WriteLine("Friendly " + item.FriendlyName);
                foreach (var endpoint in item.Endpoints)
                {
                    Console.WriteLine(endpoint.Key);
                    Console.WriteLine(endpoint.Value);
                }
            }
            Console.ReadKey();
        }
        #endregion

        #region Create
        static void MeuCreate(CrmServiceClient serviceProxy)
        {
            for (int i = 0; i < 10; i++)
            {
                var entidade = new Entity("account"); //Entiti representa uma tabela no banco de dados, e o accont diz qual é a tabela
                Guid registro = new Guid(); //Guid é uma chave do banco de dados
                String[] nome = new string[5] { "Osaka", "Game Set", "Premier Clube", "Mitelli Equipamentos", "Ace Zone" };


                entidade.Attributes.Add("name", nome);  //cria um registro no campo name com i valor especificado 
                // Criei no Dynamics

                registro = serviceProxy.Create(entidade);
            }
        }



        #endregion

        #region MeuUpdate

        static void MeuUpdate(CrmServiceClient serviceProxy)
        {
            for (int i = 0; i < 10; i++)
            {
                var entidade = new Entity("contact");
                Guid idregistro = new Guid();

                var idTeste = Guid.NewGuid();
                entidade.Attributes.Add("contactid", idTeste);
                // Criei no Dynamics

                idregistro = serviceProxy.Create(entidade);

                if (idregistro == idTeste)
                    Console.WriteLine("igual");

                var registroDynamics = serviceProxy.Retrieve("account", idregistro, new ColumnSet("name"));
                if (registroDynamics.Attributes.Contains("name"))
                {

                    registroDynamics.Attributes["name"] = "Novo Valor " + i.ToString();
                }
                else
                {
                    registroDynamics.Attributes.Add("name", "Novo Valor " + i.ToString());
                }


                serviceProxy.Update(registroDynamics);

            }
        }


        #endregion

        #region Delete

        static void MeuDelete(CrmServiceClient serviceProxy)
        {
            for (int i = 0; i < 10; i++)
            {
                var entidade = new Entity("account");
                Guid idRegistro = new Guid();

                entidade.Attributes.Add("name", "Treinamento " + i.ToString());
                //Criei no Dyanamics

                idRegistro = serviceProxy.Create(entidade);

                serviceProxy.Delete("account", idRegistro);

            }
        }
        #endregion

        #region QueryExpression 1

        static EntityCollection RetornarMultiplo(CrmServiceClient serviceProxy)
        {
            QueryExpression queryExpression = new QueryExpression("account");

            queryExpression.Criteria.AddCondition("websiteurl", ConditionOperator.NotNull);
            queryExpression.ColumnSet = new ColumnSet("websiteurl");
            EntityCollection colecaoEtidades = serviceProxy.RetrieveMultiple(queryExpression);

            var retornoLista = RetornarMultiplo(serviceProxy);
            foreach (var item in retornoLista.Entities)
            {
                if (item.Attributes.Contains("websiteurl"))
                    Console.WriteLine(item["websiteurl"]);
                else
                    Console.WriteLine("Campo não encontrado");
            }
            Console.WriteLine("Po po por hoje é só pessoal");

            return colecaoEtidades;

        }
        #endregion

        #region QueryExpression 2
        static void RetornarMultipliComLink(CrmServiceClient serviceProxy)
        {
            QueryExpression queryExpression = new QueryExpression("account");
            queryExpression.ColumnSet = new ColumnSet(true);

            ConditionExpression condicao = new ConditionExpression("address1_city", ConditionOperator.Equal, "Natal");
            queryExpression.Criteria.AddCondition(condicao);

            LinkEntity link = new LinkEntity("account", "contact", "primarycontactid", "contactid", JoinOperator.Inner);
            link.Columns = new ColumnSet("firstname", "lastname");
            link.EntityAlias = "Contato";
            queryExpression.LinkEntities.Add(link);

            EntityCollection colecaoEntidades = serviceProxy.RetrieveMultiple(queryExpression);
            foreach (var entidade in colecaoEntidades.Entities)
            {
                Console.WriteLine("Id: " + entidade.Id);
                Console.WriteLine("Nome conta " + entidade["name"]);
                Console.WriteLine("Nome contato " + ((AliasedValue)entidade["Contato.firstname"]).Value);
                Console.WriteLine("Sobrenome Cotato " + ((AliasedValue)entidade["Contato.lastname"]).Value);
            }
        }

        #endregion

        #region Linq1
        static void ConsultaLinq(CrmServiceClient serviceProxy)
        {
            OrganizationServiceContext context = new OrganizationServiceContext(serviceProxy);

            var resultados = from a in context.CreateQuery("contact")
                             join b in context.CreateQuery("account")
                                    on a["contactid"] equals b["primarycontactid"]
                             //where a ["contactid"].ToString().Contains("en")
                             select new
                             {
                                 retorno = new
                                 {
                                     FirstName = a["firstname"],
                                     LastName = a["lastname"],
                                     NomeConta = b["name"]
                                 }
                             };
            foreach (var entidade in resultados)
            {
                Console.WriteLine("Nome : " + entidade.retorno.FirstName);
                Console.WriteLine("Sobrenome : " + entidade.retorno.LastName);
                Console.WriteLine("NomeConta : " + entidade.retorno.NomeConta);
            }



        }


        #endregion

        #region CRUD Linq

        static void CriacaoLinq(CrmServiceClient serviceProxy)
        {
            OrganizationServiceContext context = new OrganizationServiceContext(serviceProxy);
            for (int i = 0; i < 10; i++)
            {
                Entity account = new Entity("account");
                account["name"] = "Conta Linq" + i;
                context.AddObject(account); // adicionando um array de registros 

            }
            context.SaveChanges();
        }

        static void UpdateLinq(CrmServiceClient serviceProxy)
        {
            OrganizationServiceContext context = new OrganizationServiceContext(serviceProxy);
            var resultados = from a in context.CreateQuery("contact")
                             where ((string)a["firstname"]) == "Dan"
                             select a;

            foreach (var item in resultados)
            {
                item.Attributes["firstname"] = "Daniel Geraldeli";
                context.UpdateObject(item);
            }
            context.SaveChanges();
        }

        static void ExcluirLinq(CrmServiceClient serviceProxy)
        {
            OrganizationServiceContext context = new OrganizationServiceContext(serviceProxy);
            var resultados = from a in context.CreateQuery("account")
                             where ((string)a["name"]) == "Treinamento Extending 2"
                             select a;

            foreach (var item in resultados)
            {
                context.DeleteObject(item);

            }
            context.SaveChanges();
        }

        #endregion

        #region FetchXML
        static void FetchXML(CrmServiceClient serviceProxy)
        {

            string query = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name= 'account'>
                                <attribute name='name' />
                                <attribute name='primarycontactid' />
                                <attribute name='telephone1'/>
                                <attribute name='accountid'/>
                                <attribute name='createdon'/>
                                <order attribute= 'name' descending='false'/>
                                <filter type = 'and'>
                                    <condition attribute= 'name' operator='eq' value='Tlsv Eng'/>
                                    <condition attribute= 'accountnumber' operator='not-null'/>
                                </filter>
                              </entity>
                            </fetch>";

            EntityCollection colecao = serviceProxy.RetrieveMultiple(new FetchExpression(query));
            foreach (var item in colecao.Entities)
            {
                Console.WriteLine(item["name"]);
            }
        }

        #endregion

        #region FetchXML2

        static void FetchXMLAgrefate(CrmServiceClient serviceProxy)
        {
            string query = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' aggregate='true'>
                                <entity name='account'>
                                    <attribute name='creditlimit' alias='creditlimit_soma' aggregate='avg'/>
                                </entity>
                            </fetch>";

            EntityCollection colecao = serviceProxy.RetrieveMultiple(new FetchExpression(query));

            foreach (var item in colecao.Entities)
            {
                Console.WriteLine(((Money)((AliasedValue)item["creditlimit_soma"]).Value).Value.ToString());
            }
        }

        #endregion





        #region TratamentoDigitacao

        static string InicialMaiuscula(string texto)
        {
            string[] textoExplode = texto.Split(' '); //separa o nome em várias variáveis pelo espaço
            texto = "";
            for (int j = 0; j < textoExplode.Length; j++)
            {
                textoExplode[j] = textoExplode[j].ToLower(); //converte todas as palavras para minusculo

                if (textoExplode[j].Length > 2) //não realiza a função em palavras de 2 letras
                    textoExplode[j] = char.ToUpper(textoExplode[j][0]) + textoExplode[j].Substring(1); //coloca a inicial em maiuscula

                texto += textoExplode[j]; //reconstroi o nome
                if (j != (textoExplode.Length - 1)) //se for o último, não adiciona um nome ao final
                    texto += " ";
            }
            return texto;
        }



        #endregion


        #region TesteCreateConta

        static void TesteCreate(CrmServiceClient serviceProxy)
        {
            Console.WriteLine("Vamos fazer o teste de de cadastro de contas.");
            Console.WriteLine(" ");
            string resp = "s";
            while (resp == "s")
            {
                var entidade = new Entity("account");
                Guid registro = new Guid();

                Console.WriteLine("Qual o nome da empresa: ");
                string nome = Console.ReadLine();
                entidade.Attributes.Add("name", nome);


                Console.WriteLine("Qual o telefone: ");
                string fone1 = Console.ReadLine();
                entidade.Attributes.Add("telephone1", fone1);

                Console.WriteLine("Quer cadastrar mais alguma conta s/n ? ");
                resp = Console.ReadLine();

                registro = serviceProxy.Create(entidade);
            }
        }

        #endregion

        #region TesteCreateContato

        static void TesteCreateContct(CrmServiceClient serviceProxy)
        {
            Console.WriteLine("Vamos fazer o teste de cadastro de registro S/N.\n");
            string resp = "S";
            while (resp == "S")
            {

                Console.WriteLine("Para cadatrar contatos digite 1 para contas diite 2\n");
                string i = Console.ReadLine();

                if (i == "1")
                {
                    var entidade = new Entity("contact");
                    Guid registro = new Guid();
                    //Adição do nome do contado
                    Console.WriteLine("Qual é o Primeiro nome: \t");
                    string nome1 = Console.ReadLine();
                    entidade.Attributes.Add("firstname", nome1);



                    //Adição do sobrenome do contado
                    Console.WriteLine("Qual o sobrenome: \t");
                    string nome2 = Console.ReadLine();
                    entidade.Attributes.Add("lastname", nome2);

                    Console.WriteLine("Qual o CPF: \t");
                    string cpf = Console.ReadLine();
                    int cpfnum = Convert.ToInt32(cpf); 
                    entidade.Attributes.Add("new_cpf", cpfnum);
                    

                    //Adição do sobrenome do contato
                    Console.WriteLine("Qual o cargo: \t");
                    string cargo = Console.ReadLine();
                    entidade.Attributes.Add("jobtitle", cargo);

                    //Adição da conta filiada desse contato
                    //Console.WriteLine("A qual conta está filiado: ");
                    //string conta = Console.ReadLine();
                    //entidade.Attributes.Add("parentcustomerid", conta);

                    //Adiçãp de email
                    Console.WriteLine("Qual o e-mail: \n");
                    string email = Console.ReadLine();
                    
                    entidade.Attributes.Add("emailaddress1", email);

                    


                    registro = serviceProxy.Create(entidade);
                }


                else if (i == "2")
                {
                    var entidade = new Entity("account");
                    Guid registro = new Guid();

                    Console.WriteLine("Qual o nome da empresa: \t");
                    string nome = Console.ReadLine();
                    entidade.Attributes.Add("name", nome);

                    Console.WriteLine("Qual o CNPJ: \t");
                    string cpf = Console.ReadLine();
                    int cpfnum = Convert.ToInt32(cpf);
                    entidade.Attributes.Add("new_cpf", cpfnum);


                    Console.WriteLine("Qual o telefone: \t");
                    string fone1 = Console.ReadLine();
                    entidade.Attributes.Add("telephone1", fone1);

                  
                    registro = serviceProxy.Create(entidade);
                }
                Console.WriteLine("Quer cadastrar mais alguma conta s/n ? ");
                resp = Console.ReadLine();
                resp = resp.ToUpper();
            }
            Console.WriteLine("Fim");

           
        }
        #endregion


        #region BuscaCpf
        static void BuscaCpf(CrmServiceClient serviceProxy)
        {
            string query = @"<fetch version='1.0' output-format= 'xml-platform' mapping='logical' distinct='false'>
                               <entity name='contact'>
                                <attribute name='firstname' />
                                <attribute name = 'lastname' />
                                <attribute name = 'new_cpf' />
                                <attribute name = 'jobtitle' />
                                <attribute name = 'emailaddress1' />
                                <order attribute='firstname' desceding='false' />
                                <filter type='and'> 
                                    <condition attribute='firstname' operator='like' value='%' />
                                    <condition attribute='new_cpf' operator='not-null' />
                                    <condition attribute='emailaddress1' operator='not-null' />

                                </filter>
                              </entity>
                            </fetch>";

            EntityCollection colecao = serviceProxy.RetrieveMultiple(new FetchExpression(query));



            foreach (var item in colecao.Entities)
            {
                Console.Write("Nome: " + item["firstname"] + " ");
                Console.Write(item["lastname"] + " \t");
                Console.Write("CPF: " + item["new_cpf"] + " \t");
                Console.Write("Cargo: " + item["jobtitle"] + " \t");
                Console.Write("Email: " + item["emailaddress1"] + "  \n");

            }

            


        }
        #endregion

        #region Insenrindo dados nas tabelas

        static void InserirDados(CrmServiceClient serviceProxy)
        {
            // criando laço de repetica
            var situacao = "S"; // se a situacao for verdadeira 
            while (situacao == "S") // para a situacao verdadeira
            {
                Console.WriteLine("\t\t\t BEM VINDO AO PROGRAMA \t\t\t\n");// mensagem exibida na tela
                Console.WriteLine("Para adicionar Conta tecle 1\nPara adicionar Contato tecle 2\n"); // pergutna ao usuario
                var resposta = Console.ReadLine().ToString(); //  variavel "respsota" esta recebendo a resposta do usuario e convertendo em String

                /*******************ENTRANDO NA TABELA DE CONTAS*********************/

                if (resposta == "1")// condicional da respota do usuario
                {
                    var entradaConta = new Entity("account");// criando variavel para receber a conexao com a tabela "account" e transformando ela em OBJ com Entity
                    Guid registroConta = new Guid(); // gera uma chave unica para nao ter problema acessando o Banco

                    // adicionando Nome na tabela Conta
                    Console.Write("Insira o nome da Empresa: "); //mostra mensagem na tela para o usuario informar dados
                    var lerNome = InicialMaiuscula(Console.ReadLine()); // cria uma variavel para armazenar o dado lido na linha de cima
                    entradaConta.Attributes.Add("name", lerNome); //  pega a conexao feita com a tabela e adiciona um atributo na coluna "nome" e insere o dado informado pelo usuario

                    // adicionando Telefone na tabela Conta
                    Console.Write("\nInsira o Telefone: ");
                    var lerCel = InicialMaiuscula(Console.ReadLine());
                    entradaConta.Attributes.Add("telephone1", lerCel);

                    // adicionando País na tabela Conta
                    Console.Write("\nInsira o País: ");
                    var lerPais = InicialMaiuscula(Console.ReadLine());
                    entradaConta.Attributes.Add("address1_country", lerPais);

                    registroConta = serviceProxy.Create(entradaConta); // a chave Guid() é usada para acessar o CRM e criar os dados informados na tabela "account"
                    Console.WriteLine("\t\t\t CONTA CRIADA COM SUCESSO!!\t\t\t\n"); // mensagem exibida na tela
                }



                /*******************ENTRANDO NA TABELA DE CONTATOS*********************/

                else if (resposta == "2") // condicional da resposta do usuario
                {
                    var entradaUsuario = new Entity("contact");// criando variavel para receber a conexao com a tabela "contact" e transformando ela em OBJ com Entity
                    Guid registroUsuario = new Guid(); // gera uma chave unica para nao ter problema acessando o Banco

                    // adicionando Nome na tabela Contatos
                    Console.Write("Insira seu Nome: ");//mostra mensagem na tela para o usuario informar dados
                    var lerNome = InicialMaiuscula(Console.ReadLine());// cria uma variavel para armazenar o dado lido na linha de cima
                    entradaUsuario.Attributes.Add("firstname", lerNome);//  pega a conexao feita com a tabela e adiciona um atributo na coluna "nome" e insere o dado informado pelo usuario

                    // adicionando Sobrenome na tabela Contatos
                    Console.Write("\nInsira o Sobrenome: ");
                    var lerSobrenome = InicialMaiuscula(Console.ReadLine());
                    entradaUsuario.Attributes.Add("lastname", lerSobrenome);

                    // adicionar o CPF
                    Console.Write("\nInsira o CPF: ");
                    var lerCpf = InicialMaiuscula(Console.ReadLine());
                    entradaUsuario.Attributes.Add("new_cpf", lerCpf);

                    // adicionando Cargo na tabela Contatos
                    Console.Write("\nInsira seu Cargo: ");
                    var cargo = InicialMaiuscula(Console.ReadLine());
                    entradaUsuario.Attributes.Add("jobtitle", cargo);

                    registroUsuario = serviceProxy.Create(entradaUsuario);// a chave Guid() é usada para acessar o CRM e criar os dados informados na tabela "account"
                    Console.WriteLine("\t\t\t CONTATO CRIADo COM SUCESSO!!\t\t\t\n"); // mensagem exibida na tela

                }
                else
                {
                    Console.WriteLine("\t\t\t DADOS INVALIDOS\t\t\t\n"); // mensagem exibida na tela
                }

                Console.WriteLine("Deseja realizar mais cadastros? (s/n)");// mensagem exibida na tela para usuario informar se deseja realizar mais cadastros
                var condicao = Console.ReadLine();// cria uma variavel para armazenar o dado lido na linha de cima
                situacao = condicao.ToUpper();// a "situacao" do WHILE recebe o valor informado pelo usuario e "ToUper" transforma oque o usuario insere para MAIUSCULO.
            }
        }
        #endregion

        #region Alterar Registro de Conta
        static void AlterarConta(CrmServiceClient serviceProxy)
        {
            Console.Write("Nº Conta..: "); // usuaio informa numero de conta
            String accountnumber = Console.ReadLine(); // cria uma string accountnumber para salvar o dado informado pelo usuario

            QueryExpression accountQuery = new QueryExpression("account"); // fazer uma busca na tabela "account" e salvar na accountQuery que criamos
            accountQuery.Criteria.AddCondition("accountnumber", ConditionOperator.Equal, accountnumber); // realiza uma busca no criterio e adiciona uma condicao no campo "accountnumber" deve ser igual a informaçao que o usuario informou e foi salvo na string accountnumber
            EntityCollection contactColl = serviceProxy.RetrieveMultiple(accountQuery);// gera uma coleçao de entidades do nome contactColl e faz a ligaçao ao CRM e recupera a busca feita na tabela "account"

            if (contactColl.Entities.Count > 0) // condicional se a colecao de entidades é > que 0
            {
                OrganizationServiceContext context = new OrganizationServiceContext(serviceProxy); // faz busca de contexto direto no CRM
                var resultados = from a in context.CreateQuery("account") // RESULTADOS vai filtra o dado da busca (parecido como SQL)
                                 where ((string)a["accountnumber"]) == accountnumber
                                 select a;
                foreach (var item in resultados) // para cada resultado
                {
                    Console.WriteLine("\n    Registro Encontrado!" + // mensagem exibida
                                      "\n    Nome Conta.: " + item.Attributes["name"] + // mensagem mais nome da conta
                                      "\n    Nº Conta...: " + item.Attributes["accountnumber"]); // mensagem mais accountnumber

                    Console.WriteLine("\n    Alterar Registro:"); // mensagem exibida
                    Console.Write("    Nome Conta.: "); String nameNew = Console.ReadLine(); // mensagem mais guardar o dado na string nameNew
                    Console.Write("    Nº Conta...: "); String accountnumberNew = Console.ReadLine(); // mensagem mais guardar o dado na string accountnumberNew

                    QueryExpression verificaraccountQuery = new QueryExpression("account"); // fazer uma busca na tabela "account" e salvar na verificaraccountQuery que criamos
                    verificaraccountQuery.Criteria.AddCondition("accountnumber", ConditionOperator.Equal, accountnumberNew); // realiza uma busca no criterio e adiciona uma condicao no campo "accountnumber" deve ser igual a informaçao que o usuario informou e foi salvo na string accountnumberNew
                    EntityCollection verificarcontactColl = serviceProxy.RetrieveMultiple(verificaraccountQuery);// gera uma coleçao de entidades do nome contactColl e faz a ligaçao ao CRM e recupera a busca feita na tabela "account"

                    if (verificarcontactColl.Entities.Count > 0) // condicional se a colecao de entidades é > que 0
                    {
                        Console.WriteLine("Alteração Abortada!"); // exibir mensagem na tela
                    }
                    else //ENTAO ele atribui valores e sobe no banco 
                    {
                        item.Attributes["name"] = nameNew;
                        item.Attributes["accountnumber"] = accountnumberNew;
                        context.UpdateObject(item);
                        context.SaveChanges();
                        Console.WriteLine("Sucesso!");
                    }
                }
            }
            else
            {
                Console.WriteLine("Erro!"); // mensagem de erro
            }
        }
        #endregion


        /*Em contato adicionar o campo CPF, tipo string OK

        Em contas adicionar o campo CNPJ, tipo string.  OK

        Adicionar em contas e em contatos o campo Endereço 1 Bairro  OK

        Retirar de contas os campos Endereço 1 Rua 2 e Endereço 1 Rua 3  

        Em contas adicionar o campo capital social, do tipo moeda.  



        Inserir, manualmente mesmo, pelo menos 3 contas com pelo menos os seguintes campos preenchidos: endereço 1 completo, CNPJ, capital social.

        Inserir, manualmente mesmo, pelo menos 5 contatos de cargos diferentes e marcar em uma dessas 3 contas.Colocar telefone nesses contatos.




        Pesquisar e listar os contatos das contas que tenham capital social acima de R$100.000,00

        Retornar um relatório com o nome da conta, CNPJ, o capital social.

        Dentro desse, retornar os contatos, com o nome, o cargo e o telefone.

        Fazer esta consulta usando o QueryExpression, Linq e FetchXML.*/


    }
}






