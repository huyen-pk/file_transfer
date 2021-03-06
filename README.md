[![.NET](https://github.com/huyen-pk/file_transfer/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/huyen-pk/file_transfer/actions/workflows/dotnet.yml)

# File transfer
Transferring files from one folder to another asynchronously.

## Assumptions
- When a file is transferred, a copy of this file is placed under the new folder, keeping the original intact.
- Files of the same type are processed sequentially, even if issued from different folders.
- A message queue is issued for each file type (per file extension). Each queue is processed by one worker process.
- When transferred files from subfolders, the subfolder structure is not maintained. 
All files would be copied into the destination folder without taking into account the subfolder structure.

## Dependencies
- RabbitMq
- Docker

## Architecture

<div style='text-align:center'><img src='system_design.svg' style='width:400px'/></div>

```
├───RdxFileTransfer
│   ├───Options
├───RdxFileTransfer.EventBus
│   ├───BusEvents
│   ├───Constants
│   ├───Enums
│   └───RabbitMq
└───RdxFileTransfer.Scheduler
    ├───Options
    ├───Orchestrator
    └───Workers
```

- ```RdxFileTransfer``` : receives input from users and queue it for background processing.

- ```RdxFileTransfer.EventBus```: a common interface used for message transporting.
The current implementation only provides RabbitMq, but it is possible to extend the library to support different types of event bus (activemq, azure...).

- ```RdxFileTransfer.Scheduler```: receives messages from  the task queue (transfer commands from users) and schedules workers to process these commands.
The scheduler runs in a separate process and listens to the task queue. When a message arrives:

<ol style='list-style-position: inside;margin-left:35px'>
    <li>Creates a worker process to scan the folder.</li>
    <li>The scanner creates a queue for each file extension and queues each file to its respective queue.</li>
    <li>The scheduler then creates a worker process for each file extention (if there is no worker currently handling this file type).</li>
    <li>These transfer workers listen to the file queues and transfer files as they process these events.</li>
</ol>

## Message queue structure
<table border="0">
    <tr>
        <td><b>Queue</b></td>
        <td><b>Events</b></td>
    </tr>
    <tr>
        <td>transfer_jobs</td>
        <td>User commands to transfer folders.</td>
    </tr>
    <tr>
        <td>success</td>
        <td>Successfully transferred files.</td>
    </tr>
    <tr>
        <td>error</td>
        <td>All errors of reading and copying files or handling folders.</td>
    </tr>
    <tr>
        <td>no_ext</td>
        <td>Files without extension waiting to be transferred.</td>
    </tr>
    <tr>
        <td>docx, pdf, txt...</td>
        <td>Files with a specific extension waiting to be transferred.</td>
    </tr>
</table>

**Message queue behavior**
- Queue is durable so events are not lost in case of abrupt shutdown.

- A message is removed from queue once a worker starts reading it. This is only to simplify our task flows, not to avoid race condition. Because we only have one worker per queue so a race condition should not happen.

## How to run the application
Run the first script to setup environment with default values. You must leave the console open afterwards so that the scheduler can process messages.

```sh
setup
```
In another console windows, run transfer script to start application:

```sh
transfer [sourceFolder] [destinationFolder]
```

- The ```setup``` script:
<ol style='list-style-position: inside;margin-left:35px'>
    <li>Builds projects in the solution and publishes it to <i>Release</i> folder.</li>
    <li>Sets environment variables to default values.</li>
    <li>Starts a RabbitMq server in container, listens at port 1978.</li>
    <li>Starts the scheduler.</li>
</ol>

- The ```transfer``` script runs RdxFileTransfer application which then starts listening to user input.

- The default event bus is RabbitMq. You can choose the event bus using ```-b``` option when running RdxFileTransfer and RdxFileTransfer.Scheduler or by setting environment variable ```eventbus```.
<table border="0">
 <tr>
    <td><b>Environment variables</b></td>
    <td><b>Usage</b></td>
 </tr>
  <tr>
    <td>RabbitMqConfig__ExchangeKey</td>
    <td>RabbitMq exchange.<br>
    Default to <i>transfer_commands</i>.</td>
 </tr>
   <tr>
    <td>RabbitMqConfig__ServerUri</td>
    <td>RabbitMq server uri. Default to <br>
    <i>amqp://guest:guest@localhost:1978</i></td>
 </tr>
  </tr>
   <tr>
    <td>eventbus</td>
    <td>Type of event bus.<br>
    Default to <i>rabbitmq</i>.</td>
 </tr>
</table>

<table border="0">
 <tr>
    <td><b>RdxFileTransfer.Scheduler Option</b></td>
    <td><b>Usage</b></td>
 </tr>
 <tr>
    <td>-b, --bus</td>
    <td>Event bus. Default to <i>rabbitmq</i>.</td>
 </tr>
</table>

<table border="0">
 <tr>
    <td><b>RdxFileTransfer Option</b></td>
    <td><b>Usage</b></td>
 </tr>
  <tr>
    <td>-s, --source</td>
    <td>Source folder, app start and when promted at run time</td>
 </tr>
   <tr>
    <td>-d, --destination</td>
    <td>Destination folder, app start and when promted at run time</td>
 </tr>
</table>

## Notices
- Linux supported but not fully tested.

## Further topics to explore
- Implement health monitor and autorecovery.
- Extend IEventBus to support other types of event bus (azure bus, ActiveMq...).
- Extend IOrchestrator to support other task flows.
- Process error and success queues.
- Integration tests.
- More unit tests.

