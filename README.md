# C_Flat
A c# interpreter/transpiler for our general purpose programming language with a WPF GUI

## Getting Started

### Clone github repository and open in your IDE of choice (I'm going with JetBrains Rider)

To clone the repository, you can run the command below from a shell window (e.g. cmd or git bash) to clone the repository.
If you don't have git installed you can follow this guide: https://git-scm.com/book/en/v2/Getting-Started-Installing-Git

```sh
git clone git@github.com:UEA-advanced-programmers/C_Flat.git
```

If you opt to use Github Desktop instead you can do so:
Go to the repository page, click 'Code' -> 'Open with GitHub desktop' -> clone

Once you've cloned the rpeository, you should be able to open the project in your IDE of choice.

##Solution Structure:
### Overall description
In this solution there are 3 projects: `C_Flat_GUI`, `C_Flat_Interpreter` and `C_Flat_Tests`. Each being used as their name suggests. This approach has the benefit of allowing for better parallel-programming resulting in less merge-conflicts. It also simplifies cross-project imports. As the 'GUI' will require access to the 'Interpreter' project, however will not need the 'Tests' project. Additionally, due to WPF only supporting Windows, it is better to store the majority of the non-GUI code in a regular `.Net6` project. Which allows for compilation and subsequent Continuous Integration, on other platforms such as Linux. (Which is used by GitHub Actions for automated testing).  

### GUI Project
`C_Flat_GUI` is a Windows Presentation Foundation (WPF) project targeting `.Net6.0-windows`. Due to the W in WPF this project ***only*** supports compilation on/for Windows. The project will house the front-end GUI code for our solution and will rely on the 'Interpreter` project for the back-end.

### Interpreter Project
`C_Flat_Interpreter` is a class library project which will house the back-end code for our solution. The interpreter will be split up into lexing, parsing and transpiling the C_Flat code provided to it by the GUI. As it is a class library it will not compile an executable* however, the written code will be tested by the final of the three projects.

<sub>*Whilst we want the interpreter project to stay as a standalone class-library, to compile the parsed code into valid C# we may have to resort to creating an executable for this project but this is subject to change.</sub>

### Test Project
`C_Flat_Tests` is a test project which uses NUnit Framework to test the code within the interpreter project. The benefit of separating the tests into it's own project, is that it allows us to use tools such as `dotnet test` to automate testing which will prevent bugs from entering the project. This can be facilitated using GitHub Actions, through requiring all tests within the solution to pass before a Pull Request can merge.

## Testing

This project will be using NUnit as a testing framework. You can run these from within your IDE using the testing window. To do this in Rider:
CLick "Unit Tests" at the bottom of the IDE window or press `Alt+Shift+8`.
