using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Watchdog
{
    class Program
    {
        //variable declaration
        public static string Applicationname = "";
        public static bool badapplicationflag = false;
        public static bool goodflag = false;
        public static bool badflag = false;
        public static int goodcount = 0;
        public static int badcount = 0;
        public static string applicationpath = "";
        static void Main(string[] args)
        {
           //variable declartion
            int res = 1;           
            using (var application = new RouterSocket("@tcp://127.0.0.1:9045"))   //use router socket           
            {
                Console.WriteLine("Press any key to exit the application.");

                 Program objProgram = new Program(); //create obj in program class
                 /* When the router scoket is not connected and the dealersocket not initiate the thread. In that case we must close the router using keypress event ...
                       regarding this sistuation we can't use the return or exit , So we are using the process.getcurrentprocess method to kille the process and exit 
                       the application..*/
                 Thread threadCloseApplication = new Thread(() => 
                 {
                     res = objProgram.fnThreadToCloseApplication();
                     if (res == 0)
                     {
                         var process = Process.GetCurrentProcess();
                         if (process.ProcessName == "Watchdog.vshost" || process.ProcessName == "Watchdog.exe" || process.ProcessName == "Watchdog")
                         {
                             process.Kill();
                         }
                     }
                 });
                 threadCloseApplication.Name = "CloseApplicationThread"; //declartion of thread name
                 threadCloseApplication.Start(); //thread start   

                   while (true) //infinitive loop start
                   {                       
                          
                            List<string> msg = application.ReceiveStringMessages(); //get msg from good and bad application
                            Applicationname = msg[1].Split('_')[0]; //get application name from messages
                            Console.WriteLine(msg[1]);   //print the good and bad application messages                 
                            if(goodflag == false && badflag == false) //check to start the timer at initial state only
                            {
                                if (Applicationname == "GoodApp")
                                {
                                    goodflag = true;
                                }
                                else
                                {
                                    badflag = true;
                                }
                                System.Timers.Timer myTimer = new System.Timers.Timer(1 * 15 * 1000); //initiate the timer
                                myTimer.Start();//timer start
                                myTimer.Elapsed += new ElapsedEventHandler(goodTimer_Elapsed); //timer events elapsed
                            }                           
                          
                            if(msg.Count ==3) //check msg count == 3 means get the bad application environment path
                            {
                               applicationpath = msg[2]; //assign tha bad application path in global
                            }

                            Applicationname = "";
                            

                    }
            }
        }


        public int fnThreadToCloseApplication()
        {
            while (true)
                if (Console.KeyAvailable) //Any key available in console exit the application 
                {
                    return 0;
                }
        }
        public  static void goodTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {          
     
                if (Applicationname == "GoodApp") //If continously get good application messages then add the good application count
                {                 
                    goodcount = goodcount + 1;
                    badcount = 0;
                   
                }
                if (Applicationname == "BadApp") //If continously get bad application messages then add the bad application count
                {
                    badcount = badcount + 1;
                    goodcount = 0;
                   
                }
                if (Applicationname == "") //suppose good and bad application is forzen state 
                {
                    goodcount = goodcount + 1; //Add the good application count
                   
                    if(applicationpath !="") //Its only coming for bad application messages then add the bad application count and set true in flag variable
                    {
                        badcount = badcount + 1;
                        badapplicationflag = true;
                    }
                }
                if (goodcount > 3) // Sets the count more then 3 for frozen state ..
                {
                    if (applicationpath != "")
                    {
                       //check goodcount greater than three and application path not equal means bad application is frozen.So watch dog restart the application
                        Console.WriteLine("Bad Application is Frozen state.Watch dog restart the application");
                        Process secondProc = new Process();
                        secondProc.StartInfo.FileName = applicationpath;
                        foreach (var process in Process.GetProcesses())
                        {
                            if (process.ProcessName == "BadApplication.vshost" || process.ProcessName == "BadApplication.exe" || process.ProcessName == "BadApplication")
                            {
                                process.Kill();
                            }
                        }
                        Process.Start(applicationpath);
                        badcount = 0;
                    }
                    else
                    {

                        Console.WriteLine("Good Application is Frozen state");
                        goodcount = 0; 
                    }
                }

                if(badcount > 3)
                {
                    //check goodcount greater than three and application path equal empty means bad application is frozen.So watch dog restart the application
                    Console.WriteLine("Good Application is Frozen state");
                    goodcount = 0;                   
                }

                if(badapplicationflag==true)
                {
                    //suppose good and bad application is frozen state 
                    if (goodcount > 3)
                    {
                        Console.WriteLine("Good Application is Frozen state");
                        goodcount = 0;  

                    }
                    if (badcount > 3)
                    {
                        Console.WriteLine("Bad Application is Frozen state.Watch dog restart the application");
                        Process secondProc = new Process();
                        secondProc.StartInfo.FileName = applicationpath;
                        foreach (var process in Process.GetProcesses())
                        {
                            if (process.ProcessName == "BadApplication.vshost" || process.ProcessName == "BadApplication.exe" || process.ProcessName == "BadApplication")
                            {
                                process.Kill();
                            }
                        }
                        Process.Start(applicationpath);
                        badcount = 0;
                    }
                }
               
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    
    }
}
