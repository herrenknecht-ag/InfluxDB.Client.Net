using System;
using System.Collections.Generic;
using System.Text;

namespace AdysTech.InfluxDB.Client.Net
{ 
    /// <summary>
    /// Extend the results returned by Query end point of the InfluxDB engine with the informations of Partial
    /// and StatementID.
    /// </summary>
    public class InfluxResultDict
    {
        public int StatementID { get; set; }

        public List<IInfluxSeriesDict> InfluxSeries { get; set; }

        /// <summary>
        /// True if the influx query was answered with a partial response due to e.g. exceeding a configured
        /// max-row-limit in the InfluxDB. As we don't know which series was truncated by InfluxDB, all series
        /// of the response will be flagged with Partial=true.
        /// </summary>
        public bool Partial { get; set; }
    }
}
