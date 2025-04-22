using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




namespace RunPlan.Model
{
    public class RunningDataModel
    {
        public string? WeekLabel { get; set; }
        public double Distance { get; set; }
        public string MonthKey { get; set; }   
        public int WeekNumber { get; set; }   
      
        public string Time { get; set; }
        public string Grade { get; set; }
        public string Description { get; set; }
    }
};