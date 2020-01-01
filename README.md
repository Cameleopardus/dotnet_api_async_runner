# AsyncRunner

This project is an implementation of horizontal scalability in a restful API built with .NET Core MVC.
By default an instance will start in API mode unless an environment variable `APP_ENV` is set to `"worker"`.
When the api performs async work a DistributedTask object is created with a unique `task_id` and a default status of `"Queued"`, the task is then serialized and stored in redis by its `task_id`, and the id is returned the user via http(s) response.
The API then puts the task by its name and the parameters needed for execution into a RabbitMQ Queue.

Using the competing worker pattern, the worker instances of the application will receive messages from the queue and execute them as they are available. 
As the workers perform their task, they can update their tasks status by its id using the WorkerTaskStatus class.
Using the task id, the API can query redis for the tasks updated status to display to the end user.

## Why not just use Worker Services?
Because it's better if you only have to write your code once.
Also this works on older versions of the .NET Framework, worker services are 3.0.

## Getting started
First things first, you'll need a RabbitMQ and Redis set up for the app to talk to. There is a docker compose configuration in the repository that will set up RMQ + Management and Redis. The default rabbitmq management ui port is 15672, and the username and password will be `asyncrunner` and `localdev`.

First start the app locally with dotnet run to set up the API instance.
In another terminal set the environment variable `APP_ENV` to "worker", and run the application.

You should now be able to POST to `localhost:5001/api/tasks/` to create a new task, provided you format your request correctly. Examples on creating new tasks and checking their status exist in the provided Postman collection.

Creating new task via POST to API:
![Creating a new task via POST](https://github.com/Cameleopardus/dotnet_api_async_runner/blob/master/.readme_images/count_by_seconds.png?raw=true)
Output from worker receiving the task:
![Output from worker receiving the task](https://github.com/Cameleopardus/dotnet_api_async_runner/blob/master/.readme_images/worker_output.png?raw=true)
Fetching task status from API:
![Fetching task status from API](https://github.com/Cameleopardus/dotnet_api_async_runner/blob/master/.readme_images/check_task_status.png?raw=true)

## Disclaimer & Licensing
Nothing in this repository is intented to be used in a production environment, none of it is intented to be used as a basis for  any type of best practices.
You may use the code for whatever you want.
