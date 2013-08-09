using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;
using Cker;
using Cker.Models;
using Cker.Presenters;
using Cker.Authentication;

namespace CKerTests
{
    public class PresenterTest
    {
        // Presenter used throughout tests.
        VesselPresenter vesselPresenter = new VesselPresenter();
        LoginPresenter loginPresenter = new LoginPresenter();

        /// <summary>
        /// Tests that the presenter login matches actual authenticator.
        /// </summary>
        [Test]
        public void PresenterTest_Authentication()
        {
            // Result from presenter should be the same as authenticator.
            Assert.AreEqual(loginPresenter.Authenticate("admin", "fullaccess"), Authenticator.Login("admin", "fullaccess"));
            Assert.AreEqual(loginPresenter.Authenticate("random", "pass"), Authenticator.Login("random", "pass"));
        }

        /// <summary>
        /// Tests that the presenter knows the correct user type.
        /// </summary>
        [Test]
        public void PresenterTest_UserType()
        {
            Assert.AreEqual(loginPresenter.GetUserImagePath("admin"), LoginPresenter.ADMIN_IMAGE_PATH);
            Assert.AreEqual(loginPresenter.GetUserImagePath("operator"), LoginPresenter.OPERATOR_IMAGE_PATH);
        }

        /// <summary>
        /// Tests that the presenter vessel data matches actual simulator data.
        /// </summary>
        [Test]
        public void PresenterTest_VesselDataMatchesSimulation()
        {
            // First start the simulation through presenter.
            bool isStarted = vesselPresenter.Start("test_oneofeach.vsf");
            Assert.IsTrue(isStarted, "Could not start simulation.");

            // Stop it immediately because we don't need the timer updates.
            Cker.Simulator.Stop();

            // Now, the presenter vessel list should match up the simulator vessel list.
            Assert.AreEqual(vesselPresenter.DisplayedVessels.Count, Cker.Simulator.Vessels.Count, "Vessel lists are different sizes.");
            Assert.AreEqual(vesselPresenter.DisplayedVessels.Intersect(Cker.Simulator.Vessels).Count(), Cker.Simulator.Vessels.Count, "Vessel lists have different elements.");
        }

        /// <summary>
        /// Tests that the filtering function works
        /// </summary>
        [Test]
        public void PresenterTest_Filtering()
        {
            // First start the simulation through presenter.
            bool isStarted = vesselPresenter.Start("test_oneofeach.vsf");
            Assert.IsTrue(isStarted, "Could not start simulation.");

            // Stop it immediately because we don't need the timer updates.
            Cker.Simulator.Stop();

            // Choose a few types to filter with
            List<Vessel.TargetType> wantedTypes = new List<Vessel.TargetType>() 
            { 
                Vessel.TargetType.FishingBoat, 
                Vessel.TargetType.SpeedBoat
            };

            // Before filtering, those types are present along with other types.
            Assert.IsTrue(vesselPresenter.DisplayedVessels.Exists(v => wantedTypes.Contains(v.Type)), "Initial list does not contain wanted types.");
            Assert.IsTrue(vesselPresenter.DisplayedVessels.Exists(v => !wantedTypes.Contains(v.Type)), "Initial list does not contain other types.");

            // Now filter, and those types should be the only ones present.
            vesselPresenter.FilterVessels(wantedTypes);
            Assert.IsTrue(vesselPresenter.DisplayedVessels.Exists(v => wantedTypes.Contains(v.Type)), "Final list does not contain wanted types.");
            Assert.IsTrue(!vesselPresenter.DisplayedVessels.Exists(v => !wantedTypes.Contains(v.Type)), "Final list contain other types.");
        }

        /// <summary>
        /// Tests that the sorting function works
        /// </summary>
        [Test]
        public void PresenterTest_Sorting()
        {
            // First start the simulation through presenter.
            bool isStarted = vesselPresenter.Start("test_oneofeach.vsf");
            Assert.IsTrue(isStarted, "Could not start simulation.");

            // Stop it immediately because we don't need the timer updates.
            Cker.Simulator.Stop();

            // Copy the list to perform the expected value later.
            List<Vessel> vesselCopy = vesselPresenter.DisplayedVessels.ToList();

            // Sort twice to see the order switch.
            string attributeName = "ID";
            vesselPresenter.SortVessels(attributeName);

            // Sort the copy and compare
            vesselCopy = vesselPresenter.CurrentSortDirection == VesselPresenter.SortDirection.Ascending ? vesselCopy.OrderBy(v => v.ID).ToList() : vesselCopy.OrderByDescending(v => v.ID).ToList();
            Assert.IsTrue(Enumerable.SequenceEqual(vesselCopy, vesselPresenter.DisplayedVessels), "List not sorted correctly.");

            // Again.
            vesselPresenter.SortVessels(attributeName);
            vesselCopy = vesselPresenter.CurrentSortDirection == VesselPresenter.SortDirection.Ascending ? vesselCopy.OrderBy(v => v.ID).ToList() : vesselCopy.OrderByDescending(v => v.ID).ToList();
            Assert.IsTrue(Enumerable.SequenceEqual(vesselCopy, vesselPresenter.DisplayedVessels), "List not sorted correctly.");
        }
    }
}
