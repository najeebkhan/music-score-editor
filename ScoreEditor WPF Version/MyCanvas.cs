using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
namespace ScoreEditor_WPF_Version
{
    class MyCanvas:Canvas
    {
        int elementnum;
        int meaurenum;
        int notenum;
        int modifier;
        public int NoteNum
        {
            get { return notenum; }
            set { notenum = value; }
        }

        public int Modifier
        {
            get { return modifier; }
            set { modifier = value; }
        }

        public int ElementNum
        {
            get { return elementnum; }
            set { elementnum = value; }
        }

        public int MeasureNum
        {
            get { return meaurenum; }
            set { meaurenum = value; }
        }

    }
}
