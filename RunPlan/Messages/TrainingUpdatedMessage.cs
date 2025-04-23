using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace RunPlan.Messages
{
    public class TrainingUpdatedMessage : ValueChangedMessage<bool>
    {
        public TrainingUpdatedMessage() : base(true) { }
    }
}
