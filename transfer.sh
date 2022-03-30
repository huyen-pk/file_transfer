export eventbus=rabbitmq
export RabbitMqConfig__ExchangeKey=transfer_commands
export RabbitMqConfig__ServerUri=amqp://guest:guest@localhost:1978

while getopts s:d: flag
do
    case "${flag}" in
        s) source=${OPTARG};;
        d) destination=${OPTARG};;
    esac
done

Release/RdxFileTransfer.exe -s $source -d $destination