using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Threading.Tasks;

namespace MQTTPublisher
{
    class Publisher
    {
        static async Task Main(string[] args)
        {
            var mqttFactory = new MqttFactory();
            IMqttClient client = mqttFactory.CreateMqttClient();
            //opções que o cliente ira usar para se conectar ao broker
            var options = new MqttClientOptionsBuilder()
                        .WithClientId(Guid.NewGuid().ToString())
                        .WithTcpServer("test.mosquitto.org", 1883) // detalhes do servidor broker para uma conexão TCP/ip usando protocolo MQTT
                        .WithCleanSession()
                        .Build();
            client.UseConnectedHandler(e =>    //evento ao conectar com o broker
            {
                Console.WriteLine("Conectado ao broker com sucesso ");
            });

            client.UseDisconnectedHandler(e =>  //evento ao Desconectar do broker
            {
                Console.WriteLine("Desconectado do broker com sucesso");
            });

            await client.ConnectAsync(options); // estabelecendo a conexão 

            Console.WriteLine("Por favor, pressione uma tecla para publicar a mensagem"); 
            Console.ReadLine();

           
            await PublishMessageAsync(client);
            Console.WriteLine("Para sair presione uma tecla");
            Console.ReadLine();
            await client.DisconnectAsync(); //desconecata apos publicao 
        }

        static async Task PublishMessageAsync(IMqttClient client) // metodo que publica a mensagem 
        {
            Console.WriteLine("Por favor, digite a mensagem");
            var mensagem = Console.ReadLine();
            string messagePayLoad = $"{mensagem} ";
            var message = new MqttApplicationMessageBuilder()
                        .WithTopic("Lucas") // definição do topico para aonde a mensagem será publicada 
                        .WithPayload(messagePayLoad)
                        .WithAtLeastOnceQoS()
                        .Build();

            if (client.IsConnected)  //verificando se o cliente está conectado 
            {
                await client.PublishAsync(message);
                Console.WriteLine($"mensagem publicada - {messagePayLoad}");
            }
        }
    }
}
