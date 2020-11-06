using Microsoft.Ink;
using SouGouTest;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;

namespace Commons
{
    /// <summary>
    /// 识别核心类
    /// </summary>
    public class RecognizationCore : INotifyPropertyChanged
    {

        #region Alternates

        private IEnumerable<string> _alternates;

        /// <summary>
        /// Get 候选词列表
        /// </summary>
        public IEnumerable<string> Alternates
        {
            get { return _alternates; }
            private set
            {
                if (_alternates != value)
                {
                    _alternates = value;
                    NotifyPropertyChanged("Alternates");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// 属性变化时触发
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        /// <summary>
        /// 单字识别 以及 多文字识别...
        /// </summary>
        /// <param name="canvas">画板对象</param>
        /// <param name="count">识别字的最大个数</param>
        /// <param name="action">委托，可执行你需要的界面操作</param>
        /// <returns></returns>
        public void Recognizer(InkCanvas canvas, int count,Action action =null)
        {
            if (canvas.Strokes.Count < 1)
                return;

            RecognizerContext recognizerContext = new Recognizers().GetDefaultRecognizer().CreateRecognizerContext();
            Ink ink = new Ink();
            recognizerContext.Strokes = ink.CreateStrokes();

            //---------------单字识别，效果更佳------------------------
            var points = new List<Point>();
            for (int i = 0; i < canvas.Strokes.Count; i++)
            {
                foreach (var ss in canvas.Strokes[i].StylusPoints)
                {
                    points.Add(new Point((int)ss.X, (int)ss.Y));
                }
            }
            recognizerContext.Strokes.Add(ink.CreateStroke(points.ToArray()));

            ////------------------多写识别，可能出现将一个字分为两个字-----------------
            //for (int i = 0; i < canvas.Strokes.Count; i++)
            //{
            //    var stroke = new List<Point>();
            //    foreach (var ss in canvas.Strokes[i].StylusPoints)
            //    {
            //        stroke.Add(new Point((int)ss.X, (int)ss.Y));
            //    }

            //    if (stroke.Count > 0) 
            //    {
            //        recognizerContext.Strokes.Add(ink.CreateStroke(stroke.ToArray()));
            //    }                 
            //}
            //if (recognizerContext.Strokes.Count > 40)    //笔画过多容易导致内存爆炸，超过40笔画就不识别，
            //{
            //    canvas.Strokes.Clear();
            //    return;
            //}          


            if (canvas.Strokes.Count < 1)   //防止清除后识别为空报错
                return;

            List<string> chars = new List<string>();
            RecognitionStatus recognitionStatus = RecognitionStatus.NoError;
            RecognitionResult recognitionResult = recognizerContext.Recognize(out recognitionStatus);
            if (recognitionStatus == RecognitionStatus.NoError)
            {
                RecognitionAlternates alts = recognitionResult.GetAlternatesFromSelection();
                for (int i = 0; i < alts.Count && i < count; i++)
                {
                    chars.Add(alts[i].ToString());
                }
            }

            action?.Invoke();

            this.Alternates = chars;

        }

        /// <summary>
        /// 清空候选词
        /// </summary>
        public void ClearAlternates()
        {
            this.Alternates = Constants.EmptyAlternates;
        }

        /// <summary>
        /// 判断是否为数字或字母
        /// </summary>
        /// <param name="word">要判断的字符串</param>
        /// <returns>结果</returns>
        public static bool IsDigitOrLetter(string word)
        {
            bool bResult = false;

            string pattern = @"^[a-zA-Z0-9]*$"; //匹配所有字符都在字母和数字之间

            if (System.Text.RegularExpressions.Regex.IsMatch(word, pattern))
            {
                bResult = true;
            }
            else
            {
                bResult = false;
            }

            return bResult;
        }

        #region 引用win32api方法
        /// <summary>
        /// 导入模拟键盘的方法
        /// </summary>
        /// <param name="bVk" >按键的虚拟键值</param>
        /// <param name= "bScan" >扫描码，一般不用设置，用0代替就行</param>
        /// <param name= "dwFlags" >选项标志：0：表示按下，2：表示松开</param>
        /// <param name= "dwExtraInfo">一般设置为0</param>
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
        #endregion

        public ICommand btnPYKey
        {
            get
            {
                return new RelayCommand((i) =>
                {
                    byte bb = Convert.ToByte(i);
                    keybd_event(bb, 0, 0, 0);
                    keybd_event(bb, 0, 2, 0);
                });
            }
        }
    }

    /// <summary>
    /// 手写识别器接口
    /// </summary>
    public interface ICharactorRecognizer
    {
        /// <summary>
        /// Get 识别器名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 识别
        /// </summary>
        /// <param name="strokes">笔迹集合</param>
        /// <returns>候选词数组</returns>
        string[] Recognize(StrokeCollection strokes);
    }

    /// <summary>
    /// 常量类
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// 空候选词数组
        /// </summary>
        public static readonly string[] EmptyAlternates = new string[0];

        /// <summary>
        /// 简体中文ID，参考：https://docs.microsoft.com/en-us/windows/desktop/intl/language-identifier-constants-and-strings
        /// </summary>
        public static readonly int ChsLanguageId = 0x0804;
    }
}
