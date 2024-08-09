using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{
    public enum CommandType { SwitchView, EntityChange, CanvasChange }

    public class SaveState<First, Second>
    {
        public First CommandType { get; set; }
        public Second SavedState { get; set; }

        public SaveState()
        {

        }

        public SaveState(First commandType, Second stateSaved)
        {
            CommandType = commandType;
            SavedState = stateSaved;
        }
    }
}
