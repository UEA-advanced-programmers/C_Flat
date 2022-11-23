using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using C_Flat_Interpreter.Lexer;
using C_Flat_Interpreter.Parser;
using C_Flat_Interpreter.Transpiler;
using Microsoft.Win32;
using Serilog.Events;
using Wpf.Ui.Controls;
using Button = Wpf.Ui.Controls.Button;
using MessageBox = System.Windows.MessageBox;
using TreeViewItem = System.Windows.Controls.TreeViewItem;

namespace C_Flat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : UiWindow
    {
        private readonly Lexer _lexer;
        private readonly Parser _parser;
        private readonly Transpiler _transpiler;
        private bool _programChanged;
        private LinearGradientBrush _executionBrush;
        private Storyboard _executionAnim;

        private TreeView? _parseTree;
        private Button _ShowTree;
        private TextBlock _CodeView;
        private Button _ShowCode;
        private TextBlock? _ExecutionOutput;
        private Button _ShowOutput;

        public MainWindow()
        {
            InitializeComponent();
            _lexer = new();
            _parser = new();
            _transpiler = new();
            CreateExecuteAnimation();

            ExecuteButton.IsEnabled = false;
            
            _ShowTree = new Button()
            {
                Content = "Show parse tree",
                Margin = new Thickness(5),
                IsEnabled = false,
                Visibility = Visibility.Visible,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Background = new SolidColorBrush(Color.FromRgb(106, 27, 154)),
            };
            _ShowTree.Click += ShowTree_Click;
            
            _ShowOutput = new Button()
            {
                Content = "Show execution output",
                Margin = new Thickness(5),
                IsEnabled = false,
                Visibility = Visibility.Visible,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Background = new SolidColorBrush(Color.FromRgb(106, 27, 154)),
            };
            _ShowOutput.Click += ShowOutput_Click;
            
            _ShowCode = new Button()
            {
                Content = "Show transpiled code",
                Margin = new Thickness(5),
                IsEnabled = true,
                Visibility = Visibility.Visible,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Background = new SolidColorBrush(Color.FromRgb(106, 27, 154)),
            };
            _ShowCode.Click += ShowCode_Click;
            
            LeftButton.Content = _ShowTree;
            RightButton.Content = _ShowOutput;

            _CodeView = new TextBlock()
            {
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Stretch,
                TextWrapping = TextWrapping.NoWrap,
                IsEnabled = false,
            };
            OutputBorder.Child = _CodeView;
        }

        private void ButtonTranspile_Click(object sender, RoutedEventArgs e)
        {
            _CodeView.Text = "";
            SourceInput.BorderThickness = new Thickness(0);
            OutputBorder.BorderThickness = new Thickness(0);
            ExecuteButton.IsEnabled = false;
            _ExecutionOutput = null;
            _parseTree = null;
            _programChanged = true;
            _lexer.ClearLogs();
            if (_lexer.Tokenise(SourceInput.Text) != 0)
            {
                //Lexer Failed!
                _CodeView.Inlines.Clear();
                var run = new Run
                {
                    Text = "Parsing Failed! Printing logs: \n",
                    Background = Brushes.DarkRed,
                    Foreground = Brushes.White
                };
                _CodeView.Inlines.Add(new Bold(run));
                foreach (var errorMessage in _parser.GetInMemoryLogs()
                             .Where(log => log.Level > LogEventLevel.Information))
                {
                    run.Background = errorMessage.Level switch
                    {
                        LogEventLevel.Warning => new SolidColorBrush(Colors.Goldenrod),
                        _ => Brushes.DarkRed
                    };
                    run.Text = errorMessage.RenderMessage();
                    _CodeView.Inlines.Add(run);
                }
                SourceInput.BorderBrush = new SolidColorBrush(Colors.DarkRed);
                SourceInput.BorderThickness = new Thickness(2);
                _ShowTree.IsEnabled = false;
                return;
            }

            var tokens = _lexer.GetTokens();
            _parser.ClearLogs();
            
            if (_parser.Parse(tokens) != 0)
            {
                //Parser failed!
                _CodeView.Inlines.Clear();
                var run = new Run
                {
                    Text = "Parsing Failed! Printing logs: \n",
                    Background = Brushes.DarkRed,
                    Foreground = Brushes.White
                };
                _CodeView.Inlines.Add(new Bold(run));
                foreach (var errorMessage in _parser.GetInMemoryLogs()
                             .Where(log => log.Level > LogEventLevel.Information))
                {
                    run.Background = errorMessage.Level switch
                    {
                        LogEventLevel.Warning => new SolidColorBrush(Colors.Goldenrod),
                        _ => Brushes.DarkRed
                    };
                    run.Text = errorMessage.RenderMessage() + "\n";
                    _CodeView.Inlines.Add(run);
                }
                
                SourceInput.BorderBrush = new SolidColorBrush(Colors.DarkRed);
                SourceInput.BorderThickness = new Thickness(2);
                _ShowTree.IsEnabled = false;
                return;
            }

            //Construct parse tree element passing parse tree
            ConstructParseTree();
            var transpiledProgram = _transpiler.Transpile(tokens);
            SourceInput.BorderBrush = new SolidColorBrush(Colors.LawnGreen);
            SourceInput.BorderThickness = new Thickness(2);
            ExecuteButton.IsEnabled = true;
            _CodeView.Text = $"{transpiledProgram}";
            ShowCode_Click(default, default);
        }

        private async void ButtonExecuteCode_Click(object sender, RoutedEventArgs e)
        {
            
            TranspileButton.IsEnabled = false;
            ExecuteButton.IsEnabled = false;
            _ExecutionOutput = new TextBlock
            {
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Stretch,
                TextWrapping = TextWrapping.NoWrap,
                IsEnabled = false,
                Text = "Execution in progress..."
            };
            ShowOutput_Click(default, default);
            //_ShowOutput.IsEnabled = true;
            //Begin execution textbox animation
            Mouse.OverrideCursor = Cursors.Wait;
            OutputBorder.BorderBrush = _executionBrush;
            OutputBorder.BorderThickness = new Thickness(3);
            _executionAnim.Begin(this);

            //Create and start dotnet run process
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    Verb = "runas",
                    FileName = "cmd.exe",
                    Arguments = "/C " + "dotnet run Program.cs",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = @"../../../../C_Flat_Output/"
                }
            };
            proc.Start();

            //store process output logs
            var output = await proc.StandardOutput.ReadToEndAsync();
            await proc.WaitForExitAsync();

            //End execution animation
            _executionAnim.Stop(this);
            Mouse.OverrideCursor = Cursors.Arrow;

            //Check for errors and format stored output logs
            if (proc.ExitCode != 0)
            {
                _ExecutionOutput.Inlines.Clear();
                OutputBorder.BorderBrush = new SolidColorBrush(Colors.DarkRed);
                OutputBorder.BorderThickness = new Thickness(2);
                var errorLogs = output.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
                foreach (var error in errorLogs)
                {
                    var start = error.IndexOf("Program.cs", 0);
                    var end = error.LastIndexOf('[');
                    var trimmedError = error.Substring(start, end - start);
                    _ExecutionOutput.Inlines.Add(new Run()
                    {
                        Text = trimmedError + "\n",
                        Background = Brushes.DarkRed,
                    });
                }
                ExecuteButton.IsEnabled = false;
                return;
            }
            //Otherwise just copy output directly to text-box and apply a green "success border"
            _ExecutionOutput.Text = output;
            OutputBorder.BorderBrush = new SolidColorBrush(Colors.LawnGreen);
            OutputBorder.BorderThickness = new Thickness(2);
            TranspileButton.IsEnabled = true;
            ExecuteButton.IsEnabled = true;
        }

        private void ConstructParseTree()
        {
            _parseTree = new TreeView()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                Name = "ParseTree",
            };
            //TEST CODE, REPLACE WITH PARSER OUTPUT
            var testItem = new TreeViewItem()
            {
                Header = "Function declaration",
                Items =
                {
                    new TreeViewItem() {Header = "If statement",}
                }
            };
            var testItem2 = new TreeViewItem()
            {
                Header = "Variable declaration",
            };
            _parseTree.Items.Add(testItem);
            _parseTree.Items.Add(testItem2);

        }

        private async void ShowTree_Click(object sender, RoutedEventArgs e)
        {
            _ShowOutput.IsEnabled = _ExecutionOutput != null;
            LeftButton.Content = _ShowOutput;
            RightButton.Content = _ShowCode;
            OutputBorder.Child = _parseTree;
        }

        private async void ShowOutput_Click(object sender, RoutedEventArgs e)
        {
            LeftButton.Content = _ShowCode;
            RightButton.Content = _ShowTree;
            OutputBorder.Child = _ExecutionOutput;
        }

        private async void ShowCode_Click(object sender, RoutedEventArgs e)
        {
            _ShowTree.IsEnabled = _parseTree != null;
            LeftButton.Content = _ShowTree;
            _ShowOutput.IsEnabled = _ExecutionOutput != null;
            RightButton.Content = _ShowOutput;
            OutputBorder.Child = _CodeView;
        }

        private void CreateExecuteAnimation()
        {
            //Create animation for execute
            _executionBrush = new LinearGradientBrush();
            
            // Create gradient stops for the brush.
            var stop1 = new GradientStop(Colors.Plum, 0.0);
            var stop2 = new GradientStop(Colors.MediumPurple, 0.5);
            var stop3 = new GradientStop(Colors.Purple, 1.0);
            
            RegisterName("Execute1", stop1);
            RegisterName("Execute2", stop2);
            RegisterName("Execute3", stop3);
            
            _executionBrush.GradientStops.Add(stop1);
            _executionBrush.GradientStops.Add(stop2);
            _executionBrush.GradientStops.Add(stop3);

            var stop1Anim = new ColorAnimation
            {
                From = Colors.Plum,
                To = Colors.MediumPurple,
                Duration = TimeSpan.FromSeconds(0.5),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };
            Storyboard.SetTargetName(stop1Anim, "Execute1");
            Storyboard.SetTargetProperty(stop1Anim,
                new PropertyPath(GradientStop.ColorProperty));

            var stop2Anim = new ColorAnimation
            {
                From = Colors.MediumPurple,
                To = Colors.Purple,
                Duration = TimeSpan.FromSeconds(1.0),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };
            Storyboard.SetTargetName(stop2Anim, "Execute2");
            Storyboard.SetTargetProperty(stop2Anim,
                new PropertyPath(GradientStop.ColorProperty));
            
            var stop3Anim = new ColorAnimation
            {
                From = Colors.Purple,
                To = Colors.Plum,
                Duration = TimeSpan.FromSeconds(1.0),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };
            Storyboard.SetTargetName(stop3Anim, "Execute3");
            Storyboard.SetTargetProperty(stop3Anim,
                new PropertyPath(GradientStop.ColorProperty));

            _executionAnim = new Storyboard();
            _executionAnim.Children.Add(stop1Anim);
            _executionAnim.Children.Add(stop2Anim);
            _executionAnim.Children.Add(stop3Anim);
        }
        
        private void OnWindowClose(object sender, CancelEventArgs e)
        {
            // If the user has transpiled something ask if they want to save
            if (_programChanged)
            {
                var result = 
                    MessageBox.Show(
                        "Would you like to save your compiled program? \r(Yes will prompt you to choose a save location)", 
                        "Closing C_Flat Transpiler", 
                        MessageBoxButton.YesNo, 
                        MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    // User doesn't want to keep their compiled program
                    _transpiler.ResetOutput();
                }
                else
                {
                    // User wants to save their program elsewhere
                    SaveFileDialog saveFileDialog = new()
                    {
                        Filter = "C# file (*.cs)|*.cs|Text file (*.txt)|*.txt",
                        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    };
                    if(saveFileDialog.ShowDialog() == true)
                        File.Copy(_transpiler.GetProgramPath(), saveFileDialog.FileName);
                    _transpiler.ResetOutput();
                }
            }
        }
    }
}