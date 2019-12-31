using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading;
using System.Text;
using System.Reflection;
using Newtonsoft.Json;
using StackExchange.Redis;


namespace async_runner.Worker {

    public class DistributedTask
    {
        public string taskname { get; set;}
        public string task_id { get; set;}
        public Dictionary<string, object> taskparams {get;set;}

        public string Serialize(){
            return JsonConvert.SerializeObject(this);
        }
        public static DistributedTask FromJson(string taskdata){

            var task = JsonConvert.DeserializeObject<DistributedTask>(taskdata);
            return task;
        }
        public DistributedTask(string taskname, Dictionary<string, object> taskparams){
  
                this.taskname = taskname;
                this.task_id = System.Guid.NewGuid().ToString();
                this.taskparams = taskparams;

        }
    }

    public class WorkerTaskStatus{
        public string task_id { get; set;}
        public string status { get; set;}
        public WorkerTaskStatus(string task_id, string status=null){
            this.task_id = task_id;
            if (status != null){
                this.status = status;
            } else {

            }
            
        }

        private IDatabase _redisconn;

        private IDatabase GetRedisConnection(){
            if (this._redisconn != null){
                return this._redisconn;
            }
            ConnectionMultiplexer muxer = ConnectionMultiplexer.Connect("localhost:6379");
            IDatabase conn = muxer.GetDatabase();
            this._redisconn = conn;
            return conn;
        }
        public void UpdateStatus(string status){
            this.status = status;
            var conn = this.GetRedisConnection();
            conn.StringSet(this.task_id, JsonConvert.SerializeObject(this), new TimeSpan(0, 5, 0));
        }
        public void RefreshStatus(){
            string statusjson = this.GetStatusAsJson();
            if (statusjson == null){
                return;
            }
            var freshtstatus = JsonConvert.DeserializeObject<WorkerTaskStatus>(statusjson);
            this.status = freshtstatus.status;
        }
        public string GetStatusAsJson(){
            var conn = this.GetRedisConnection();
            string status = conn.StringGet(this.task_id);
            return status;
        }
    }
    public class TaskHandler{
        public string ReceiveTaskFromJson(string taskdata){
            var task = DistributedTask.FromJson(taskdata);
            Console.WriteLine(task);
            return this.ReceiveTask(task);
        }
        public string ReceiveTask(DistributedTask task){
            
            Type thisType = this.GetType();
            MethodInfo taskmethod = thisType.GetMethod("_task_" + task.taskname);
            if (taskmethod == null){
                Console.WriteLine("Could not route task: {0}", task.taskname);
                return "nondeliverable";
            }
            Console.WriteLine("Performing task: {0}", task.taskname);
            Object[] tparams = new Object[1];
            tparams[0] = task;
            taskmethod.Invoke(this, tparams);
            return task.task_id;
        }

        private bool TaskIncludesRequiredParams(string[] required_params, DistributedTask task){
            bool success = true;
            foreach(string p in required_params){
                Console.WriteLine(p);
                if (!task.taskparams.ContainsKey(p)){
                    Console.WriteLine("key {0} is missing!", p );
                    success = false;
                    break;
                }
            }
            return success;
        }



        // all tasks that are to be executed from a message must follow the format of "_task_{taskname}"
        public void _task_count_by_seconds(DistributedTask task){
            var required_params = new string[1]{"seconds"};
            bool missing_params = !TaskIncludesRequiredParams(required_params, task);
            
            
            if(missing_params){
                return;
            }
            
            int seconds = int.Parse(task.taskparams["seconds"].ToString());
            if (seconds > 120){
                Console.WriteLine("120 seconds is maximum count time.");
                return;
            }
            WorkerTaskStatus status = new WorkerTaskStatus(task.task_id, "Queued");
            for (var x=1; x < seconds+1; x++){
                var newstatus = String.Format("{0}/{1} counted.", x, seconds);
                status.UpdateStatus(newstatus);
                Thread.Sleep(1000);
                Console.WriteLine(x);
            }
            Console.WriteLine("done");
            

        }

    }


}