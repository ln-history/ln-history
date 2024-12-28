# Lightning Network History

## Summary
This project provides extensive methods to query a [QuestDb](https://questdb.io/) database.

It provides similar functionality like the TimeMachine-Tool built by [Christian Decker]()

The schema of the database consists of three tables:

- node_announcements
- channel_announcements
- channel_updates

The goal of this project is to provide tooling for analyzing the history of the Lightning Network.

## Data
The data used for this project has been collected by [Christian Decker]() using his Lightning-Network-Research--Topology [repository](https://github.com/lnresearch/topology). 

## Technologies
This project uses Microsofts open-source framework [.NET]().
To abstract the ORM to the database the open-source tool [Dapper]() is used.
A [QuestDb](https://questdb.io/) database is used where the two tables that contain timestamps have been indexed for fast queries.

## Development

To start first create a `appsettings.Developement.json` file. 
Paste the following contents into the file:
```
{
  "ConnectionStrings": {
    "QuestDb": "host=<your-host-url>;port=8812;username=<your-username>;password=<your-password>;Database=questdb"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```
Note that you need to have a questDb instance running.

## Contribution
This project is an open-source project that is right now under heavy development. 
As soon as a stable v1 is released feel free to contribute. 

## LICENSE
Apache 2.0 - see `LICENSE` for more information.

2018-04-20T05:01:56.000000Z
