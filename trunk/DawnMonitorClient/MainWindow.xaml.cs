using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
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
using System.Windows.Threading;
using SharedConstants;

namespace DawnMonitorClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DawnClient.DawnClient _dawnClient = new DawnClient.DawnClient();
        private DispatcherTimer _dispatcherTimer;


        public MainWindow()
        {
            InitializeComponent();

            if (!_dawnClient.Connect())
            {
                throw new ServerException("Dawn server not found");
            }

            DispatcherTimerSetup();
        }

        private void Update()
        {
            _dawnClient.Update();

            EntityTable.ItemsSource = _dawnClient.DawnWorld.GetEntities();
        }

        public void DispatcherTimerSetup()
        {
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += dispatcherTimer_Tick;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            _dispatcherTimer.Start();
        }

        void dispatcherTimer_Tick(object sender, object e)
        {
            Update();
        }
    }
}
