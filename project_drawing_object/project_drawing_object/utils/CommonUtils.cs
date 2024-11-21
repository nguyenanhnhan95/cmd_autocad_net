using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace project_drawing_object.utils
{
    public class CommonUtils
    {
        public static bool EnterKeyIntoCommand(string key, string value)
        {
            string upKey = key.ToLower();
            string upValue = value.ToLower();
            if (upKey.StartsWith(upValue)) { 
                return true;
            }
            return false;
        }
    }
}
