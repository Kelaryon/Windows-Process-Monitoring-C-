using System;
using System.Collections.Generic;
using System.Timers;
using System.Diagnostics;

namespace Windows_Process_Monitor
{
    class WindowsProcessMonitoring
    {
        Dictionary<int, ProcessTimer> processList;
        Timer aTimer;

        static void Main(string[] args)
        {
            //Checks if the minimum number of arguments is introduced
            if (args.Length < 3)
            {
                Console.Write("Not Enought Arguments");
                System.Environment.Exit(1);
            }
            //Verifies if the time related arguments are integers
            if (!int.TryParse(args[1], out int duration))
            {
                Console.Write("Invalid Duration Argument");
                System.Environment.Exit(1);
            }
            if (!int.TryParse(args[2], out int interval))
            {
                Console.Write("Invalid Interval Argument ");
                System.Environment.Exit(1);
            }
            WindowsProcessMonitoring pr = new();
            //Generate process dictionary
            pr.processList = new();
            //Set Timer
            pr.SetTimer(args[0],duration, interval);
            Console.Write("Press 'Q' to exit the process...\n");
            // here it ask to press "Q" to exit
            while (Console.ReadKey().Key != ConsoleKey.Q)
            {
            }
        }
        public void SetTimer(string process, int duration, int frequency)
        {
            //Initial process dictionary fill
            OnTimerComplete(process, duration, frequency);
            // Create a timer .
            aTimer = new(frequency * 60000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += (sender, e) => OnTimerComplete(process, duration, frequency);
            //Auto reset the timer on completion
            aTimer.AutoReset = true;
            //Start the timer
            aTimer.Enabled = true;
        }
        private void OnTimerComplete(string process,int duration,int frequency)
        {
            //List created so that recenly killed processes are not reintoduced in the list
            List<int> killedRecently = new();
            //Checks each element of the process dictionary and reduces thei lifespan
            //If the ReduceLifespan function returns true then remove the process key form the dictionary
            foreach (KeyValuePair<int,ProcessTimer> key in processList)
            {
                if (key.Value.ReduceLifespan(frequency))
                {
                    //Add the process ID to the recently killed List
                    killedRecently.Add(key.Key);
                    //Remove Key and value from the list
                    processList.Remove(key.Key);
                }
            }
            //Populates the dicitonary with all active process(Not in the process of being killed) with the name from the initial parameters
            Process[] processCollection = Process.GetProcessesByName(process);
            foreach (Process p in processCollection)
            {
                if (!processList.ContainsKey(p.Id) && !killedRecently.Contains(p.Id))
                {
                    processList.Add(p.Id, new ProcessTimer(p, duration));
                }
            }
        }
    }

    //Class containing the process along with is maximum duration
    class ProcessTimer
    {
        readonly Process process;
        int lifeSpan;

        //ProcessTimer Constructor
        public ProcessTimer(Process process, int lifeSpan)
        {
            this.process = process;
            this.lifeSpan = lifeSpan;
        }
        //Reduces the lifespan by the frequency variable, then checks if its lifespan is smaller than 0 and kills the process
        //The function return true if it killed the process or false otherwise
        public bool ReduceLifespan(int frequency)
        {
            lifeSpan -= frequency;
            if(lifeSpan <= 0)
            {
                //Write in the console the Process name and ID
                Console.WriteLine(process.ProcessName + " ID:" + process.Id);
                process.Kill();
                return true;
            }
            return false;
        }
    }
}
