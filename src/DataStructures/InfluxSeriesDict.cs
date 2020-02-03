﻿using System.Collections.Generic;

namespace AdysTech.InfluxDB.Client.Net
{
    public class InfluxSeriesDict : IInfluxSeriesDict
    {
        /// <summary>
        /// Name of the series. Usually its MeasurementName in case of select queries
        /// </summary>
        public string SeriesName { get; set; }

        /// <summary>
        /// Dictionary of tags, and their respective values.
        /// </summary>
        public IDictionary<string, string> Tags { get; set; }

        /// <summary>
        /// Indicates whether this Series has any entries or not
        /// </summary>
        public bool HasEntries { get; set; }

        /// <summary>
        /// Read only List of ExpandoObjects (in the form of dynamic) representing the entries in the query result 
        /// The objects will have columns as Peoperties with their current values
        /// </summary>
        public IList<Dictionary<string, object>> Entries { get; set; }

        /// <summary>
        /// True if the influx query was answered with a partial response due to e.g. exceeding a configured
        /// max-row-limit in the InfluxDB. As we don't know which series was truncated by InfluxDB, all series
        /// of the response will be flagged with Partial=true.
        /// </summary>
        public bool Partial { get; set; }

        public InfluxSeriesDict() { }
    }
}