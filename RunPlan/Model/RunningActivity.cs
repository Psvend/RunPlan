using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RunPlan.Model
{
    // ✅ Define the RunningActivity Model
    public class RunningActivity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Distance { get; set; }
        public string Time { get; set; } = "00:00:00";
        public string Date { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
        public string Grade { get; set; } = "N/A";
        public string Description { get; set; } = string.Empty; // Optional field

    }

}