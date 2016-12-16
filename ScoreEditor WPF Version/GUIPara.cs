using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Globalization;
using System.Text.RegularExpressions;
namespace ScoreEditor_WPF_Version
{
    

    public static class GUIPara
    {
       
        

        // top to down staff lines
        //calculated in pixels using this formula 14.9 + 6.15 * i where i=0 for highest pitch and 20 for lowest pitch
        //where pixels can be converted to font units by fu=1024* pixels/FontSize where 1024/60 = 17.07
        //In the code below we use the font units and not the pixels so we will multiply this constant with the 
        // StaffLines....
        //However note that the StaffLines values are dependent on the fontsize and are calculated in pixels using
        // a fixed fontsize of 60 giving the constants 14.9 and 6.15
        public static double[] StaffLines = { 14.9, 21.05, 27.2, 33.35, 39.5, 45.65, 51.8, 57.95, 64.1, 70.25, 76.4, 82.55, 88.7, 94.85, 101, 107.15, 113.3, 119.45, 125.6, 131.75, 137.9 };
        
        //DICTIONARIES TO STORE THE STAFF LINES Y CORDINATES FOR G F AND C CLEFS
        static Dictionary<Tuple<int, string>, double> StaffLinesG = new Dictionary<Tuple<int, string>, double>();
        static Dictionary<Tuple<int, string>, double> StaffLinesF = new Dictionary<Tuple<int, string>, double>();
        static Dictionary<Tuple<int, string>, double> StaffLinesC = new Dictionary<Tuple<int, string>, double>();

        //DICTIONARIES TO MAP STAFFLINES TO PITCH OCTAVE AND PITCHSTEP BASED ON CLEF
        static Dictionary<int, Tuple<int, string>> StaffLinesGR = new Dictionary<int, Tuple<int, string>>();
        static Dictionary<int, Tuple<int, string>> StaffLinesFR = new Dictionary<int, Tuple<int, string>>();
        static Dictionary<int, Tuple<int, string>> StaffLinesCR = new Dictionary<int, Tuple<int, string>>();
        
        //key signature
        public static Tuple<string, int>[] FKeySig = { new Tuple<string, int>("B", 2), new Tuple<string, int>("E", 3), 
                                       new Tuple<string, int>("A", 2), new Tuple<string, int>("D", 3), 
                                       new Tuple<string, int>("G", 2), new Tuple<string, int>("C", 3), 
                                       new Tuple<string, int>("F", 2),
                                       new Tuple<string, int>("F", 3), new Tuple<string, int>("C", 3), 
                                       new Tuple<string, int>("G", 3), new Tuple<string, int>("D", 3), 
                                       new Tuple<string, int>("A", 2), new Tuple<string, int>("E", 3), 
                                       new Tuple<string, int>("B", 2)
                                       };
        //public static Tuple<string, int>[] GKeySig = { new Tuple<string, int>("B", 4), new Tuple<string, int>("E", 5), 
        //                               new Tuple<string, int>("A", 4), new Tuple<string, int>("D", 5), 
        //                               new Tuple<string, int>("G", 4), new Tuple<string, int>("C", 5), 
        //                               new Tuple<string, int>("F", 4),
        //                               new Tuple<string, int>("F", 5), new Tuple<string, int>("C", 5), 
        //                               new Tuple<string, int>("G", 5), new Tuple<string, int>("D", 5), 
        //                               new Tuple<string, int>("A", 4), new Tuple<string, int>("E", 5), 
        //                               new Tuple<string, int>("B", 4)
        //                               };
       
        //public static Tuple<string, int>[] CKeySig = { new Tuple<string, int>("B", 3), new Tuple<string, int>("E", 4), 
        //                               new Tuple<string, int>("A", 3), new Tuple<string, int>("D", 4), 
        //                               new Tuple<string, int>("G", 3), new Tuple<string, int>("C", 4), 
        //                               new Tuple<string, int>("F", 3),
        //                               new Tuple<string, int>("F", 4), new Tuple<string, int>("C", 4), 
        //                               new Tuple<string, int>("G", 4), new Tuple<string, int>("D", 4), 
        //                               new Tuple<string, int>("A", 3), new Tuple<string, int>("E", 4), 
        //                               new Tuple<string, int>("B", 3)
        //                               };



        //YPixelCorrection is the correction of font cordinates to display the desired part at the input location

        static Dictionary<int, int> YPixelCorrection = new Dictionary<int, int>(); // corrections for each symbol


        //enum for storing the unicode mapping for each of the glyph to be displayed
        enum UnicodesUpStems
        {
            _whole = 172, _half = 176, _quarter = 177, _eighth = 196, _16th = 197, _32nd = 198, _dot = 46,
            Rest_whole = 229, Rest_half = 228, Rest_quarter = 163, Rest_eighth = 224, Rest_16th = 225, Rest_32nd = 226, bar = 124,
            flat = 64, sharp = 35, natural = 36, doublesharp = 42, G = 165, F = 168, C=170, Space=32, 
            // the following symbols are not available in the Bach font
            repeatleft = 123, repeatright = 125, flatflat = 208
        };

        enum UnicodesDownStems { _whole = 172, _half = 129, _quarter = 130, _eighth = 131, _16th = 132, _32nd = 133, _dot = 46};


        public static void Initialize()
        {

            // FOR G CLEFF on line 2 SET THE CORDINATES FOR STAFF AND LEDGER LINES

            //6
            StaffLinesG.Add(new Tuple<int, string>(6, "E"), 17.07*StaffLines[0]);
            StaffLinesG.Add(new Tuple<int, string>(6, "D"), 17.07*StaffLines[1]);
            StaffLinesG.Add(new Tuple<int, string>(6, "C"), 17.07*StaffLines[2]);

            //5
            StaffLinesG.Add(new Tuple<int, string>(5, "B"), 17.07*StaffLines[3]);
            StaffLinesG.Add(new Tuple<int, string>(5, "A"), 17.07*StaffLines[4]);
            StaffLinesG.Add(new Tuple<int, string>(5, "G"), 17.07*StaffLines[5]);
            StaffLinesG.Add(new Tuple<int, string>(5, "F"), 17.07*StaffLines[6]);
            StaffLinesG.Add(new Tuple<int, string>(5, "E"), 17.07*StaffLines[7]);
            StaffLinesG.Add(new Tuple<int, string>(5, "D"), 17.07*StaffLines[8]);
            StaffLinesG.Add(new Tuple<int, string>(5, "C"), 17.07*StaffLines[9]);

            //4
            StaffLinesG.Add(new Tuple<int, string>(4, "B"), 17.07*StaffLines[10]);
            StaffLinesG.Add(new Tuple<int, string>(4, "A"), 17.07*StaffLines[11]);
            StaffLinesG.Add(new Tuple<int, string>(4, "G"), 17.07*StaffLines[12]);
            StaffLinesG.Add(new Tuple<int, string>(4, "F"), 17.07*StaffLines[13]);
            StaffLinesG.Add(new Tuple<int, string>(4, "E"), 17.07*StaffLines[14]);
            StaffLinesG.Add(new Tuple<int, string>(4, "D"), 17.07*StaffLines[15]);
            StaffLinesG.Add(new Tuple<int, string>(4, "C"), 17.07*StaffLines[16]);

            //for octave 3
            StaffLinesG.Add(new Tuple<int, string>(3, "B"), 17.07*StaffLines[17]);
            StaffLinesG.Add(new Tuple<int, string>(3, "A"), 17.07*StaffLines[18]);
            StaffLinesG.Add(new Tuple<int, string>(3, "G"), 17.07*StaffLines[19]);
            StaffLinesG.Add(new Tuple<int, string>(3, "F"), 17.07*StaffLines[20]);
            //G clef

            // FOR F CLEFF on line 4 SET THE CORDINATES FOR STAFF AND LEDGER LINES
            //4
            StaffLinesF.Add(new Tuple<int, string>(4, "G"), 17.07*StaffLines[0]);
            StaffLinesF.Add(new Tuple<int, string>(4, "F"), 17.07*StaffLines[1]);
            StaffLinesF.Add(new Tuple<int, string>(4, "E"), 17.07*StaffLines[2]);
            StaffLinesF.Add(new Tuple<int, string>(4, "D"), 17.07*StaffLines[3]);
            StaffLinesF.Add(new Tuple<int, string>(4, "C"), 17.07*StaffLines[4]);

            //3
            StaffLinesF.Add(new Tuple<int, string>(3, "B"), 17.07*StaffLines[5]);
            StaffLinesF.Add(new Tuple<int, string>(3, "A"), 17.07*StaffLines[6]);
            StaffLinesF.Add(new Tuple<int, string>(3, "G"), 17.07*StaffLines[7]);
            StaffLinesF.Add(new Tuple<int, string>(3, "F"), 17.07*StaffLines[8]);
            StaffLinesF.Add(new Tuple<int, string>(3, "E"), 17.07*StaffLines[9]);
            StaffLinesF.Add(new Tuple<int, string>(3, "D"), 17.07*StaffLines[10]);
            StaffLinesF.Add(new Tuple<int, string>(3, "C"), 17.07*StaffLines[11]);

            //2
            StaffLinesF.Add(new Tuple<int, string>(2, "B"), 17.07*StaffLines[12]);
            StaffLinesF.Add(new Tuple<int, string>(2, "A"), 17.07*StaffLines[13]);
            StaffLinesF.Add(new Tuple<int, string>(2, "G"), 17.07*StaffLines[14]);
            StaffLinesF.Add(new Tuple<int, string>(2, "F"), 17.07*StaffLines[15]);
            StaffLinesF.Add(new Tuple<int, string>(2, "E"), 17.07*StaffLines[16]);
            StaffLinesF.Add(new Tuple<int, string>(2, "D"), 17.07*StaffLines[17]);
            StaffLinesF.Add(new Tuple<int, string>(2, "C"), 17.07*StaffLines[18]);

            //1
            StaffLinesF.Add(new Tuple<int, string>(1, "B"), 17.07*StaffLines[19]);
            StaffLinesF.Add(new Tuple<int, string>(1, "A"), 17.07*StaffLines[20]);
            //F Clef


            // FOR C CLEFF on line 3 SET THE CORDINATES FOR STAFF AND LEDGER LINES

            //4
            StaffLinesC.Add(new Tuple<int, string>(5, "F"), 17.07*StaffLines[0]);
            StaffLinesC.Add(new Tuple<int, string>(5, "E"), 17.07*StaffLines[1]);
            StaffLinesC.Add(new Tuple<int, string>(5, "D"), 17.07*StaffLines[2]);
            StaffLinesC.Add(new Tuple<int, string>(5, "C"), 17.07*StaffLines[3]);

            //3
            StaffLinesC.Add(new Tuple<int, string>(4, "B"), 17.07*StaffLines[4]);
            StaffLinesC.Add(new Tuple<int, string>(4, "A"), 17.07*StaffLines[5]);
            StaffLinesC.Add(new Tuple<int, string>(4, "G"), 17.07*StaffLines[6]);
            StaffLinesC.Add(new Tuple<int, string>(4, "F"), 17.07*StaffLines[7]);
            StaffLinesC.Add(new Tuple<int, string>(4, "E"), 17.07*StaffLines[8]);
            StaffLinesC.Add(new Tuple<int, string>(4, "D"), 17.07*StaffLines[9]);
            StaffLinesC.Add(new Tuple<int, string>(4, "C"), 17.07*StaffLines[10]);

            //2
            StaffLinesC.Add(new Tuple<int, string>(3, "B"), 17.07*StaffLines[11]);
            StaffLinesC.Add(new Tuple<int, string>(3, "A"), 17.07*StaffLines[12]);
            StaffLinesC.Add(new Tuple<int, string>(3, "G"), 17.07*StaffLines[13]);
            StaffLinesC.Add(new Tuple<int, string>(3, "F"), 17.07*StaffLines[14]);
            StaffLinesC.Add(new Tuple<int, string>(3, "E"), 17.07*StaffLines[15]);
            StaffLinesC.Add(new Tuple<int, string>(3, "D"), 17.07*StaffLines[16]);
            StaffLinesC.Add(new Tuple<int, string>(3, "C"), 17.07*StaffLines[17]);

            //1
            StaffLinesC.Add(new Tuple<int, string>(2, "B"), 17.07*StaffLines[18]);
            StaffLinesC.Add(new Tuple<int, string>(2, "A"), 17.07*StaffLines[19]);
            StaffLinesC.Add(new Tuple<int, string>(2, "G"), 17.07*StaffLines[20]);





            // FOR G CLEFF on line 2 SETMAP THE STAFFLINE TO PITCH STEP AND OCTAVE

            //6
            StaffLinesGR.Add(0, new Tuple<int, string>(6, "E"));
            StaffLinesGR.Add(1, new Tuple<int, string>(6, "D"));
            StaffLinesGR.Add(2, new Tuple<int, string>(6, "C"));

            //5
            StaffLinesGR.Add(3, new Tuple<int, string>(5, "B"));
            StaffLinesGR.Add(4, new Tuple<int, string>(5, "A"));
            StaffLinesGR.Add(5, new Tuple<int, string>(5, "G"));
            StaffLinesGR.Add(6, new Tuple<int, string>(5, "F"));
            StaffLinesGR.Add(7, new Tuple<int, string>(5, "E"));
            StaffLinesGR.Add(8, new Tuple<int, string>(5, "D"));
            StaffLinesGR.Add(9, new Tuple<int, string>(5, "C"));

            //4
            StaffLinesGR.Add(10, new Tuple<int, string>(4, "B"));
            StaffLinesGR.Add(11, new Tuple<int, string>(4, "A"));
            StaffLinesGR.Add(12, new Tuple<int, string>(4, "G"));
            StaffLinesGR.Add(13, new Tuple<int, string>(4, "F"));
            StaffLinesGR.Add(14, new Tuple<int, string>(4, "E"));
            StaffLinesGR.Add(15, new Tuple<int, string>(4, "D"));
            StaffLinesGR.Add(16, new Tuple<int, string>(4, "C"));

            //for octave 3
            StaffLinesGR.Add(17, new Tuple<int, string>(3, "B"));
            StaffLinesGR.Add(18, new Tuple<int, string>(3, "A"));
            StaffLinesGR.Add(19, new Tuple<int, string>(3, "G"));
            StaffLinesGR.Add(20, new Tuple<int, string>(3, "F"));
            //G clef

            // FOR F CLEFF on line 4 SETMAP THE STAFFLINE TO PITCH STEP AND OCTAVE
            //4
            StaffLinesFR.Add(0, new Tuple<int, string>(4, "G"));
            StaffLinesFR.Add(1, new Tuple<int, string>(4, "F"));
            StaffLinesFR.Add(2, new Tuple<int, string>(4, "E"));
            StaffLinesFR.Add(3, new Tuple<int, string>(4, "D"));
            StaffLinesFR.Add(4, new Tuple<int, string>(4, "C"));

            //3
            StaffLinesFR.Add(5, new Tuple<int, string>(3, "B"));
            StaffLinesFR.Add(6, new Tuple<int, string>(3, "A"));
            StaffLinesFR.Add(7, new Tuple<int, string>(3, "G"));
            StaffLinesFR.Add(8, new Tuple<int, string>(3, "F"));
            StaffLinesFR.Add(9, new Tuple<int, string>(3, "E"));
            StaffLinesFR.Add(10, new Tuple<int, string>(3, "D"));
            StaffLinesFR.Add(11, new Tuple<int, string>(3, "C"));

            //2
            StaffLinesFR.Add(12, new Tuple<int, string>(2, "B"));
            StaffLinesFR.Add(13, new Tuple<int, string>(2, "A"));
            StaffLinesFR.Add(14, new Tuple<int, string>(2, "G"));
            StaffLinesFR.Add(15, new Tuple<int, string>(2, "F"));
            StaffLinesFR.Add(16, new Tuple<int, string>(2, "E"));
            StaffLinesFR.Add(17, new Tuple<int, string>(2, "D"));
            StaffLinesFR.Add(18, new Tuple<int, string>(2, "C"));

            //1
            StaffLinesFR.Add(19, new Tuple<int, string>(1, "B"));
            StaffLinesFR.Add(20, new Tuple<int, string>(1, "A"));
            //F Clef


            // FOR C CLEFF on line 3 SETMAP THE STAFFLINE TO PITCH STEP AND OCTAVE

            //5
            StaffLinesCR.Add(0, new Tuple<int, string>(5, "F"));
            StaffLinesCR.Add(1, new Tuple<int, string>(5, "E"));
            StaffLinesCR.Add(2, new Tuple<int, string>(5, "D"));
            StaffLinesCR.Add(3, new Tuple<int, string>(5, "C"));

            //4
            StaffLinesCR.Add(4, new Tuple<int, string>(4, "B"));
            StaffLinesCR.Add(5, new Tuple<int, string>(4, "A"));
            StaffLinesCR.Add(6, new Tuple<int, string>(4, "G"));
            StaffLinesCR.Add(7, new Tuple<int, string>(4, "F"));
            StaffLinesCR.Add(8, new Tuple<int, string>(4, "E"));
            StaffLinesCR.Add(9, new Tuple<int, string>(4, "D"));
            StaffLinesCR.Add(10, new Tuple<int, string>(4, "C"));

            //3
            StaffLinesCR.Add(11, new Tuple<int, string>(3, "B"));
            StaffLinesCR.Add(12, new Tuple<int, string>(3, "A"));
            StaffLinesCR.Add(13, new Tuple<int, string>(3, "G"));
            StaffLinesCR.Add(14, new Tuple<int, string>(3, "F"));
            StaffLinesCR.Add(15, new Tuple<int, string>(3, "E"));
            StaffLinesCR.Add(16, new Tuple<int, string>(3, "D"));
            StaffLinesCR.Add(17, new Tuple<int, string>(3, "C"));

            //2
            StaffLinesCR.Add(18, new Tuple<int, string>(2, "B"));
            StaffLinesCR.Add(19, new Tuple<int, string>(2, "A"));
            StaffLinesCR.Add(20, new Tuple<int, string>(2, "G"));






            // for each of the symbols store correction values

            //notes downstem
           
            
            YPixelCorrection.Add((int)UnicodesDownStems._half, 305);
            YPixelCorrection.Add((int)UnicodesDownStems._quarter, 305);
            YPixelCorrection.Add((int)UnicodesDownStems._eighth, 305);
            YPixelCorrection.Add((int)UnicodesDownStems._16th, 305);
            YPixelCorrection.Add((int)UnicodesDownStems._32nd, 305);
            
            //notes upstem
            YPixelCorrection.Add((int)UnicodesUpStems._whole, 760);
            YPixelCorrection.Add((int)UnicodesUpStems._half, 795);
            YPixelCorrection.Add((int)UnicodesUpStems._quarter, 795);
            YPixelCorrection.Add((int)UnicodesUpStems._eighth, 795);
            YPixelCorrection.Add((int)UnicodesUpStems._16th, 795);
            YPixelCorrection.Add((int)UnicodesUpStems._32nd, 795);

            //rests 
            //A whole rest stays on the second line of the staff, the 8th in our program
            YPixelCorrection.Add((int)UnicodesUpStems.Rest_whole, 440 - (int)(17.07*StaffLines[8]));
            YPixelCorrection.Add((int)UnicodesUpStems.Rest_half, 735 - (int)(17.07 * StaffLines[10]));
            YPixelCorrection.Add((int)UnicodesUpStems.Rest_quarter, 600 - (int)(17.07 * StaffLines[10]));
            YPixelCorrection.Add((int)UnicodesUpStems.Rest_eighth, 635 - (int)(17.07 * StaffLines[10]));
            YPixelCorrection.Add((int)UnicodesUpStems.Rest_16th, 505 - (int)(17.07 * StaffLines[10]));
            YPixelCorrection.Add((int)UnicodesUpStems.Rest_32nd, 570 - (int)(17.07 * StaffLines[10]));

            //miscelinuous symbols
            YPixelCorrection.Add((int)UnicodesUpStems.G, 710 - (int)(17.07 * StaffLines[12]));
            YPixelCorrection.Add((int)UnicodesUpStems.C, 600 - (int)(17.07 * StaffLines[10]));
            YPixelCorrection.Add((int)UnicodesUpStems.F, 450 - (int)(17.07 * StaffLines[8]));
            
            YPixelCorrection.Add((int)UnicodesUpStems.sharp, 680);
            YPixelCorrection.Add((int)UnicodesUpStems.doublesharp, 560);
            YPixelCorrection.Add((int)UnicodesUpStems.flat, 800);
            YPixelCorrection.Add((int)UnicodesUpStems.natural, 690);
            YPixelCorrection.Add((int)UnicodesUpStems.bar, -(int)(17.07 * StaffLines[4]));
            YPixelCorrection.Add((int)UnicodesUpStems.Space, 0);
            YPixelCorrection.Add((int)UnicodesUpStems._dot, 900);

            //The following are not available yet please update
            YPixelCorrection.Add((int)UnicodesUpStems.flatflat, 2000);
            YPixelCorrection.Add((int)UnicodesUpStems.repeatright, 0);
            YPixelCorrection.Add((int)UnicodesUpStems.repeatleft, 0);




        }

        /// <summary>
        /// This function returns the unicode character for the MusiSync font, it also returns the picel location for the notes and other
        /// symbols based on the picth and clef information passed, the cordinate returned is with respect to 0 location at the top left
        /// of the cursor window. The units of the cordinate is em points and not pixels
        /// </summary>
        /// <param name="symbol">The symbol for which we want to get the unicode address and location</param>
        /// <param name="pitchstep"> The pitch step of the note or accidental</param>
        /// <param name="pitchoctave">the Octave og the note or accidental</param>
        /// <param name="clefsign"> the current clef sign</param>
        /// <returns>returns 2d vector, int[0]=unicode address, int[1]=Location</returns>
        public static int[] PixelCalc(string symbol, string pitchstep, string pitchoctave, string clefsign, int updown=0)
        {

            //returning array
            int[] UnicodePlusCordinate = new int[2];
            

            if (symbol == "whole" | symbol == "half" | symbol == "quarter" | symbol == "eighth" | symbol == "16th" | symbol == "32nd" | symbol == "dot")
            {
                //using enums and enums cannot start with a number so concatenating underscore
                symbol = "_" + symbol;

                //find stem info based on the clef sign and pitch information
                if (getStemDirection(pitchstep, pitchoctave, clefsign) == 1 & updown == 0 | updown == 1)
                    UnicodePlusCordinate[0] = (int)Enum.Parse(typeof(UnicodesUpStems), symbol);
                else if (getStemDirection(pitchstep, pitchoctave, clefsign) == -1 & updown == 0 | updown == -1)
                    UnicodePlusCordinate[0] = (int)Enum.Parse(typeof(UnicodesDownStems), symbol);
              
                //find y cordinate based on the clef sign and pitch information
                switch (clefsign)
                {
                    case "G":
                        {                            
                            //find the ypixel information based on clef, pitch octave, pitch step and notetype
                            UnicodePlusCordinate[1] = (int)StaffLinesG[new Tuple<int, string>(int.Parse(pitchoctave), pitchstep)] - YPixelCorrection[UnicodePlusCordinate[0]];
                            break;
                        }

                    case "F":
                        {                            
                            //find the ypixel information based on clef, pitch octave, pitch step and notetype
                            UnicodePlusCordinate[1] = (int)StaffLinesF[new Tuple<int, string>(int.Parse(pitchoctave), pitchstep)] - YPixelCorrection[UnicodePlusCordinate[0]];
                            break;
                        }
                    case "C":
                        {                            
                            //find the ypixel information based on clef, pitch octave, pitch step and notetype
                            UnicodePlusCordinate[1] = (int)StaffLinesC[new Tuple<int, string>(int.Parse(pitchoctave), pitchstep)] - YPixelCorrection[UnicodePlusCordinate[0]];
                            break;
                        }

                }


                return UnicodePlusCordinate;
            }
            else if (symbol == "natural" | symbol == "sharp" | symbol == "flat" | symbol == "double-sharp" | symbol == "flat-flat")
            {
                //from musicxml the string contain specail character not suitable for enums so remove that
                symbol = symbol.IndexOf("-") > 0 ? symbol.Remove(symbol.IndexOf("-"), 1) : symbol;
                //assign unicode
                UnicodePlusCordinate[0] = (int)Enum.Parse(typeof(UnicodesUpStems), symbol);

                //assign staf lines 
                switch (clefsign)
                {
                    case "G":
                        {
                            UnicodePlusCordinate[1] = (int)StaffLinesG[new Tuple<int, string>(int.Parse(pitchoctave), pitchstep)] - YPixelCorrection[UnicodePlusCordinate[0]];
                            break;
                        }

                    case "F":
                        {
                            UnicodePlusCordinate[1] = (int)StaffLinesF[new Tuple<int, string>(int.Parse(pitchoctave), pitchstep)] - YPixelCorrection[UnicodePlusCordinate[0]];
                            break;
                        }
                    case "C":
                        {
                            UnicodePlusCordinate[1] = (int)StaffLinesC[new Tuple<int, string>(int.Parse(pitchoctave), pitchstep)] - YPixelCorrection[UnicodePlusCordinate[0]];
                            break;
                        }

                }
                return UnicodePlusCordinate;
            }
            else // if not a note and some other symbol
            {
                try
                {
                    UnicodePlusCordinate[0] = (int)Enum.Parse(typeof(UnicodesUpStems), symbol);
                    UnicodePlusCordinate[1] = -YPixelCorrection[UnicodePlusCordinate[0]];
                    return UnicodePlusCordinate;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception in pixelcalc() "+ symbol +" exception message " + e.Message);
                    return null;
                    
                }

            }



        }

        public static int getStemDirection(string pitchstep, string pitchoctave, string clefsign)
        {
            int stemdirec = 0;

            switch (clefsign)
            {
                case "G":
                    {
                        if (StaffLinesG[new Tuple<int, string>(int.Parse(pitchoctave), pitchstep)] <= 17.07 * StaffLines[10])
                            stemdirec = -1;
                        else
                            stemdirec = 1;
                        break;
                    }

                case "F":
                    {
                        if (StaffLinesF[new Tuple<int, string>(int.Parse(pitchoctave), pitchstep)] <= 17.07 * StaffLines[10])
                            stemdirec = -1;
                        else
                            stemdirec = 1;
                        break;
                    }
                case "C":
                    {
                        if (StaffLinesC[new Tuple<int, string>(int.Parse(pitchoctave), pitchstep)] <= 17.07 * StaffLines[10])
                            stemdirec = -1;
                        else
                            stemdirec = 1;
                        break;
                    }

            }
            return stemdirec;
        }

        public static int[] getNoteStemPosition(string pitchstep, string pitchoctave, string clefsign, int updown)
        {
            int[] stemposition = new int[2];
            
            
          
            //find y cordinate based on the clef sign and pitch information
            switch (clefsign)
            {
                case "G":
                    {
                        if (updown == 1)
                        {
                            stemposition[0] = 256;
                            //find the ypixel information based on clef, pitch octave, pitch step and notetype
                            stemposition[1] = (int)StaffLinesG[new Tuple<int, string>(int.Parse(pitchoctave), pitchstep)] - YPixelCorrection[(int)Enum.Parse(typeof(UnicodesUpStems), "_quarter")]+84;


                        }
                        else if (updown == -1)
                        {
                            stemposition[0] = 70;
                            //find the ypixel information based on clef, pitch octave, pitch step and notetype
                            stemposition[1] = (int)StaffLinesG[new Tuple<int, string>(int.Parse(pitchoctave), pitchstep)] - YPixelCorrection[(int)Enum.Parse(typeof(UnicodesDownStems), "_quarter")]+1100;
                        }
                        break;
                    }

                case "F":
                    {
                        if (updown == 1)
                        {
                            stemposition[0] = 256;
                            //find the ypixel information based on clef, pitch octave, pitch step and notetype
                            stemposition[1] = (int)StaffLinesF[new Tuple<int, string>(int.Parse(pitchoctave), pitchstep)] - YPixelCorrection[(int)Enum.Parse(typeof(UnicodesUpStems), "_quarter")] + 84;
                        }
                        else if (updown == -1)
                        {
                            
                            stemposition[0] = 70;
                            //find the ypixel information based on clef, pitch octave, pitch step and notetype
                            stemposition[1] = (int)StaffLinesF[new Tuple<int, string>(int.Parse(pitchoctave), pitchstep)] - YPixelCorrection[(int)Enum.Parse(typeof(UnicodesDownStems), "_quarter")] + 1100;
                        }
                        break;
                    }
                case "C":
                    {
                        if (updown == 1)
                        {
                            stemposition[0] = 256;
                            //find the ypixel information based on clef, pitch octave, pitch step and notetype
                            stemposition[1] = (int)StaffLinesC[new Tuple<int, string>(int.Parse(pitchoctave), pitchstep)] - YPixelCorrection[(int)Enum.Parse(typeof(UnicodesUpStems), "_quarter")] + 84;

                        }
                        else if (updown == -1)
                        {
                            stemposition[0] = 70;
                            //find the ypixel information based on clef, pitch octave, pitch step and notetype
                            stemposition[1] = (int)StaffLinesC[new Tuple<int, string>(int.Parse(pitchoctave), pitchstep)] - YPixelCorrection[(int)Enum.Parse(typeof(UnicodesDownStems), "_quarter")] + 1100;
                        }
                        break;
                    }

            }

            return stemposition;
            
        }
        
        public static int[] getSlurTiePosition(string pitchstep, string pitchoctave, string clefsign, int updown, bool stemdiff=false)
        {
            int[] stemposition = new int[2];            
            int y1 = stemdiff == false ? 1000 : 30;
            int y2 = stemdiff == false ? 150 : 1150;

            //find y cordinate based on the clef sign and pitch information
            switch (clefsign)
            {
                case "G":
                    {
                        if (updown == 1)
                        {
                            stemposition[0] = 130;
                            //find the ypixel information based on clef, pitch octave, pitch step and notetype                           
                            stemposition[1] = (int)StaffLinesG[new Tuple<int, string>(int.Parse(pitchoctave), pitchstep)] - YPixelCorrection[(int)Enum.Parse(typeof(UnicodesUpStems), "_quarter")] + y1;


                        }
                        else if (updown == -1)
                        {
                            stemposition[0] = 200;
                            //find the ypixel information based on clef, pitch octave, pitch step and notetype                            
                            stemposition[1] = (int)StaffLinesG[new Tuple<int, string>(int.Parse(pitchoctave), pitchstep)] - YPixelCorrection[(int)Enum.Parse(typeof(UnicodesDownStems), "_quarter")] + y2;
                        }
                        break;
                    }

                case "F":
                    {
                        if (updown == 1)
                        {
                            stemposition[0] = 130;
                            //find the ypixel information based on clef, pitch octave, pitch step and notetype                            
                            stemposition[1] = (int)StaffLinesF[new Tuple<int, string>(int.Parse(pitchoctave), pitchstep)] - YPixelCorrection[(int)Enum.Parse(typeof(UnicodesUpStems), "_quarter")] + y1;
                        }
                        else if (updown == -1)
                        {

                            stemposition[0] = 200;
                            //find the ypixel information based on clef, pitch octave, pitch step and notetype                            
                            stemposition[1] = (int)StaffLinesF[new Tuple<int, string>(int.Parse(pitchoctave), pitchstep)] - YPixelCorrection[(int)Enum.Parse(typeof(UnicodesDownStems), "_quarter")] + y2;
                        }
                        break;
                    }
                case "C":
                    {
                        if (updown == 1)
                        {
                            stemposition[0] = 130;
                            //find the ypixel information based on clef, pitch octave, pitch step and notetype                           
                            stemposition[1] = (int)StaffLinesC[new Tuple<int, string>(int.Parse(pitchoctave), pitchstep)] - YPixelCorrection[(int)Enum.Parse(typeof(UnicodesUpStems), "_quarter")] + y1;

                        }
                        else if (updown == -1)
                        {
                            stemposition[0] = 200;
                            //find the ypixel information based on clef, pitch octave, pitch step and notetype                            
                            stemposition[1] = (int)StaffLinesC[new Tuple<int, string>(int.Parse(pitchoctave), pitchstep)] - YPixelCorrection[(int)Enum.Parse(typeof(UnicodesDownStems), "_quarter")] + y2;
                        }
                        break;
                    }

            }

            return stemposition;

        }

        /// <summary>
        /// Calculates the width of the Glyphs of a text string. Implicitly assumes a font size of 
        /// 60 and the Bach TTF font
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static double MeassureGlyphWidth(string text)
        {
            
            FormattedText ft = new FormattedText(text,
                                                CultureInfo.CurrentCulture,
                                                FlowDirection.LeftToRight,
                                               new Typeface("Bach"),
                                                60,
                                                Brushes.Black);
            return ft.Width;



        }

        /// <summary>
        /// Returns the pitch octave and step, given a staff line number and a clefsign
        /// </summary>
        /// <param name="LineNumber"></param>
        /// <param name="clefsign"></param>
        /// <returns></returns>
        public static Tuple<int, string> GetPitchOctaveAndStep(int LineNumber, string clefsign)
        {

            switch (clefsign)
            {
                case "G":
                        return StaffLinesGR[LineNumber];                        
                    
                case "F":
                        return StaffLinesFR[LineNumber];                        
                   
                case "C":
                        return StaffLinesCR[LineNumber];
                       
                default:
                    return null;
            }

        }

        

    }


}
            