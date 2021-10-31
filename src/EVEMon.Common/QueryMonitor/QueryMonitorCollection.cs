﻿using System;
using System.Collections.Generic;
using System.Linq;
using EVEMon.Common.Collections;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Extensions;
using EVEMon.Common.Interfaces;
using EVEMon.Common.Serialization.Eve;

namespace EVEMon.Common.QueryMonitor
{
    public sealed class QueryMonitorCollection : ReadonlyCollection<IQueryMonitor>
    {
        /// <summary>
        /// Gets the monitor for the given query.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public IQueryMonitor this[Enum method] => Items.FirstOrDefault(monitor => method.Equals(monitor.Method));

        /// <summary>
        /// Gets true when at least one of the monitors encountered an error on last try.
        /// </summary>
        public bool HasErrors => Items.Any(x => x.LastResult != null && x.LastResult.HasError);

        /// <summary>
        /// Gets the last API results gotten.
        /// </summary>
        public IEnumerable<IAPIResult> APIResults => Items.Where(x => x.LastResult != null).Select(x => x.LastResult);

        /// <summary>
        /// Gets the list of monitors to be auto-updated, ordered from the earliest to the latest.
        /// </summary>
        public IEnumerable<IQueryMonitor> OrderedByUpdateTime
        {
            get
            {
                var monitors = Items.OrderBy(x => x.NextUpdate);

                // Returns the monitors which are planned for an autoupdate
                foreach (var monitor in monitors.Select(monitor => (IQueryMonitorEx)monitor).Where(
                    monitor => monitor.Status == QueryStatus.Pending || monitor.Status == QueryStatus.Updating))
                {
                    yield return monitor;
                }

                // Returns the monitors which won't autoupdate
                foreach (var monitor in monitors.Select(monitor => (IQueryMonitorEx)monitor).Where(
                    monitor => monitor.Status != QueryStatus.Pending && monitor.Status != QueryStatus.Updating))
                {
                    yield return monitor;
                }
            }
        }

        /// <summary>
        /// Gets the next query to be auto-updated, or null.
        /// </summary>
        public IQueryMonitor NextMonitorToBeUpdated
        {
            get
            {
                var nextTime = DateTime.MaxValue;
                IQueryMonitor nextMonitor = null;
                foreach (var monitor in Items.Cast<IQueryMonitorEx>())
                {
                    if (monitor.Status != QueryStatus.Pending && monitor.Status != QueryStatus.Updating)
                        continue;

                    var monitorNextTime = monitor.NextUpdate;
                    if (monitorNextTime >= nextTime)
                        continue;

                    nextMonitor = monitor;
                    nextTime = monitorNextTime;
                }
                return nextMonitor;
            }
        }

        /// <summary>
        /// Requests an update for the given method.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <exception cref="System.ArgumentNullException">method</exception>
        public void Query(Enum method)
        {
            method.ThrowIfNull(nameof(method));

            var monitor = this[method] as IQueryMonitorEx;
            if (monitor != null && monitor.HasAccess)
                monitor.ForceUpdate();
        }

        /// <summary>
        /// Requests an update for the specified methods.
        /// </summary>
        /// <param name="methods">The methods.</param>
        public void Query(IEnumerable<Enum> methods)
        {
            var monitors = methods.Select(apiMethod => this[apiMethod]).Cast<IQueryMonitorEx>();
            foreach (var monitor in monitors.Where(monitor => monitor.HasAccess))
            {
                monitor.ForceUpdate();
            }
        }

        /// <summary>
        /// Requests an update for all monitor.
        /// </summary>
        public void QueryEverything()
        {
            foreach (var monitor in Items.Where(monitor => monitor.HasAccess).Cast<IQueryMonitorEx>())
            {
                monitor.ForceUpdate();
            }
        }

        /// <summary>
        /// Adds this monitor to the collection.
        /// </summary>
        /// <param name="monitor"></param>
        internal void Add(IQueryMonitorEx monitor)
        {
            Items.Add(monitor);
        }

        /// <summary>
        /// Removes this monitor from the collection.
        /// </summary>
        /// <param name="monitor"></param>
        internal void Remove(IQueryMonitorEx monitor)
        {
            Items.Remove(monitor);
        }
    }
}