using System;

namespace liquidclient.Classes
{
    public class ButtonInfo
    {
        public string buttonText = "-";
        public string overlapText = null;
        public Action method = null;
        public Action enableMethod = null;
        public Action disableMethod = null;
        public bool enabled = false;
        public bool isTogglable = true;
        public string toolTip = "";
        
        public string[] aliases;
        
        public bool adminOnly = false;

        public void Invoke()
        {
            if (adminOnly && !Console.Console.IsAuthorizedComplicated())
                return;

            method?.Invoke();
        }

        public void Enable()
        {
            if (adminOnly && !Console.Console.IsAuthorizedComplicated())
                return;

            enableMethod?.Invoke();
        }

        public void Disable()
        {
            if (adminOnly && !Console.Console.IsAuthorizedComplicated())
                return;

            disableMethod?.Invoke();
        }
    }
}
