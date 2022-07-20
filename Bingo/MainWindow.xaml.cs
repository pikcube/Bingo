using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bingo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            BallObjectDefault = BallObject.Margin;
            BallNumberDefault = BallNumber.Margin;
            AllButtons = new List<Button>
            {
                B1, B2, B3, B4, B5, B6, B7, B8, B9, B10, B11, B12, B13, B14, B15, B16, B17, B18, B19, B20, B21, B22,
                B23, B24, B25, B26, B27, B28, B29, B30, B31, B32, B33, B34, B35, B36, B37, B38, B39, B40, B41, B42, B43,
                B44, B45, B46, B47, B48, B49, B50, B51, B52, B53, B54, B55, B56, B57, B58, B59, B60, B61, B62, B63, B64,
                B65, B66, B67, B68, B69, B70, B71, B72, B73, B74, B75, BReset, BUndo, BWin
            };
            ResetBoard();
        }

        private const int VWWidth = 2200;
        private readonly Thickness BallObjectDefault;
        private readonly Thickness BallNumberDefault;
        private int Animating;
        private readonly List<Button> AllButtons;
        private readonly Stack<Button> CallStack = new Stack<Button>();
        private readonly SolidColorBrush DefaultColor = Brushes.AliceBlue;
        
        private async Task BallAnimation()
        {
            await StartAnimation();

            MoveBallOffScreen(out Thickness PositionBall, out Thickness PositionNumber);
            textRotation.Angle = 0;
            const int Frames = 180;
            const int RotationDegreeses = 720;
            double v = (double)VWWidth / Frames;
            const double RotationChange = (double)RotationDegreeses / Frames;
            List<List<Thickness>> CalculatedMargins = new List<List<Thickness>>();
            List<double> CalculatedRotations = new List<double>();
            await Task.Run(() =>
            {
                double Angle = 0;
                for (int n = 0; n < Frames; ++n)
                {
                    BallModify(ref PositionBall, ref PositionNumber);
                    Angle += RotationChange;
                    Angle %= 360;
                    CalculatedMargins.Add(new List<Thickness> { PositionBall, PositionNumber });
                    CalculatedRotations.Add(Angle);
                }
            });

            BallObject.Visibility = Visibility.Visible;
            BallNumber.Visibility = Visibility.Visible;

            for (int n = 0; n < Frames && Animating == 1; ++n)
            {
                Task p = Pause(1);
                BallObject.Margin = CalculatedMargins[n][0];
                BallNumber.Margin = CalculatedMargins[n][1];
                textRotation.Angle = CalculatedRotations[n];
                await p;
            }

            BallObject.Margin = BallObjectDefault;
            BallNumber.Margin = BallNumberDefault;
            textRotation.Angle = 0;

            EndAnimation();

            void BallModify(ref Thickness PositionBalla, ref Thickness PositionNumbera)
            {
                PositionBalla.Left += v;
                PositionBalla.Right -= v;
                PositionNumbera.Left += v;
                PositionNumbera.Right -= v;
            }
        }

        private void EndAnimation()
        {
            --Animating;
            if (Animating < 0)
            {
                Animating = 0;
            }
        }

        private async Task StartAnimation()
        {
            ++Animating;
            if (Animating > 2)
            {
                Animating = 2;
            }

            await Task.Run(() =>
            {
                while (Animating != 1)
                {
                }
            });
        }

        private void MoveBallOffScreen(out Thickness PositionBall, out Thickness PositionNumber)
        {
            PositionBall = BallObjectDefault;
            PositionNumber = BallNumberDefault;
            PositionBall.Left -= VWWidth;
            PositionBall.Right += VWWidth;
            PositionNumber.Left -= VWWidth;
            PositionNumber.Right += VWWidth;
            BallObject.Margin = PositionBall;
            BallNumber.Margin = PositionNumber;
            BallObject.Visibility = Visibility.Hidden;
            BallNumber.Visibility = Visibility.Hidden;
        }

        private async Task Pause(double Frames)
        {
            const int FramesPerSecond = 120;
            await Task.Run(() => Thread.Sleep((int) (1000 * Frames / FramesPerSecond)));
        }

        private async void BNum_Clicked(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button Caller))
            {
                MessageBox.Show(new NullReferenceException().Message);
                return;
            }

            if (Caller.Background == Brushes.Yellow)
            {
                Caller.Background = DefaultColor;
            }
            else
            {
                BallNumber.Text = Caller.Content.ToString();
                Caller.Background = Brushes.Yellow;
                if (Animations)
                {
                    await BallAnimation();
                }
            }

            CallStack.Push(Caller);
            Handled = true;
        }

        private async void ResetBoard()
        {
            await StartAnimation();

            foreach (Button B in AllButtons)
            {
                B.Background = DefaultColor;
                B.FontSize = 40;
                B.FontWeight = FontWeights.Bold;
            }


            TextWin.Text = "";
            TextWin.Visibility = Visibility.Hidden;
            EndAnimation();
        }

        private async void B0_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button))
            {
                MessageBox.Show(new NullReferenceException().Message);
            }
            else switch (button.Content.ToString())
            {
                case "Sure?":
                    ResetBoard();
                    MoveBallOffScreen(out _, out _);
                    CallStack.Clear();
                    button.Content = "Reset";
                    return;
                case "Reset":
                {
                    button.Content = "Sure?";
                    for (int n = 0; n < 3000; ++n)
                    {
                        if (button.Content.ToString() == "Reset")
                        {
                            return;
                        }
                        await Task.Run(() =>
                        {
                            Thread.Sleep(1);
                        });
                    }
                    button.Content = "Reset";
                    return;
                }
            }
        }

        bool Animations = true;
        private bool? Handled;

        private async void BUndo_Click(object sender, RoutedEventArgs e)
        {
            if (CallStack.Count == 0)
            {
                return;
            }

            Animations = false;
            Button ButtonToCall = CallStack.Pop();
            Handled = false;

            ButtonToCall.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

            await Task.Run(() =>
            {
                while (!Handled.Value)
                {
                }
            });

            Handled = null;

            //BNum_Clicked(ButtonToCall, e);
            Animations = true;

            _ = CallStack.Pop();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MoveBallOffScreen(out _, out _);
        }

        private async void BWin_Click(object sender, RoutedEventArgs e)
        {
            BWin.IsEnabled = false;
            await StartAnimation();

            if (TextWin.Text == "")
            {
                TextWin.Visibility = Visibility.Visible;
                char[] Winner = $"WE HAVE A{Environment.NewLine}WINNER!".ToCharArray();
                if (Animations)
                {
                    foreach (char a in Winner)
                    {
                        if (Animating != 1)
                        {
                            break;
                        }
                        TextWin.Text += a;
                        await Pause(10);
                    }
                }

                TextWin.Text = new string(Winner);
            }
            else
            {
                TextWin.Text = "";
                TextWin.Visibility = Visibility.Hidden;
            }

            EndAnimation();

            CallStack.Push(sender as Button);
            BWin.IsEnabled = true;
            Handled = true;
        }
    }
}
