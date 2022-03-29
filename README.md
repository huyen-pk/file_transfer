# File transfer
Transferring files from one folder to another asynchronously

## Assumptions
- When a file is transferred, a copy of this file is placed under the new folder, keeping the original intact.
- Files of the same type are processed sequentially, even if issued from different folders.
- A message queue is issued for each file type (per file extension). Each queue is processed by one worker process.
- When transferred files from subfolders, the subfolder structure is not maintained. 
All files would be copied into the destination folder without taking into account the subfolder structure.

## Architecture
- RdxFileTransfer : receives input from users and queue it for background processing.

- RdxFileTransfer.EventBus: a common interface used for message transporting.
The current implementation only provides RabbitMq, but it is possible to extend the library to support different types of event bus (rabbitmq, azure...).

- RdxFileTransfer.Scheduler: receives messages from  the task queue (transfer command from users) and schedule workers to process these commands.
The scheduler runs in the a separate process and listens to the task queue. When a message arrives:
    1. Create a worker process to scan the folder.
    2. The scanner creates a queue per file extension and queue each file to its respective queue.
    3. The scheduler then creates a worker process for each file extention (if there is no worker currently handling this file type).
    4. These transfer workers listen to the file queues and transfer files as they process these events.

## How to run the application
Run these scripts in order:

```sh
setup
```

```sh
transfer
```

- The 'setup' scripts:
    1. Build projects in the solution and publish it to Release folder.
    2. Set environments variables to default values.
    3. Starts a RabbitMq server in container, listens at port 1978.
    4. Starts the scheduler.

- The 'transfer' scripts runs RdxFileTransfer application which then starts listen to user input.

- The default event bus is RabbitMq. You can choose the event bus using -b option when running RdxFileTransfer and RdxFileTransfer.Scheduler or by setting environment variable 'eventbus'.

**Options**

*Start up*
-b, --bus
Event bus

*Run*
-s, --source
Source folder

-d, --destination
Destination folder

