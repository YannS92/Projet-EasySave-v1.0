using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave_2._0
{
    class LogLine
    {

        public LogLine(string _content)
        {
            Time = DateTime.Now.ToString();
            Content = _content;
        }

        private string content;

        public string Content
        {
            get { return content; }
            set { content = value; }
        }

        private string time;

        public string Time
        {
            get { return time; }
            set { time = value; }
        }

    }
}