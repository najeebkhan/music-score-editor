using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using com.audiveris.proxymusic.util;
using com.audiveris.proxymusic;
using System.Globalization;
using Microsoft.Windows.Controls.Ribbon;
using System.Threading;
using System.Runtime.InteropServices;

namespace ScoreEditor_WPF_Version
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        //Global Variables
        
        //stores the score
        ScorePartwise scorePartwise;
        ScoreProperties scoreprop;
        string SelectedSymbol = ""; //Note type and rest type such as whole half etc
        enum Action {Insert, Add, Delete, Idle};
        enum MIDIStep { C=48, D=50, E=52, F=53, G=55, A=57, B=59 };
        int MIDITempo=120;
        int MIDIVol = 70;
        bool MIDIContinue;
        Thread playthread;       
        Action action=Action.Idle;
        MyCanvas CurrentNoteCanvas = null;         
        string CurrentClef = "";
        int CurrentDivisions = 0;
        int CurrentBeatType = 4;
        string CurrentFifths = "";
        int StaffSize = 20;

        public MainWindow()
        {
            InitializeComponent();
            Marshalling.getContext();
            GUIPara.Initialize();
            DrawStaff(numberofstaffs: StaffSize);
            playthread = new Thread(new ThreadStart(PlayNotes));
        }

        void DrawStaff(double width = 1024, double height = 6.25, int numberofstaffs = 5)
        {
            double deltaYTop = 11.56;
            double deltaYBottom = 20;
            if (numberofstaffs * 153>700)
                StaffStackPanel.Height=numberofstaffs*153;
            for (int i = 0; i < numberofstaffs; i++)
            {
                List<StaffLineCanvas> staflines = new List<StaffLineCanvas>();
                
                //line
                Line l = new Line();


                //some border gap

                staflines.Add(new StaffLineCanvas());
                staflines[0].Width = width;
                staflines[0].Height = deltaYTop;
                staflines[0].Background = Brushes.Transparent;


                for (int loop = 1; loop <= 21; loop++)
                {

                    //For each of the pitch

                    StaffLineCanvas slc = new StaffLineCanvas();
                    slc.LineNumber = loop;
                    staflines.Add(slc);
                    staflines[loop].Width = width;
                    staflines[loop].Height = height;
                    staflines[loop].Background = Brushes.Transparent;
                    staflines[loop].MouseLeftButtonDown += new MouseButtonEventHandler(StaffLines_MouseLeftButtonDown);

                }
              

                //Bottom offset for lyrics etc
                staflines.Add(new StaffLineCanvas());
                staflines[22].Width = width;
                staflines[22].Height = deltaYBottom;
                staflines[22].Background = Brushes.Transparent;
                

                //add solid lines to the middle rectangles

                for (int odds = 7; odds < 17; odds += 2)
                {
                    l = new Line();
                    l.X1 = 0;
                    l.Y1 = height / 2;
                    l.X2 = width;
                    l.Y2 = height / 2;
                    l.Stroke = Brushes.Black;
                    staflines[odds].Children.Add(l);
                }

                //add the bar line at the end of the staf

                l = new Line();
                l.X1 = 0;
                l.Y1 = height/2;
                l.X2 = 0;
                l.Y2 = height*8.5;
                l.Stroke = Brushes.Black;
                l.StrokeThickness = 2;

                Canvas.SetRight(l, 0);
                staflines[7].Children.Add(l);


                //add all the canvases to the stackpanel
                foreach (Canvas can in staflines)
                    StaffStackPanel.Children.Add(can);
            }
        }

        void AddSpaceToDisplay(int MeasureID, int width, StackPanel measuresp, int index=-1)
        {

            MyCanvas c = new MyCanvas();
            c.Height = 162.8;         //calculated
            c.Width = width;
            c.MeasureNum = MeasureID;
            c.NoteNum = 0;
            c.Modifier = -10;

            //add
            if (index == -1)
                measuresp.Children.Add(c);
            else
                measuresp.Children.Insert(index, c);

        }

        void AddNoteToDisplay(int ElementID, int MeasureID, int NoteID, int Modifier, double xOffset, double y, int Unicode, double SymbolMarginX,  StackPanel measuresp, string lyrics="-1", int index=-1)
        {
           
            MyCanvas c = new MyCanvas();
           
            c.Height = 162.8;         //calculated
            c.ElementNum = ElementID;
            c.MeasureNum = MeasureID;
            c.NoteNum = NoteID;
            c.Modifier = Modifier;
            
            //c.MouseLeftButtonDown += new MouseButtonEventHandler(Note_MouseLeftButtonDown);
            
            //assigning mouse entering event for the canvases in case required later other wise
            // delete these along with their handlers
            
            //c.MouseEnter += new MouseEventHandler(Note_MouseEnter);
            //c.MouseLeave += new MouseEventHandler(Note_MouseLeave);

            TextBlock txtblock = new TextBlock();
            txtblock.FontSize = 60;
            txtblock.FontFamily = new FontFamily("Bach");
            txtblock.Foreground = Brushes.Black;
            
            txtblock.Text = char.ConvertFromUtf32(Unicode);
            txtblock.MouseEnter += new MouseEventHandler(txtblock_MouseEnter);
            txtblock.MouseLeave += new MouseEventHandler(txtblock_MouseLeave);
            txtblock.MouseLeftButtonDown += new MouseButtonEventHandler(txtblock_MouseLeftButtonDown);

            c.Width = GUIPara.MeassureGlyphWidth(txtblock.Text)+ SymbolMarginX;
            MyCanvas.SetLeft(txtblock, xOffset);
            MyCanvas.SetTop(txtblock, y);

            c.Children.Add(txtblock);

            if (lyrics != "-1" & lyrics!= "")
            {
                TextBox lyricsbox = new TextBox();
                lyricsbox.Text = lyrics;
                lyricsbox.BorderBrush = Brushes.Transparent;
                Canvas.SetTop(lyricsbox, 150);
                c.Children.Add(lyricsbox);
                
            }


            //add
            if (index == -1)
                measuresp.Children.Add(c);
            else
            {
                //replace
                //measuresp.Children.RemoveAt(index);
                //measuresp.Children[index] = c;
                
                //add
                measuresp.Children.Insert(index, c);

            }
            
        }

        void AddTimeSignature(int ID, double xOffset, double SymbolMarginX, string TimeBeats, string BeatType, StackPanel measuresp)
        {
                       
            MyCanvas c = new MyCanvas();

            c.Height = 162.8;          //calculated
            c.Name = "t" + ID.ToString();
            c.MouseLeftButtonDown += new MouseButtonEventHandler(Note_MouseLeftButtonDown);

            
            TextBlock txtblock = new TextBlock();
            txtblock.FontSize = 30;
            txtblock.FontFamily = new FontFamily("Courier");
            txtblock.Foreground = Brushes.Black;

            System.Windows.Media.FormattedText ft = new System.Windows.Media.FormattedText(TimeBeats,
                                                 CultureInfo.CurrentCulture,
                                                 FlowDirection.LeftToRight,
                                                new Typeface("Courier"),
                                                 txtblock.FontSize,
                                                 Brushes.Black);
            c.Width = ft.Width;
            ft = new System.Windows.Media.FormattedText(BeatType,
                                    CultureInfo.CurrentCulture,
                                    FlowDirection.LeftToRight,
                                    new Typeface("Courier"),
                                    txtblock.FontSize,
                                    Brushes.Black);

            if (c.Width > ft.Width)
            {
                BeatType = " " + BeatType;
            }
            else if (c.Width < ft.Width)
            {
                TimeBeats = " " + TimeBeats;
                c.Width = ft.Width;
            }
            else
                c.Width = ft.Width;

            c.Height = c.Height = 162.8; 
            txtblock.LineHeight = 30;
            txtblock.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
            txtblock.Text = TimeBeats + "\n" + BeatType;
            
            c.Width = GUIPara.MeassureGlyphWidth(txtblock.Text) + SymbolMarginX;
            MyCanvas.SetLeft(txtblock, xOffset);
            MyCanvas.SetTop(txtblock, GUIPara.StaffLines[6]);
            c.Children.Add(txtblock);
            //Canvas.SetTop(txtblock, 10);
            //StackPanel tempsp = new StackPanel();
            //tempsp.Children.Add(c);
            measuresp.Children.Add(c);
        }

        void AddKeySignature(int fifths, string clef, StackPanel measuresp, int index = -1)
        {            
            double xOffset = 0;
            int i = 0;
         
            //align it to the array storing key signatures            
            string accidentals;
            if (fifths < 0)
            {
                fifths = -fifths;
                accidentals="flat";
            }
            else
            {
                fifths += 7;
                i += 7;
                accidentals="sharp";
            }          
                       
            MyCanvas c = new MyCanvas();
            
            c.Height = 162.8;         //calculated
            
            //for each accidental in the key signature
            for (; i < fifths; i++)
            {
                TextBlock txtblock = new TextBlock();
                txtblock.FontSize = 60;
                txtblock.FontFamily = new FontFamily("Bach");
                txtblock.Foreground = Brushes.Black;
                int[] uplusc = new int[2];
                switch(clef)
                {
                    case "F":
                        {
                            uplusc = GUIPara.PixelCalc(accidentals, GUIPara.FKeySig[i].Item1, GUIPara.FKeySig[i].Item2.ToString(), clef);
                            break;
                        }
                    case "C":
                        {
                            //C clef key sig is 1 octive higher than f clef
                            uplusc = GUIPara.PixelCalc(accidentals, GUIPara.FKeySig[i].Item1, (GUIPara.FKeySig[i].Item2+1).ToString(), clef);
                            break;
                        }
                    case "G":
                        {
                            //G clef key sig is 2 octive higher than f clef
                            uplusc = GUIPara.PixelCalc(accidentals, GUIPara.FKeySig[i].Item1, (GUIPara.FKeySig[i].Item2 + 2).ToString(), clef);
                            break;
                        }
                }
                
                uplusc[1] = uplusc[1] * 60 / 1024;
                txtblock.Text = char.ConvertFromUtf32(uplusc[0]);
               
                MyCanvas.SetLeft(txtblock, xOffset);
                MyCanvas.SetTop(txtblock, uplusc[1]);
                c.Children.Add(txtblock);
                
                xOffset += GUIPara.MeassureGlyphWidth(txtblock.Text) - 5;
            }
            c.Width = xOffset+15;

            //add
            if (index == -1)
                measuresp.Children.Add(c);
            else
            {
                //replace
                //measuresp.Children.RemoveAt(index);
                //measuresp.Children[index] = c;

                //add
                measuresp.Children.Insert(index, c);

            }

        }
        
        private void DisplayScore(ScorePartwise scorePartwise)
        {
            //Some objects to store score data
            ScorePartwise.Part part;            
            AttributeProperties attributeprop;
            NoteProperties noteprop;

           //For chords
           int ChordCanvas;

            //Read the first part in the score
            part = (ScorePartwise.Part)scorePartwise.getPart().get(0);

            //calculate the size of the score
            int stafflength = 0;
            for (int i = 0; i < part.getMeasure().size(); i++)
            {
                stafflength += ((ScorePartwise.Part.Measure)part.getMeasure().get(i)).getNoteOrBackupOrForward().size();
            }
            
            //DrawStaff(numberofstaffs:StaffSize);
            //DrawStaff(numberofstaffs: 50);

            //Read the overall score properties
            scoreprop = ReadDS.ReadScore(scorePartwise);

            //Display any top level information about the score here

            //Draw the staves

            //initialize the printing position

            int[] uplusc; //store pixel correction and unicode

            //string CurrentClef = "";
            //int CurrentDivisions = 8;
            int NoteSpacing = 15;
            int Elementsnumber = 0;
            
            //for each measure in the score
            for (int i = 0; i < part.getMeasure().size(); i++)
            {
                ////////////////////////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////// Ith MEASURE ///////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////////////////////

                //stackpanel for the notes belonging to single measure
                StackPanel MeasureStackPanel = new StackPanel();
                MeasureStackPanel.Orientation = Orientation.Horizontal;
               
                attributeprop = ReadDS.ReadAttributes((ScorePartwise.Part.Measure)part.getMeasure().get(i));
                if (attributeprop != null)
                {
                    CurrentDivisions = attributeprop.divisions != 0 ? attributeprop.divisions : CurrentDivisions;
                    
                    //check if clefsign is to be printed or not
                    if (attributeprop.ClefSign != "")
                    {
                        CurrentClef = attributeprop.ClefSign;

                        //Cleff
                        uplusc = GUIPara.PixelCalc(attributeprop.ClefSign, "", "", "");
                        if (uplusc != null)
                        {
                            uplusc[1] = uplusc[1] * 60 / 1024;
                            AddNoteToDisplay(Elementsnumber++, i, -1, 0, 0, uplusc[1], uplusc[0], NoteSpacing, MeasureStackPanel);

                        }
                    }

                    CurrentFifths = attributeprop.fifths;
                    //check if Key signature is to be printed
                    if (attributeprop.fifths != "0")
                    {
                        
                        AddKeySignature(int.Parse(attributeprop.fifths), CurrentClef, MeasureStackPanel);
                    }
                    //check if the time signature is to be printed or not
                    if (attributeprop.BeatType != "")
                    {
                        //change this man
                        AddTimeSignature(000000001, 0, 0, attributeprop.BeatsPerMeasure, attributeprop.BeatType, MeasureStackPanel);
                        CurrentBeatType = int.Parse(attributeprop.BeatType);
                    }

                    //print attributproperties and increment the cursor

                }

                //for each note in the measure
                int j = 1;
                bool bflag = false;
                int stemdirection = 0;
                List<GraphicBeamedProperties> BeamedNotes = new List<GraphicBeamedProperties>();
                while (ReadDS.ReadNote(j, (ScorePartwise.Part.Measure)part.getMeasure().get(i)) != null)
                {
                    ////////////////////////////////////////////////////////////////////////////////
                    //////////////////////////////Jth Note///////////////////////////////////////////
                    ////////////////////////////////////////////////////////////////////////////////
                   
                    noteprop = ReadDS.ReadNote(j, (ScorePartwise.Part.Measure)part.getMeasure().get(i));

                    //print the note and increment cursor
                    if (noteprop.rest == true)
                    {
                        if (noteprop.NoteType != "") // if notetype is specified
                            uplusc = GUIPara.PixelCalc("Rest_" + noteprop.NoteType, "", "", "");
                        else
                        {
                            // if note type is not specified then calculate based on the current 
                            // number of divisions
                            double x = (double)noteprop.duration / (double)CurrentDivisions;

                            if (Math.Abs(x - 4) <= 0.001)
                                noteprop.NoteType = "whole";
                            else if (Math.Abs(x - 2) <= 0.001)
                                noteprop.NoteType = "half";
                            else if (Math.Abs(x - 1) <= 0.001)
                                noteprop.NoteType = "quarter";
                            else if (Math.Abs(x - 0.5) <= 0.001)
                                noteprop.NoteType = "eighth";
                            else if (Math.Abs(x - 0.25) <= 0.001)
                                noteprop.NoteType = "16th";
                            else if (Math.Abs(x - 0.125) <= 0.001)
                                noteprop.NoteType = "32nd";
                            if (noteprop.NoteType != "")
                                uplusc = GUIPara.PixelCalc("Rest_" + noteprop.NoteType, "", "", "");
                            else
                                uplusc = GUIPara.PixelCalc("Space", "", "", ""); // if nothing works then just assume
                        }

                    }
                    else if (noteprop.chord == true)
                    {
                        //if this note is a chord then

                        //implement the chording here
                        uplusc = null;

                    }

                    else
                    {
                        //if it is a note
                        // if there is an accidental
                        if (noteprop.AccidentalType != "")
                        {

                            uplusc = GUIPara.PixelCalc(noteprop.AccidentalType, noteprop.PitchStep, noteprop.Pitchoctave.ToString(), CurrentClef);
                            uplusc[1] = uplusc[1] * 60 / 1024;

                            AddNoteToDisplay(Elementsnumber++, i, j, -1, 0, uplusc[1], uplusc[0], 0, MeasureStackPanel);
                        }
                       

                        //if this note is beamed then stem directions are affected
                        if (noteprop.beamnumber.Count > 0)
                        {
                            if (!bflag) bflag = true;
                            if (stemdirection == 0 | noteprop.beamvalue[0]=="begin") stemdirection = GUIPara.getStemDirection(noteprop.PitchStep, noteprop.Pitchoctave.ToString(), CurrentClef);
                            // place note without flag
                            uplusc = GUIPara.PixelCalc("quarter", noteprop.PitchStep, noteprop.Pitchoctave.ToString(), CurrentClef, stemdirection);
                            //StemXY = GUIPara.getNoteStemPosition(noteprop.PitchStep, noteprop.Pitchoctave.ToString(), CurrentClef, stemdirection);
                        }
                        else
                        {
                            if (bflag) bflag = false;
                            if (stemdirection != 0) stemdirection = 0;
                            //just a normal note
                            uplusc = GUIPara.PixelCalc(noteprop.NoteType, noteprop.PitchStep, noteprop.Pitchoctave.ToString(), CurrentClef);
                        }

                    }
                    
                    // whether note or rest, just display the symbol
                    if (uplusc != null)
                    {
                        ChordCanvas = Elementsnumber; // for chord function
                        uplusc[1] = uplusc[1] * 60 / 1024;
                        AddNoteToDisplay(Elementsnumber++, i, j, 0, 0, uplusc[1], uplusc[0], 0, MeasureStackPanel, noteprop.LyricText); 
                        if (noteprop.dot==true)
                        {
                            //place a dot
                            uplusc = GUIPara.PixelCalc("dot", noteprop.PitchStep, noteprop.Pitchoctave.ToString(), CurrentClef);
                            uplusc[1] = uplusc[1] * 60 / 1024;
                            AddNoteToDisplay(Elementsnumber++, i, j, +1,-5, uplusc[1], uplusc[0], 0, MeasureStackPanel);
                        }
                    }

                    //replace this with some neat spacing mechanism
                    AddSpaceToDisplay(i, NoteSpacing, MeasureStackPanel);
                    j++;

                    //////////////////////////////////////////////////////////////////////////////////////////////
                    ///////////////////////////////// End of Jth note ///////////////////////////////////////////
                    ////////////////////////////////////////////////////////////////////////////////////////////

                }

                //At the end of the measure print the bar
                uplusc = GUIPara.PixelCalc("bar", "", "", "");
                uplusc[1] = uplusc[1] * 60 / 1024;
                AddNoteToDisplay(Elementsnumber++, i, 0, 10, 0, uplusc[1], uplusc[0], NoteSpacing, MeasureStackPanel);

                NotesWrappanel.Children.Add(MeasureStackPanel);
                
                
                MeasureStackPanel.UpdateLayout();
                NotesWrappanel.UpdateLayout();

                ////////////////////////////////////////////////////////////////////////////////////////////
                /////////////////////////////////// End of Ith MEASURE /////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////////////////////


            }
            
            FormattingPass();

            BeamTiePass(part);
        }
        
        void BeamTiePass(ScorePartwise.Part part)
        {          
            bool slurtieflag = false;
            List<GraphicSlurTieProperties> SlurTieProp = new List<GraphicSlurTieProperties>();
            //for each measure in the score
            for (int i = 0; i < part.getMeasure().size(); i++)
            {
                ////////////////////////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////// Ith MEASURE ///////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////////////////////

                //for each note in the measure
                int j = 1;
                bool bflag = false;
               
                int stemdirection = 0;
                NoteProperties noteprop;                
                List<GraphicBeamedProperties> BeamedNotes = new List<GraphicBeamedProperties>();
                while (ReadDS.ReadNote(j, (ScorePartwise.Part.Measure)part.getMeasure().get(i)) != null)
                {
                    noteprop = ReadDS.ReadNote(j, (ScorePartwise.Part.Measure)part.getMeasure().get(i));
                    ////////////////////////////////////////////////////////////////////////////////
                    //////////////////////////////Jth Note///////////////////////////////////////////
                    ////////////////////////////////////////////////////////////////////////////////
                    
                    #region get beam information
                    
                    int[] StemXY = null;
                    
                    //if this note is beamed
                    if (noteprop.beamnumber.Count > 0)
                    {
                        if (!bflag) bflag = true;
                        if (stemdirection == 0 | noteprop.beamvalue[0] == "begin") stemdirection = GUIPara.getStemDirection(noteprop.PitchStep, noteprop.Pitchoctave.ToString(), CurrentClef);
                        StemXY = GUIPara.getNoteStemPosition(noteprop.PitchStep, noteprop.Pitchoctave.ToString(), CurrentClef, stemdirection);
                    }
                    else
                    {
                        if (bflag) bflag = false;
                        if (stemdirection != 0) stemdirection = 0;
                    }

                    if (noteprop.beamvalue.Count > 0)
                    {
                        GraphicBeamedProperties bp = new GraphicBeamedProperties();                      

                        var canvas = ((StackPanel)NotesWrappanel.Children[i]).Children.Cast<MyCanvas>().FirstOrDefault(o => o.NoteNum == j & o.Modifier == 0);
                        //MessageBox.Show(" Local Canvas " + (StemXY[0] * 60 / 1024).ToString()+ "  " + (StemXY[1] * 60 / 1024).ToString());
                        if (canvas != null)                        
                            bp.StemEnd = canvas.TranslatePoint(new Point(StemXY[0] * 60 / 1024, StemXY[1] * 60 / 1024), TopCanvas);
                        //MessageBox.Show(" Top Canvas " + bp.StemEnd.X.ToString() + "  " + bp.StemEnd.Y.ToString());  
                        bp.beamnumber = noteprop.beamnumber;
                        bp.beamvalue = noteprop.beamvalue;
                        bp.stemdirection = stemdirection;
                        BeamedNotes.Add(bp);
                    }
                    #endregion

                    #region Get Slur/Tie Information                    
                    if (noteprop.TiedType != "" | noteprop.SlurType != "")
                    {
                        GraphicSlurTieProperties slurtieprop = new GraphicSlurTieProperties();
                        slurtieprop.type = noteprop.TiedType == "" ? noteprop.SlurType : noteprop.TiedType;
                        slurtieprop.measure = i;
                        slurtieprop.note = j;
                        slurtieprop.updown = stemdirection == 0 ? GUIPara.getStemDirection(noteprop.PitchStep, noteprop.Pitchoctave.ToString(), CurrentClef) : stemdirection;
                        
                        int[] point;
                        bool stemdiff=false;
                                             
                        if (slurtieprop.type == "start" & slurtieflag)
                            slurtieprop.startstart = true;
                        else if (slurtieprop.type == "start")
                            slurtieflag = true;
                        else if (slurtieprop.type == "stop")
                            slurtieflag = false;

                        if (SlurTieProp.Count > 0)
                        {
                            stemdiff = (SlurTieProp[SlurTieProp.Count - 1].type == "start") &
                                (SlurTieProp[SlurTieProp.Count - 1].updown != slurtieprop.updown) &
                                !(SlurTieProp[SlurTieProp.Count - 1].startstart) ? stemdiff = true : stemdiff = false;                           

                        }
                        point = GUIPara.getSlurTiePosition(noteprop.PitchStep, noteprop.Pitchoctave.ToString(), CurrentClef, slurtieprop.updown, stemdiff);
                        slurtieprop.point = new Point(point[0] * 60 / 1024, point[1] * 60 / 1024);
                        if (SlurTieProp.Count > 0)
                        {
                            slurtieprop.updown = slurtieprop.startstart ? SlurTieProp[SlurTieProp.Count - 1].updown : slurtieprop.updown;
                        }
                        var canvas = ((StackPanel)NotesWrappanel.Children[i]).Children.Cast<MyCanvas>().FirstOrDefault(o => o.NoteNum == j & o.Modifier == 0);
                        if (canvas != null)                       
                            slurtieprop.point = canvas.TranslatePoint(slurtieprop.point, TopCanvas);                            
                        SlurTieProp.Add(slurtieprop);

                    }

                    #endregion
                                      
                    j++;

                }

                #region Beam Proecessing
                List<List<GraphicBeamedProperties>> singlebeams = new List<List<GraphicBeamedProperties>>();
                               
                //separate two or more beams in a measure
                while (BeamedNotes.Count != 0)
                {
                    List<GraphicBeamedProperties> onebeam = new List<GraphicBeamedProperties>();
                    foreach (GraphicBeamedProperties bp in BeamedNotes)
                    {
                        onebeam.Add(bp);
                        if (bp.beamvalue[0] == "end")
                            break;
                    }
                    foreach (GraphicBeamedProperties bp in onebeam)
                        BeamedNotes.Remove(bp);
                    singlebeams.Add(onebeam);
                }
                   
                double tmpslope = 0;
                double extremeval = 0;
                //for each single beam in a measure
                foreach (List<GraphicBeamedProperties> bpl in singlebeams)
                {
                    //for topmost beam

                    ///////////////////////////////////////////////////////////////////////////////////
                    //find if the points are acending or descending... inclined beam
                    /////////////////////////////////////////////////////////////////////////////////////
                    if (checkorder(bpl) != 0)
                    {
                        //find slope
                        double slope = (bpl[bpl.Count - 1].StemEnd.Y - bpl[0].StemEnd.Y) / (bpl[bpl.Count - 1].StemEnd.X - bpl[0].StemEnd.X);
                        tmpslope = slope;

                        for (int it = 1; it < bpl.Count - 1; it++)
                        {
                            if (Math.Abs((bpl[it].StemEnd.Y - bpl[0].StemEnd.Y) / (bpl[it].StemEnd.X - bpl[0].StemEnd.X)) > Math.Abs(tmpslope))
                            {
                                tmpslope = (bpl[0].StemEnd.Y - bpl[it].StemEnd.Y) / (bpl[0].StemEnd.X - bpl[it].StemEnd.X);
                            }
                        }
                        //Draw the beam with tmpslope
                        Line l = new Line();
                        l.X1 = bpl[0].StemEnd.X;
                        l.Y1 = bpl[0].StemEnd.Y;
                        l.X2 = bpl[bpl.Count - 1].StemEnd.X;
                        l.Y2 = bpl[0].StemEnd.Y + tmpslope * (bpl[bpl.Count - 1].StemEnd.X - bpl[0].StemEnd.X);

                        l.Stroke = Brushes.Black;
                        l.StrokeThickness = 6;
                        TopCanvas.Children.Add(l);

                        ////extend the stems for short notes in ascending notes
                        for (int it = 1; it < bpl.Count ; it++)
                        {
                            Line le = new Line();
                            le.X1 = bpl[it].StemEnd.X;
                            le.Y1 = bpl[it].StemEnd.Y;
                            le.X2 = bpl[it].StemEnd.X;
                            le.Y2 = tmpslope * (bpl[it].StemEnd.X - bpl[0].StemEnd.X) + bpl[0].StemEnd.Y;

                            le.Stroke = Brushes.Black;
                            le.StrokeThickness = 1;
                            TopCanvas.Children.Add(le);
                        }

                        //separate two or more beams in a measure
                        foreach (GraphicBeamedProperties nbp in bpl)
                        {
                            if (nbp.beamvalue.Count > 1)
                                if (nbp.beamvalue[1] == "backward hook")
                                {
                                    //Draw the beam with tmpslope
                                    int hooklength = 20;

                                    Line lfh = new Line();
                                    //current note stem x position
                                    lfh.X1 = nbp.StemEnd.X;
                                    //y postion of the line at current postion -/+ 10 up down
                                    lfh.Y1 = tmpslope * (nbp.StemEnd.X - bpl[0].StemEnd.X) + bpl[0].StemEnd.Y + 10 * nbp.stemdirection;


                                    lfh.X2 = nbp.StemEnd.X - hooklength;

                                    lfh.Y2 = lfh.Y1 - tmpslope * hooklength;

                                    lfh.Stroke = Brushes.Black;
                                    lfh.StrokeThickness = 6;
                                    TopCanvas.Children.Add(lfh);
                                }
                                else if (nbp.beamvalue[1] == "forward hook")
                                {
                                    //Draw the beam with tmpslope
                                    int hooklength = 20;

                                    Line lfh = new Line();
                                    //current note stem x position
                                    lfh.X1 = nbp.StemEnd.X;
                                    //y postion of the line at current postion -/+ 10 up down
                                    lfh.Y1 = tmpslope * (nbp.StemEnd.X - bpl[0].StemEnd.X) + bpl[0].StemEnd.Y + 10 * nbp.stemdirection;

                                    lfh.X2 = nbp.StemEnd.X + hooklength;

                                    lfh.Y2 = lfh.Y1 + tmpslope * hooklength;

                                    lfh.Stroke = Brushes.Black;
                                    lfh.StrokeThickness = 6;
                                    TopCanvas.Children.Add(lfh);
                                }
                        }
                    }


                    ////////////////////////////////////////////////////////////////////////////
                    //if the points are same height or up and down.... so flat beam
                    ////////////////////////////////////////////////////////////////////////
                    else //if the points are same height or up and down.... so flat beam
                    {

                        if (bpl[0].stemdirection == -1)
                        {
                            extremeval = int.MinValue;
                            foreach (GraphicBeamedProperties bpp in bpl)
                            {
                                if (bpp.StemEnd.Y > extremeval)
                                    extremeval = bpp.StemEnd.Y;
                            }
                        }
                        if (bpl[0].stemdirection == 1)
                        {
                            extremeval = int.MaxValue;
                            foreach (GraphicBeamedProperties bpp in bpl)
                            {
                                if (bpp.StemEnd.Y < extremeval)
                                    extremeval = bpp.StemEnd.Y;
                            }
                        }

                        

                        //Draw the beam straight between two endpoints
                        Line l = new Line();
                        l.X1 = bpl[0].StemEnd.X;
                        l.Y1 = extremeval;
                        l.X2 = bpl[bpl.Count - 1].StemEnd.X;
                        l.Y2 = extremeval;

                        l.Stroke = Brushes.Black;
                        l.StrokeThickness = 6;
                        TopCanvas.Children.Add(l);

                        for (int it = 0; it < bpl.Count; it++)
                        {
                            ////extend the stems for short notes in ascending notes
                            Line le = new Line();
                            le.X1 = bpl[it].StemEnd.X;
                            le.Y1 = bpl[it].StemEnd.Y;
                            le.X2 = bpl[it].StemEnd.X;
                            le.Y2 = extremeval;

                            le.Stroke = Brushes.Black;
                            le.StrokeThickness = 1;
                            TopCanvas.Children.Add(le);
                        }
                        tmpslope = 0;


                        //separate two or more beams in a measure
                        foreach (GraphicBeamedProperties nbp in bpl)
                        {
                            if (nbp.beamvalue.Count > 1)

                                if (nbp.beamvalue[1] == "forward hook")
                                {
                                    //Draw the beam with tmpslope
                                    int hooklength = 20;

                                    Line lfh = new Line();
                                    //current note stem x position
                                    lfh.X1 = nbp.StemEnd.X;
                                    //y postion of the line at current postion -/+ 10 up down
                                    if (tmpslope == 0)
                                        lfh.Y1 = tmpslope * (nbp.StemEnd.X - bpl[0].StemEnd.X) + extremeval + 10 * nbp.stemdirection;                                    
                                    else
                                        lfh.Y1 = tmpslope * (nbp.StemEnd.X - bpl[0].StemEnd.X) + bpl[0].StemEnd.Y + 10 * nbp.stemdirection;


                                    lfh.X2 = nbp.StemEnd.X + hooklength;

                                    lfh.Y2 = lfh.Y1 + tmpslope * hooklength;

                                    lfh.Stroke = Brushes.Black;
                                    lfh.StrokeThickness = 6;
                                    TopCanvas.Children.Add(lfh);
                                }
                                else if (nbp.beamvalue[1] == "backward hook")
                                {
                                    //Draw the beam with tmpslope
                                    int hooklength = 20;

                                    Line lfh = new Line();
                                    //current note stem x position
                                    lfh.X1 = nbp.StemEnd.X;
                                    //y postion of the line at current postion -/+ 10 up down
                                    if (tmpslope == 0)
                                        lfh.Y1 = tmpslope * (nbp.StemEnd.X - bpl[0].StemEnd.X) + extremeval + 10 * nbp.stemdirection;                                    
                                    else
                                        lfh.Y1 = tmpslope * (nbp.StemEnd.X - bpl[0].StemEnd.X) + bpl[0].StemEnd.Y + 10 * nbp.stemdirection;


                                    lfh.X2 = nbp.StemEnd.X - hooklength;

                                    lfh.Y2 = lfh.Y1 - tmpslope * hooklength;

                                    lfh.Stroke = Brushes.Black;
                                    lfh.StrokeThickness = 6;
                                    TopCanvas.Children.Add(lfh);
                                }
                        }
                    }
                }

                #endregion

            }

            #region Slur/Tie Processing
            for (int loop = 0; loop < SlurTieProp.Count; loop = SlurTieProp[loop+1].startstart?loop+1:loop+2)
            {
                if (SlurTieProp[loop + 1].startstart)
                    SlurTieProp[loop].point.X -= 5;
                if (SlurTieProp[loop].startstart)
                    SlurTieProp[loop].point.X += 5;
                curve(SlurTieProp[loop].point, SlurTieProp[loop + 1].point, SlurTieProp[loop].updown * 15, 0, SlurTieProp[loop].measure, SlurTieProp[loop].note, SlurTieProp[loop + 1].measure, SlurTieProp[loop + 1].note);                    
                         
            }
            #endregion


        }
        
        void FormattingPass()
        {
            //do the spacing stuff here
            int offset = 0;
            double width = 0;
            for (int i = 0; i < NotesWrappanel.Children.Count; i++)
            {
                if (width + ((StackPanel)NotesWrappanel.Children[i]).ActualWidth <= NotesWrappanel.ActualWidth)
                {
                    width += ((StackPanel)NotesWrappanel.Children[i]).ActualWidth;
                }
                else
                {
                    int spaces = 0;
                    //remove the bar from the last measure
                    StackPanel sp= (StackPanel)NotesWrappanel.Children[i-1];
                    sp.Children.RemoveAt(sp.Children.Count-1);



                    for (int j = offset; j < i; j++)
                    {
                        var canvas = ((StackPanel)NotesWrappanel.Children[j]).Children.Cast<MyCanvas>().Where(o => o.MeasureNum == j & o.NoteNum == 0 & o.Modifier == -10).ToList();
                        spaces += canvas.Count();
                    }
                    //deteremine free space at the end of the staff
                    width = NotesWrappanel.ActualWidth - width;
                    double ExpandSpaceBy = (width / spaces);
                    width = 0;
                    for (int j = offset; j < i; j++)
                    {  
                        var canvas = ((StackPanel)NotesWrappanel.Children[j]).Children.Cast<MyCanvas>().Where(o => o.MeasureNum == j & o.NoteNum == 0 & o.Modifier == -10);
                        foreach (MyCanvas oldspace in canvas)
                        {
                            oldspace.Width += ExpandSpaceBy;
                        }
                    }                    
                    //Add Cleff, key and space
                    int[] uplusc = GUIPara.PixelCalc(CurrentClef, "", "", "");
                    uplusc[1] = uplusc[1] * 60 / 1024;
                    AddNoteToDisplay(0, i, -1, 0, 0, uplusc[1], uplusc[0], 0, (StackPanel)NotesWrappanel.Children[i], index: 0);
                    if (CurrentFifths != "")
                    {
                        AddKeySignature(int.Parse(CurrentFifths), CurrentClef, (StackPanel)NotesWrappanel.Children[i], index: 1);
                        AddSpaceToDisplay(i, 15, (StackPanel)NotesWrappanel.Children[i], index: 2);
                    }
                    else
                        AddSpaceToDisplay(i, 15, (StackPanel)NotesWrappanel.Children[i], index: 1);
                    
                    ((StackPanel)NotesWrappanel.Children[i]).UpdateLayout();
                    offset = i;
                    i--;

                }
            }
            NotesWrappanel.UpdateLayout();
        }
                
        private void curve(Point startpt, Point endpt, int updown, int RadiusRatio, int startmeasure, int startnote, int stopmeasure, int stopnote)
        {
            PathGeometry pathGeometry = new PathGeometry();
            PathFigure figure = new PathFigure();
            figure.StartPoint = startpt;

            QuadraticBezierSegment qbs = new QuadraticBezierSegment(new Point((startpt.X + (endpt.X - startpt.X) / 2), ((endpt.Y + startpt.Y) / 2) + updown), endpt, true); //the control point in the middle
            figure.Segments.Add(qbs);
            pathGeometry.Figures.Add(figure);

            System.Windows.Shapes.Path path = new System.Windows.Shapes.Path();
            path.Data = pathGeometry;
            path.StrokeThickness = 2.5;
            path.Stroke = Brushes.Black;
            path.MouseLeftButtonDown += new MouseButtonEventHandler(TieSlur_MouseLeftButtonDown);

            PathAttachedPropertties.SetStartMeasure(path, startmeasure);
            PathAttachedPropertties.SetStartNote(path, startnote);
            PathAttachedPropertties.SetStopMeasure(path, stopmeasure);
            PathAttachedPropertties.SetStopNote(path, stopnote);           
           

            TopCanvas.Children.Add(path);


        }   
                        
        int checkorder(List<GraphicBeamedProperties> bpl)
        {
            
            //check if descending
            int ascdesc = 0;
            int prevValue = int.MaxValue;
            foreach (GraphicBeamedProperties bp in bpl)
            {
                if (bp.StemEnd.Y >= prevValue)
                {
                    ascdesc = -1;
                    break;
                }
                prevValue = (int)bp.StemEnd.Y;
            }
            if (ascdesc == 0) return -1;


            //check if ascending
            ascdesc = 0;
            prevValue = int.MinValue;
            foreach (GraphicBeamedProperties bp in bpl)
            {
                if (bp.StemEnd.Y <= prevValue)
                {
                    ascdesc = -1;
                    break;
                }
                prevValue = (int)bp.StemEnd.Y;
            }
            if (ascdesc == 0) return 1;
            else return 0;
            
        }
        
        /*****************************Event Handlers Onwards**************************************/
        /*****************************************************************************************/

        void txtblock_MouseLeave(object sender, MouseEventArgs e)
        {
            ((TextBlock)sender).Foreground = Brushes.Black;
        }

        void txtblock_MouseEnter(object sender, MouseEventArgs e)
        {
            ((TextBlock)sender).Foreground = Brushes.Blue;
        }

        void txtblock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //set the note to be the current note
            CurrentNoteCanvas=((MyCanvas)((TextBlock)sender).Parent);
            
            //Do the appropriate actions for tie slurs and also for dot and sharps
            //please update
            if (action == Action.Delete)
            {
                //For display
                //Get the index of the current canvas in the NotesWrappanel
                int index = NotesWrappanel.Children.IndexOf(CurrentNoteCanvas);

                //For MusicXML
                //Get the Measure and note number of the note inside the current canvas
                int noteIndex = ReadDS.getnoteindex(CurrentNoteCanvas.NoteNum,
                    (ScorePartwise.Part.Measure)((ScorePartwise.Part)scorePartwise.getPart().get(0)).getMeasure().get(CurrentNoteCanvas.MeasureNum));

                //Remove the note in the measure at the specified note index
                ((ScorePartwise.Part.Measure)((ScorePartwise.Part)scorePartwise.getPart().get(0)).getMeasure().get(CurrentNoteCanvas.MeasureNum)).
                    getNoteOrBackupOrForward().remove(noteIndex);


                //clear the staff and the notes if there are any 
               // StaffStackPanel.Children.Clear();
                NotesWrappanel.Children.Clear();
                TopCanvas.Children.Clear();

                if (Check.CheckAndReAlign(scorePartwise) != null)
                    scorePartwise = Check.CheckAndReAlign(scorePartwise);
                else
                {
                    System.Windows.MessageBox.Show("Not a Valid Music File");
                }


                DisplayScore(scorePartwise);

              
            }

        }


        //Event Handlers for mouse click and move on a Mycanvas note

        void Note_MouseEnter(object sender, MouseEventArgs e)
        {

            ((MyCanvas)sender).Background = Brushes.LightPink;
            
        }
        
        void Note_MouseLeave(object sender, MouseEventArgs e)
        {
            ((MyCanvas)sender).Background = null;
        }

        void Note_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
           System.Windows.MessageBox.Show("I am Measure Number  " + ((MyCanvas)sender).MeasureNum.ToString() + "  Note Number " + ((MyCanvas)sender).ElementNum.ToString());
        }

       /*************************************************************************/
                //Event Handlers for all the staff line handlers
        /***********************************************************************/
        
        void StaffLines_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (scorePartwise == null)
                return;
            //mainWindow.Title = (((StaffLineCanvas)sender).LineNumber - 1).ToString();
            
            //get item1 as pitch octave and item2 as pitch step
            //MessageBox.Show("Pitch Octave: " + GUIPara.GetPitchOctaveAndStep(((StaffLineCanvas)sender).LineNumber-1, CurrentClef).Item1.ToString()
            //    +" Pitch Step: "+ GUIPara.GetPitchOctaveAndStep(((StaffLineCanvas)sender).LineNumber-1, CurrentClef).Item2 + "  Duration: " + SelectedSymbol);

            //This code checks if a symbol is selected and generates a note based on the information received
            #region Generates a Note based on the information received

            if (SelectedSymbol == "")
            {
                MessageBox.Show("Please Select a Note");
                return;
            }
            NoteProperties noteprop = new NoteProperties();

            if (SelectedSymbol.ToCharArray()[0] == '_')
                noteprop.NoteType = SelectedSymbol.Remove(0, 1);
            else if (SelectedSymbol.ToCharArray()[0] == 'R')
            {
                noteprop.NoteType = SelectedSymbol.Remove(0, 5);
                noteprop.rest = true;
            }

            switch (noteprop.NoteType)
            {
                case "whole": noteprop.duration = 4 * CurrentDivisions; break;
                case "half": noteprop.duration = 2 * CurrentDivisions; break;
                case "quarter": noteprop.duration = CurrentDivisions; break;
                case "eighth": noteprop.duration = CurrentDivisions / 2; break;
                case "16th": noteprop.duration = CurrentDivisions / 4; break;
                case "32nd": noteprop.duration = CurrentDivisions / 8; break;
            }

            noteprop.Pitchoctave = GUIPara.GetPitchOctaveAndStep(((StaffLineCanvas)sender).LineNumber - 1, CurrentClef).Item1;
            noteprop.PitchStep = GUIPara.GetPitchOctaveAndStep(((StaffLineCanvas)sender).LineNumber - 1, CurrentClef).Item2;

            //mainWindow.Title += noteprop.PitchStep;
            #endregion

            #region Insert Action

            if (action == Action.Insert & CurrentNoteCanvas!=null)
            {
                
                //For display
                //Get the index of the current canvas in the NotesWrappanel
                int index = NotesWrappanel.Children.IndexOf(CurrentNoteCanvas);

                //For MusicXML
                //Get the Measure and note number of the note inside the current canvas
                int noteIndex = ReadDS.getnoteindex(CurrentNoteCanvas.NoteNum, 
                    (ScorePartwise.Part.Measure)((ScorePartwise.Part)scorePartwise.getPart().get(0)).getMeasure().get( CurrentNoteCanvas.MeasureNum));
                
                // Insert the note generated early in the measure at the specified note index +1
                ((ScorePartwise.Part.Measure)((ScorePartwise.Part)scorePartwise.getPart().get(0)).getMeasure().get(CurrentNoteCanvas.MeasureNum)).
                    getNoteOrBackupOrForward().add(noteIndex+1, CreateDS.CreateNote(noteprop));


                //clear the staff and the notes if there are any 
               // StaffStackPanel.Children.Clear();
                NotesWrappanel.Children.Clear();
                TopCanvas.Children.Clear();

                ScorePartwise tmpscore = Check.CheckAndReAlign(scorePartwise);
                if (tmpscore != null)
                    scorePartwise = tmpscore;
                else
                {
                    System.Windows.MessageBox.Show("Not a Valid Music File");
                }
                tmpscore = null;
                
                DisplayScore(scorePartwise);

                CurrentNoteCanvas = null;             
               
            }


            #endregion

            #region Add Action
            if (action == Action.Add)
            {
                bool fail=false;
                //For display
                //Get the index of the current canvas in the NotesWrappanel
                int spindex = NotesWrappanel.Children.Count;
                int mcindex=((StackPanel)NotesWrappanel.Children[spindex - 1]).Children.Count;
                //if it is not the start of the score

                //Get a canvas which corresponds to a note
                CurrentNoteCanvas = (MyCanvas)((StackPanel)NotesWrappanel.Children[spindex - 1]).Children[mcindex-1];
                while (CurrentNoteCanvas.NoteNum == 0 & CurrentNoteCanvas.Modifier != 0)
                {
                    mcindex--;
                    if (mcindex < 0)
                    {
                        fail = true;
                        break;

                    }
                    CurrentNoteCanvas = (MyCanvas)((StackPanel)NotesWrappanel.Children[spindex - 1]).Children[mcindex];
                }
                
                if (fail)  //put the note in the first measure which surely exists
                {
                   
                    // Insert the note generated early in the measure at the specified note index +1
                    ((ScorePartwise.Part.Measure)((ScorePartwise.Part)scorePartwise.getPart().get(0)).getMeasure().get(0)).
                        getNoteOrBackupOrForward().add(CreateDS.CreateNote(noteprop));
                }
                else
                {
                    ////For MusicXML
                    ////Get the Measure and note number of the note inside the current canvas
                    //int noteIndex = ReadDS.getnoteindex(CurrentNoteCanvas.NoteNum,
                    //    (ScorePartwise.Part.Measure)((ScorePartwise.Part)scorePartwise.getPart().get(0)).getMeasure().get(CurrentNoteCanvas.MeasureNum));

                    // Insert the note generated early in the measure at the specified note index +1
                    ((ScorePartwise.Part.Measure)((ScorePartwise.Part)scorePartwise.getPart().get(0)).getMeasure().get(CurrentNoteCanvas.MeasureNum)).
                        getNoteOrBackupOrForward().add(CreateDS.CreateNote(noteprop));
                }
               

                //clear the staff and the notes if there are any 
               // StaffStackPanel.Children.Clear();
                NotesWrappanel.Children.Clear();
                TopCanvas.Children.Clear();

                if (Check.CheckAndReAlign(scorePartwise) != null)
                    scorePartwise = Check.CheckAndReAlign(scorePartwise);
                else
                {
                    System.Windows.MessageBox.Show("Not a Valid Music File");
                }

                DisplayScore(scorePartwise);


            }

            #endregion


        }
        
        /***********************Here is the score Creation Part******************************************/

        private void New_ButtonDialog_Click(object sender, RoutedEventArgs e)
        {
            string[] items={"G", "F", "C"};
            ClefCombo.ItemsSource = items;

            newgrid.Height = 496;
            ScoreScrollViewer.Visibility = Visibility.Hidden;
            newgrid.Visibility = Visibility.Visible;
            //NewOpenSave.IsEnabled = false;
            Ribbon.IsEnabled = false;
        }

        private void CreateBtn_Click(object sender, RoutedEventArgs e)
        {
            
            //Get the scoreProperties here

            ScoreProperties Newscoreprop = new ScoreProperties();
            Newscoreprop.MovementTitle = TitleTxtBox.Text;
            Newscoreprop.InstrumentName = InstrumentTxtBox.Text;
            Newscoreprop.EncodingSoftware = "UoU ScoreEditor";
            Newscoreprop.MidiChannel = new java.lang.Integer(1);
            Newscoreprop.MidiProgram = new java.lang.Integer(41);
            //Newscoreprop.Creator = "Here is the creator name";
            //Newscoreprop.CreditWords = "these are the credit words";
            Newscoreprop.ScorePartID = "P1";
            Newscoreprop.InstrumentID = "P1-I1";

            //create a score with the given properties
            scorePartwise = CreateDS.CreateScorePartwise(Newscoreprop);

            //create the appropriate attributes element for the first measure
            AttributeProperties Nattrubuteprop = new AttributeProperties();

            Nattrubuteprop.BeatType = BeatType.Value.ToString();
            Nattrubuteprop.BeatsPerMeasure = TimesBeat.Value.ToString();

            StaffSize = (int)StaffLength.Value;

            if (keysiglistbox.SelectedIndex < 8)
                Nattrubuteprop.fifths = (-keysiglistbox.SelectedIndex).ToString();
            else
                Nattrubuteprop.fifths = (keysiglistbox.SelectedIndex-7).ToString();

            // latter these values to be selected by user
            Nattrubuteprop.mode = "major";
            
            Nattrubuteprop.divisions = 8;
           
            //Newscoreprop.PartDisplayName = "Violin";
            //Newscoreprop.PartAbbreviation = "vln.";


            if ((string)ClefCombo.SelectedItem == "G")
            {
                Nattrubuteprop.ClefLine = "2";
                Nattrubuteprop.ClefSign = "G";
            }
            else if ((string)ClefCombo.SelectedItem == "F")
            {
                Nattrubuteprop.ClefLine = "4";
                Nattrubuteprop.ClefSign = "F";
            }
            else if ((string)ClefCombo.SelectedItem == "C")
            {
                Nattrubuteprop.ClefLine = "3";
                Nattrubuteprop.ClefSign = "C";
            }
            
            //set the clef
            CurrentClef = Nattrubuteprop.ClefSign;
            
            ScorePartwise.Part.Measure measure = CreateDS.CreateMeasure("1");
            
            measure.getNoteOrBackupOrForward().add(CreateDS.CreateAttribute(Nattrubuteprop));
            ((ScorePartwise.Part)scorePartwise.getPart().get(0)).getMeasure().add(measure);


            //finally display the score

           // StaffStackPanel.Children.Clear();
            NotesWrappanel.Children.Clear();
            TopCanvas.Children.Clear();
            DisplayScore(scorePartwise);



            newgrid.Height = 0;
            ScoreScrollViewer.Visibility = Visibility.Visible;
            newgrid.Visibility = Visibility.Hidden;
            Ribbon.IsEnabled = true;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
           
            newgrid.Height = 0;
            ScoreScrollViewer.Visibility = Visibility.Visible;
            newgrid.Visibility = Visibility.Hidden;
            Ribbon.IsEnabled = true;
        }
         
        //Menu bar event handlers... these handlers will be deleted and the code will
        //transfered to toolbar and menubar event handlers

        private void Open_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OpenXmlFileDialog = new OpenFileDialog();
            OpenXmlFileDialog.FileName = "Music"; // Default file name
            OpenXmlFileDialog.DefaultExt = ".xml"; // Default file extension
            OpenXmlFileDialog.Filter = "MusicXML Files (.xml)|*.xml"; // Filter files by extension 

            // Show open file dialog box
            Nullable<bool> result = OpenXmlFileDialog.ShowDialog();

            if (result == true)
            {
                java.io.File xmlFile = new java.io.File(OpenXmlFileDialog.FileName);
                java.io.InputStream inputstream = new java.io.FileInputStream(xmlFile);
                scorePartwise = Marshalling.unmarshal(inputstream);
                //clear the staff and the notes if there are any 
               // StaffStackPanel.Children.Clear();
                NotesWrappanel.Children.Clear();
                TopCanvas.Children.Clear();

                //check for time signature and realign
                if (Check.CheckAndReAlign(scorePartwise) != null)
                    scorePartwise = Check.CheckAndReAlign(scorePartwise);
                else
                {
                    System.Windows.MessageBox.Show("Not a Valid Music File");
                }


                DisplayScore(scorePartwise);
            }
            else
                System.Windows.MessageBox.Show("Please Choose a Standard MusicXML File");





        }

        private void zoomIn_Click(object sender, RoutedEventArgs e)
        {
            
            MainViewbox.Width *= 1.2;
            MainViewbox.Height *= 1.2;

        }

        private void zoomOut_Click(object sender, RoutedEventArgs e)
        {
            MainViewbox.Width /= 1.2;
            MainViewbox.Height /= 1.2;
           
        }

        private void zoomAll_Click(object sender, RoutedEventArgs e)
        {
            //MainViewbox.Stretch = Stretch.Uniform;
            MainViewbox.Width = StaffStackPanel.Width;
            MainViewbox.Height = StaffStackPanel.Height;

        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog SaveXmlFileDialog = new SaveFileDialog();
            SaveXmlFileDialog.FileName = "Music";
            SaveXmlFileDialog.DefaultExt = ".xml";
            SaveXmlFileDialog.Filter = "MusicXML File (.xml)|*.xml";

            // Show open file dialog box
            Nullable<bool> result = SaveXmlFileDialog.ShowDialog();

            if (result == true)
            {
                java.io.File xmlFile = new java.io.File(SaveXmlFileDialog.FileName);
                java.io.OutputStream outstream = new java.io.FileOutputStream(xmlFile);
                Marshalling.marshal(scorePartwise, outstream);
                
            }
            else
                System.Windows.MessageBox.Show("Please Choose a Standard MusicXML File");




        }

        private void Notes_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var i in NotesGroup.Items)
            {
                if (i is RibbonToggleButton)
                    if ((RibbonToggleButton)i != (RibbonToggleButton)sender)
                    {
                        ((RibbonToggleButton)i).IsChecked = false;
                    }
            }
            SelectedSymbol= ((RibbonToggleButton)sender).Name;
        }

        private void BeatType_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
                if (e.OldValue != null)
                {

                    int dif = ((int)e.NewValue - (int)e.OldValue);

                    int p = (int)Math.Log((int)e.OldValue, 2);
                    int val=0;
                    if (dif > 0)
                        val = (int)Math.Pow(2, p + 1);
                    else if (dif < 0)
                        val = (int)Math.Pow(2, p - 1);
                    else if (dif == 0)
                        val = (int)e.OldValue;
                    ((Xceed.Wpf.Toolkit.IntegerUpDown)sender).ValueChanged -= BeatType_ValueChanged;
                    ((Xceed.Wpf.Toolkit.IntegerUpDown)sender).Value = val;
                    ((Xceed.Wpf.Toolkit.IntegerUpDown)sender).ValueChanged += BeatType_ValueChanged;
                    

                }

            
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Score Editor Version 0.00001 by SSP Lab UoU");
        }

        private void InsertReplaceDelete_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var i in InsertReplaceDeleteGroup.Items)
            {
                if (i is RibbonToggleButton)
                    if ((RibbonToggleButton)i != (RibbonToggleButton)sender)
                    {
                        ((RibbonToggleButton)i).IsChecked = false;
                    }
            }
            
            action = (Action)Enum.Parse(typeof(Action), ((RibbonToggleButton)sender).Name);

        }

        void TieSlur_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            MessageBox.Show("Start " + PathAttachedPropertties.GetStartMeasure(((System.Windows.Shapes.Path)sender)).ToString()
                + " , " + PathAttachedPropertties.GetStartNote(((System.Windows.Shapes.Path)sender)).ToString()
                + " Stop " + PathAttachedPropertties.GetStopMeasure(((System.Windows.Shapes.Path)sender)).ToString()
                 + " , " + PathAttachedPropertties.GetStopNote(((System.Windows.Shapes.Path)sender)).ToString()
                );
        }
            
        void PlayNotes()
        {
            if (scorePartwise == null)
                return;
            //Read the first part in the score
            ScorePartwise.Part part = (ScorePartwise.Part)scorePartwise.getPart().get(0);

            int program=0;
            MIDIStep step;
            double DivisionsPerMillisecond =(4*(double)CurrentDivisions/(double)CurrentBeatType)*(MIDITempo/60.0);

            scoreprop = ReadDS.ReadScore(scorePartwise);
            switch (scoreprop.InstrumentName)
            {
                case "Violin":
                    {
                        program = 42;
                        break;
                    }
                case "Piano":
                    {
                        program = 0;
                        break;
                    }
                default:
                    {
                        program = 0;
                        break;
                    }

            }

            //open midi out
            Playback.OpenProgram(program);
            
            //for each measure in the score
            for (int i = 0; i < part.getMeasure().size(); i++)
            {
                ////////////////////////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////// Ith MEASURE ///////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////////////////////
                if (!MIDIContinue)
                    break;

                //for each note in the measure
                int j = 1;          
                              
                NoteProperties noteprop;
              
                while (ReadDS.ReadNote(j, (ScorePartwise.Part.Measure)part.getMeasure().get(i)) != null )
                {
                    
                    ////////////////////////////////////////////////////////////////////////////////
                    //////////////////////////////Jth Note///////////////////////////////////////////
                    ////////////////////////////////////////////////////////////////////////////////
                    noteprop = ReadDS.ReadNote(j, (ScorePartwise.Part.Measure)part.getMeasure().get(i));

                    int pitch=0;
                    if (noteprop.PitchStep != "")
                    {

                        step = (MIDIStep)Enum.Parse(typeof(MIDIStep), noteprop.PitchStep);

                        //calculate the pitch 
                        // C4 is 48 in MIDI, an octave higher is 12 higher, alteration is added or subtracted
                        
                        if (noteprop.PitchAlter == float.MaxValue)
                            pitch = (int)step + (noteprop.Pitchoctave - 4) * 12;
                        else
                            pitch = (int)step + (noteprop.Pitchoctave - 4) * 12 - (int)noteprop.PitchAlter;
                       
                    }

                    
                    //play the note                    
                    Playback.NoteOn(pitch, MIDIVol);
                    

                    UpdateColor(Brushes.Blue, i, j);

                    //The duration is given by note duration in divisions * miliseconds per division                  
                    Thread.Sleep((int)(noteprop.duration * 1000 / DivisionsPerMillisecond));
                    
                    
                    //((TextBlock)(canvas.Children[0])).Foreground = Brushes.Black;
                    Playback.NoteOff(pitch, MIDIVol);

                    j++;                                                 
                } 
            }
            Playback.CloseProgram();
            
        }

        private void play_Click(object sender, RoutedEventArgs e)
        {
            MIDIContinue = true;
            if (playthread.ThreadState == ThreadState.Unstarted)
                playthread.Start();
            else
            {
                playthread = new Thread(new ThreadStart(PlayNotes));
                playthread.Start();
            }
            
        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {           
            MIDIContinue = false;
            Playback.CloseProgram();           
           
        }



        private void UpdateColor(Brush color, int i, int j)
        {
            //this doesn't work because of the thread ownership                    
            //var canvas = ((StackPanel)NotesWrappanel.Children[i]).Children.Cast<MyCanvas>().FirstOrDefault(o => o.NoteNum == j & o.Modifier == 0);
            //((TextBlock)(canvas.Children[0])).Foreground = color;       

        }
        public delegate void UpdateTextCallback(Brush color, int i, int j);


    }

    public class GraphicBeamedProperties
    {
        public int measure;
        public int note;
        public StackPanel meaeuresp;
        public Point StemEnd;
        public int stemdirection;
        public List<int> beamnumber = new List<int>();
        public List<string> beamvalue = new List<string>();
       
    }

    public class GraphicSlurTieProperties
    {
        public Point point;
        public string type;
        public int startstop;
        public int updown;
        public int RadiusRatio;
        public int measure;
        public int note;
        public bool startstart;
    }

}
