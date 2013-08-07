using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Cker.Models;

namespace Cker.Presenters
{
    /// <summary>
    /// Presenter allows the view to get information needed to display.
    /// This is essentially the list of vessels to be displayed.
    /// Logic for filtering and sorting vessels is also handled here.
    /// </summary>
    public class VesselPresenter
    {
        /// <summary>
        /// Accesses the list of vessels to be displayed.
        /// This list is the final filtered and sorted list.
        /// </summary>
        public List<Vessel> DisplayedVessels { get; private set; }

        /// <summary>
        /// Accesses the list of active alarms.
        /// </summary>
        public List<Cker.Simulator.OnAlarmEventArgs> CurrentAlarms { get; private set; }

        /// <summary>
        /// Accesses the current sort direction.
        /// </summary>
        public SortDirection CurrentSortDirection { get; private set; }
        public enum SortDirection
        {
            Descending,
            Ascending,
        }

        // Track current attribute to sort by.
        private string currentSortByAttribute;

        // Track current filtering options.
        private List<Vessel.TargetType> currentWantedTypes;

        /// <summary>
        /// Must specify the scenario file to start, simply the filename "name.vsf" without the directory.
        /// </summary>
        /// <param name="scenarioFile"></param>
        public bool Start(string scenarioFile)
        {
            // Starts the simulation here.
            Cker.Simulator.Stop();
            bool startSuccess = Cker.Simulator.Start("Assets/", scenarioFile) == 0;
            if (startSuccess)
            {
                // Register event handlers.
                Cker.Simulator.BeforeUpdate += OnSimulationBeforeUpdateEvent;
                AddUpdateAction(OnSimulationAfterUpdateEvent);
                AddAlarmAction(OnSimulationAlarmEvent);

                // Display all vessels at first.
                DisplayedVessels = GetAllVesselsWithinRadarRange();

                currentWantedTypes = null;
                currentSortByAttribute = null;
                CurrentSortDirection = SortDirection.Descending;

                CurrentAlarms = new List<Simulator.OnAlarmEventArgs>();
            }
            return startSuccess;
        }

        /// <summary>
        /// Registers a function to be called at each simulation update step.
        /// Meant to allow the view to know when the update occurs.
        /// </summary>
        /// <param name="action"></param>
        public void AddUpdateAction(Cker.Simulator.AfterUpdateEventHandler action)
        {
            Cker.Simulator.AfterUpdate += action;
        }

        /// <summary>
        /// Registers a function to be called when an alarm occurs.
        /// </summary>
        /// <param name="action"></param>
        public void AddAlarmAction(Cker.Simulator.OnAlarmEventHandler action)
        {
            Cker.Simulator.OnAlarm += action;
        }

        /// <summary>
        /// Filters the vessels by type. This will modify the DisplayedVessels list.
        /// If any sorting is applied before, it will be retained in that order.
        /// </summary>
        /// <param name="wantedTypes">specifies the types of vessels wanted</param>
        public void FilterVessels(List<Vessel.TargetType> wantedTypes)
        {
            currentWantedTypes = wantedTypes;

            // Get all vessels that correspond to one of the wanted types.
            DisplayedVessels = GetAllVesselsWithinRadarRange().FindAll(vessel => currentWantedTypes.Contains(vessel.Type));

            // Make sure these retain sorted order if they were sorted before.
            if (currentSortByAttribute != null)
            {
                SortVessels(currentSortByAttribute, CurrentSortDirection);
            }
        }

        /// <summary>
        /// Sorts the vessels by attribute. This will modify the DisplayedVessels list.
        /// If the same attribute is sorted consecutively, only the direction changes (descending/ascending).
        /// </summary>
        /// <param name="attributeName"></param>
        public void SortVessels(string attributeName)
        {
            // Compare the new attribute to the old one to see if we need to swap directions.
            // Otherwise just assume a default direction.
            if (currentSortByAttribute == attributeName)
            {
                CurrentSortDirection = CurrentSortDirection == SortDirection.Descending ? SortDirection.Ascending : SortDirection.Descending;
            }
            else
            {
                CurrentSortDirection = SortDirection.Descending;
            }
            currentSortByAttribute = attributeName;

            SortVessels(currentSortByAttribute, CurrentSortDirection);
        }

        private void SortVessels(string attributeName, SortDirection direction)
        {
            if (DisplayedVessels.Count > 0)
            {
                // Get the name of the attributes as strings so we can compare later.
                var dummyVessel = DisplayedVessels[0];
                var idName = GetVariableName(() => dummyVessel.ID);
                var typeName = GetVariableName(() => dummyVessel.Type);
                var posXName = GetVariableName(() => dummyVessel.X);
                var posYName = GetVariableName(() => dummyVessel.Y);
                var speedName = "Speed";
                var distName = GetVariableName(() => dummyVessel.CourseDistance);
                var timeName = GetVariableName(() => dummyVessel.UpdateTime);

                // Compare the attribute name to see which attribute to sort by.
                if (attributeName == idName)
                {
                    DisplayedVessels = DisplayedVessels.OrderByDescending(v => v.ID).ToList();
                }
                else if (attributeName == typeName)
                {
                    DisplayedVessels = DisplayedVessels.OrderByDescending(v => v.Type).ToList();
                }
                else if (attributeName == posXName)
                {
                    DisplayedVessels = DisplayedVessels.OrderByDescending(v => v.X).ToList();
                }
                else if (attributeName == posYName)
                {
                    DisplayedVessels = DisplayedVessels.OrderByDescending(v => v.Y).ToList();
                }
                else if (attributeName == speedName)
                {
                    // For speed we check both X and Y and sort by magnitude.
                    DisplayedVessels = DisplayedVessels.OrderByDescending(v => v.VX_0 * v.VX_0 + v.VY_0 * v.VY_0).ToList();
                }
                else if (attributeName == distName)
                {
                    DisplayedVessels = DisplayedVessels.OrderByDescending(v => v.CourseDistance).ToList();
                }
                else if (attributeName == timeName)
                {
                    DisplayedVessels = DisplayedVessels.OrderByDescending(v => v.UpdateTime).ToList();
                }
                else
                {
                    Debug.Assert(false, "SortVessels : unable to associate the attribute " + attributeName);
                }

                // If the direction was to be ascending, just reverse the list now.
                if (direction == SortDirection.Ascending)
                {
                    DisplayedVessels.Reverse();
                }
            }
        }

        /// <summary>
        /// Gets a list containing all vessels that are within the radar range.
        /// </summary>
        /// <returns></returns>
        public List<Vessel> GetAllVesselsWithinRadarRange()
        {
            List<Vessel> vessels = new List<Vessel>();
            vessels = Cker.Simulator.Vessels.Where(v => Math.Sqrt(v.X * v.X + v.Y * v.Y) <= Cker.Simulator.Range).ToList();
            return vessels;
        }

        private void OnSimulationBeforeUpdateEvent()
        {
            // Clear current alarms every update.
            CurrentAlarms.Clear();
        }

        private void OnSimulationAfterUpdateEvent()
        {
            // Refilter to eliminate vessels that moved out of range.
            if (currentWantedTypes != null)
            {
                FilterVessels(currentWantedTypes);
            }
            else
            {
                DisplayedVessels = GetAllVesselsWithinRadarRange();
                if (currentSortByAttribute != null)
                {
                    SortVessels(currentSortByAttribute, CurrentSortDirection);
                }
            }
        }

        private void OnSimulationAlarmEvent(Cker.Simulator.OnAlarmEventArgs alarm)
        {
            CurrentAlarms.Add(alarm);

            // Keep only the high risk alarm if there is both a low and a high risk alarm for a vessel.
            if (alarm.type == Simulator.AlarmType.High)
            {
                CurrentAlarms.RemoveAll(a1 => a1.type == Simulator.AlarmType.Low && IsAlarmVesselShared(a1, alarm));
            }
        }

        // helper to get the name of a variable; 
        // usage : GetVariableName( () => variable );
        private string GetVariableName<T>(Expression<Func<T>> expression)
        {
            MemberExpression expressionBody = (MemberExpression)expression.Body;
            return expressionBody.Member.Name;
        }

        // helper to check whether two alarms share the same vessel.
        private bool IsAlarmVesselShared(Cker.Simulator.OnAlarmEventArgs a1, Cker.Simulator.OnAlarmEventArgs a2)
        {
            return a1.first == a2.first || a1.first == a2.second || a1.second == a2.first || a1.second == a2.second;
        }
    }
}
