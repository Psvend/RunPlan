using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions.Attributes;
using RunPlan.Model;
using RunPlan.ViewModel;


namespace RunPlan.Model
{
    public class Training
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Time { get; set; }
        public string Grade { get; set; } = string.Empty;
        public int Distance { get; set; }


        // <-- tell the library “I have many TrainingField children”
        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<TrainingField> Fields { get; set; }
    }
}
