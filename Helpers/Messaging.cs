using System.Runtime.InteropServices.ComTypes;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Impl;
using System;
using System.Text;
using System.Threading;
using async_runner.Worker;

namespace async_runner.Helpers
{


    public class DefaultRabbitConnectionInfo{
        public static string DEFAULT_HOSTNAME = "localhost";
        public static string DEFAULT_USERNAME = "asyncrunner";
        public static string DEFAULT_PASSWORD = "localdev";
        public static string DEFAULT_VHOST = "/runnertasks";
    }

    public class BasicTaskHandler{
        public IConnection connection {get; set;}
        public IModel channel { get; set;}
        
        public IConnection GetConnection(){
            ConnectionFactory factory = new ConnectionFactory(){
                    HostName = DefaultRabbitConnectionInfo.DEFAULT_HOSTNAME,
                    UserName = DefaultRabbitConnectionInfo.DEFAULT_USERNAME,
                    Password = DefaultRabbitConnectionInfo.DEFAULT_PASSWORD,
                    VirtualHost = DefaultRabbitConnectionInfo.DEFAULT_VHOST
            };

            IConnection conn = factory.CreateConnection();
            return conn;
        }
        public BasicTaskHandler(){
            this.connection = GetConnection();
            this.channel = this.connection.CreateModel();
            this.channel.QueueDeclare(queue: "async_tasks", durable: true, exclusive: false, autoDelete: false, arguments: null);
            this.channel.QueueBind("async_tasks", "AsyncTasks", "async_tasks", null);
            this.channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
        }
    }

    public class TaskDispatcher : BasicTaskHandler
    {

        
        public void DispatchMessage(string message){

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "AsyncTasks",
                                    routingKey: "async_tasks",
                                    basicProperties: properties,
                                    body: body);


        }
        public TaskDispatcher(){
            connection = GetConnection();
        }



    } 
        public class TaskReceiver  : BasicTaskHandler
    {

        public IConnection connection { get; set; }
        public IConnection GetConnection()
        {
            ConnectionFactory factory = new ConnectionFactory()
            {
                
                HostName = DefaultRabbitConnectionInfo.DEFAULT_HOSTNAME,
                UserName = DefaultRabbitConnectionInfo.DEFAULT_USERNAME,
                Password = DefaultRabbitConnectionInfo.DEFAULT_PASSWORD,
                VirtualHost = DefaultRabbitConnectionInfo.DEFAULT_VHOST
            };

            IConnection conn = factory.CreateConnection();
            return conn;
        }

        
        public void StartConsumer(TaskHandler worker){

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine("Received {0}", message);
                    Console.WriteLine(worker);
                    worker.ReceiveTaskFromJson(message);
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

    
                };
                channel.BasicConsume(queue: "async_tasks", autoAck: false, consumer: consumer);
            
        }
        public TaskReceiver()
        {

        }

    }


}