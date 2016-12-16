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
using System.Windows;


namespace ScoreEditor_WPF_Version
{
    
    static public class ReadDS
    {        
        //Reads the top level score
        static public ScoreProperties ReadScore(ScorePartwise scorePartwise)
        {
            //get the top level properties of the score
            ScoreProperties i = new ScoreProperties();
            if (scorePartwise.getWork() != null)
                i.WorkTitle = scorePartwise.getWork().getWorkTitle();
            if (scorePartwise.getMovementTitle() != null)
                i.MovementTitle = scorePartwise.getMovementTitle();

            if (scorePartwise.getIdentification().getEncoding().getEncodingDateOrEncoderOrSoftware().get(0) != null)
            {
                javax.xml.bind.JAXBElement x = (javax.xml.bind.JAXBElement)scorePartwise.getIdentification().getEncoding().getEncodingDateOrEncoderOrSoftware().get(0);
                i.EncodingSoftware = x.getValue().ToString();
            }
            if (scorePartwise.getCredit().size()>0)
            {
                Credit credit = (Credit)scorePartwise.getCredit().get(0);
                if (credit.getCreditTypeOrLinkOrBookmark().get(0) is FormattedText)
                {
                    FormattedText ft = (FormattedText)credit.getCreditTypeOrLinkOrBookmark().get(0);
                    i.CreditWords = ft.getValue();
                }
            }
            PartList partlist = scorePartwise.getPartList();
            if (partlist.getPartGroupOrScorePart().get(0) is ScorePart)
            {
                ScorePart scorepart = (ScorePart)partlist.getPartGroupOrScorePart().get(0);
                i.ScorePartID = scorepart.getId();
                i.PartDisplayName = scorepart.getPartName().getValue();
                i.PartAbbreviation = scorepart.getPartAbbreviation() is PartName?scorepart.getPartAbbreviation().getValue(): "";
                ScoreInstrument scoreinstrument = (ScoreInstrument)scorepart.getScoreInstrument().get(0);
                i.InstrumentID = scoreinstrument.getId();
                i.InstrumentName = scoreinstrument.getInstrumentName();
                if (scorepart.getMidiDeviceAndMidiInstrument().get(0) is MidiInstrument)
                {
                    MidiInstrument midiinstrument = (MidiInstrument)scorepart.getMidiDeviceAndMidiInstrument().get(0);
                    i.MidiChannel = midiinstrument.getMidiChannel();
                    i.MidiProgram = midiinstrument.getMidiProgram();
                }
            }
            if (scorePartwise.getPart().size()>0)
                i.Part = (ScorePartwise.Part)scorePartwise.getPart().get(0);
            return i;
        }

        //Read the attribute
        public static AttributeProperties ReadAttributes(ScorePartwise.Part.Measure measure)
        {
            /* get the attributes Initialization properties */
            for (int i = 0; i < measure.getNoteOrBackupOrForward().size(); i++ )
                if (measure.getNoteOrBackupOrForward().get(i) is Attributes)
                {

                    Attributes attribute = (Attributes)measure.getNoteOrBackupOrForward().get(i);
                    AttributeProperties attprop = new AttributeProperties();
                    if (attribute.getKey().size() > 0)
                    {
                        Key key = (Key)attribute.getKey().get(0);

                        attprop.fifths = key.getFifths().toString();
                        attprop.mode = key.getMode();
                    }
                    if (attribute.getClef().size() > 0)
                    {
                        Clef clef = (Clef)attribute.getClef().get(0);
                        attprop.ClefSign = clef.getSign().name();
                        attprop.ClefLine = clef.getLine().toString();
                    }
                    if (attribute.getTime().size() > 0)
                    {
                        Time time = (Time)attribute.getTime().get(0);
                        javax.xml.bind.JAXBElement x = (javax.xml.bind.JAXBElement)time.getTimeSignature().get(0);
                        attprop.BeatsPerMeasure = (string)x.getValue();
                        x = (javax.xml.bind.JAXBElement)time.getTimeSignature().get(1);
                        attprop.BeatType = (string)x.getValue();
                    }
                    if (attribute.getDivisions() is BigDecimal)
                        attprop.divisions = attribute.getDivisions().intValue();
                    attprop.staves = attribute.getStaves() is BigInteger ? attribute.getStaves().intValue() : 1;
                    return attprop;
                }
            return null;
        }

        //Read the note
        public static NoteProperties ReadNote(int notenumber, ScorePartwise.Part.Measure measure)
        {
            if(getnoteindex(notenumber, measure)!=-1)
            {
                Note note = (Note)measure.getNoteOrBackupOrForward().get(getnoteindex(notenumber, measure));
                NoteProperties noteprop = new NoteProperties();

                noteprop.chord = note.getChord() == new Empty() ? true : false;

                if (note.getRest() != null)
                {
                    //MessageBox.Show("Rest Read!");
                    
                    noteprop.rest = true;
                    try
                    {
                        noteprop.NoteType = note.getType().getValue();
                    }
                    catch (System.Exception)
                    {
                        noteprop.NoteType = "";
                    }
                }
                else if (note.getPitch() is Pitch)
                {
                    noteprop.NoteType = note.getType().getValue();      // get the notetype only if it is not a rest 
                    noteprop.PitchStep = note.getPitch().getStep().value();
                    noteprop.Pitchoctave = note.getPitch().getOctave();
                    if (note.getPitch().getAlter() != null)
                        noteprop.PitchAlter = note.getPitch().getAlter().floatValue();
                }

                noteprop.duration = note.getDuration().intValue();

                if (note.getTie().size() != 0)
                {
                    Tie tie = (Tie)note.getTie().get(0);
                    noteprop.TieType = tie.getType().toString();
                }

                

                noteprop.dot = note.getDot().size()==0? false:true;

                noteprop.AccidentalType =note.getAccidental()==null? "":note.getAccidental().getValue().value();



                if (note.getTimeModification() != null)
                {
                    noteprop.TimeModactualnotes = note.getTimeModification().getActualNotes().intValue().ToString();
                    noteprop.TimeModnormalnotes = note.getTimeModification().getNormalNotes().intValue().ToString();
                    noteprop.TimeModnormaltype = note.getTimeModification().getNormalType() == null ? "" : note.getTimeModification().getNormalType();
                }

                ///////////////////////////////////COMPLETE ME
                Beam beam;
                for (int i = 0; i < note.getBeam().size(); i++)
                {
                    beam = (Beam)note.getBeam().get(i);
                    noteprop.beamvalue.Add(beam.getValue().value());
                    noteprop.beamnumber.Add(beam.getNumber());
                }   

                if (note.getNotations().size() != 0)
                {                   
                    Notations notations = (Notations)note.getNotations().get(0);
                    for (int i = 0; i < notations.getTiedOrSlurOrTuplet().size(); i++)
                    {
                        if (notations.getTiedOrSlurOrTuplet().get(i) is Tied)
                        {
                            Tied tied = (Tied)notations.getTiedOrSlurOrTuplet().get(i);
                            //if(tied.getNumber().intValue()==1)
                                noteprop.TiedType = tied.getType().value();
                        }

                        if (notations.getTiedOrSlurOrTuplet().get(i) is Slur)
                        {                            
                            Slur slur =(Slur)notations.getTiedOrSlurOrTuplet().get(i);
                            if (slur.getNumber() == 1)
                            {
                                noteprop.SlurType = slur.getType().value();                               
                            }
                        }

                        if (notations.getTiedOrSlurOrTuplet().get(i) is Tuplet)
                        {
                            Tuplet tuplet = (Tuplet)notations.getTiedOrSlurOrTuplet().get(i);
                            noteprop.TupletType = tuplet.getType().value();
                            //noteprop.TupletNumber = tuplet.getNumber().intValue();
                        }
                        if (notations.getTiedOrSlurOrTuplet().get(i) is Dynamics)
                        {
                            Dynamics dynamics = (Dynamics)notations.getTiedOrSlurOrTuplet().get(i);
                            javax.xml.bind.JAXBElement x = (javax.xml.bind.JAXBElement)dynamics.getPOrPpOrPpp().get(0);
                            noteprop.dynamic = x.getName().ToString();                 
                        }

                    }
                }

                if (note.getLyric().size() != 0)
                {
                    Lyric lyric = (Lyric)note.getLyric().get(0);
                    for (int i = 0; i < lyric.getElisionAndSyllabicAndText().size(); i++)
                    {
                        if (lyric.getElisionAndSyllabicAndText().get(i) is Syllabic)
                        {
                            Syllabic syllabic = (Syllabic)lyric.getElisionAndSyllabicAndText().get(i);
                            noteprop.LyricSyllabic = syllabic.value();
                        }

                        if (lyric.getElisionAndSyllabicAndText().get(i) is TextElementData)
                        {
                            TextElementData text = (TextElementData)lyric.getElisionAndSyllabicAndText().get(i);
                            noteprop.LyricText = text.getValue();
                        }

                    }
                    noteprop.LyricEndline = lyric.getEndLine()==null?false:true;
                }
                return noteprop;

            }
            return null;
        }

        //Read the barline
        public static BarlineProperties ReadBarline(int barnumber, ScorePartwise.Part.Measure measure)
        {
            if (getbarindex(barnumber, measure) != -1)
            {
                Barline barline = (Barline)measure.getNoteOrBackupOrForward().get(getbarindex(barnumber, measure));
                BarlineProperties barprop = new BarlineProperties();
                if (barline.getEnding() != null)
                {
                    barprop.EndingType = barline.getEnding().getType().value();
                    barprop.EndingValue = barline.getEnding().getValue();
                }
                if (barline.getRepeat() != null)
                {
                    barprop.RepeatTimes = barline.getRepeat().getTimes().intValue().ToString();
                    barprop.RepeatDirection = barline.getRepeat().getDirection().value();
                }
                barprop.Location = barline.getLocation().value();
                return barprop;
            }
            return null;
        }

        //some functions for finding the indices

        public static int getnoteindex(int index, ScorePartwise.Part.Measure measure)
        {
            int x = 0;
            for (int i = 0; i < measure.getNoteOrBackupOrForward().size(); i++)
            {
                if (measure.getNoteOrBackupOrForward().get(i) is Note)
                    x++;
                if (index == x)
                    return i;
            }
            return -1;
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
