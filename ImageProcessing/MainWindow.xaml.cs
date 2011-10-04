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
using System.IO;
using System.Drawing;
using AForge.Imaging.Filters;
using AForge.Imaging;

namespace ImageProcessing
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process();
        }
        void Process()
        {
            /*
             * 1. Convert to BW
             * 2. process centerline
             */
            System.Drawing.Bitmap bitmap = processAll(txtBox.Text);

            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();

            img2.Stretch = Stretch.Fill;
            img2.Source = bi;
        }
        Bitmap processImageCenterline(string filename)
        {
            using (Bitmap SampleImage = (Bitmap)System.Drawing.Image.FromFile(filename))
            {
                // We must convert it to grayscale because  
                // the filter accepts 8 bpp grayscale images  
                Grayscale GF = new Grayscale(0.2125, 0.7154, 0.0721);
                using (Bitmap GSampleImage = GF.Apply(SampleImage))
                {
                    // Saving the grayscale image, so we could see it later  
                    // Detecting image edges and saving the result 
                    CannyEdgeDetector CED = new CannyEdgeDetector(0, 70);
                    //CED.ApplyInPlace(GSampleImage);
                    //BradleyLocalThresholding bwfilter = new BradleyLocalThresholding();
                    //bwfilter.ApplyInPlace(GSampleImage);
                    // create filter

                    // create filter sequence
                    FiltersSequence filterSequence = new FiltersSequence();


                    // Inverting image to get white image on black background
                    filterSequence.Add(new Invert());
                    filterSequence.Add(new SISThreshold());
                    // Finding skeleton
                    filterSequence.Add(new SimpleSkeletonization());
                    //clean image from scratches
                    short[,] se = new short[,] {
                                                { -1, -1, -1 },
                                                {  0,  1,  0 },
                                                { -1, -1, -1 }};

                    filterSequence.Add(new HitAndMiss(se, HitAndMiss.Modes.Thinning));
                    //filterSequence.Add(new Median( ));
                    //filterSequence.Add(new Dilatation()); 
                    filterSequence.Add(new Invert());
                    // apply the filter and rfeturn value
                    return filterSequence.Apply(GSampleImage);

                }

            }
        }
        Bitmap processImageContour(string filename)
        {
            using (Bitmap SampleImage = (Bitmap)System.Drawing.Image.FromFile(filename))
            {
                // We must convert it to grayscale because  
                // the filter accepts 8 bpp grayscale images  
                Grayscale GF = new Grayscale(0.2125, 0.7154, 0.0721);
                using (Bitmap GSampleImage = GF.Apply(SampleImage))
                {
                    // Saving the grayscale image, so we could see it later  
                    // Detecting image edges and saving the result 
                    CannyEdgeDetector CED = new CannyEdgeDetector(0, 70);
                    //CED.ApplyInPlace(GSampleImage);
                    //BradleyLocalThresholding bwfilter = new BradleyLocalThresholding();
                    //bwfilter.ApplyInPlace(GSampleImage);
                    // create filter

                    // create filter sequence
                    FiltersSequence filterSequence = new FiltersSequence();


                    // Inverting image to get white image on black background
                    filterSequence.Add(new Invert());
                    filterSequence.Add(new SISThreshold());
                    // Finding skeleton
                    filterSequence.Add(new CannyEdgeDetector(0, 70));
                    //clean image from scratches
                    short[,] se = new short[,] {
                                                { -1, -1, -1 },
                                                {  0,  1,  0 },
                                                { -1, -1, -1 }};

                    //filterSequence.Add( new HitAndMiss(se, HitAndMiss.Modes.Thinning));
                    //filterSequence.Add(new Median( ));
                    //filterSequence.Add(new Dilatation()); 
                    filterSequence.Add(new Invert());
                    // apply the filter and rfeturn value
                    return filterSequence.Apply(GSampleImage);

                }

            }
        }

        Bitmap prcessImageContourCenterLine(string filename)
        {
            Merge filter = new Merge(processImageCenterline(filename));
            Bitmap resultImage = filter.Apply(processImageContour(filename));
            Invert filterInvert = new Invert();
            filterInvert.ApplyInPlace(resultImage);
            return resultImage;
        }
        Bitmap processAll(string filename)
        {
            Bitmap SampleImage = (Bitmap)System.Drawing.Image.FromFile(filename);
            Bitmap rChannel = processImageCenterline(filename);
            Bitmap bChannel = processImageContour(filename);

            ReplaceChannel replaceRFilter = new ReplaceChannel(RGB.R, rChannel);
            ReplaceChannel replaceGFilter = new ReplaceChannel(RGB.G, rChannel);
            ReplaceChannel replaceBFilter = new ReplaceChannel(RGB.B, bChannel);

            replaceRFilter.ApplyInPlace(SampleImage);
            replaceGFilter.ApplyInPlace(SampleImage);

            replaceBFilter.ApplyInPlace(SampleImage);
            return SampleImage;
            //comment
        }

    }
}
