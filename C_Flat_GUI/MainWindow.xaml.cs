using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using C_Flat_Interpreter.Lexer;
using C_Flat_Interpreter.Parser;
using C_Flat_Interpreter.Transpiler;
using Microsoft.Win32;
using Serilog.Events;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;
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
        public MainWindow()
        {
            InitializeComponent();
            _lexer = new();
            _parser = new();
            _transpiler = new();
        }

        private void ButtonTranspile_Click(object sender, RoutedEventArgs e)
        {
            OutputBlock.Text = "";
            SourceInput.BorderThickness = new Thickness(0);
            OutputBorder.BorderThickness = new Thickness(0);

            _programChanged = true;
            _lexer.ClearLogs();
            if (_lexer.Tokenise(SourceInput.Text) != 0)
            {
                //Lexer Failed!
                OutputBlock.Inlines.Clear();
                var run = new Run
                {
                    Text = "Parsing Failed! Printing logs: \n",
                    Background = Brushes.DarkRed,
                    Foreground = Brushes.White
                };
                OutputBlock.Inlines.Add(new Bold(run));
                foreach (var errorMessage in _parser.GetInMemoryLogs().Where(log => log.Level > LogEventLevel.Information))
                {
                    run.Background = errorMessage.Level switch
                    {
                        LogEventLevel.Warning => new SolidColorBrush(Colors.Goldenrod),
                        _ => Brushes.DarkRed
                    };
                    run.Text = errorMessage.RenderMessage();
                    OutputBlock.Inlines.Add(run);
                }
                SourceInput.BorderBrush = new SolidColorBrush(Colors.DarkRed);
                SourceInput.BorderThickness = new Thickness(2);
                ExecuteButton.IsEnabled = false;
                return;
            }
            var tokens = _lexer.GetTokens();
            _parser.ClearLogs();
            if (_parser.Parse(tokens) != 0)
            {
                //Parser failed!
                OutputBlock.Inlines.Clear();
                var run = new Run
                {
                    Text = "Parsing Failed! Printing logs: \n",
                    Background = Brushes.DarkRed,
                    Foreground = Brushes.White
                };
                OutputBlock.Inlines.Add(new Bold(run));
                foreach (var errorMessage in _parser.GetInMemoryLogs().Where(log => log.Level > LogEventLevel.Information))
                {
                    run.Background = errorMessage.Level switch
                    {
                        LogEventLevel.Warning => new SolidColorBrush(Colors.Goldenrod),
                        _ => Brushes.DarkRed
                    };
                    run.Text = errorMessage.RenderMessage();
                    OutputBlock.Inlines.Add(run);
                }
                SourceInput.BorderBrush = new SolidColorBrush(Colors.DarkRed);
                SourceInput.BorderThickness = new Thickness(2);
                ExecuteButton.IsEnabled = false;
                return;
            }
            var transpiledProgram = _transpiler.Transpile(tokens);
            SourceInput.BorderBrush = new SolidColorBrush(Colors.LawnGreen);
            SourceInput.BorderThickness = new Thickness(2);
            ExecuteButton.IsEnabled = true;
            OutputBlock.Text = $"{transpiledProgram}";
        }
        
        private async void ButtonExecuteCode_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            OutputBlock.Text = "Executing transpiled code!!";
            OutputBorder.BorderBrush = new SolidColorBrush(Colors.MediumPurple);
            OutputBorder.BorderThickness = new Thickness(2);
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    Verb = "runas",
                    FileName = "cmd.exe",
                    Arguments = "/C "+ "dotnet run Program.cs",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = @"../../../../C_Flat_Output/"
                }
            };
            proc.Start();
            var output = await proc.StandardOutput.ReadToEndAsync();
            await proc.WaitForExitAsync();
            Mouse.OverrideCursor = Cursors.Arrow;
            OutputBlock.Text = output;
            OutputBorder.BorderBrush = new SolidColorBrush(Colors.LawnGreen);
            OutputBorder.BorderThickness = new Thickness(2);

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