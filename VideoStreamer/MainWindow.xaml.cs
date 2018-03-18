using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Ozeki.Media.IPCamera;
using Ozeki.Media.MediaHandlers;
using Ozeki.Media.MediaHandlers.Video;
using Ozeki.Media.MJPEGStreaming;
using Ozeki.Media.Video.Controls;
using Ozeki.Media.Video;

namespace VideoStreamer
{
    public partial class MainWindow : Window
    {
        private IVideoSender sender;
        private MJPEGStreamer streamer;
        private BitmapSourceProvider provider;
        private IIPCamera ipCam;
        private WebCamera webCam;
        private VideoDeviceInfo info;
        private MediaConnector connector;
        private VideoViewerWPF viewer;

        public MainWindow()
        {
            InitializeComponent();
            var localIp = string.Empty;
            provider = new BitmapSourceProvider();
            connector = new MediaConnector();
            viewer = new VideoViewerWPF
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background = Brushes.Black
            };
            CameraBox.Children.Add(viewer);
            viewer.SetImageProvider(provider);
            foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ip.AddressFamily != AddressFamily.InterNetwork) continue;
                localIp = ip.ToString();
                break;
            }
            ipAddressText.Text = localIp;
        }

        private void Connect()
        {
            usbconnect.IsEnabled = false;
            usbdisconnect.IsEnabled = true;
            ipconnect.IsEnabled = false;
            ipdisconnect.IsEnabled = true;
            startstream.IsEnabled = true;
        }

        private void Disconnect()
        {
            usbconnect.IsEnabled = true;
            usbdisconnect.IsEnabled = false;
            ipconnect.IsEnabled = true;
            ipdisconnect.IsEnabled = false;
            stopstream.IsEnabled = false;            
        }

        //usb button function
        private void ClickUsb(object sender_1, RoutedEventArgs e)
        {
            webCam = WebCamera.GetDefaultDevice();
            if (webCam == null) return;
            connector.Connect(webCam, provider); 
            sender = webCam;
            webCam.Start();
            viewer.Start();
            Connect();
        }
        private void unClickUsb(object sender_1, RoutedEventArgs e)
        {
            viewer.Stop();
            webCam.Stop();
            webCam.Dispose();
            connector.Disconnect(webCam, provider);
            Disconnect();
        }

        //ip button function
        private void ClickIp(object sender_1, RoutedEventArgs e)
        {
            ipCam = IPCameraFactory.GetCamera(HostTextBox.Text, UserTextBox.Text, Password.Password);
            if (ipCam == null) return;
            connector.Connect(ipCam.VideoChannel, provider);
            sender = ipCam.VideoChannel;
            ipCam.Start();
            viewer.Start();
            Connect();
        }
        private void unClickIp(object sender_1, RoutedEventArgs e)
        {
            viewer.Stop();
            ipCam.Disconnect();
            ipCam.Dispose();
            connector.Disconnect(ipCam.VideoChannel, provider);
            Disconnect();
        }

        //stream button function
        private void Start_Streaming_Click(object sender_1, RoutedEventArgs e)
        {
         
            streamer = new MJPEGStreamer(ipAddressText.Text, int.Parse(PortText.Text));
            connector.Connect(sender, streamer.VideoChannel);
            streamer.ClientConnected += streamer_ClientConnected;
            streamer.ClientDisconnected += streamer_ClientDisconnected;
            streamer.Start();
            startstream.IsEnabled = false;
            stopstream.IsEnabled = true;
        }
        void streamer_ClientConnected(object sender_1, Ozeki.VoIP.VoIPEventArgs<IMJPEGStreamClient> e)
        {
            e.Item.StartStreaming();
        }

        void streamer_ClientDisconnected(object sender_1, Ozeki.VoIP.VoIPEventArgs<IMJPEGStreamClient> e)
        {
            e.Item.StopStreaming();
        }

        private void Stop_Streaming_Click(object sender_1, RoutedEventArgs e)
        {
            streamer.Stop();
            connector.Disconnect(sender, streamer.VideoChannel);
            startstream.IsEnabled = true;
            stopstream.IsEnabled = false;
        }
    }
}
