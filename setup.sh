echo Building projects...
dotnet build RdxFileTransfer/RdxFileTransfer.sln --configuration Release --output Release

echo Setting up environments...
export eventbus=rabbitmq
export RabbitMqConfig__ExchangeKey=transfer_commands
export RabbitMqConfig__ServerUri=amqp://guest:guest@localhost:1978

echo Starting RabbitMq in container...
docker-compose -f RdxFileTransfer/docker-compose.yml up -d

sleep 20

echo Starting scheduler...
Release/RdxFileTransfer.Scheduler.dll