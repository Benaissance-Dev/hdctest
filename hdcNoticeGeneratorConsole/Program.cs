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

            object lockObj= new object();

            string rmqHostName = ConfigurationManager.AppSettings["rmqhost"];
            string rmqUserName = ConfigurationManager.AppSettings["rmquser"];
            string rmqPassword = ConfigurationManager.AppSettings["rmqpassword"];

            string uri = $"amqp://{rmqHostName}:45672/";
            string exchange = "hdc";

            long messageCounter = 0;

            Console.WriteLine(uri);

            using (var connection = new ConnectionFactory { Uri = uri, UserName = rmqUserName, Password = rmqPassword }.CreateConnection())
            {

                var containerName = $"{Dns.GetHostName()}_{DateTime.Now.ToString("yyyyMMddhhmmss")}";

                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange, ExchangeType.Fanout, true, false, null);
                    channel.QueueDeclare(exchange, true, false, false, null);
                    channel.QueueBind(exchange, exchange, "");
                    channel.BasicQos(0, 1, true);

                    while (true)
                    {
                        var consumer = new EventingBasicConsumer(channel);
                        consumer.Received += (model, ea) =>
                        {
                            lock (lockObj)
                            {
                                var body = ea.Body;
                                var message = Encoding.ASCII.GetString(body);
                                Console.WriteLine($"Received message:{message}");

                                //process a PDF here
                                Console.WriteLine($"Generating PDF for {message}");
                                var msg = $"{messageCounter}_{message}";
                                NoticeGen gen = new NoticeGen(msg);

                                gen.Generate($"{messageCounter}_{containerName}");
                                Console.WriteLine($"Completed PDF generation for {message}");

                                messageCounter++;

                                Console.WriteLine($"Processed {messageCounter} messages on {Dns.GetHostName()}");
                            }
                            //send completion message?  with stats?

                        };
                        channel.BasicConsume(queue: exchange, noAck: true, consumer: consumer);

                        System.Threading.Thread.Sleep(10);
                    }
                }
            }

       
        }
    }
}
