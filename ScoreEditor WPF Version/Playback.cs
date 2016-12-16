using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
namespace ScoreEditor_WPF_Version
{  
    class Playback
    {
        [DllImport("winmm.dll")]
        public static extern int midiOutOpen(ref IntPtr lphMidiOut, uint uDeviceID, IntPtr dwCallback, IntPtr dwinstance, int dwflags);
        [DllImport("winmm.dll")]
        public static extern int midiOutClose(IntPtr hMidiOut);
        [DllImport("winmm.dll")]
        public static extern int midiOutShortMsg(IntPtr hMidiOut, uint dwMsg);
        private static uint dwMsg = 0;
        private static IntPtr OutHandle = IntPtr.Zero;
        private const int MMSYSERR_NOERROR = 0;
        private static int MIDI_MAPPER = 0;
       

        public static void OpenProgram(int program)
        {
            midiOutOpen(ref OutHandle, (uint)MIDI_MAPPER - 1, IntPtr.Zero, IntPtr.Zero, 0);
            //29 program number
            //C0 program change message
            dwMsg = (uint)((0x00 << 24) | (00 << 16) | program << 8) | (0xC0);
            
            midiOutShortMsg(OutHandle, dwMsg);
        }

        public static void CloseProgram()
        {
            midiOutClose(OutHandle);
        }

        public static void NoteOn(int pitch, int vol)
        { 
            dwMsg = (uint)((0x00 << 24) | (vol << 16) | pitch << 8) | (0x90);
            midiOutShortMsg(OutHandle, dwMsg);

        }

        public static void NoteOff(int pitch, int vol)
        {
            dwMsg = (uint)((0x00 << 24) | (vol << 16) | (pitch << 8) | (0x80));
            midiOutShortMsg(OutHandle, dwMsg);
        }



        //static System.Timers.Timer timer;

        //public static void PlayNote(double dur,int pit, int vo)
        //{
        //    duration = dur;
        //    pitch=pit;
        //    vol = vo;
        //    dwMsg = (uint)((0x00 << 24) | (vol << 16) | pitch << 8) | (0x90);
        //    midiOutShortMsg(OutHandle, dwMsg);


        //    //timer = new System.Timers.Timer(duration);
        //    //timer.Enabled = true; // Enable it
        //    //timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);

        //}

        //static void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    dwMsg = (uint)((0x00 << 24) | (vol << 16) | (pitch << 8) | (0x80));
        //    midiOutShortMsg(OutHandle, dwMsg);
        //}
        

    }
}
