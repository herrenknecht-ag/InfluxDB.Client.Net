#### Custom fork

**Please refer to the original repository at** https://github.com/AdysTech/InfluxDB.Client.Net **to get a tested and stable version of this library!*** 

This is a fork of https://github.com/AdysTech/InfluxDB.Client.Net . There's currently quite some work under development in this fork:
- Inclusion of the "partial" flag from InfluxDB responses
- Support of MultiQueries (returning multiple results) in one Influx request

Those topics are currently under development and probably not (yet) ready for use outside of our own narrow use cases. Feel free to take a glance and copy the code or a modified version of it. No guarantee however that it will work in your case.

#### Original README:

**Now supports .Net Core, run same .Net code in Windows and Linux**

#### Nuget

.Net Standard 2.0 and .Net 4.5.1 > [AdysTech.InfluxDB.Client.Net.Core](https://www.nuget.org/packages/AdysTech.InfluxDB.Client.Net.Core/) _(this version now supports both .NET Framework and .NET Core)_

# InfluxDB.Client.Net

[InfluxDB](http://influxdb.com) is new awesome open source time series database. But there is no official .Net client model for it. This is a feature rich .Net client for InfluxDB. All methods are exposed as Async methods, so that the calling thread will not be blocked. 
It currently supports

1. Connecting using credentials
2. Querying all existing databases
3. Creating new database
4. Querying for the whole DB structure (hierarchical structure of all measurements, and fields)
5. Writing single, multiple values to db
6. Retention policy management
7. Post data to specific retention policies
8. Query for all Continuous Queries
9. Create a new Continuous Query
10. Drop continuous queries
11. Chunked queries
12. Drop database
13. Delete entire or partial measurement data
14. Series and point count for each of the measurements

To be added are

1. Query for all tags, their unique values



### Usage

#### Creating Client

```Csharp
InfluxDBClient client = new InfluxDBClient (influxUrl, dbUName, dbpwd);
```

#### Querying all DB names

```Csharp
List<String> dbNames = await client.GetInfluxDBNamesAsync ();
```

#### Querying DB structure

```Csharp
Dictionary<string, List<String>> = await client.GetInfluxDBStructureAsync("<db name>");
```

This returns a hierarchical structure of all measurements, and fields (but not tags)!

#### Create new database

```Csharp
bool success = await client.CreateDatabaseAsync("<db name>");
```

Creates a new database in InfluxDB. Does not raise exceptions if the DB already exists.

#### Type safe data points

##### `InfluxDatapoint<T>`

It represents a single Point (collection of fields) in a series. In InfluxDB each point is uniquely identified by its series and timestamps.
Currently this class (as well as InfluxDB as of 0.9) supports `Boolean`, `String`, `Integer` and `Double` (additionally `decimal` is supported in client, which gets stored as a double in InfluxDB) types for field values.
Multiple fields of same type are supported, as well as tags.

#### Writing data points to DB

```Csharp
var valMixed = new InfluxDatapoint<InfluxValueField>();
valMixed.UtcTimestamp = DateTime.UtcNow;
valMixed.Tags.Add("TestDate", time.ToShortDateString());
valMixed.Tags.Add("TestTime", time.ToShortTimeString());
valMixed.Fields.Add("Doublefield", new InfluxValueField(rand.NextDouble()));
valMixed.Fields.Add("Stringfield", new InfluxValueField(DataGen.RandomString()));
valMixed.Fields.Add("Boolfield", new InfluxValueField(true));
valMixed.Fields.Add("Int Field", new InfluxValueField(rand.Next()));

valMixed.MeasurementName = measurementName;
valMixed.Precision = TimePrecision.Seconds;
valMixed.Retention = new InfluxRetentionPolicy() { Name = "Test2" };

var r = await client.PostPointAsync(dbName, valMixed);
```

A collection of points can be posted using `await client.PostPointsAsync (dbName, points)`, where `points` can be collection of different types of `InfluxDatapoint`

#### Writing strongly typed data points to DB

```Csharp
class YourPoint
{
    [InfluxDBMeasurementName]
    public string Measurement { get; set; }

    [InfluxDBTime]
    public DateTime Time { get; set; }

    [InfluxDBPrecision]
    public TimePrecision Precision { get; set; }

    [InfluxDBRetentionPolicy]
    public InfluxRetentionPolicy Retention { get; set; }

    [InfluxDBField("StringFieldName")]
    public string StringFieldProperty { get; set; }

    [InfluxDBField("IntFieldName")]
    public int IntFieldProperty { get; set; }

    [InfluxDBField("BoolFieldName")]
    public bool BoolFieldProperty { get; set; }

    [InfluxDBField("DoubleFieldName")]
    public double DoubleFieldProperty { get; set; }

    [InfluxDBTag("TagName")]
    public string TagProperty { get; set; }

}

var point = new YourPoint
{
    Time = DateTime.UtcNow,
    Measurement = measurementName,
    Precision = TimePrecision.Seconds,
    StringFieldProperty = "FieldValue",
    IntFieldProperty = 42,
    BoolFieldProperty = true,
    DoubleFieldProperty = 3.1415,
    TagProperty = "TagValue",
    Retention = new InfluxRetentionPolicy() { Name = "Test2" };
};

var r = await client.PostPointAsync(dbName, point);
```

This supports all types `InfluxValueField` supports. Additionally it supports tags other than strings, as long as they can be converted to string.

The parameter that has `InfluxDBRetentionPolicy` applied to it can be an `IInfluxRetentionPolicy` alternatively it will be treated as a string and will be used as the name of the retention policy.

A collection of points can be posted using `await client.PostPointsAsync<T>(dbName, points)`, where `points` can be collection of arbitrary type `T` with appropriate attributes

#### Query for data points

```Csharp
var r = await client.QueryMultiSeriesAsync ("_internal", "select * from runtime limit 10");
var s = await client.QueryMultiSeriesAsync("_internal", "SHOW STATS");
var rc = await client.QueryMultiSeriesAsync ("_internal", "select * from runtime limit 100",10);
```

`QueryMultiSeriesAsync` method returns `List<InfluxSeries>`, `InfluxSeries` is a custom object which will have a series name, set of tags (e.g. columns you used in `group by` clause. For the actual values, it will use dynamic object(`ExpandoObject` to be exact). The example #1 above will result in a single entry list, and the result can be used like `r.FirstOrDefault()?.Entries[0].time`. This also opens up a way to have an update mechanism as you can now query for data, change some values/tags etc, and write back. Since Influx uses combination of timestamp, tags as primary key, if you don't change tags, the values will be overwritten.

Second example above will provide multiple series objects, and allows to get data like `r.FirstOrDefault(x=>x.SeriesName=="queryExecutor").Entries[0].QueryDurationNs`.

The last example above makes InfluxDB to split the selected points (100 limited by `limit` clause) to multiple series, each having 10 points as given by `chunk` size.


#### Query for strongly typed data points

```Csharp
var r = await client.QueryMultiSeriesAsync<YourPoint>(dbName, $"select * from {measurementName}");
```

`QueryMultiSeriesAsync<T>` method returns `List<InfluxSeries<T>>`, `InfluxSeries<T>` behaves similar to `InfluxSeries` except Entries are not dynamic but strongly typed as T. It uses the same attributes as used for writing the points to match the fields and tags to the matching properties. If a matching property can't be found, the field is discarded. If a property doesn't have a matching field, it's left empty.

It supports multiple chuncks similarly to `QueryMultiSeriesAsync`.


#### Retention Policies

This library uses a cutsom .Net object to represent the Influx Retention Policy. The `Duration` concept is nicely wraped in `TimeSpan`, so it can be easily manipulated using .Net code. It also supports `ShardDuration` concept introduced in recent versions of InfluxDB.

##### Get all Retention Policies from DB

```Csharp
var policies = await client.GetRetentionPoliciesAsync (dbName);
```

##### Create Retention Policy and Write points to a specific retention policy

The code below will create a new retention policy with a retention period of 6 hours, and write a point to that policy.

```Csharp
var rp = new InfluxRetentionPolicy () { Name = "Test2", DBName = dbName, Duration = TimeSpan.FromHours (6), IsDefault = false };
if (!await client.CreateRetentionPolicyAsync (rp))
    throw new InvalidOperationException ("Unable to create Retention Policy");

valMixed.MeasurementName = measurementName;
valMixed.Precision = TimePrecision.Seconds;
valMixed.Retention = new InfluxRetentionPolicy () { Name = "Test2"}; //or you can just assign this to rp
var r = await client.PostPointAsync (dbName, valMixed);
```

#### Continuous Queries

Similar to retention policy the library also creates a custom object to represent the `Continuous Queries`. Because the queries can be very complex, the actual query part of the CQ is exposed as just a string. But the timing part of [CQ](https://docs.influxdata.com/influxdb/v1.0/query_language/continuous_queries) specifically <intervals> given in `[RESAMPLE [EVERY <interval>] [FOR <interval>]]` are exposed as `TimeSpan` objects. Since the `GROUP BY time(<interval>)` is mandated for CQs this interval again is exposed as `TimeSpan`. This allows you to run LINQ code on collection of CQs.

##### Get all Continuous Queries present in DB

```Csharp
var cqList = await client.GetContinuousQueriesAsync ();
```

##### Create a new Continuous Query

```Csharp
var p = new InfluxContinuousQuery () { Name = "TestCQ1",
                            DBName = dbName,
                            Query = "select mean(Intfield) as Intfield into cqMeasurement from testMeasurement group by time(1h),*",
                            ResampleDuration = TimeSpan.FromHours (2),
                            ResampleFrequency = TimeSpan.FromHours (0.5) };
var r = await client.CreateContinuousQueryAsync (p);
```

##### Drop a Continuous Query

```Csharp
var r = await client.DropContinuousQueryAsync (p);
```

`p` has to be an existing CQ, which is already saved.


##### Drop a Database

```Csharp
var r = await client.DropDatabaseAsync (db);
```

`db` has to be an `InfluxDatabase` instance, which can be created via `new InfluxDatabase(dbName)`


##### Drop a Measurement

```Csharp
var r = await client.DropMeasurementAsync (m);
```

`m` has to be an `InfluxMeasurement` instance, which can be created via `new InfluxMeasurement(measurementName)`

##### Delete from Measurement with optional `where` clause

```Csharp
 r = await client.DeletePointsAsync(
                    new InfluxDatabase(dbName), 
                    new InfluxMeasurement(measurement), 
                    whereClause: new List<string>() { 
                        "purge = yes", 
                        $"time() > {DateTime.UtcNow.AddDays(-4).ToEpoch(TimePrecision.Hours)}"
                    });

Currently `where` clause is just a list of conditions, without any intelligence. SO time conversion, quoting should be handled by the consumer.
