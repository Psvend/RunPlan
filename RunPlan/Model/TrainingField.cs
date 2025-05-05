using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLiteNetExtensions.Attributes;




namespace RunPlan.Model
{
    public class TrainingField
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // foreign key back to the Training this belongs to
        public int TrainingId { get; set; }

        // the text label/value of this field
        public string Text { get; set; } = string.Empty;

        // ordering if you care about display order
        public int SortOrder { get; set; }

       
        [ManyToOne]
        public Training ParentTraining { get; set; }
    }
}
