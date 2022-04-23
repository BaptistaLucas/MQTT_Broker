using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MQTTSubscriber
{
    class Subscriber
    {
        static async Task Main(string[] args)
        {
            var mqttFactory = new MqttFactory();
            IMqttClient client = mqttFactory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder()
                        .WithClientId(Guid.NewGuid().ToString())
                        .WithTcpServer("test.mosquitto.org", 1883)
                        .WithCleanSession()
                        .Build();
            client.UseConnectedHandler(async e =>
            {
                Console.WriteLine("Conectado ao broker com sucesso ");
                var topicFilter = new TopicFilterBuilder()   //assim que conectado passa o topico de mesmo nome do Publisher que ele ira assinar
                            .WithTopic("Lucas")
                            .Build();

                await client.SubscribeAsync(topicFilter); //assinando o topico
            });

            client.UseDisconnectedHandler(e =>
            {
                Console.WriteLine("Desconectado do broker com sucesso");
            });

            client.UseApplicationMessageReceivedHandler(async e => //manipula as mensagem recebidas 
            {
                //como payload é uma matriz de byte precisamos converter em string
                Console.WriteLine($"mensagem recebida {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}\n");
                var enviar = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                if(enviar!=null) // se a enviar estiver diferente de nulo envia para o webhook 
                {
                    var httpCliente = new HttpClient();
                    var objeto = new { mensagem = enviar };
                    var content = ToRequest(objeto);
                    var response = await httpCliente.PostAsync("https://localhost:44370/v1/MensagemBroker", content);

                }

            });

            
            await client.ConnectAsync(options);

            Console.ReadLine();

            //await client.DisconnectAsync();


        }

  
        private static StringContent ToRequest(object obj) // metodo para serealizar em json
        {
            var json = JsonConvert.SerializeObject(obj);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            return data;
        }
    }
}
