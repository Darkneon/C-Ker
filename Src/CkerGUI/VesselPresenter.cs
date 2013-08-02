using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.ComponentModel;
using Cker.Models;
using CkerGUI.ViewModels;

namespace CkerGUI
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

        public VesselPresenter()
        {
            // Starts the simulation here.
            Cker.Server.Start("Assets/", "scenario_100vessels.vsf");

            // Display all vessels at first.
            DisplayedVessels = Cker.Server.Vessels;

            currentSortByAttribute = null;
            CurrentSortDirection = SortDirection.Descending;
        }

        /// <summary>
        /// Registers a function to be called at each simulation update step.
        /// Meant to allow the view to know when the update occurs.
        /// </summary>
        /// <param name="action"></param>
        public void AddUpdateAction(Cker.Server.AfterUpdateEventHandler action)
        {
            Cker.Server.AfterUpdate += action;
        }

        /// <summary>
        /// Filters the vessels by type. This will modify the DisplayedVessels list.
        /// If any sorting is applied before, it will be retained in that order.
        /// </summary>
        /// <param name="wantedTypes">specifies the types of vessels wanted</param>
        public void FilterVessels(List<Vessel.TargetType> wantedTypes)
        {
            // Get all vessels that correspond to one of the wanted types.
            DisplayedVessels = Cker.Server.Vessels.FindAll(vessel => wantedTypes.Contains(vessel.Type));

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
                var velXName = GetVariableName(() => dummyVessel.VX_0);
                var velYName = GetVariableName(() => dummyVessel.VY_0);
                var timeName = GetVariableName(() => dummyVessel.StartTime);

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
                else if (attributeName == velXName)
                {
                    DisplayedVessels = DisplayedVessels.OrderByDescending(v => v.VX_0).ToList();
                }
                else if (attributeName == velYName)
                {
                    DisplayedVessels = DisplayedVessels.OrderByDescending(v => v.VY_0).ToList();
                }
                else if (attributeName == timeName)
                {
                    DisplayedVessels = DisplayedVessels.OrderByDescending(v => v.StartTime).ToList();
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

        // helper to get the name of a variable; 
        // usage : GetVariableName( () => variable );
        private string GetVariableName<T>(Expression<Func<T>> expression)
        {
            MemberExpression expressionBody = (MemberExpression)expression.Body;
            return expressionBody.Member.Name;
        }
    }
}
