using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.audiveris.proxymusic.util;
using com.audiveris.proxymusic;
using java.math;
using javax.xml.datatype;
using java.lang;
using System.Windows;

namespace ScoreEditor_WPF_Version
{
    public static class Check
    {
        /// <summary>
        /// Checks if the notes are according to the timesignature
        /// </summary>
        /// <param name="scorePartwise"></param>
        /// <returns></returns>
        public static ScorePartwise CheckAndReAlign(ScorePartwise scorePartwise)
        {
            double DivPerMeasure=16;
          
            for (int i = 0; i < scorePartwise.getPart().size(); i++)
            {
                //for each part in the score
                ScorePartwise.Part part = (ScorePartwise.Part)scorePartwise.getPart().get(i);
                Queue<Note> ExtraNotes = new Queue<Note>();

                for (int j = 0; j < part.getMeasure().size(); j++)
                {
                    //for each measure in a part

                    int DSum = 0;
                    ScorePartwise.Part.Measure measure = (ScorePartwise.Part.Measure)part.getMeasure().get(j);
                    //ScorePartwise.Part.Measure TempMeasure = CreateDS.CreateMeasure(j.ToString());
                    AttributeProperties attprop;
                    //check if timesignature is updated this measure or not
                    if (ReadDS.ReadAttributes(measure) != null)
                    {

                        attprop = ReadDS.ReadAttributes(measure);
                        DivPerMeasure = (attprop.divisions != 0 & attprop.BeatsPerMeasure != "" & attprop.BeatType != "") ?
                            int.Parse(attprop.BeatsPerMeasure) * 4 * attprop.divisions / int.Parse(attprop.BeatType) : DivPerMeasure;
                            
                    }

                    //check if there are any notes from the previous measure to be added to this measure
                    //and add them to the start of the current measure
                    int max=ExtraNotes.Count;
                    for (int k = 0; k < max; k++)
                    {
                        measure.getNoteOrBackupOrForward().add(k, ExtraNotes.Dequeue());
                            
                    }

                    //check if the notes do not fit in this measure then remove them from the current measure
                    //System.Windows.MessageBox.Show("Initial Number of notes in the measure: " + EditDS.getNotesperMeasure(measure).ToString());
                    int numNotes = EditDS.getNotesperMeasure(measure);
                    int thresholdIndex=-1;
                    for (int k = 1; k <= numNotes; k++)
                    {
                        NoteProperties noteprop = ReadDS.ReadNote(k, measure);

                        if (DSum + noteprop.duration > DivPerMeasure)
                        {
                            // if the note is too big to fit a measure
                            if (noteprop.duration > DivPerMeasure) return null;
                            thresholdIndex = k;
                            break;
                        }
                        else
                        {
                            DSum += noteprop.duration;
                        }
                    }
                    if (thresholdIndex != -1)
                    {
                        //first queue the extra notes
                        for (int l = thresholdIndex; l <= numNotes; l++)
                            ExtraNotes.Enqueue((Note)(measure.getNoteOrBackupOrForward().get(EditDS.getnoteindex(l, measure))));
                        //then remove from the current measure
                        for (int l = thresholdIndex; l <= numNotes; l++)
                        {
                            measure.getNoteOrBackupOrForward().remove(EditDS.getnoteindex(thresholdIndex, measure));
                                
                        }

                        List<int> RestDurations = new List<int>();
                        int DivLeft = (int)DivPerMeasure - DSum; //number of divisions left empty in this measure
                        int power = 0;
                        while (DivLeft != 0)
                        {
                            if ((DivLeft & 1) != 0) //check the leftmost bit
                            {
                                RestDurations.Add(1 << power);  //rest durations in powers of two

                            }
                            ++power;
                            DivLeft = DivLeft >> 1;

                        }

                        foreach (int dur in RestDurations)
                        {
                            NoteProperties restprop = new NoteProperties();
                            restprop.rest = true;
                            restprop.duration = dur;

                            measure.getNoteOrBackupOrForward().add(CreateDS.CreateNote(restprop));
                        }

                    }
                    //set the measure in the part
                    part.getMeasure().set(j, measure);


                }

                //After all the measures in the part are finished and the queue still have notes

                while (ExtraNotes.Count != 0)
                {
                    ScorePartwise.Part.Measure measure = CreateDS.CreateMeasure("2");
                    int DSum = 0;
                    while (DSum < DivPerMeasure)
                    {
                        if (ExtraNotes.Count == 0) break;
                        if ((DSum + ((BigDecimal)ExtraNotes.Peek().getDuration()).intValue()) > DivPerMeasure)
                            break;
                        else
                        {
                            DSum += ((BigDecimal)ExtraNotes.Peek().getDuration()).intValue();
                            measure.getNoteOrBackupOrForward().add(ExtraNotes.Dequeue());
                        }
                    }
                    part.getMeasure().add(measure);
                }

            }
           
            return scorePartwise;
        }
    }
}
