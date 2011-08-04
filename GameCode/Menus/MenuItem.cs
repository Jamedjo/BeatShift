using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeatShift.Menus
{

    public class MenuItem
    {
        public String description;
        public Action clickedAction;
        public Boolean isEnabled;

        Boolean valueUpdates = false;
        public Func<String> updateValue;
        public string value;

        private MenuItem(String description, Boolean isEnabled, Action clicked_Action, Boolean vUpdates, Func<String> updateV)
        {
            this.description = description;
            this.isEnabled = isEnabled;
            clickedAction = clicked_Action;
            valueUpdates = vUpdates;
            updateValue = updateV;
        }
        public MenuItem(String description, Boolean isEnabled, Action clicked_Action, Func<String> valueFunction)
            : this(description, isEnabled, clicked_Action, true, valueFunction)
        {
        }
        public MenuItem(String description, Action clicked_Action, Func<String> valueFunction)
            : this(description, true, clicked_Action, true, valueFunction)
        {
        }
        public MenuItem(String description, Boolean isEnabled, Action clicked_Action)
            : this(description, isEnabled, clicked_Action, false, null)
        {
        }
        public MenuItem(String description, Action clicked_Action)
            : this(description, true, clicked_Action)
        {
        }

        public void update()
        {
            if (valueUpdates)
                value = updateValue();
        }
    }

}
