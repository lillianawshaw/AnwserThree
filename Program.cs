using System;
using System.Collections.Generic;
using System.Linq;

namespace Rextester
{
    /// <summary>
    /// The code below checks the return value from GetTimeWorked against several
    /// test cases, defined by the property TestCases. The test cases represent
    /// clock in/out times from a clock-in system.
    /// The values are strings in the format "HH:mm-HH:mm HH:mm-HH:mm". HH:mm represents
    /// the hour and minute of the day in 24-hour format. Times to the left of the dash "-"
    /// are clock-in times, times to the right are clock out times. There are always 4
    /// times - a clock-in followed by a clock-out, then a space (for a lunch break), then
    /// another clock-in/clock-out. We can assume that times on the left always precede the
    /// times on the right, and that all times occur within the same day.
    ///
    /// At the end of each question please save the entire file as the response to that
    /// question. You should send back a total of 3 files.
    ///
    /// 1) Please implement GetTimeWorked so that the test cases are satisfied.
    /// 2) It's now a requirement that each clock-in period cannot exceed 6 hours.
    ///    Please update the test cases, and update your implementation of GetTimeWorked.
    /// 3) It's now a requirement that the minimum period which can be taken for a lunch
    ///    break is 20 minutes. If the break is less than 20 minutes, time must be taken
    ///    out of either the morning or afternoon work period. If either period was more
    ///    than 6 hours, then you should use the excess time from that period to contribute
    ///    to the lunch break. For example, if I work 06:00-12:02 12:20-14:30, I would expect
    ///    that the 2 excess minutes from my morning period would be added to my lunch
    ///    break to make up the 20 minutes. The total working time would be 08:10.
    ///    Please update the test cases, and update your implementation of GetTimeWorked.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Runs test cases for the GetTimeWorked method.
        /// Please don't alter this method.
        /// </summary>
        /// 
        // magic number to set the Max hours working
        public static TimeSpan Max = TimeSpan.FromHours(6);
        // Minmum Break Length
        public static TimeSpan Min = TimeSpan.FromMinutes(20);

        public static void Main(string[] args)
        {

            foreach (var testCase in TestCases)
            {
                var times = testCase.Key;
                var expectedResult = testCase.Value;
                var testResult = String.Empty;

                try
                {
                    var timeWorked = GetTimeWorked(times);
                    testResult = timeWorked == expectedResult
                                 ? String.Format("OK    {0}", timeWorked)
                                 : String.Format("Expected {0} but got {1}", testCase.Value, timeWorked);
                }
                catch (Exception ex)
                {
                    testResult = ex.Message;
                }

                Console.WriteLine("{0}{1}", times.PadRight(27), testResult);
            }
        }

        /// <summary>
        /// Gets the test cases which check the results of GetTimeWorked.
        /// These are correct for the first question. They will need to be
        /// altered for the second and third questions. You may add to the
        /// test cases, but please make sure that the original 5 clock-in
        /// strings remain (the TimeSpan values will need to be updated for
        /// questions 2 and 3).
        /// </summary>
        public static IDictionary<String, TimeSpan> TestCases
        {
            get
            {
                return new Dictionary<String, TimeSpan>
                {
                    { "09:10-12:33 13:07-17:02", TimeSpan.Parse("07:18") },
                    { "08:52-12:13 12:45-17:02", TimeSpan.Parse("07:38") },
                    { "07:22-14:11 14:58 16:09", TimeSpan.Parse("08:00") },
                    { "06:10-12:43 13:00-15:00", TimeSpan.Parse("08:00") },
                    { "09:47-12:32 12:45-18:48", TimeSpan.Parse("08:45") },
                    { "09:00-16:00 12:00-19:00", TimeSpan.Parse("12:00") },
                    { "09:00-11:55 12:00-19:00", TimeSpan.Parse("08:55") },
                    { "09:00-10:00 10:15-14:00", TimeSpan.Parse("04:45") }
                };
            }
        }

        /// <summary>
        /// Given a string in the format "HH:mm-HH:mm HH:mm-HH:mm" works
        /// out how long an employee was clocked in for. ("09:10-12:33 13:07-17:02")
        /// </summary>
        public static TimeSpan GetTimeWorked(String times)
        {

            // Split the string into morning or afternoon given the format is consitent
            var Morning = times.Substring(0, 11);
            var Afternoon = times.Substring(12);

            // split the times into 4 objects for calculations
            var MorningIn = TimeSpan.Parse(Morning.Substring(0, 5));
            //This Warns the User they must clock out before 6 hours have passed 
            TimeSpan limit = MorningIn.Add(TimeSpan.FromHours(6));
            Console.WriteLine("Please clock out before " + limit);
            var MorningOut = TimeSpan.Parse(Morning.Substring(6));
            
            var AfternoonIn = TimeSpan.Parse(Afternoon.Substring(0, 5));
            limit = AfternoonIn.Add(TimeSpan.FromHours(6));
            Console.WriteLine("Please clock out before " + limit);
            var AfternoonOut = TimeSpan.Parse(Afternoon.Substring(6));

            //calculate each block time
            TimeSpan MorningTime = MorningOut - MorningIn;
            TimeSpan AfternoonTime = AfternoonOut - AfternoonIn;

            //Calculating break time
            TimeSpan Break = AfternoonIn - MorningOut;
            if (Break.TotalMinutes < Min.TotalMinutes)
            {
                var Difference = Min.TotalMinutes - Break.TotalMinutes; 
                Console.WriteLine("Please take a break of minmum {0}, adjusting break by {1} minutes", Min, Difference);
                //Need to remove excess time from shifts to adjust for break.
                
                if (MorningTime.TotalHours > Max.TotalHours)
                {
                    // excess time to be added to break
                    TimeSpan MorningExtra = MorningTime - Max;
                    
                    Break = Break + MorningExtra;
                    //sets the shift to max allowed 
                    MorningTime = Max;
                }
                // checks if we still need to add more time to the Break while removing time from excess shifts
                if (Break.TotalMinutes < Min.TotalMinutes)
                {
                    if (AfternoonTime.TotalHours > Max.TotalHours)
                    {
                        // excess time to be added to break
                        TimeSpan AfternoonExtra = AfternoonTime - Max;
                        Break = Break + AfternoonExtra;
                        //Sets Shift to the Max Allowed
                        AfternoonTime = Max;
                    }
                }

                if (Break < Min)
                {
                    //If this triggers means that the employee either did not work longer than 6 hours each shift
                    // Or They did however not enough to supplement the break up to the 20 min mark
                    //Using MorningShift - (Min - Break) could implement a hard rule on ensuring all shifts have a 20 min break
                    //However this would effect peoples real terms pay so would appreciate clarifcation on the intent of this rule. 
                    Break = Min;
                }

            }

            // checks time clocked in and sets it too the max if over
            if (MorningTime > Max)
            {
                MorningTime = Max;
            }
            if (AfternoonTime > Max)
            {
                AfternoonTime = Max;
            }
            //return total time
            return MorningTime + AfternoonTime;
        }
    }
}