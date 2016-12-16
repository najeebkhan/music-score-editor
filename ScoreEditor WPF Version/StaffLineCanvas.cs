using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace ScoreEditor_WPF_Version
{
    class StaffLineCanvas: Canvas
    {
        int linenumber;
        public int LineNumber
        {
            get { return linenumber; }
            set { linenumber = value; }
        }
    }
}
