FROM rabbitmq:management
RUN rabbitmq-plugins enable rabbitmq_management  --offline
RUN rabbitmq-plugins list
ADD ./rabbitmqconf /etc/rabbitmq
