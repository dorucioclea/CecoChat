# Overall design

![Overall design](images/cecochat-01-overall.png)

* Clients register and manage their profile in the User service via the BFF
* Clients create their session using the User service via the BFF
* Clients acquire other users public profiles from the User service via the BFF
* Clients acquire user chats and chat history from the State and History services via the BFF
* Clients connect to Messaging service in order to chat with each other
* Messaging servers exchange data between each other indirectly using a PUB/SUB backplane
* PUB/SUB backplane distributes the traffic between Messaging servers
* ID Gen service is used to generate Snowflake message IDs
* State service transform the messages from the PUB/SUB backplane into State database which is the source of truth 
* History service transform messages and reactions from the PUB/SUB backplane into a History database which is the source of truth
* Splitting the different responsibilities between separate services allows for independent scaling
* The services use dynamic configuration which is updated centrally
* Observability is achieved via health checks, distributed tracing, log aggregation and metrics with monitoring
* Deployment infrastructure takes care of failover, growth/shrinkage of the different services based on load and predictive analytics

More details on the design of each aspect:

* [Messaging](design-messaging.md)
* [User chats, chat history](design-state-history.md)
* [User profiles](design-users.md)
* [Clients](design-clients.md)
* [Configuration](design-configuration.md)
* [Observability](design-observability.md)
* [Deployment](design-deployment.md)

All the diagrams are in the [diagrams](diagrams) folder and [draw.io](https://app.diagrams.net/) is needed in order to view them. From the `Help` item in the menu a desktop tool could be downloaded, if preferred. Currently this is the [link with the releases](https://github.com/jgraph/drawio-desktop/releases).
