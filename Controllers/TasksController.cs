using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using async_runner.Helpers;
using System.Text.Json;
using System.Text.Json.Serialization;
using async_runner.Worker;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace async_runner.Controllers
{


    [Route("api/tasks/")]
    [ApiController]
    [Produces("application/json")]


    public class TasksController : ControllerBase
    {
        private string GetJsonFromStream(Stream body){
            var json = new StreamReader(Request.Body).ReadToEnd();
            return json;
        }
        private TaskDispatcher dispatcher = new TaskDispatcher();

        [HttpGet("{id}")]
        public WorkerTaskStatus Get(string id)
        {
            WorkerTaskStatus status = new WorkerTaskStatus(id, "unknown");
            status.RefreshStatus();
            return status;
        }

        
        [HttpPost]
        public object Post(){
            DistributedTask dt;
            try{
                dt = JsonConvert.DeserializeObject<DistributedTask>(
                    GetJsonFromStream(Request.Body)
                );
            } catch (Exception e){
                Response.StatusCode = 400;
                var respdict = new Dictionary<string, object>();
                respdict.Add("status", 400);
                respdict.Add("error", "Failed to parse input.");
                return respdict;
            }
            
            var taskdata = dt.Serialize();
            dispatcher.DispatchMessage(taskdata);
            var status = new WorkerTaskStatus(dt.task_id);
            status.UpdateStatus("Queued");
            return status;
        }

    }
}
