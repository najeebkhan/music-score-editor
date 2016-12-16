using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.audiveris.proxymusic;
using com.audiveris.proxymusic.util;
using java.math;
using System.Diagnostics;
using java.util;
using javax.xml.datatype;
using java.lang;

namespace ScoreEditor_WPF_Version
{
    public class NoteProperties
    {
        public string PitchStep = "";
        public int Pitchoctave = 0;
        public float PitchAlter = float.MaxValue;
        public string SlurType = "";
        public string TieType = "";
        public string TiedType = "";
        public string NoteType = "";
        public string LyricSyllabic = "";
        public string LyricText = "";
        public string AccidentalType = "";
        public string TimeModactualnotes = "";
        public string TimeModnormalnotes = "";
        public string TimeModnormaltype = "";
        public List<int> beamnumber = new List<int>();
        public List<string> beamvalue = new List<string>();
        public int TupletNumber = 0;
        public string TupletType = "";
        public int duration = 0;
        public string dynamic = "";
        public int staff = 1;
        public Nullable<bool> rest = null;
        public Nullable<bool> LyricEndline = null;
        public Nullable<bool> dot = null;
        public Nullable<bool> chord = null;
    }
}
