$echo OFF

set eventbus=rabbitmq
set RabbitMqConfig__ExchangeKey=transfer_commands
set RabbitMqConfig__ServerUri=amqp://guest:guest@localhost:1978

set source=%1
set destination=%2

Release\RdxFileTransfer.exe -s %source% -d %destination%