using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace hdcNoticeGeneratorConsole
{
    class Program
    {
        static void Main(string[] args)
        {

            Stopwatch stp = new Stopwatch();
            stp.Start();

            string rmqHostName = ConfigurationManager.AppSettings["rmqhost"];
            string rmqUserName = ConfigurationManager.AppSettings["rmquser"];
            string rmqPassword = ConfigurationManager.AppSettings["rmqpassword"];

            string uri = $"amqp://{rmqHostName}:45672/";
            string exchange = "hdc";

            Console.WriteLine(uri);

            using (var connection = new ConnectionFactory { Uri = uri, UserName = rmqUserName, Password = rmqPassword }.CreateConnection())
            {

                var containerName = $"{Dns.GetHostName()}_{DateTime.Now.ToString("yyyyMMddhhmmss")}";

                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange, ExchangeType.Fanout, true, false, null);
                    channel.QueueDeclare(exchange, true, false, false, null);
                    channel.QueueBind(exchange, exchange, "");
                    while (true)
                    {
                        var consumer = new EventingBasicConsumer(channel);
                        consumer.Received += (model, ea) =>
                        {
                            var body = ea.Body;
                            var message = Encoding.ASCII.GetString(body);
                            Console.WriteLine($"Received message:{message}");

                            //process a PDF here
                            Console.WriteLine($"Generating PDF for {message}");
                            var msg = message;
                            NoticeGen gen = new NoticeGen(msg);

                            gen.Generate(containerName);
                            Console.WriteLine($"Completed PDF generation for {message}");

                            //send completion message?  with stats?

                        };
                        channel.BasicConsume(queue: exchange, noAck: true, consumer: consumer);

                        System.Threading.Thread.Sleep(10);
                    }
                }
            }

            //subscribe
            

            Console.WriteLine("Hello World...!");
            Console.WriteLine("Hello World...!");
            Console.WriteLine("Hello World...!");
            Console.WriteLine("Hello World...!");
            Console.WriteLine("Hello World...!");
        }
    }
}
