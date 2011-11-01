using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FishComponents
{
    class PID
    {
        public double P = 1.0, I = 0.0, D = 0.0;
        double last_proportional = 0.0;
        double integral = 0.0;

        public PID()
        {
        }

        public PID(double p, double i, double d)
        {
            P = p;
            I = i;
            D = d;
        }

        public double update(double newValue)
        {
            double proportional = newValue;
            double derivative = last_proportional - proportional;
            integral += proportional;

            last_proportional = proportional;

            newValue = P * proportional + I * integral + D * derivative;
            return newValue;
        }
    }
}
