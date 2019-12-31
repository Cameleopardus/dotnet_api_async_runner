rabbitmqctl add_user asyncrunner localdev
rabbitmqctl set_user_tags  administrator
rabbitmqctl set_permissions -p / asyncrunner ".*" ".*" ".*"