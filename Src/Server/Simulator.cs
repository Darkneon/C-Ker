﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.IO;
using Cker.Models;

namespace Cker
{
    public class Simulator
    {
        
        static public double ALARM_HIGH_DISTANCE = 50.0;
        static public double ALARM_LOW_DISTANCE  = 200.0;

        static public int StartTime { get; set; }
        static public int TimeStep { get; set; }
        static public int Time { get; set; }
        static public int Range { get; set; }

        private static Timer timer = new Timer();
        private static int m_currentTime = 0;

        private static List<Vessel> m_vesselsList = new List<Vessel>();
        public static List<Vessel> Vessels 
        {
            get { return m_vesselsList; }
        }

        public enum AlarmType
        {
            Low = 1,            
            High
        }

        public struct OnAlarmEventArgs 
        { 
            AlarmType type;
            Vessel first;
            Vessel second;

            public OnAlarmEventArgs(AlarmType type, Vessel first, Vessel second) 
            {
                this.type   = type;
                this.first  = first;
                this.second = second;
            }
        }

        public delegate void AfterUpdateEventHandler();
        public static event AfterUpdateEventHandler AfterUpdate;

        public delegate void OnAlarmEventHandler(OnAlarmEventArgs e);
        public static event OnAlarmEventHandler OnAlarm;


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

            if (OnAlarm != null)
            {
                CheckCollisions();
            }

            if (TimeRemaining() <= 0)
            {
                Stop();
            }
        }

        //-------------------------------------------------------------------
        // Private Methods
        //-------------------------------------------------------------------

        private static void CheckCollisions() 
        {
            foreach (Vessel first in Vessels) 
            {
                foreach (Vessel second in Vessels)
                {
                    
                    if (first != second) 
                    {
                        double distance = first.GetDistanceBetween(second);
                     
                        if (distance < ALARM_HIGH_DISTANCE) 
                        {
                            OnAlarm(new OnAlarmEventArgs(AlarmType.High, first, second)); 
                        }
                        else if (distance < ALARM_LOW_DISTANCE) 
                        {
                            OnAlarm(new OnAlarmEventArgs(AlarmType.Low, first, second)); 
                        }
                    }

                } //end for
            } //end for
        }
    }
}
