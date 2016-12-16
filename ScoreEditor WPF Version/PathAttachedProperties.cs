using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Shapes;

namespace ScoreEditor_WPF_Version
{
    public class PathAttachedPropertties
    {

        public static readonly DependencyProperty StartMeasure = DependencyProperty.RegisterAttached("StartMeasure", typeof(int), typeof(PathAttachedPropertties));
        public static void SetStartMeasure(Path path, int value)
        {
            path.SetValue(StartMeasure, value);
        }
        public static int GetStartMeasure(Path path)
        {
            return (int)path.GetValue(StartMeasure);
        }



        public static readonly DependencyProperty StopMeasure = DependencyProperty.RegisterAttached("StopMeasure", typeof(int), typeof(PathAttachedPropertties));
        public static void SetStopMeasure(Path path, int value)
        {
            path.SetValue(StopMeasure, value);
        }
        public static int GetStopMeasure(Path path)
        {
            return (int)path.GetValue(StopMeasure);
        }

        public static readonly DependencyProperty StartNote = DependencyProperty.RegisterAttached("StartNote", typeof(int), typeof(PathAttachedPropertties));
        public static void SetStartNote(Path path, int value)
        {
            path.SetValue(StartNote, value);
        }
        public static int GetStartNote(Path path)
        {
            return (int)path.GetValue(StartNote);
        }


        public static readonly DependencyProperty StopNote = DependencyProperty.RegisterAttached("StopNote", typeof(int), typeof(PathAttachedPropertties));
        public static void SetStopNote(Path path, int value)
        {
            path.SetValue(StopNote, value);
        }
        public static int GetStopNote(Path path)
        {
            return (int)path.GetValue(StopNote);
        }


    }
}
