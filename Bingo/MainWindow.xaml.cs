using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Bingo
{
    public class Animating
    {
        public int AnimatingQueueLength;

        private class Ticket
        {
            public bool InLine = true;
        }

        public Animating()
        {
            AnimatingQueueLength = 0;
        }

        private readonly Queue<Ticket> animatingQueue = new();

        public Task StartAnimating()
        {
            ++AnimatingQueueLength;
            return AnimatingQueueLength > 1 ? EnterQueue() : Task.CompletedTask;
        }

        private Task EnterQueue()
        {
            Ticket t = new();
            animatingQueue.Enqueue(t);
            return Task.Run(() =>
            {
                while (t.InLine)
                {
                    //Wait
                }
            });
        }

        public void FinishAnimating()
        {
            if (animatingQueue.Count > 0)
            {
                Ticket t = animatingQueue.Dequeue();
                t.InLine = false;
            }

            --AnimatingQueueLength;
        }
    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            ballObjectDefault = BallObject.Margin;
            ballNumberDefault = BallNumber.Margin;
            allButtons = new List<Button>
            {
                B1, B2, B3, B4, B5, B6, B7, B8, B9, B10, B11, B12, B13, B14, B15, B16, B17, B18, B19, B20, B21, B22,
                B23, B24, B25, B26, B27, B28, B29, B30, B31, B32, B33, B34, B35, B36, B37, B38, B39, B40, B41, B42, B43,
                B44, B45, B46, B47, B48, B49, B50, B51, B52, B53, B54, B55, B56, B57, B58, B59, B60, B61, B62, B63, B64,
                B65, B66, B67, B68, B69, B70, B71, B72, B73, B74, B75, BReset, BUndo, BWin
            };
            ResetBoard();
        }

        private const int VwWidth = 2200;
        private readonly Thickness ballObjectDefault;
        private readonly Thickness ballNumberDefault;
        private readonly Animating animating = new();
        private readonly List<Button> allButtons;
        private readonly Stack<Button> callStack = new();
        private readonly SolidColorBrush defaultColor = Brushes.AliceBlue;

        private async Task BallAnimation()
        {
            await StartAnimation();

            MoveBallOffScreen(out Thickness positionBall, out Thickness positionNumber);
            textRotation.Angle = 0;
            const int frames = 180;
            const int rotationDegreeses = 720;
            double v = (double)VwWidth / frames;
            const double rotationChange = (double)rotationDegreeses / frames;
            List<List<Thickness>> calculatedMargins = new();
            List<double> calculatedRotations = new();
            await Task.Run(() =>
            {
                double angle = 0;
                for (int n = 0; n < frames; ++n)
                {
                    BallModify(ref positionBall, ref positionNumber);
                    angle += rotationChange;
                    angle %= 360;
                    calculatedMargins.Add(new List<Thickness> { positionBall, positionNumber });
                    calculatedRotations.Add(angle);
                }
            });

            BallObject.Visibility = Visibility.Visible;
            BallNumber.Visibility = Visibility.Visible;

            for (int n = 0; n < frames; ++n)
            {
                Task p = Pause(1);
                BallObject.Margin = calculatedMargins[n][0];
                BallNumber.Margin = calculatedMargins[n][1];
                textRotation.Angle = calculatedRotations[n];
                await p;
                lock (animating)
                {
                    if (animating.AnimatingQueueLength > 1)
                    {
                        break;
                    }
                }
            }

            BallObject.Margin = ballObjectDefault;
            BallNumber.Margin = ballNumberDefault;
            textRotation.Angle = 0;

            EndAnimation();

            void BallModify(ref Thickness positionBalla, ref Thickness positionNumbera)
            {
                positionBalla.Left += v;
                positionBalla.Right -= v;
                positionNumbera.Left += v;
                positionNumbera.Right -= v;
            }
        }

        private void EndAnimation()
        {
            lock (animating)
            {
                animating.FinishAnimating();
            }
        }

        private async Task StartAnimation()
        {
            Task ticket;

            lock (animating)
            {
                ticket = animating.StartAnimating();
            }

            await ticket;
        }

        private void MoveBallOffScreen(out Thickness positionBall, out Thickness positionNumber)
        {
            positionBall = ballObjectDefault;
            positionNumber = ballNumberDefault;
            positionBall.Left -= VwWidth;
            positionBall.Right += VwWidth;
            positionNumber.Left -= VwWidth;
            positionNumber.Right += VwWidth;
            BallObject.Margin = positionBall;
            BallNumber.Margin = positionNumber;
            BallObject.Visibility = Visibility.Hidden;
            BallNumber.Visibility = Visibility.Hidden;
        }

        private async Task Pause(double frames)
        {
            const int framesPerSecond = 120;
            await Task.Run(() => Thread.Sleep((int)(1000 * frames / framesPerSecond)));
        }

        private async void BNum_Clicked(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button caller))
            {
                MessageBox.Show(new NullReferenceException().Message);
                return;
            }

            if (caller.Background == Brushes.Yellow)
            {
                caller.Background = defaultColor;
            }
            else
            {
                BallNumber.Text = caller.Content.ToString();
                caller.Background = Brushes.Yellow;
                if (animations)
                {
                    await BallAnimation();
                }
            }

            callStack.Push(caller);
            handled = true;
        }

        private async void ResetBoard()
        {
            await StartAnimation();

            foreach (Button b in allButtons)
            {
                b.Background = defaultColor;
                b.FontSize = 40;
                b.FontWeight = FontWeights.Bold;
            }


            TextWin.Text = "";
            TextWin.Visibility = Visibility.Hidden;
            EndAnimation();
        }

        private async void B0_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
            {
                MessageBox.Show(new NullReferenceException().Message);
            }
            else switch (button.Content.ToString())
            {
                case "Sure?":
                    ResetBoard();
                    MoveBallOffScreen(out _, out _);
                    callStack.Clear();
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

        bool animations = true;
        private bool? handled;

        private async void BUndo_Click(object sender, RoutedEventArgs e)
        {
            if (callStack.Count == 0)
            {
                return;
            }

            animations = false;
            Button buttonToCall = callStack.Pop();
            handled = false;

            buttonToCall.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

            await Task.Run(() =>
            {
                while (!handled.Value)
                {
                }
            });

            handled = null;

            //BNum_Clicked(ButtonToCall, e);
            animations = true;

            _ = callStack.Pop();
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
                char[] winner = $"WE HAVE A{Environment.NewLine}WINNER!".ToCharArray();
                if (animations)
                {
                    foreach (char a in winner)
                    {
                        lock (animating)
                        {
                            if (animating.AnimatingQueueLength != 1)
                            {
                                break;
                            }
                        }
                        TextWin.Text += a;
                        await Pause(10);
                    }
                }

                TextWin.Text = new string(winner);
            }
            else
            {
                TextWin.Text = "";
                TextWin.Visibility = Visibility.Hidden;
            }

            EndAnimation();

            callStack.Push((sender as Button)!);
            BWin.IsEnabled = true;
            handled = true;
        }
    }
}
