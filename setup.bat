@echo off

echo Building projects...
dotnet build RdxFileTransfer\RdxFileTransfer.sln --configuration Release --output Release

echo Setting up environments...
set eventbus=rabbitmq
set RabbitMqConfig__ExchangeKey=transfer_commands
set RabbitMqConfig__ServerUri=amqp://guest:guest@localhost:1978

echo Starting RabbitMq in container...
docker-compose -f RdxFileTransfer\docker-compose.yml up -d

SLEEP 20

echo Starting scheduler
Release\RdxFileTransfer.Scheduler.exe