using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMirror.Models
{
    public class WidgetOption
    {
        public WidgetOption(string displayName, string className)
        {
            this.DisplayName = displayName;
            this.ClassName = className;
        }
        public string DisplayName { get; set; }
        public string ClassName { get; set; }
    }
}
