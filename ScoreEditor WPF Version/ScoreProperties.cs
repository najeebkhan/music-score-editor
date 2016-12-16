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
  
        public class ScoreProperties
        {
            public string WorkTitle = "";
            public string MovementTitle = "";
            public string EncodingSoftware = "";
            public string Creator = "";
            public string CreditWords = "";
            public string PartDisplayName = "";
            public string PartAbbreviation = "";
            public string ScorePartID = "";
            public string InstrumentID = "";
            public string InstrumentName = "";
            public Integer MidiChannel = new Integer(0);
            public Integer MidiProgram = new Integer(0);
            public ScorePartwise.Part Part = null;
            
        }
}
