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
    class CreateDS
    {
        //***************************************** Create Object Factory *******************************************//
        //The Object Factory, Used to create the elements of the score as required
        
        //***************************************** ScorePartwise *******************************************//
        // Create the top level Partwise Score Element
        
        
        public static ScorePartwise CreateScorePartwise(ScoreProperties i)
        {
             
            ObjectFactory factory = new ObjectFactory();
            //Create a scorepart
            ScorePartwise.Part part;
            Marshalling.getContext();
            ///////////////////// Following are the main components required for the Score in Phase 1

           
            //***************************************** ScorePartwise *******************************************//
            ScorePartwise scorePartwise = factory.createScorePartwise();

            // Set the version of MusicXML
            scorePartwise.setVersion("3.0");

            //***************************************** Set Work and Movement Titles *******************************************//
            // Create the work element and set its title
            Work work = factory.createWork();
            work.setWorkTitle(i.WorkTitle);


            //Setting some properties of the root element.
            scorePartwise.setMovementTitle(i.MovementTitle);

            //Set the work title of the scorepartwise
            scorePartwise.setWork(work);

            //***************************************** Create and set Identification Element *******************************************//

            //Create identification and set its properties
            Identification identification = factory.createIdentification();
            com.audiveris.proxymusic.Encoding encoding = factory.createEncoding();

            //set the software used for encoding
            encoding.getEncodingDateOrEncoderOrSoftware().add(factory.createEncodingSoftware(i.EncodingSoftware));
            
            // set the encoding
            identification.setEncoding(encoding);
            scorePartwise.setIdentification(identification);


            //***************************************** Set the Credit words *******************************************//

            Credit credit = factory.createCredit();
            FormattedText creditwordtext = new FormattedText();
            creditwordtext.setValue(i.CreditWords);
            credit.getCreditTypeOrLinkOrBookmark().add(creditwordtext);

            scorePartwise.getCredit().add(credit);
            //***************************************** Create a PartList *******************************************//

            PartList partlist = factory.createPartList();

            // create a scorepart
            ScorePart scorepart = factory.createScorePart();

            ////////////////////////Some Properties of the scorepart
            PartName partname = factory.createPartName();
            partname.setValue(i.PartDisplayName);
            scorepart.setPartName(partname);

            //set part abbreviation
            partname.setValue(i.PartAbbreviation);
            scorepart.setPartAbbreviation(partname);

            //set the scorepart id
            scorepart.setId(i.ScorePartID);

            // set the scorepart instrument 
            ScoreInstrument scoreinstrument = factory.createScoreInstrument();
            scoreinstrument.setId(i.InstrumentID);
            scoreinstrument.setInstrumentName(i.InstrumentName);

            scorepart.getScoreInstrument().add(scoreinstrument);

            //set the midi device parameters
            MidiInstrument midiinstrument = factory.createMidiInstrument();
            //set midi instrument Id
            midiinstrument.setId(scoreinstrument);

            //set midi channel
            java.lang.Integer midichannel = i.MidiChannel; //channel number
            midiinstrument.setMidiChannel(midichannel);

            //set midi program
            java.lang.Integer midiprogram = i.MidiProgram; //program number
            midiinstrument.setMidiProgram(midiprogram);


            scorepart.getMidiDeviceAndMidiInstrument().add(midiinstrument);

            // add the scorepart to the partlist
            partlist.getPartGroupOrScorePart().add(scorepart); //Ambiguity

            scorePartwise.setPartList(partlist);

            //***************************************** Create a Part *******************************************//
            if (i.Part == null)
                part = factory.createScorePartwisePart();
            else
                part = i.Part;
            //set part id
            part.setId(scorepart);

            // Add the part to the score
            scorePartwise.getPart().add(part);

            return scorePartwise;
        }

        public static ScorePartwise.Part.Measure CreateMeasure(string MeasureNumber)
        {
            ObjectFactory factory = new ObjectFactory();

            ScorePartwise.Part.Measure measure = factory.createScorePartwisePartMeasure();
            measure.setNumber(MeasureNumber);
            return measure;
        }

        public static Attributes CreateAttribute(AttributeProperties ai)
        {
            ObjectFactory factory = new ObjectFactory();
            Attributes attributes = factory.createAttributes();
            //create a key
            Key key = factory.createKey();
            //create a time element
            Time time = factory.createTime();
            //create a clef element
            Clef clef = factory.createClef();
            
            //set the staffs
            attributes.setStaves(new BigInteger(ai.staves.ToString()));
            

            //now add the elements to the attributes
            
            if(ai.divisions!=0)
                //set the divisions
                attributes.setDivisions(new BigDecimal(ai.divisions));
            if (ai.fifths != "")
            {
                //set the key
                key.setFifths(new BigInteger(ai.fifths));
                key.setMode(ai.mode);

                //add the key
                attributes.getKey().add(key);
            }
            if (ai.BeatsPerMeasure != "")
            {
                //set the time signature
                time.getTimeSignature().add(factory.createTimeBeats(ai.BeatsPerMeasure));
                time.getTimeSignature().add(factory.createTimeBeatType(ai.BeatType));

                // add the time signature
                attributes.getTime().add(time);
            }
            // add the cleff
            if (ai.ClefSign != "")
            {
                

                //set the clef
                clef.setSign(ClefSign.fromValue(ai.ClefSign));
                clef.setLine(new BigInteger(ai.ClefLine));
                clef.setNumber(new BigInteger(ai.ClefStaff.ToString()));

                attributes.getClef().add(clef);
            }
            return attributes;

        }

        public static Note CreateNote(NoteProperties ni)
        {
            ObjectFactory factory = new ObjectFactory();
            Note note = factory.createNote();
            
            if(ni.chord==true)
                note.setChord(new Empty());
            
            
            if(ni.rest==true)
                note.setRest(factory.createRest());
            else
                note.setPitch(CreatePitch(ni.PitchStep, ni.Pitchoctave, ni.PitchAlter));
            
            //set the duration
            note.setDuration(new BigDecimal(ni.duration));

            //set the tie element
            if(ni.TieType!="")
                note.getTie().add(CreateTie(ni.TieType));


            //Set the Voice
            note.setVoice("1");

            //assign the notetype
            note.setType(CreateNotetype(ni.NoteType));

            //Set a dot
            if(ni.dot==true)
                note.getDot().add(new EmptyPlacement());

            //assign the accidental
            if(ni.AccidentalType!="")
                note.setAccidental(CreateAccidental(ni.AccidentalType));

            //assign the timemodification
            if(ni.TimeModactualnotes!="")
                note.setTimeModification(CreateTimemodification(ni.TimeModactualnotes, ni.TimeModnormalnotes, ni.TimeModnormaltype));

            //Set the note staff
            note.setStaff(new BigInteger(ni.staff.ToString()));

            ////add a beam
           for(int i =0; i<ni.beamnumber.Count; i++)
                note.getBeam().add(CreateBeam(ni.beamnumber.ElementAt<int>(i), ni.beamvalue.ElementAt<string>(i)));

            if (ni.SlurType != "" | ni.TupletType != "" | ni.TiedType != "" | ni.dynamic!="")
            {
                Notations notations = factory.createNotations();

                //add a tied element
                if (ni.TiedType != "")
                    notations.getTiedOrSlurOrTuplet().add(CreateTied(ni.TiedType));


                //add a slur
                if (ni.SlurType != "")
                    notations.getTiedOrSlurOrTuplet().add(CreateSlur(ni.SlurType));


                //add a tuplet
                if (ni.TupletNumber != 0)
                    notations.getTiedOrSlurOrTuplet().add(CreateTuplet(ni.TupletNumber, ni.TupletType));

                //Adding dynamics here
                if (ni.dynamic != "")
                {
                    Dynamics dynamics = new Dynamics();
                    
                    switch (ni.dynamic)
                    {
                        case "pp":
                            {
                                dynamics.getPOrPpOrPpp().add(factory.createDynamicsPp(new Empty()));
                                //add the dynamic to the direction
                               notations.getTiedOrSlurOrTuplet().add(dynamics);
                                break;
                            }
                        case "p":
                            {
                                dynamics.getPOrPpOrPpp().add(factory.createDynamicsP(new Empty()));
                                //add the dynamic to the direction
                               notations.getTiedOrSlurOrTuplet().add(dynamics);
                                break;
                            }
                        case "mp":
                            {
                                dynamics.getPOrPpOrPpp().add(factory.createDynamicsMp(new Empty()));
                                //add the dynamic to the direction
                               notations.getTiedOrSlurOrTuplet().add(dynamics);
                                break;
                            }
                        case "mf":
                            {
                                dynamics.getPOrPpOrPpp().add(factory.createDynamicsMf(new Empty()));
                                //add the dynamic to the direction
                               notations.getTiedOrSlurOrTuplet().add(dynamics);
                                break;
                            }
                        case "f":
                            {
                                dynamics.getPOrPpOrPpp().add(factory.createDynamicsF(new Empty()));
                                //add the dynamic to the direction
                               notations.getTiedOrSlurOrTuplet().add(dynamics);
                                break;
                            }
                        case "ff":
                            {
                                dynamics.getPOrPpOrPpp().add(factory.createDynamicsFf(new Empty()));
                                //add the dynamic to the direction
                               notations.getTiedOrSlurOrTuplet().add(dynamics);
                                break;
                            }

                    }
                }
                    //add the notations to the note
                    note.getNotations().add(notations);
            }
            //assign the lyric
            if(ni.LyricText!="")
                note.getLyric().add(CreateLyric(ni.LyricSyllabic, ni.LyricText, ni.LyricEndline));
            
            return note;

        }

        public static Barline CreateBarline(BarlineProperties bi)
        {
            ObjectFactory factory = new ObjectFactory();
            Barline barline = factory.createBarline();
           
            //create an ending
            if (bi.EndingValue != "")
            {
                Ending ending = factory.createEnding();
                ending.setType(StartStopDiscontinue.fromValue(bi.EndingType));
                ending.setValue(bi.EndingValue);
                barline.setEnding(ending);
            }

            //create a repeat
            if (bi.RepeatTimes != "")
            {
                Repeat repeat = factory.createRepeat();
                repeat.setTimes(new BigInteger(bi.RepeatTimes));
                repeat.setDirection(BackwardForward.fromValue(bi.RepeatDirection));
                barline.setRepeat(repeat);
            }

            //set bar line location
            if(bi.Location!="")
                barline.setLocation(RightLeftMiddle.fromValue(bi.Location));

            return barline;
        }

        public static Direction CreateDirection(DirectionProperties di)
        {
            ObjectFactory factory = new ObjectFactory();
            Direction direction = factory.createDirection();

            //Create a dynamic element
            Dynamics dynamics = factory.createDynamics();
            // set the dynamic symbol p, f, and so on
            if(di.DynamicSymbol!="")
                switch (di.DynamicSymbol)
                    {
                        case "pp":
                            {
                                dynamics.getPOrPpOrPpp().add(factory.createDynamicsPp(new Empty()));
                                //add the dynamic to the direction
                                direction.getDirectionType().add(dynamics);
                                break;
                            }
                        case "p":
                            {
                                dynamics.getPOrPpOrPpp().add(factory.createDynamicsP(new Empty()));
                                //add the dynamic to the direction
                                direction.getDirectionType().add(dynamics);
                                break;
                            }
                        case "mp":
                            {
                                dynamics.getPOrPpOrPpp().add(factory.createDynamicsMp(new Empty()));
                                //add the dynamic to the direction
                                direction.getDirectionType().add(dynamics);
                                break;
                            }
                        case "mf":
                            {
                                dynamics.getPOrPpOrPpp().add(factory.createDynamicsMf(new Empty()));
                                //add the dynamic to the direction
                                direction.getDirectionType().add(dynamics);
                                break;
                            }
                        case "f":
                            {
                                dynamics.getPOrPpOrPpp().add(factory.createDynamicsF(new Empty()));
                                //add the dynamic to the direction
                                direction.getDirectionType().add(dynamics);
                                break;
                            }
                        case "ff":
                            {
                                dynamics.getPOrPpOrPpp().add(factory.createDynamicsFf(new Empty()));
                                //add the dynamic to the direction
                                direction.getDirectionType().add(dynamics);
                                break;
                            }

                    }

            return direction;
            ////create a segno sign
            //DirectionType directiontype = factory.createDirectionType();
            //directiontype.getSegno().add(new EmptyPrintStyleAlign());
            ////assign to the direction
            //direction.getDirectionType().add(directiontype);

            ////create a metronome
            //Metronome metronome = factory.createMetronome();
            //PerMinute pm = factory.createPerMinute();
            //pm.setValue("120");
            //metronome.setPerMinute(pm);
            //directiontype.setMetronome(metronome);

        }

        public static Pitch CreatePitch(string step, int octave, float alter)
        {
            ObjectFactory factory = new ObjectFactory();
            Pitch pitch = factory.createPitch();
            //Set Pitch step
            pitch.setStep(Step.fromValue(step));
            //set pitch octave
            pitch.setOctave(octave);
            //set pitch alter
            if(alter!=float.MaxValue)
                pitch.setAlter(new BigDecimal(alter));
            return pitch;

        }

        public static Slur CreateSlur(string type)
        {            
            //create a slur
            Slur slur = new Slur();
            slur.setType(StartStopContinue.fromValue(type));
            slur.setNumber(new Integer(1));
            return slur;
        }

        public static Tied CreateTied(string type)
        {

            ObjectFactory factory = new ObjectFactory();
            //create a tied element
            Tied tied = factory.createTied();
            tied.setType(StartStopContinue.fromValue(type));
            tied.setNumber(new Integer(1));
            return tied;
        }

        public static Tie CreateTie(string type)
        {
            ObjectFactory factory = new ObjectFactory();
            Tie tie = factory.createTie();
            tie.setType(StartStop.fromValue(type));            
            return tie;
        }

        public static NoteType CreateNotetype(string type)
        {
            ObjectFactory factory = new ObjectFactory();
            //Create a notetype
            NoteType notetype = factory.createNoteType();
            notetype.setValue(type);
            return notetype;
        }

        public static Lyric CreateLyric(string syllabic, string text, Nullable<bool> endline)
        {
            ObjectFactory factory = new ObjectFactory();
            Lyric lyric = factory.createLyric();

            //create a text element
            TextElementData textelementdata = factory.createTextElementData();
            //set a syllabic
            if(syllabic!="")
                lyric.getElisionAndSyllabicAndText().add(Syllabic.fromValue(syllabic));
            textelementdata.setValue(text);
            lyric.getElisionAndSyllabicAndText().add(textelementdata);

            //set the line end
            if(endline==true)
                lyric.setEndLine(new Empty());
            
            return lyric;
        }

        public static Accidental CreateAccidental(string type)
        {
            ObjectFactory factory = new ObjectFactory();
            ////////////////////////////////////////Create an accidental
            Accidental accidental = factory.createAccidental();
            accidental.setValue(AccidentalValue.fromValue(type));
            return accidental;
        }

        public static TimeModification CreateTimemodification(string actualnotes, string normalnotes, string normaltype="")
        {
            ObjectFactory factory = new ObjectFactory();
            /////////////////////////////////Create a time modification element
            TimeModification timemodification = factory.createTimeModification();
            timemodification.setActualNotes(new BigInteger(actualnotes));
            timemodification.setNormalNotes(new BigInteger(normalnotes));
            if(normaltype!="")
                timemodification.setNormalType(normaltype);
            return timemodification;
        }

        public static Beam CreateBeam(int beamnumber, string value)
        {
            ObjectFactory factory = new ObjectFactory();
            /////////////////////////Create a Beam
            Beam beam = factory.createBeam();
            beam.setNumber(new Integer(beamnumber));
            beam.setValue(BeamValue.fromValue(value));
            return beam;
        }

        public static Tuplet CreateTuplet(int number,string type)
        {
            ObjectFactory factory = new ObjectFactory();
            Tuplet tuplet = factory.createTuplet();
            if(type=="start")
                tuplet.setBracket(YesNo.YES);
            tuplet.setNumber(new Integer(number));
            tuplet.setType(StartStop.fromValue(type));
            return tuplet;
        }


    }
}
