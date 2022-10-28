using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using C_Flat_Interpreter.Transpiler;
using Microsoft.Win32;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;

namespace C_Flat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : UiWindow
    {
        private readonly Transpiler _transpiler;
        private bool _programChanged;
        public MainWindow()
        {
            InitializeComponent();
            _transpiler = new();
        }

        private void ButtonTranspile_Click(object sender, RoutedEventArgs e)
        {
            _programChanged = true;
            _transpiler.Transpile(SourceInput.Text);
            TranspilerOutput.Text = $"> Transpiled input source. See the transpiled C# code in: {_transpiler.GetProgramPath()}";
            SourceInput.Clear();
        }
        
        private async void ButtonTranspileAndRun_Click(object sender, RoutedEventArgs e)
        {
            ExecutionOutput.Text = "Executing transpiled code!!";
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
            ExecutionOutput.Text = output;
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