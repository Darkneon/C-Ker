using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.IO;
using Cker.Models;

namespace Cker
{
    public class Server
    {         
        private static Timer timer = new Timer();
        private static int m_currentTime = 0;

        private static List<Vessel> m_vesselsList = new List<Vessel>();
        public static List<Vessel> Vessels 
        {
            get { return m_vesselsList; }
        }

        public delegate void AfterUpdateEventHandler();
        public static event AfterUpdateEventHandler AfterUpdate;


        //-------------------------------------------------------------------
        // Public Methods
        //-------------------------------------------------------------------

        public static void Start(string path, string filename) 
        {
            m_vesselsList = Parser.Parse(path, filename);
            m_currentTime = Simulator.StartTime;
            
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.Interval = Simulator.TimeStep * 1000; //To milliseconds            
            timer.Enabled = true;
            timer.Start();
        }

        public static void Stop() 
        {
            timer.Enabled = false;
        }   

        private static int TimeRemaining()
        {
            return Simulator.Time - m_currentTime;
        }

        //-------------------------------------------------------------------
        // Events
        //-------------------------------------------------------------------

        private static void timer_Elapsed(object sender, EventArgs e)
        {
            m_currentTime += Simulator.TimeStep;

            foreach (Vessel v in m_vesselsList)
            {
                v.UpdatePositions(Simulator.TimeStep);
            }

            if (AfterUpdate != null)
            {
                AfterUpdate();
            }

            if (TimeRemaining() <= 0)
            {
                Stop();
            }
        }

    }
}
