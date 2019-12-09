using System.Collections.Generic;

namespace AdysTech.InfluxDB.Client.Net
{
    /// <summary>
    /// Represents the results returned by Query end point of the InfluxDB engine
    /// </summary>
    public interface IInfluxSeries
    {
        /// <summary>
        /// Read only List of ExpandoObjects (in the form of dynamic) representing the entries in the query result 
        /// The objects will have columns as Peoperties with their current values
        /// </summary>
        IList<dynamic> Entries { get; set; }
        
        /// <summary>
        /// Indicates whether this Series has any entries or not
        /// </summary>
        bool HasEntries { get; set; }

        /// <summary>
        /// Name of the series. Usually its MeasurementName in case of select queries
        /// </summary>
        string SeriesName { get; set; }

        /// <summary>
        /// Dictionary of tags, and their respective values.
        /// </summary>
        IDictionary<string, string> Tags { get; set; }

        /// <summary>
        /// True if the influx query was answered with a partial response due to e.g. exceeding a configured
        /// max-row-limit in the InfluxDB. As we don't know which series was truncated by InfluxDB, all series
        /// of the response will be flagged with Partial=true.
        /// </summary>
        bool Partial { get; set; }
    }
}