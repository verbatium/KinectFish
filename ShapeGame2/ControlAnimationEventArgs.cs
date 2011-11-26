using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShapeGame2
{
    class ControlAnimationEventArgs:EventArgs
    {
        public string ObjectEvent;
        public object element;
        public ControlAnimationEventArgs(object element, string ObjectEvent)
        {
            this.element = element;
            this.ObjectEvent=ObjectEvent;
        }
    }
}
