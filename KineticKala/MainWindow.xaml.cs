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
using Microsoft.Research.Kinect.Nui;
using KineticKala;
namespace KineticKala
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        Runtime nui;
        int totalFrames = 0;
        int lastFrames = 0;
        DateTime lastTime = DateTime.MaxValue;

        const int RED_IDX = 2;
        const int GREEN_IDX = 1;
        const int BLUE_IDX = 0;
        byte[] depthFrame32 = new byte[320 * 240 * 4];

        Dictionary<JointID, Brush> jointColors =
            new Dictionary<JointID, Brush>(){
            {JointID.HipCenter,new SolidColorBrush(Color.FromRgb(169,176,155))},
            {JointID.Spine,new SolidColorBrush(Color.FromRgb(169,176,155))},
            {JointID.ShoulderCenter,new SolidColorBrush(Color.FromRgb(168,230,29))},
            {JointID.Head,new SolidColorBrush(Color.FromRgb(200,0,0))},

            {JointID.ShoulderLeft,new SolidColorBrush(Color.FromRgb(200,0,0))},
            {JointID.ElbowLeft,new SolidColorBrush(Color.FromRgb(200,0,0))},
            {JointID.WristLeft,new SolidColorBrush(Color.FromRgb(200,0,0))},
            {JointID.HandLeft,new SolidColorBrush(Color.FromRgb(200,0,0))},
            {JointID.ShoulderRight,new SolidColorBrush(Color.FromRgb(200,0,0))},
            {JointID.ElbowRight,new SolidColorBrush(Color.FromRgb(200,0,0))},
            {JointID.WristRight,new SolidColorBrush(Color.FromRgb(200,0,0))},
            {JointID.HandRight,new SolidColorBrush(Color.FromRgb(200,0,0))},
            {JointID.HipLeft,new SolidColorBrush(Color.FromRgb(200,0,0))},
            {JointID.KneeLeft,new SolidColorBrush(Color.FromRgb(200,0,0))},
            {JointID.AnkleLeft,new SolidColorBrush(Color.FromRgb(200,0,0))},
            {JointID.FootLeft,new SolidColorBrush(Color.FromRgb(200,0,0))},
            {JointID.HipRight,new SolidColorBrush(Color.FromRgb(200,0,0))},
            {JointID.KneeRight,new SolidColorBrush(Color.FromRgb(200,0,0))},
            {JointID.AnkleRight,new SolidColorBrush(Color.FromRgb(200,0,0))},
            {JointID.FootRight,new SolidColorBrush(Color.FromRgb(200,0,0))}
            };

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            nui  = new Runtime();
            try
            {
                nui.Initialize(RuntimeOptions.UseDepthAndPlayerIndex | RuntimeOptions.UseSkeletalTracking | RuntimeOptions.UseColor);
            }
            catch (Exception)
            {
                return;
                //throw;
            }
            try
            {
                nui.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);
                nui.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution320x240, ImageType.DepthAndPlayerIndex);

            }
            catch (Exception)
            {
                return; 
             //   throw;
            }
            lastTime = DateTime.Now;
            //nui.DepthFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_DepthFrameReady);
            nui.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(nui_SkeletonFrameReady);
            //nui.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_ColorFrameReady);


        }

        void nui_ColorFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            PlanarImage Image = e.ImageFrame.Image;
            
        }
        void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame skeletonFrame = e.SkeletonFrame;
            int iSkeleton = 0;
            Brush[] brushes = new Brush[6];
            brushes[0] = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            brushes[1] = new SolidColorBrush(Color.FromRgb(0, 255, 0));
            brushes[2] = new SolidColorBrush(Color.FromRgb(64, 255, 255));
            brushes[3] = new SolidColorBrush(Color.FromRgb(255, 255, 64));
            brushes[4] = new SolidColorBrush(Color.FromRgb(255, 64, 255));
            brushes[5] = new SolidColorBrush(Color.FromRgb(128, 128, 255));

            skeleton.Children.Clear();
            foreach (SkeletonData data in skeletonFrame.Skeletons)
            {
                if (SkeletonTrackingState.Tracked == data.TrackingState)
                {
                    // Draw bones
                    Brush brush = brushes[iSkeleton % brushes.Length];
                    //skeleton.Children.Add(getBodySegment(data.Joints, brush, JointID.HipCenter, JointID.Spine, JointID.ShoulderCenter, JointID.Head));
                    //skeleton.Children.Add(getBodySegment(data.Joints, brush, JointID.ShoulderCenter, JointID.ShoulderLeft, JointID.ElbowLeft, JointID.WristLeft, JointID.HandLeft));
                    //skeleton.Children.Add(getBodySegment(data.Joints, brush, JointID.ShoulderCenter, JointID.ShoulderRight, JointID.ElbowRight, JointID.WristRight, JointID.HandRight));
                    //skeleton.Children.Add(getBodySegment(data.Joints, brush, JointID.HipCenter, JointID.HipLeft, JointID.KneeLeft, JointID.AnkleLeft, JointID.FootLeft));
                    //skeleton.Children.Add(getBodySegment(data.Joints, brush, JointID.HipCenter, JointID.HipRight, JointID.KneeRight, JointID.AnkleRight, JointID.FootRight));
                    skeleton.Children.Add(getFishBody(data.Joints, brush));
            
                    
                    
                    // Draw joints
                    //foreach (Joint joint in data.Joints)
                    //{
                    //    Point jointPos = getDisplayPosition(joint);
                    //    Line jointLine = new Line();
                    //    jointLine.X1 = jointPos.X - 3;
                    //    jointLine.X2 = jointLine.X1 + 6;
                    //    jointLine.Y1 = jointLine.Y2 = jointPos.Y;
                    //    jointLine.Stroke = jointColors[joint.ID];
                    //    jointLine.StrokeThickness = 6;
                    //    skeleton.Children.Add(jointLine);
                    //}
                }
                iSkeleton++;
            } // for each skeleton
        }
        public Point getDisplayPosition(Joint joint)
        {
            float depthX, depthY;
            nui.SkeletonEngine.SkeletonToDepthImage(joint.Position, out depthX, out depthY);
            depthX = depthX * 320; //convert to 320, 240 space
            depthY = depthY * 240; //convert to 320, 240 space
            int colorX, colorY;
            ImageViewArea iv = new ImageViewArea();
            // only ImageResolution.Resolution640x480 is supported at this point
            nui.NuiCamera.GetColorPixelCoordinatesFromDepthPixel(ImageResolution.Resolution640x480, iv, (int)depthX, (int)depthY, (short)0, out colorX, out colorY);

            // map back to skeleton.Width & skeleton.Height
            return new Point((int)(skeleton.Width * colorX / 640.0), (int)(skeleton.Height * colorY / 480));
        }

        public Polyline getFishBody(Microsoft.Research.Kinect.Nui.JointsCollection joints, Brush brush)
        {
            PointCollection points = new PointCollection(6);

            points.Add(getDisplayPosition(joints[JointID.Head]));
           // points.Add(getDisplayPosition(joints[JointID.ShoulderCenter]));
            points.Add(getDisplayPosition(joints[JointID.Spine]));
           // points.Add(getDisplayPosition(joints[JointID.HipCenter]));

            //points.Add(Average(
            //getDisplayPosition(joints[JointID.HipRight]),
            //getDisplayPosition(joints[JointID.HipLeft]))
            //);


            //points.Add(Average(
            //getDisplayPosition(joints[JointID.KneeLeft]),
            //getDisplayPosition(joints[JointID.KneeRight]))
            //);

            points.Add(Average(
            getDisplayPosition(joints[JointID.AnkleLeft]),
            getDisplayPosition(joints[JointID.AnkleRight]))
            );

            Polyline polyline = new Polyline();
            polyline.Points = points;
            polyline.Stroke = brush;
            polyline.StrokeThickness = 5;
            return polyline;
        }
        Path fish(Microsoft.Research.Kinect.Nui.JointsCollection joints, Brush brush)
        {
            PathFigure myPathFigure = new PathFigure();
            
            Point a =  Average(
            getDisplayPosition(joints[JointID.AnkleLeft]),
            getDisplayPosition(joints[JointID.AnkleRight]) );
            Point b = getDisplayPosition(joints[JointID.Head]);
            Point c = getDisplayPosition(joints[JointID.Spine]);

            c = multiplyDistance( a, b, c, 2);
            
            myPathFigure.StartPoint = a;

            myPathFigure.Segments.Add(
                new BezierSegment(
                  c  , //first controll point
                  c, //second controll point
                  b ,
                    true /* IsStroked */));

            /// Create a PathGeometry to contain the figure.
            PathGeometry myPathGeometry = new PathGeometry();
            myPathGeometry.Figures.Add(myPathFigure);

            // Display the PathGeometry. 
            Path myPath = new Path();
            myPath.Stroke = brush;
            myPath.StrokeThickness = 5;
            myPath.Data = myPathGeometry;

            return myPath;
        }
        Point Average(Point a, Point b)
        {

            int dx = (int)((a.X - b.X) / 2);
            int dy = (int)((a.Y-b.Y)/2);
 
            
            return new Point(a.X-dx,a.Y-dy);
        }

        int dispanceFromLine(Point a, Point b, Point c)
        {
            //
            // find the distance from the point (cx,cy) to the line
            // determined by the points (ax,ay) and (bx,by)
            //
            // distanceSegment = distance from the point to the line segment
            // distanceLine = distance from the point to the line (assuming
            //					infinite extent in both directions
            //

            /*

            Subject 1.02: How do I find the distance from a point to a line?


                Let the point be C (Cx,Cy) and the line be AB (Ax,Ay) to (Bx,By).
                Let P be the point of perpendicular projection of C on AB.  The parameter
                r, which indicates P's position along AB, is computed by the dot product 
                of AC and AB divided by the square of the length of AB:
    
                (1)     AC dot AB
                    r = ---------  
                        ||AB||^2
    
                r has the following meaning:
    
                    r=0      P = A
                    r=1      P = B
                    r<0      P is on the backward extension of AB
                    r>1      P is on the forward extension of AB
                    0<r<1    P is interior to AB
    
                The length of a line segment in d dimensions, AB is computed by:
    
                    L = sqrt( (Bx-Ax)^2 + (By-Ay)^2 + ... + (Bd-Ad)^2)

                so in 2D:   
    
                    L = sqrt( (Bx-Ax)^2 + (By-Ay)^2 )
    
                and the dot product of two vectors in d dimensions, U dot V is computed:
    
                    D = (Ux * Vx) + (Uy * Vy) + ... + (Ud * Vd)
    
                so in 2D:   
    
                    D = (Ux * Vx) + (Uy * Vy) 
    
                So (1) expands to:
    
                        (Cx-Ax)(Bx-Ax) + (Cy-Ay)(By-Ay)
                    r = -------------------------------
                                      L^2

                The point P can then be found:

                    Px = Ax + r(Bx-Ax)
                    Py = Ay + r(By-Ay)

                And the distance from A to P = r*L.

                Use another parameter s to indicate the location along PC, with the 
                following meaning:
                       s<0      C is left of AB
                       s>0      C is right of AB
                       s=0      C is on AB

                Compute s as follows:

                        (Ay-Cy)(Bx-Ax)-(Ax-Cx)(By-Ay)
                    s = -----------------------------
                                    L^2


                Then the distance from C to P = |s|*L.

            */


            double r_numerator = (c.X - a.X) * (b.X - a.X) + (c.Y - a.Y) * (b.Y - a.Y);
            double r_denomenator = (b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y);
            double r = r_numerator / r_denomenator;
            //
            double px = a.X + r * (b.X - a.X);
            double py = a.Y + r * (b.Y - a.Y);
            //     
            double s = ((a.Y - c.Y) * (b.X - a.X) - (a.X - c.X) * (b.Y - a.Y)) / r_denomenator;

            double distanceLine = Math.Abs(s) * Math.Sqrt(r_denomenator);
            double distanceSegment;
            //
            // (xx,yy) is the point on the lineSegment closest to (cx,cy)
            //
            double xx = px;
            double yy = py;

            Point x = new Point(px, py);

            if ((r >= 0) && (r <= 1))
            {
                distanceSegment = distanceLine;
            }
            else
            {

                double dist1 = (c.X - a.X) * (c.X - a.X) + (c.Y - a.Y) * (c.Y - a.Y);
                double dist2 = (c.X - b.X) * (c.X - b.X) + (c.Y - b.Y) * (c.Y - b.Y);
                if (dist1 < dist2)
                {
                    xx = a.X;
                    yy = a.Y;
                    distanceSegment = Math.Sqrt(dist1);
                }
                else
                {
                    xx = b.X;
                    yy = b.Y;
                    distanceSegment = Math.Sqrt(dist2);
                }


            }


            return 0;
        }
        Point ClosestPointOnLine(Point a, Point b, Point c)
        {
            double r_numerator = (c.X - a.X) * (b.X - a.X) + (c.Y - a.Y) * (b.Y - a.Y);
            double r_denomenator = (b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y);
            double r = r_numerator / r_denomenator;
            //
            double px = a.X + r * (b.X - a.X);
            double py = a.Y + r * (b.Y - a.Y);
            //     
            double s = ((a.Y - c.Y) * (b.X - a.X) - (a.X - c.X) * (b.Y - a.Y)) / r_denomenator;

            double distanceLine = Math.Abs(s) * Math.Sqrt(r_denomenator);


            Point x = new Point(px, py);

            return x;
        }

        Point multiplyDistance(Point a, Point b, Point c, int k = 1)
        {
            Point o = ClosestPointOnLine(a, b, c);
            double dx = o.X - c.X;
            double dy = o.Y - c.Y;
            return new Point(c.X - dx * k, c.Y -dy * k);
        }

        Polyline getBodySegment(Microsoft.Research.Kinect.Nui.JointsCollection joints, Brush brush, params JointID[] ids)
        {
            PointCollection points = new PointCollection(ids.Length);
            for (int i = 0; i < ids.Length; ++i)
            {
                points.Add(getDisplayPosition(joints[ids[i]]));
            }

            Polyline polyline = new Polyline();
            polyline.Points = points;
            polyline.Stroke = brush;
            polyline.StrokeThickness = 5;
            return polyline;
        }

        void nui_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            PlanarImage Image = e.ImageFrame.Image;
            byte[] convertedDepthFrame = convertDepthFrame(Image.Bits);

        }
        // Converts a 16-bit grayscale depth frame which includes player indexes into a 32-bit frame
        // that displays different players in different colors
        byte[] convertDepthFrame(byte[] depthFrame16)
        {
            for (int i16 = 0, i32 = 0; i16 < depthFrame16.Length && i32 < depthFrame32.Length; i16 += 2, i32 += 4)
            {
                int player = depthFrame16[i16] & 0x07;
                int realDepth = (depthFrame16[i16 + 1] << 5) | (depthFrame16[i16] >> 3);
                // transform 13-bit depth information into an 8-bit intensity appropriate
                // for display (we disregard information in most significant bit)
                byte intensity = (byte)(255 - (255 * realDepth / 0x0fff));

                depthFrame32[i32 + RED_IDX] = 0;
                depthFrame32[i32 + GREEN_IDX] = 0;
                depthFrame32[i32 + BLUE_IDX] = 0;

                // choose different display colors based on player
                switch (player)
                {
                    case 0:
                        depthFrame32[i32 + RED_IDX] = (byte)(intensity / 2);
                        depthFrame32[i32 + GREEN_IDX] = (byte)(intensity / 2);
                        depthFrame32[i32 + BLUE_IDX] = (byte)(intensity / 2);
                        break;
                    case 1:
                        depthFrame32[i32 + RED_IDX] = intensity;
                        break;
                    case 2:
                        depthFrame32[i32 + GREEN_IDX] = intensity;
                        break;
                    case 3:
                        depthFrame32[i32 + RED_IDX] = (byte)(intensity / 4);
                        depthFrame32[i32 + GREEN_IDX] = (byte)(intensity);
                        depthFrame32[i32 + BLUE_IDX] = (byte)(intensity);
                        break;
                    case 4:
                        depthFrame32[i32 + RED_IDX] = (byte)(intensity);
                        depthFrame32[i32 + GREEN_IDX] = (byte)(intensity);
                        depthFrame32[i32 + BLUE_IDX] = (byte)(intensity / 4);
                        break;
                    case 5:
                        depthFrame32[i32 + RED_IDX] = (byte)(intensity);
                        depthFrame32[i32 + GREEN_IDX] = (byte)(intensity / 4);
                        depthFrame32[i32 + BLUE_IDX] = (byte)(intensity);
                        break;
                    case 6:
                        depthFrame32[i32 + RED_IDX] = (byte)(intensity / 2);
                        depthFrame32[i32 + GREEN_IDX] = (byte)(intensity / 2);
                        depthFrame32[i32 + BLUE_IDX] = (byte)(intensity);
                        break;
                    case 7:
                        depthFrame32[i32 + RED_IDX] = (byte)(255 - intensity);
                        depthFrame32[i32 + GREEN_IDX] = (byte)(255 - intensity);
                        depthFrame32[i32 + BLUE_IDX] = (byte)(255 - intensity);
                        break;
                }
            }
            return depthFrame32;
        }



        int GetAngleABC(Point a, Point b, Point c)
        {
            Point ab = new Point(b.X - a.X, b.Y - a.Y);
            Point ac = new Point(b.X - c.X, b.Y - c.Y);


            double dotabac = (ab.X * ab.Y + ac.X * ac.Y);
            double lenab = Math.Sqrt(ab.X * ab.X + ab.Y * ab.Y);
            double lenac = Math.Sqrt(ac.X * ac.X + ac.Y * ac.Y);

            double dacos = dotabac / lenab / lenac;

            double rslt = Math.Acos(dacos);
            double rs = (rslt * 180) / 3.141592;
            Math.Round(rs);
            return (int)rs;
        }


    }
}
