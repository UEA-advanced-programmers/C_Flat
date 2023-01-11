using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using C_Flat_Interpreter.Common;
using C_Flat_Interpreter.Common.Enums;
using C_Flat_Interpreter.Lexer;
using C_Flat_Interpreter.Parser;
using C_Flat_Interpreter.Transpiler;
using Microsoft.Win32;
using Serilog.Events;
using Wpf.Ui.Common;
using MessageBox = System.Windows.MessageBox;
using TreeViewItem = System.Windows.Controls.TreeViewItem;

namespace C_Flat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly Lexer _lexer;
        private readonly Parser _parser;
        private readonly Transpiler _transpiler;
        private bool _unsavedChanges;

        private LinearGradientBrush? _executionBrush;
        private Storyboard? _executionAnim;

        private readonly List<LogEvent> _currentLogs = new();
        private TreeView? _parseTree;
        private readonly TextBlock _codeView;
        private TextBlock? _executionOutput;

        public MainWindow()
        {
            _lexer = new();
            _parser = new();
            _transpiler = new();

            
            // Create component for viewing code output
            _codeView = new TextBlock()
            {
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Stretch,
                TextWrapping = TextWrapping.NoWrap,
                IsEnabled = false,
            };
            
            InitializeComponent();
            
            //  Disable buttons to begin with
            ExecuteButton.IsEnabled = false;
            SaveOutput.IsEnabled = false;

            
            //  Create execution animation
            CreateExecuteAnimation();

            //  Initialize output window to show code output
            OutputBorder.Child = _codeView;
            ExpandAll.Visibility = Visibility.Collapsed;

            //  Setup handling for line numbers
            SourceInput.TextChanged += UpdateLineNumbers;
            LineNumbers.FontSize = SourceInput.FontSize;


        }

        private void UpdateLineNumbers(object sender, TextChangedEventArgs e)
        {
            //  Recreate line numbers if source line count changes
            if (LineNumbers.Inlines.Count != SourceInput.LineCount)
            {
                LineNumbers.Inlines.Clear();
                for (int i = 1; i <= SourceInput.LineCount; i++)
                {
                    LineNumbers.Inlines.Add(new Run(i.ToString() + "\n"));
                }
            }

        }
        private void ButtonTranspile_Click(object sender, RoutedEventArgs e)
        {
            //  Set code view text to blank and reset border thickness
            _codeView.Text = "";
            _codeView.HorizontalAlignment = HorizontalAlignment.Left;
            SourceInput.BorderThickness = new Thickness(0);
            OutputBorder.BorderThickness = new Thickness(0);

            //  Disable execute button whilst transpiling
            ExecuteButton.IsEnabled = false;

            //  Null all relevant variables
            _executionOutput = null;
            _parseTree = null;
            
            //  Clear stored logs
            _currentLogs.Clear();

            var success = _lexer.Tokenise(SourceInput.Text);
            _currentLogs.AddRange(_lexer.GetInMemoryLogs().ToList());
            if (success != 0)
            {
                // Lexer failed!
                FailTranspile("Lexing");
                return;
            }

            var tokens = _lexer.GetTokens();
            success = _parser.Parse(tokens);
            _currentLogs.AddRange(_parser.GetInMemoryLogs().ToList());
            if (success != 0)
            {
                //  Parser failed!
                FailTranspile("Parsing");
                return;
            }

            //Construct parse tree element
            var parseNodes = _parser.GetParseTree();
            ConstructParseTree(parseNodes);

            success = _transpiler.Transpile(parseNodes);
            _currentLogs.AddRange(_transpiler.GetInMemoryLogs().ToList());
            
            if (success != 0)
            {
                //  Transpilation failed!
                FailTranspile("Transpilation");
                return;
            }

            if (_transpiler.GetInMemoryLogs().Any(log => log.Level > LogEventLevel.Information))
            {
                Snackbar.Appearance = ControlAppearance.Caution;
                Snackbar.Show("Caution!", "Transpile succeeded but with warnings...\n See logs for details", SymbolRegular.CheckboxWarning20);
                SourceInput.BorderBrush = new SolidColorBrush(Colors.Goldenrod);
                SourceInput.BorderThickness = new Thickness(2);
            }
            else
            {
                Snackbar.Appearance = ControlAppearance.Success;
                Snackbar.Show("Success!", "Transpile was successful!", SymbolRegular.CheckboxChecked20);
                SourceInput.BorderBrush = new SolidColorBrush(Colors.LawnGreen);
                SourceInput.BorderThickness = new Thickness(2);
            }
            //  Mark new changes to the transpiled program.
            _unsavedChanges = true;

            var transpiledProgram = _transpiler.Program;
            SaveOutput.IsEnabled = true;
            ExecuteButton.IsEnabled = true;
            _codeView.Text = $"{transpiledProgram}";
            ViewSelector.SelectedItem = TranspileViewOption;
        }

        private void FailTranspile(string stage)
        {
            //  Transpile failed!
            // Clear output text and print logs
            _codeView.Inlines.Clear();
            _codeView.HorizontalAlignment = HorizontalAlignment.Center;
            _codeView.Inlines.Add(new Run()
            {
                Text = $"{stage} Failed! See logs for details... \n",
                FontWeight = FontWeights.Medium,
                TextDecorations = TextDecorations.Underline,
                FontSize = 24,
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
            });

            //  Set input border to red to indicate failure
            SourceInput.BorderBrush = new SolidColorBrush(Colors.DarkRed);
            SourceInput.BorderThickness = new Thickness(2);

            //  Disable parse tree view
            TreeViewOption.IsEnabled = false;
            //  Show a message indicating failed lexing
            Snackbar.Appearance = ControlAppearance.Danger;
            Snackbar.Show("Fail!", $"Transpile failed at {stage.ToLower()} stage!", SymbolRegular.ErrorCircle20);

            SaveOutput.IsEnabled = false;
            //  Show code view
            ViewSelector.SelectedItem = TranspileViewOption;
        }

        private async void ButtonExecuteCode_Click(object sender, RoutedEventArgs e)
        {
            //  Disable buttons
            TranspileButton.IsEnabled = false;
            ExecuteButton.IsEnabled = false;
            _executionOutput = new TextBlock
            {
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Stretch,
                TextWrapping = TextWrapping.NoWrap,
                IsEnabled = false,
                Text = "Execution in progress..."
            };
            
            //Begin execution text box animation
            Mouse.OverrideCursor = Cursors.Wait;
            OutputBorder.BorderBrush = _executionBrush;
            OutputBorder.BorderThickness = new Thickness(3);
            _executionAnim?.Begin(this);

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
            var output = new StringBuilder();
            proc.OutputDataReceived += (_, args) =>
            {
                if (args.Data != null)
                {
                    output.AppendLine(args.Data);
                }
            };
            proc.Start();
            proc.BeginOutputReadLine();
            //Start process and kill after 5 seconds
            try
            {
                var tokenSource = new CancellationTokenSource(5000);
                await proc.WaitForExitAsync(tokenSource.Token);
                var programOutput = output.ToString();
                //  If we get a non-zero exit code, print error logs
                if (proc.ExitCode != 0)
                {
                    _executionOutput.Inlines.Clear();
                    OutputBorder.BorderBrush = new SolidColorBrush(Colors.DarkRed);
                    OutputBorder.BorderThickness = new Thickness(2);
                    var errorLogs = programOutput.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
                    foreach (var error in errorLogs)
                    {
                        var start = error.IndexOf("Program.cs", 0, StringComparison.Ordinal);
                        var end = error.LastIndexOf('[');
                        var trimmedError = error.Substring(start, end - start);
                        _executionOutput.Inlines.Add(new Run()
                        {
                            Text = trimmedError + "\n",
                            Background = trimmedError.Contains("error") ? Brushes.DarkRed : Brushes.Goldenrod,
                        });
                    }

                    // Then disable buttons
                    ExecuteButton.IsEnabled = false;
                    TranspileButton.IsEnabled = true;
                    OutputBorder.BorderBrush = Brushes.DarkRed;
                    Snackbar.Appearance = ControlAppearance.Danger;
                    Snackbar.Show("Execution Failed!");
                }
                else
                {
                    //  Otherwise just copy output directly to text-box and apply a green "success border"
                    _executionOutput.Text = programOutput;
                    OutputBorder.BorderBrush = Brushes.LawnGreen;
                    OutputBorder.BorderThickness = new Thickness(2);
                    TranspileButton.IsEnabled = true;
                    ExecuteButton.IsEnabled = true;
                    Snackbar.Appearance = ControlAppearance.Success;
                    Snackbar.Show("Execution Successful!");
                }
            }
            catch (OperationCanceledException)
            {
                //  Task has been cancelled so kill process
                proc.Kill();
                Snackbar.Appearance = ControlAppearance.Danger;
                Snackbar.Show("Error!", $"Execution timed out", SymbolRegular.ErrorCircle20);
                _executionOutput.Text = output.ToString();
                ExecuteButton.IsEnabled = false;
                TranspileButton.IsEnabled = true;
                OutputBorder.BorderBrush = Brushes.DarkRed;
            }
            finally
            {
                //End execution animation
                _executionAnim?.Stop(this);
                ViewSelector.SelectedItem = ExecutionViewOption;
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }

        private void ConstructParseTree(List<ParseNode> parseNodes)
        {
            _parseTree = new TreeView()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                Name = "ParseTree",
            };
            _parseTree.PreviewMouseWheel += treeView_PreviewMouseWheel;
            foreach (var node in parseNodes)
            {
                var treeItem = new TreeViewItem
                {
                    Header = new TextBlock()
                    {
                        Inlines =
                        {
                            node.ToString()
                        }
                    }
                };
                if (node.type is NodeType.Terminal) continue;
                foreach (var childNode in node.GetChildren())
                {
                    AddNodeTreeItems(treeItem, childNode);
                }
                _parseTree.Items.Add(treeItem);
            }
            TreeViewOption.IsEnabled = true;
        }

        private void treeView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is TreeView && !e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent?.RaiseEvent(eventArg);
            }
        }

        private void AddNodeTreeItems(TreeViewItem parentTreeItem, ParseNode node)
        {
            var treeItem = new TreeViewItem
            {
                Header = node.ToString()
            };
            if (node.type is not NodeType.Terminal)
            {
                foreach (var childNode in node.GetChildren())
                {
                    AddNodeTreeItems(treeItem, childNode);
                }
            }
            parentTreeItem.Items.Add(treeItem);
        }

        private void ExpandAll_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = ViewSelector.SelectedItem as ComboBoxItem;
            switch (selectedItem?.Name)
            {
                case "TreeViewOption":
                    if (_parseTree != null && _parseTree.Items.Count > 0)
                    {
                        foreach (TreeViewItem treeItem in _parseTree.Items)
                        {
                            treeItem.ExpandSubtree();
                        }
                    }
                    break;
                case "LogsViewOption":
                    if (OutputBorder.Child is TreeView view)
                    {
                        foreach (TreeViewItem treeItem in view.Items)
                        {
                            treeItem.ExpandSubtree();
                        }
                    }
                    break;
            }
            
        }
        
        private void OnViewChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Snackbar == null)
                return;
            var selectedItem = ViewSelector.SelectedItem as ComboBoxItem;
            switch (selectedItem?.Name)
            {
                case "TranspileViewOption":
                    //Show transpile view
                    if (TreeViewOption != null && ExecutionViewOption != null)
                    {
                        TreeViewOption.IsEnabled = _parseTree != null;
                        ExecutionViewOption.IsEnabled = _executionOutput != null;
                    }
                    OutputBorder.Child = _codeView;
                    ExpandAll.Visibility = Visibility.Collapsed;
                    SaveOutput.Visibility = Visibility.Visible;
                    break;
                case "TreeViewOption":
                    //Show parse tree
                    if (ExecutionViewOption != null)
                    {
                        ExecutionViewOption.IsEnabled = _executionOutput != null;
                    }
                    OutputBorder.Child = _parseTree;
                    ExpandAll.Visibility = Visibility.Visible;
                    SaveOutput.Visibility = Visibility.Collapsed;
                    break;
                case "ExecutionViewOption":
                    //Show execution output
                    OutputBorder.Child = _executionOutput;
                    ExpandAll.Visibility = Visibility.Collapsed;
                    SaveOutput.Visibility = Visibility.Collapsed;
                    break;
                case "LogsViewOption":
                    //Show logs output
                    OutputBorder.Child = CreateLogsView();
                    ExpandAll.Visibility = Visibility.Visible;
                    SaveOutput.Visibility = Visibility.Collapsed;
                    break;
                default:
                    throw new Exception("Combobox item has invalid name");
            }
        }

        private TreeView CreateLogsView()
        {
            var logsTree = new TreeView();
            logsTree.PreviewMouseWheel += treeView_PreviewMouseWheel;
            foreach (var levelCategory in _currentLogs.Select(log => log.Level).Distinct().OrderByDescending(x => x))
            {
                var categorisedLogs = _currentLogs.Where(log => log.Level == levelCategory);
                var levelSubtree = new TreeViewItem()
                {
                    Header = levelCategory.ToString(),
                    Background = levelCategory switch
                    {
                        LogEventLevel.Warning => new SolidColorBrush(Colors.Goldenrod),
                        LogEventLevel.Error => Brushes.DarkRed,
                        _ => Brushes.Transparent,
                    },
                };
                foreach (var log in categorisedLogs)
                {
                    levelSubtree.Items.Add(new TreeViewItem()
                    {
                        Header = log.RenderMessage()
                    });
                }
                logsTree.Items.Add(levelSubtree);
            }

            return logsTree;
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
            if (_unsavedChanges)
            {
                var result =
                    MessageBox.Show(
                        "Would you like to save your compiled program? \r(Yes will prompt you to choose a save location)",
                        "Unsaved changes!",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    // User doesn't want to keep their compiled program
                    _transpiler.ResetOutput();
                }
                else
                {
                    SaveProgram();
                }
            }
        }
        private void SaveProgram()
        {
            // User wants to save their program elsewhere
            SaveFileDialog saveFileDialog = new()
            {
                Filter = "C# file (*.cs)|*.cs|Text file (*.txt)|*.txt",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                File.Copy(_transpiler.GetProgramPath(), saveFileDialog.FileName, true);
                _unsavedChanges = false;
            }
            _transpiler.ResetOutput();
        }

        private void SaveOutput_OnClick(object sender, RoutedEventArgs e)
        {
            if (_unsavedChanges)
                SaveProgram();
        }
    }
}