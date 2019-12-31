# AsyncRunner

This project is an implementation of horizontal scalability in a restful API built with .NET Core MVC.
By default an instance will start in API mode unless an environment variable `APP_ENV` is set to `"worker"`.
When the api performs async work a DistributedTask object is created with a unique `task_id` and a default status of `"Queued"`, the task is then serialized and stored in redis by its `task_id`, and the id is returned the user via http(s) response.
The API then puts the task by its name and the parameters needed for execution into a RabbitMQ Queue.

Using the competing worker pattern, the worker instances of the application will receive messages from the queue and execute them as they are available. 
As the workers perform their task, they can update their tasks status by its id using the WorkerTaskStatus class.
Using the task id, the API can query redis for the tasks updated status to display to the end user.
