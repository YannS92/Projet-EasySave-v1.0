using System;
using System.Collections.Generic;
using System.Text;

namespace Projet_EasySave_v1._0
{
    class LogLine
    {

        public LogLine(string _content)
        {
            Time = DateTime.Now.ToString();
            Content = _content;
        }

        private string time;

        public string Time
        {
            get { return time; }
            set { time = value; }
        }

        private string content;

        public string Content
        {
            get { return content; }
            set { content = value; }
        }



    }
}