using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace LiteGraph
{
    /// <summary>
    /// Object used to measure start, end, and total time associated with an operation.
    /// </summary>
    public class Timestamps
    {
        #region Public-Members

        /// <summary>
        /// The time at which the operation started.
        /// </summary>
        [JsonProperty(PropertyName = "start", Order = -2)]
        public DateTime? Start
        {
            get
            {
                return _Start;
            }
            set
            {
                if (value == null)
                {
                    _Start = null;
                    _End = null;
                    _TotalMs = null;
                }
                else
                {
                    _Start = Convert.ToDateTime(value).ToUniversalTime();

                    if (_End != null)
                    {
                        if (_Start.Value > _End.Value)
                        {
                            _Start = null;
                            throw new ArgumentException("Start time must be before end time.");
                        }

                        _TotalMs = Math.Round(TotalMsBetween(_Start.Value, _End.Value), 2);
                    }
                }
            }
        }

        /// <summary>
        /// The time at which the operation ended.
        /// </summary>
        [JsonProperty(PropertyName = "end", Order = -1)]
        public DateTime? End
        {
            get
            {
                return _End;
            }
            set
            {
                if (value == null)
                {
                    _End = null;
                    _TotalMs = null;
                }
                else
                {
                    _End = Convert.ToDateTime(value).ToUniversalTime();

                    if (_Start != null)
                    {
                        if (_End.Value < _Start.Value)
                        {
                            _Start = null;
                            throw new ArgumentException("End time must be after start time.");
                        }

                        _TotalMs = Math.Round(TotalMsBetween(_Start.Value, _End.Value), 2);
                    }
                }
            }
        }

        /// <summary>
        /// The total number of milliseconds that transpired between Start and End.
        /// </summary>
        [JsonProperty(PropertyName = "total_ms", Order = 990)]
        public double? TotalMs
        {
            get
            {
                return _TotalMs;
            }
        }

        #endregion

        #region Private-Members

        private DateTime? _Start = null;
        private DateTime? _End = null;
        private double? _TotalMs = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public Timestamps()
        {
            _Start = DateTime.Now.ToUniversalTime();
            _End = null;
            _TotalMs = null;
        }

        #endregion

        #region Public-Methods
         
        #endregion

        #region Private-Methods
         
        private double TotalMsFrom(DateTime start)
        {
            DateTime end = DateTime.Now;
            return TotalMsBetween(start, end);
        }

        private double TotalMsBetween(DateTime start, DateTime end)
        {
            start = start.ToUniversalTime();
            end = end.ToUniversalTime();
            TimeSpan total = end - start;
            return total.TotalMilliseconds;
        }

        #endregion
    }
}
