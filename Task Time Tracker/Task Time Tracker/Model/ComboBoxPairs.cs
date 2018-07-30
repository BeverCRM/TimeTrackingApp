using System;
using Task_Time_Tracker.Model;

namespace Task_Time_Tracker
{
    public class ComboBoxPairs
    {
        public string Text { get; set; }
        public Guid Value { get; set; }

        public ComboBoxPairs(string key, Guid value)
        {
            Text = key;
            Value = value;
        }
    }

    public class ComboBoxPairs1
    {
        public string Text { get; set; }
        public ProjectTask Value { get; set; }

        public ComboBoxPairs1(string key, ProjectTask value)
        {
            Text = key;
            Value = value;
        }
    }
}