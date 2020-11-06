using Commons;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SouGouTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly RecognizationCore _core;  //识别核心类

        public MainWindow()
        {
            InitializeComponent();
            _core = new RecognizationCore();
            this.DataContext = _core;
            TimerInit();
        }

        private void btnPYClearKey_Click(object sender, RoutedEventArgs e)
        {
            txt.Clear();
            txt.Focus();
        }

        private void Clear()
        {
            this.WritingCanvas.Strokes.Clear();
            _core.ClearAlternates();
            DisplayMessage();
        }
        private void DisplayMessage()
        {
            sxMessage.Visibility = Visibility.Visible;
            sxControl.Visibility = Visibility.Collapsed;
        }

        //切换手写
        private void btnSX_Click(object sender, RoutedEventArgs e)
        {
            Clear();
            gdKeyBord8.Visibility = Visibility.Visible;
            gdKeyBord5.Visibility = Visibility.Collapsed;
        }

        private void txt_GotFocus(object sender, RoutedEventArgs e)
        {
            gdKeyBord5.Visibility = Visibility.Visible;
            gdKeyBord8.Visibility = Visibility.Collapsed;
        }

        //切换拼音
        private void btnChangePY_Click(object sender, RoutedEventArgs e)
        {
            Clear();
            gdKeyBord8.Visibility = Visibility.Collapsed;
            gdKeyBord5.Visibility = Visibility.Visible;
            txt.Focus();
        }

        //手写清空
        private void ClearButtonOnClick(object sender, RoutedEventArgs e)
        {
            txt.Text = "";
            Clear();
        }

        private void SelectCharactorButtonOnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var text = button.Content as string;
            if (text != null)
            {
                txt.Text += text;
                Clear();
            }
        }

        #region 延时处理字体识别

        private DispatcherTimer timer;

        private DateTime lastTime = DateTime.Now;

        private void TimerInit()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += timer_Tick;
            lastTime = DateTime.Now;
        }

        private void DisPlayZT()
        {
            sxMessage.Visibility = Visibility.Collapsed;
            sxControl.Visibility = Visibility.Visible;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (DateDiff(DateTime.Now) > 500)
            {
                _core.Recognizer(WritingCanvas, 9, DisPlayZT);

                timer.Stop();
                Console.WriteLine("计时器关闭成功...............");
            }

        }
        private double DateDiff(DateTime nowTime)
        {
            TimeSpan ts1 = new TimeSpan(lastTime.Ticks);
            TimeSpan ts2 = new TimeSpan(nowTime.Ticks);
            TimeSpan ts = ts1.Subtract(ts2).Duration();

            return ts.TotalMilliseconds;
        }
        #endregion

        private void WritingCanvasOnStrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            lastTime = DateTime.Now;

            if (!timer.IsEnabled)
            {
                timer.Start();
                Console.WriteLine("定时器开启成功");
            }
        }
    }
}
