using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;

namespace KinectSketalDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensorChooser _kinect = new KinectSensorChooser();
        private bool closing = false;
        private const int skeletonCount = 6;
        Skeleton[] allSkeletons = new Skeleton[skeletonCount];

        public MainWindow()
        {
            InitializeComponent();
            this.kinectUI.KinectSensorChooser = _kinect;
            _kinect.Start();
        }

        void Kinect_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            _kinect.Kinect.SkeletonStream.Enable();
            Skeleton skel = GetFirstSkeleton(e);
            if (skel == null)
            {
                return;
            }
            GetCameraPoint(skel, e);
        }

        private void GetCameraPoint(Skeleton skel, AllFramesReadyEventArgs e)
        {
            using (DepthImageFrame depth = e.OpenDepthImageFrame())
            {
                if (depth == null || _kinect.Kinect == null)
                {
                    return;
                }
                DepthImagePoint headDepthPoint = _kinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(GetSkeletonPoint(skel, JointType.Head), DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint leftDepthPoint = _kinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(GetSkeletonPoint(skel, JointType.HandLeft), DepthImageFormat.Resolution320x240Fps30);
                DepthImagePoint rightDepthPoint = _kinect.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(GetSkeletonPoint(skel, JointType.HandRight), DepthImageFormat.Resolution320x240Fps30);

                ColorImagePoint headColorPoint = _kinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, headDepthPoint, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint leftColorPoint = _kinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, leftDepthPoint, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint rightColorPoint = _kinect.Kinect.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, rightDepthPoint, ColorImageFormat.RgbResolution640x480Fps30);
                CameraPosition(Head, headColorPoint);
                CameraPosition(Left, leftColorPoint);
                CameraPosition(Right, rightColorPoint);
            }
        }

        private void CameraPosition(FrameworkElement element, ColorImagePoint point)
        {
            Canvas.SetLeft(element, point.X - element.Width / 2);
            Canvas.SetTop(element, point.Y - element.Height / 2);
        }

        private SkeletonPoint GetSkeletonPoint(Skeleton skel, JointType joint)
        {
            SkeletonPoint point = new SkeletonPoint();
            point.X = skel.Joints[joint].Position.X;
            point.Y = skel.Joints[joint].Position.Y;
            point.Z = skel.Joints[joint].Position.Z;
            return point;
        }

        private Skeleton GetFirstSkeleton(AllFramesReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame())
            {
                if (skeletonFrameData == null)
                {
                    return null;
                }
                skeletonFrameData.CopySkeletonDataTo(allSkeletons);
                Skeleton first = allSkeletons.Where<Skeleton>(skeleton => skeleton.TrackingState == SkeletonTrackingState.Tracked).FirstOrDefault();
                return first;
            }
        }

        private void Playground_Loaded(object sender, RoutedEventArgs e)
        {
            _kinect.Kinect.AllFramesReady += Kinect_AllFramesReady;
        }
    }
}
