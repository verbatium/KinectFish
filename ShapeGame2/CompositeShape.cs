using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;

namespace ShapeGame2
{
    class CompositeShape : Shape
    {

        protected override System.Windows.Media.Geometry DefiningGeometry
        {
            get { throw new NotImplementedException(); }
        }
    }

}
