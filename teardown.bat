@echo off

echo Stopping scheduler
taskkill /F /IM RdxFileTransfer.Scheduler.exe

echo Stopping RabbitMq in container...
docker-compose -f RdxFileTransfer\docker-compose.yml down
