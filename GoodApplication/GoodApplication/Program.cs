using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace GoodApplication
{
    class Program
    {
        //variable declaration
        public static bool flag = true;

        static void Main(string[] args)
        {
            using (var context = NetMQContext.Create()) //create netmqcontext  
            using (var sender = context.CreateDealerSocket()) // using dealersocket 
            {
                sender.Connect("tcp://127.0.0.1:9045"); //connect router socket
                System.Timers.Timer aTimer = new System.Timers.Timer(); //timer initalization 
                aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent); //event creation for timer
                aTimer.Interval = 60000;//set the timer interval
                aTimer.Enabled = true; //set the enabled timer
                aTimer.Start(); //start the timer
                while(true) //infinitive loop starts
                {
                    if (!Console.KeyAvailable) 
                    {
                        var message = new NetMQMessage();       //create netmqmessage               
                         String guid = Guid.NewGuid().ToString(); //create the guid generation 
                         message.Append("GoodApp" + '_' + guid.ToString()); //add the guid in netmqmessage
                         if (flag == true) 
                         {
                             sender.SendMessage(message); //sending the message to router                     
                             Console.WriteLine(message.First.ConvertToString()); //print the guid
                         }
                        Thread.Sleep(500);
                        
                    }
                    else
                    {
                        if (flag == true) //any key press is available close the application
                        {
                            var process = Process.GetCurrentProcess();
                            if (process.ProcessName == "GoodApplication.vshost" || process.ProcessName == "GoodApplication.exe" || process.ProcessName == "GoodApplication")
                            {
                                process.Kill();
                            }
                        }
                    }

                }               
               
                
            }
        }
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            { 
                if (flag == true)
                {
                    //set good application in frozen state                    
                    flag = false; 
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
    }
}
