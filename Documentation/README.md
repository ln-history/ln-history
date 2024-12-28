# Lightning Network History

This Project connects to a QuestDb (Postgres-SQL) database that has three tables containing all gossip messages of the Lightning Network.
For more information about gossip messages look under the specification [BOLT-07](https://github.com/lightning/bolts/blob/master/07-routing-gossip.md).

The data has been taken from [this](https://github.com/lnresearch/topology) repository.

The whole project has been inspired by Christian Decker.

## Model for graph

### Node
```
    public string NodeId { get; set; }
    public string Features { get; set; }
    public DateTime Timestamp { get; set; }
    public string RgbColor { get; set; }
    public string Addresses { get; set; }
```

### Edge
```
    public string Scid { get; set; }
    public string Features { get; set; }
    public string NodeId1 { get; set; }
    public string NodeId2 { get; set; }
    public DateTime Timestamp { get; set; }
    public string MessageFlags { get; set; }
    public string ChannelFlags { get; set; }
    public long CltvExpiryDelta { get; set; }
    public long HtlcMinimumMSat { get; set; }
    public long HtlcMaximumMSat { get; set; }
    public long FeeBaseMSat { get; set; }
    public long FeeProportionalMillionths { get; set; }
    public string ChainHash { get; set; }
```

## Data
The model of the gossip messages looks like the following:

- `node_announcement` message:
```
CREATE TABLE node_announcements (
    node_id STRING,
    features STRING,
    timestamp TIMESTAMP,
    rgb_color STRING,
    addresses STRING
) TIMESTAMP(timestamp);
```
- `channel_announcement` message:
```
CREATE TABLE channel_announcements (
    scid STRING,
    features STRING,
    node_id_1 STRING,
    node_id_2 STRING,
    chain_hash STRING
);
```
- `channel_update` message:
```
CREATE TABLE channel_updates (
    scid STRING,
    timestamp TIMESTAMP,
    message_flags STRING,
    channel_flags STRING,
    cltv_expiry_delta BIGINT,
    htlc_minimum_msat BIGINT,
    fee_base_msat BIGINT,
    fee_proportional_millionths BIGINT,
    htlc_maximum_msat BIGINT,
    chain_hash STRING
) TIMESTAMP(timestamp);
```

## Development

You need to add the `appsettings.Development.json` file adding the endpoint of your QuestDb database in the following section:
```
"ConnectionStrings": {
    "QuestDb": "https://YOUR-URL.com:9000;username=YOUR-USERNAME;password=YOUR-PASSWORD"
  },
```

## License

```
Copyright 2024 Fabian Kraus

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   https://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
```