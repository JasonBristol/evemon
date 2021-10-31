﻿using System.Collections.Generic;
using EVEMon.Common.Data;
using EVEMon.Common.Enumerations;

namespace EVEMon.Common.Helpers
{
    internal sealed class PathFinder
    {
        private readonly PathFinder m_parent;
        private readonly SolarSystem m_system;

        /// <summary>
        /// Constructor for a starting point
        /// </summary>
        /// <param name="system"></param>
        private PathFinder(SolarSystem system)
        {
            m_parent = null;
            m_system = system;
        }

        /// <summary>
        /// Constructor for a child.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="system"></param>
        private PathFinder(PathFinder parent, SolarSystem system)
        {
            m_parent = parent;
            m_system = system;
        }

        /// <summary>
        /// Gets the best possible path (approximate solution) using a A* alogrithm.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="target"></param>
        /// <param name="criteria"></param>
        /// <param name="minSecurityLevel">The minimum, inclusive, security level.</param>
        /// <param name="maxSecurityLevel">The maximum, inclusive, security level.</param>
        /// <returns></returns>
        public static IEnumerable<SolarSystem> FindBestPath(SolarSystem start, SolarSystem target, PathSearchCriteria criteria,
            float minSecurityLevel, float maxSecurityLevel)
        {
            var result = FindBestPathCore(start, target, criteria, minSecurityLevel, maxSecurityLevel);

            // Transforms the result into a list
            var list = new List<SolarSystem>();
            for (var item = result; item != null; item = item.m_parent)
            {
                list.Insert(0, item.m_system);
            }

            return list;
        }

        /// <summary>
        /// Gets the best possible path (approximate solution) using a A* alogrithm.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="target"></param>
        /// <param name="criteria"></param>
        /// <param name="minSecurityLevel">The minimum, inclusive, security level.</param>
        /// <param name="maxSecurityLevel">The maximum, inclusive, security level.</param>
        /// <returns></returns>
        private static PathFinder FindBestPathCore(SolarSystem start, SolarSystem target, PathSearchCriteria criteria,
            float minSecurityLevel, float maxSecurityLevel)
        {
            var bestDepths = new Dictionary<SolarSystem, int>();
            var paths = new SortedList<int, PathFinder>();
            var root = new PathFinder(start);
            bestDepths.Add(start, -1);
            paths.Add(0, root);

            while (true)
            {
                if (paths.Count == 0)
                    return null;

                PathFinder best;
                int depth;

                // Pick the best candidate path, but ensures it matches the best depth found so far
                while (true)
                {
                    // Picks the best one, returns if we found our target
                    best = paths.Values[0];

                    if (best.m_system == target)
                        return best;

                    // Removes it from the list
                    paths.RemoveAt(0);

                    int bestDepth;
                    depth = best.Depth;
                    bestDepths.TryGetValue(best.m_system, out bestDepth);

                    if (bestDepth == depth || best.m_system == start)
                        break;
                }

                // Gets the subpaths for the best path so far
                depth++;
                foreach (var child in best.GetChildren(depth, bestDepths))
                {
                    // Gets the heuristic key based on path search criteria
                    var criteriaValue = criteria == PathSearchCriteria.ShortestDistance
                        ? child.m_system.GetSquareDistanceWith(target)
                        : 1;
                    var key = criteriaValue * depth * depth;
                    if (child.m_system.SecurityLevel < minSecurityLevel || child.m_system.SecurityLevel > maxSecurityLevel)
                        key *= 100;

                    while (paths.ContainsKey(key))
                    {
                        key++;
                    }

                    // Stores it in the sorted list
                    paths.Add(key, child);
                }
            }
        }

        /// <summary>
        /// Gets all the immediate children paths.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<PathFinder> GetChildren(int depth, IDictionary<SolarSystem, int> bestDepths)
        {
            // Check this system is not already present with a lesser range
            foreach (var neighbor in m_system.Neighbors)
            {
                // Checks the best depth so far
                int bestDepth;
                bestDepths.TryGetValue(neighbor, out bestDepth);

                // Enumerates it
                if (bestDepth != 0 && depth >= bestDepth)
                    continue;

                bestDepths[neighbor] = depth;
                yield return new PathFinder(this, neighbor);
            }
        }

        /// <summary>
        /// Gets the depth of this node.
        /// </summary>
        private int Depth
        {
            get
            {
                var count = 0;
                for (var parent = m_parent; parent != null; parent = parent.m_parent)
                {
                    count++;
                }
                return count;
            }
        }

        /// <summary>
        /// Gets the name of the solar system.
        /// </summary>
        public override string ToString() => m_system.ToString();
    }
}