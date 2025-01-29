# PDI Project - Distributed computation, Masstransit variant
I have chosen to do a prime numbers search. There is sequence from `rangeStart` to `rangeEnd` and this system is going 
to compute all the primes in this range so for example when user chooses `rangeStart` to be 1 and `rangeEnd` to be 10,
he is going to get result of sequence 2, 3, 5, 7.

## Requirements
- Docker
- Docker-compose
- (Maybe getting the RabbitMQ server image to your docker repository `https://hub.docker.com/_/rabbitmq/`)

## Details
The basics of this system does was described above. However there this is a distributed system so there has to be introduced
a bit more complicated logic and architecture. 

The assigment requirements was:
- Server(REST API) to N Computing Nodes
- Usage of Masstransit
- Packages(Tasks) creation and parametrization while processing those Tasks exactly once.
- Computing Nodes failure tolerance 

PrimeTask is made of:
- TaskId (UUID, string)
- BundleId (UUID, string) - This is used to distinguish User's computation requests
- RangeStart (int)        - From which number to compute
- RangeEnd (int)          - To which number
- MaxProcessingTime (int) - It is used for `chunkSize` calculation (So the PrimeTask is not computing more 
than MaxProcessingTime), I have empirically found out that it takes about `0.001ms` to compute one number. So the `chunkSize`
is calculted as `MaxProcessingTime / 0.001`

The `PrimeTask` is consumed by PDI-node(s) `PrimeTaskConsumer` and the `PrimeResult` is sent back to the 
Server via RabbitMQ queues. 

The system(Server) is continuously sending Partial Results to Client(React frontend) using SignalR. The Server's 
`PrimeResultConsumer` is also tracking(using `ConcurrentDictionary` and synchronization primitives) how many `Tasks`
with the same `BundleId` is still pending, when there's no pending tasks the Final Result(Sorted primes from all tasks) 
are sent via SignalR Hub back to the User and computation is done.

In the project there is a simple React frontend to control the system. Start with `rangeStart`: 1, `rangeEnd`: 10,
`maxProcessingTime`: 1ms (only one Task). Then try `rangeEnd`: 100 000/1 000 000/20 000 000 with different `maxProcessingTime`.
There will be different numbers of Tasks and also different time of computation(Observe frontend and also
`docker-compose` console to see what is happening). If you try to run computation with multiple 
browsers it should not be a problem thanks to `bundleId` and SignalR Hub's Groups, the users are going to be separated
not intruding each other's computation.

## Architecture
![alt text](Diagram.svg "Title")

## How to run
To run this project you will only need the Docker and Docker-compose. You need to go to PDI/PDI directory and run
`$ docker-compose up --build` (Wait for a few seconds to let RabbitMQ and Computing Nodes to connect). 
See the `docker-compose.yml` and `Dockerfile`s for more details. Frontend should be accessed on `http://localhost:3000`