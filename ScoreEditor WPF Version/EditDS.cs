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
    public static class EditDS
    {
       
        public static ScorePartwise EditScorePartwise(ScorePartwise spw, ScoreProperties editedprop)
        {
            
            ScoreProperties scoreprop = ReadDS.ReadScore(spw);

            //if the edited properties are other than the default than assign them to the scoreprop

            scoreprop.WorkTitle = editedprop.WorkTitle==""?scoreprop.WorkTitle:editedprop.WorkTitle;
            scoreprop.MovementTitle = editedprop.MovementTitle == "" ? scoreprop.MovementTitle : editedprop.MovementTitle;
            scoreprop.EncodingSoftware = editedprop.EncodingSoftware == "" ? scoreprop.EncodingSoftware : editedprop.EncodingSoftware;
            scoreprop.Creator = editedprop.Creator == "" ? scoreprop.Creator : editedprop.Creator;
            scoreprop.CreditWords = editedprop.CreditWords == "" ? scoreprop.CreditWords : editedprop.CreditWords;
            scoreprop.PartDisplayName = editedprop.PartDisplayName == "" ? scoreprop.PartDisplayName : editedprop.PartDisplayName;
            scoreprop.PartAbbreviation = editedprop.PartAbbreviation == "" ? scoreprop.PartAbbreviation : editedprop.PartAbbreviation;
            scoreprop.ScorePartID = editedprop.ScorePartID == "" ? scoreprop.ScorePartID : editedprop.ScorePartID;
            scoreprop.InstrumentID = editedprop.InstrumentID == "" ? scoreprop.InstrumentID : editedprop.InstrumentID;
            scoreprop.InstrumentName = editedprop.InstrumentName == "" ? scoreprop.InstrumentName : editedprop.InstrumentName;
            scoreprop.MidiChannel = editedprop.MidiChannel == new Integer(0) ? scoreprop.MidiChannel : editedprop.MidiChannel;
            scoreprop.MidiProgram = editedprop.MidiProgram == new Integer(0) ? scoreprop.MidiProgram : editedprop.MidiProgram;
            

            return CreateDS.CreateScorePartwise(scoreprop);

        }

        public static Attributes EditAttributes(ScorePartwise.Part.Measure measure, AttributeProperties editprop)
        {
            AttributeProperties attprop = ReadDS.ReadAttributes(measure);

            //if the edited properties are other than the default than assign them to the scoreprop

            attprop.fifths = editprop.fifths==""? attprop.fifths: editprop.fifths;
            attprop.mode = editprop.mode==""? attprop.mode: editprop.mode;
            attprop.BeatsPerMeasure = editprop.BeatsPerMeasure==""? attprop.BeatsPerMeasure: editprop.BeatsPerMeasure;
            attprop.BeatType = editprop.BeatType==""? attprop.BeatType: editprop.BeatType;
            attprop.ClefSign = editprop.ClefSign==""? attprop.ClefSign: editprop.ClefSign;
            attprop.ClefLine = editprop.ClefLine==""? attprop.ClefLine: editprop.ClefLine;
            attprop.divisions = editprop.divisions==0? attprop.divisions: editprop.divisions;
            attprop.staves = editprop.staves==1? attprop.staves:editprop.staves;

            return CreateDS.CreateAttribute(attprop);

        }

        public static ScorePartwise.Part.Measure EditNote(ScorePartwise.Part.Measure measure, int notenumber, NoteProperties editprop)
        {
            NoteProperties noteprop = ReadDS.ReadNote(notenumber,measure);

            noteprop.PitchStep = editprop.PitchStep == "" ? noteprop.PitchStep : editprop.PitchStep;
            noteprop.Pitchoctave= editprop.Pitchoctave== 0? noteprop.Pitchoctave: editprop.Pitchoctave;
            noteprop.NoteType = editprop.NoteType == "" ? noteprop.NoteType : editprop.NoteType;
            noteprop.LyricSyllabic = editprop.LyricSyllabic == "" ? noteprop.LyricSyllabic : editprop.LyricSyllabic;
            noteprop.LyricText = editprop.LyricText == "" ? noteprop.LyricText : editprop.LyricText;
            noteprop.AccidentalType = editprop.AccidentalType == "" ? noteprop.AccidentalType : editprop.AccidentalType;
            
            noteprop.duration= editprop.duration== 0 ? noteprop.duration : editprop.duration;
            noteprop.rest = editprop.rest == null ? noteprop.rest : editprop.rest;
            noteprop.LyricEndline = editprop.LyricEndline == null ? noteprop.LyricEndline : editprop.LyricEndline;
            noteprop.dot = editprop.dot == null ? noteprop.dot : editprop.dot;
            noteprop.chord = editprop.chord == null ? noteprop.chord : editprop.chord;
            noteprop.PitchAlter = editprop.PitchAlter == float.MaxValue ? noteprop.PitchAlter : editprop.PitchAlter;

            //to remove pass -1
            noteprop.TupletNumber = editprop.TupletNumber == 0 ? noteprop.TupletNumber : (editprop.TupletNumber == -1 ? 0 : editprop.TupletNumber);
            noteprop.TupletType = editprop.TupletType == "" ? noteprop.TupletType : editprop.TupletType;
            noteprop.dynamic = editprop.dynamic == "" ? noteprop.dynamic : (editprop.dynamic == "remove" ? "" : editprop.dynamic);
            noteprop.dynamic = editprop.dynamic == "" ? noteprop.dynamic : editprop.dynamic;
            noteprop.SlurType = editprop.SlurType == ""? noteprop.SlurType:( editprop.SlurType == "remove" ? "" : editprop.SlurType);
            noteprop.TieType = editprop.TieType == "" ? noteprop.TieType : (editprop.TieType == "remove" ? "" : editprop.TieType);
            noteprop.TiedType = editprop.TiedType == "" ? noteprop.TiedType : (editprop.TiedType == "remove" ? "" : editprop.TiedType);
            noteprop.TimeModactualnotes = editprop.TimeModactualnotes == "" ? noteprop.TimeModactualnotes : (editprop.TimeModactualnotes == "remove" ? "" : editprop.TimeModactualnotes);
            noteprop.TimeModnormalnotes = editprop.TimeModnormalnotes == "" ? noteprop.TimeModnormalnotes : (editprop.TimeModnormalnotes == "remove" ? "" : editprop.TimeModnormalnotes);
            noteprop.TimeModnormaltype = editprop.TimeModnormaltype == "" ? noteprop.TimeModnormaltype : (editprop.TimeModnormaltype == "remove" ? "" : editprop.TimeModnormaltype);

            

            ///////////////////////////////// Complete Me
            ////Beams cannot Beam removed currently
            noteprop.beamnumber = editprop.beamnumber.Count == 0 ? noteprop.beamnumber : editprop.beamnumber;
            noteprop.beamvalue = editprop.beamvalue.Count == 0 ? noteprop.beamvalue : editprop.beamvalue;

            measure.getNoteOrBackupOrForward().set(getnoteindex(notenumber, measure), CreateDS.CreateNote(noteprop));
            
            return measure;
 
        }

        public static ScorePartwise.Part.Measure DeleteNote(ScorePartwise.Part.Measure measure, int notenumber)
        {
            return (ScorePartwise.Part.Measure)measure.getNoteOrBackupOrForward().remove(getnoteindex(notenumber, measure));
        }

        public static ScorePartwise.Part.Measure deleteBarline(ScorePartwise.Part.Measure measure, int barnumber)
        {
            return (ScorePartwise.Part.Measure)measure.getNoteOrBackupOrForward().remove(getbarindex(barnumber, measure));
        }
        public static int getnoteindex(int index, ScorePartwise.Part.Measure measure)
        {
            int x=0;
            for(int i =0; i < measure.getNoteOrBackupOrForward().size(); i++)
            {
                if (measure.getNoteOrBackupOrForward().get(i) is Note)
                    x++;
                if (index == x)
                    return i;
            }
            return -1;
        }

        public static int getNotesperMeasure(ScorePartwise.Part.Measure measure)
        {
            int x = 0;
            for (int i = 0; i < measure.getNoteOrBackupOrForward().size(); i++)
            {
                if (measure.getNoteOrBackupOrForward().get(i) is Note)
                    x++;
               
            }
            return x;
        }


        public static int getbarindex(int index, ScorePartwise.Part.Measure measure)
        {
            int x = 0;
            for (int i = 0; i < measure.getNoteOrBackupOrForward().size(); i++)
            {
                if (measure.getNoteOrBackupOrForward().get(i) is Barline)
                    x++;
                if (index == x)
                    return i;
            }
            return -1;
        }

    }
}
