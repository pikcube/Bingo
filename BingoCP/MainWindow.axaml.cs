using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;

namespace BingoCP
{
    public partial class MainWindow : Window
    {
        public class Animating
        {
            public int AnimatingQueueLength;

            private class Ticket
            {
                public bool InLine = true;
            }

            private readonly Queue<Ticket> _animatingQueue = new();

            public Task StartAnimating()
            {
                ++AnimatingQueueLength;
                return AnimatingQueueLength > 1 ? EnterQueue() : Task.CompletedTask;
            }

            private Task EnterQueue()
            {
                Ticket t = new();
                _animatingQueue.Enqueue(t);
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
                if (_animatingQueue.Count > 0)
                {
                    Ticket t = _animatingQueue.Dequeue();
                    t.InLine = false;
                }

                --AnimatingQueueLength;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            BallObject.Source = new Bitmap("orange-circle-hi.png");
            _ballObjectDefault = BallObject.Margin;
            _ballNumberDefault = BallNumber.Margin;
            _allButtons =
            [
                B1, B2, B3, B4, B5, B6, B7, B8, B9, B10, B11, B12, B13, B14, B15, B16, B17, B18, B19, B20, B21, B22,
                B23, B24, B25, B26, B27, B28, B29, B30, B31, B32, B33, B34, B35, B36, B37, B38, B39, B40, B41, B42, B43,
                B44, B45, B46, B47, B48, B49, B50, B51, B52, B53, B54, B55, B56, B57, B58, B59, B60, B61, B62, B63, B64,
                B65, B66, B67, B68, B69, B70, B71, B72, B73, B74, B75, BReset, BUndo, BWin, BOpen
            ];
            
            _allButtons.ForEach(b =>
            {
                b.HorizontalAlignment = HorizontalAlignment.Stretch;
                b.HorizontalContentAlignment = HorizontalAlignment.Center;
                b.VerticalAlignment = VerticalAlignment.Stretch;
                b.VerticalContentAlignment = VerticalAlignment.Center;
            });
            
            _ = ResetBoard();
        }


        private const int VwWidth = 2200;
        private readonly Thickness _ballObjectDefault;
        private readonly Thickness _ballNumberDefault;
        private readonly Animating _animating = new();
        private readonly List<Button> _allButtons;
        private readonly Stack<Button> _callStack = new();
        private readonly ISolidColorBrush _defaultColor = Brushes.AliceBlue;
        private string _gamePath = "game1.bingolog";

        private async Task BallAnimation(string? ballNumberText = null)
        {
            await StartAnimation();

            MoveBallOffScreen(out Thickness positionBall, out Thickness positionNumber);

            if (ballNumberText is not null)
            {
                BallNumber.Text = ballNumberText;
            }

            //textRotation.Angle = 0;
            const int frames = 180;
            const int rotationDegreeses = 720;
            const double v = (double)VwWidth / frames;
            const double rotationChange = (double)rotationDegreeses / frames;
            List<List<ITransform>> calculatedMargins = [];

            double angle = 0;
            for (int n = 0; n < frames; ++n)
            {
                BallModify(ref positionBall, ref positionNumber);
                angle += rotationChange;
                angle %= 360;
                calculatedMargins.Add([
                    new TranslateTransform(positionBall.Left, positionBall.Bottom),
                    new TransformGroup()
                    {
                        Children =
                        [
                            new RotateTransform(angle),
                            new TranslateTransform(positionNumber.Left, positionNumber.Bottom)
                        ]
                    }
                ]);
            }

            BallObject.IsVisible = true;
            BallNumber.IsVisible = true;

            for (int n = 0; n < frames; ++n)
            {
                Task p = Pause(1);
                BallObject.RenderTransform = calculatedMargins[n][0];
                BallNumber.RenderTransform = calculatedMargins[n][1];

                await p;
                lock (_animating)
                {
                    if (_animating.AnimatingQueueLength > 1)
                    {
                        break;
                    }
                }
            }

            BallObject.RenderTransform = new TranslateTransform(_ballObjectDefault.Left, _ballObjectDefault.Bottom);
            BallNumber.RenderTransform = new TranslateTransform(_ballNumberDefault.Left, _ballNumberDefault.Bottom);
            BallNumber.RenderTransform = new RotateTransform(0);

            EndAnimation();

            static void BallModify(ref Thickness positionBalla, ref Thickness positionNumbera)
            {
                positionBalla = new Thickness(positionBalla.Left + v, positionBalla.Top, positionBalla.Right - v,
                    positionBalla.Bottom);
                positionNumbera = new Thickness(positionNumbera.Left + v, positionNumbera.Top,
                    positionNumbera.Right - v, positionNumbera.Bottom);
            }
        }

        private static readonly string[] ItemArray = ["*.bingolog"];
        private static readonly string[] FileExtensionList = [".bingolog"];

        private void EndAnimation()
        {
            lock (_animating)
            {
                _animating.FinishAnimating();
            }
        }

        private async Task StartAnimation()
        {
            Task ticket;

            lock (_animating)
            {
                ticket = _animating.StartAnimating();
            }

            await ticket;
        }

        private void MoveBallOffScreen(out Thickness positionBall, out Thickness positionNumber)
        {
            positionBall = _ballObjectDefault;
            positionNumber = _ballNumberDefault;
            positionBall = new Thickness(positionBall.Left - VwWidth, positionBall.Top, positionBall.Right + VwWidth,
                positionBall.Bottom);
            positionNumber = new Thickness(positionNumber.Left - VwWidth, positionNumber.Top,
                positionNumber.Right + VwWidth, positionNumber.Bottom);
            BallObject.RenderTransform = new TranslateTransform(positionBall.Left, positionBall.Bottom);
            BallNumber.RenderTransform = new TranslateTransform(positionNumber.Left, positionNumber.Bottom);
            BallObject.IsVisible = false;
            BallNumber.IsVisible = false;
        }

        private static async Task Pause(double frames)
        {
            const int framesPerSecond = 120;
            await Task.Run(() => Thread.Sleep((int)(1000 * frames / framesPerSecond)));
        }

        public async void BNum_Clicked(object? sender, RoutedEventArgs e)
        {
            if (sender is not Button caller)
            {
                return;
            }

            if (_gamePath != "game0.bingolog")
            {
                await File.AppendAllTextAsync(_gamePath, $"{caller.Content}{Environment.NewLine}");
            }

            if (Equals(caller.Background, Brushes.Yellow))
            {
                caller.Background = _defaultColor;
            }
            else
            {
                caller.Background = Brushes.Yellow;
                string ballNumberText = caller.Content?.ToString() ?? "";
                if (_animations)
                {
                    await BallAnimation(ballNumberText);
                }
                BallNumber.Text = ballNumberText;
            }

            _callStack.Push(caller);
            _handled = true;
        }

        private async Task ResetBoard(string? gamepath = null)
        {
            await StartAnimation();

            foreach (Button b in _allButtons)
            {
                b.Background = _defaultColor;
                b.FontSize = 40;
                b.FontWeight = FontWeight.Bold;
            }


            TextWin.Text = "";
            TextWin.IsVisible = false;
            EndAnimation();

            if (gamepath is not null)
            {
                this._gamePath = gamepath;
                return;
            }

            int n;

            for (n = 1; File.Exists($"game{n}.bingolog"); ++n)
            {
            }

            this._gamePath = $"game{n}.bingolog";
        }

        public async void BReset_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
            {
                return;
            }

            switch (button.Content?.ToString())
            {
                case "Sure?":
                    await ResetBoard();
                    MoveBallOffScreen(out _, out _);
                    _callStack.Clear();
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

                        await Task.Run(() => { Thread.Sleep(1); });
                    }

                    button.Content = "Reset";
                    return;
                }
            }
        }

        bool _animations = true;
        private bool? _handled;

        public async void BUndo_Click(object sender, RoutedEventArgs e)
        {
            if (_callStack.Count == 0)
            {
                return;
            }

            _animations = false;
            Button buttonToCall = _callStack.Pop();
            _handled = false;

            buttonToCall.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            await Task.Run(() =>
            {
                while (!_handled.Value)
                {
                }
            });

            _handled = null;

            //BNum_Clicked(ButtonToCall, e);
            _animations = true;

            _ = _callStack.Pop();
        }

        public void Window_Loaded(object? sender, EventArgs e)
        {
            MoveBallOffScreen(out _, out _);
        }

        public async void BWin_Click(object sender, RoutedEventArgs e)
        {
            BWin.IsEnabled = false;
            await StartAnimation();

            if (TextWin.Text == "")
            {
                TextWin.IsVisible = true;
                char[] winner = $"WE HAVE A{Environment.NewLine}WINNER!".ToCharArray();
                if (_animations)
                {
                    foreach (char a in winner)
                    {
                        lock (_animating)
                        {
                            if (_animating.AnimatingQueueLength != 1)
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
                TextWin.IsVisible = false;
            }

            EndAnimation();

            _callStack.Push((sender as Button)!);
            BWin.IsEnabled = true;
            _handled = true;
        }

        public async void BOpen_OnClick(object? sender, RoutedEventArgs e)
        {
            BOpen.IsEnabled = false;

            FilePickerOpenOptions options = new()
            {
                Title = "Select Game",
                AllowMultiple = false,
                SuggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(Directory.GetCurrentDirectory()),
                FileTypeFilter = new List<FilePickerFileType>()
                {
                    new("bingolog")
                    {
                        AppleUniformTypeIdentifiers = FileExtensionList,
                        Patterns = ItemArray,

                    }
                },
            };

            IReadOnlyList<IStorageFile> openFilePicker = await StorageProvider.OpenFilePickerAsync(options);

            string[] f = openFilePicker.Select(z => z.Path.AbsolutePath).ToArray();
            BOpen.IsEnabled = true;
            if (f.Length == 0)
            {
                return;
            }
            string[] actionStrings = await File.ReadAllLinesAsync(f.First());

            _gamePath = "game0.bingolog";

            await ResetBoard(_gamePath);

            _animations = false;

            foreach (string a in actionStrings)
            {
                Button? b = _allButtons.Find(z => z.Content?.ToString() == a);

                b?.RaiseEvent(e);
            }

            _animations = true;

            _gamePath = f.First();
        }
    }
}
