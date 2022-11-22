# Testing Standards:
## Automated Testing:
Our automated testing is set up using a GitHub Actions workflow to build and test the `C_Flat_Tests` project whenever a branch is merged to main. 
The workflow is specified in the  `dotnet.yml` workflow file. This file specifies a number of things. 
Firstly it instructs GitHub when we want the workflow to run (in our case whenever something is committed to `main` or a pull request targets `main`). 
Then it defines a list of jobs it wants to run within that workflow. In our case we only have a single job `build-and-test` which builds and tests on the commit of which it is run against.
Once it has built and ran the source-code. It reports any failures/results back to GitHub which can be seen in the workflow run log.

This automated testing allows us to enforce stable additions to `main`. 
Where no pull requests can be merged unless they both compile successfully and pass all written unit tests. 
Whilst this does not protect against all possible issues; (e.g. due to GHA limitations we cannot compile/test the GUI project) It is extremely useful for ensuring a stable `main` branch.

## Unit Testing:
Unit testing is conducted via NUnit within the C_Flat_Tests project. The structure of the project is as follows:

### Project Structure:
- `C_Flat_Tests/Common` This directory houses all of the ‘Common’ classes that might need to be accessed throughout the testing project. For example the `TestLogger` class file
- `C_Flat_Tests/Tests_Unit` This directory contains the Unit testing classes for each ‘Unit’ of code. Such as `ExecutionUnit.cs`. As these are Unit tests they should only test the code within that class. I.e. `ExecutionUnit.cs` should not rely on `Lexer` or `Parser` only `Execution`.
- `C_Flat_Tests/Tests_Integration` This directory doesn’t yet exist as we have not written any integration tests, however its purpose will be to test each unit together instead of individually. An example test would be a full run through of the lex -> parse -> execution cycle. 

### Naming Conventions:
Our Naming conventions should follow that of NUnit along with other popular test-runners.

#### Class naming conventions:
The naming conventions for the `.cs` file should be the class you’re testing and the type of test e.g. `LexerUnit.cs`. 
Though for integration tests as you are testing multiple classes it should instead be the purpose of that test e.g. `TranspilationIntegration.cs`. 
The namespace for the test class should also match the appropriate directory it is stored within i.e. `C_Flat_Tests.Tests_Unit` for unit tests and `C_Flat_Tests.Tests_Integration` for integration tests.

#### Test method naming conventions:

When naming your test method you should follow this convention:
`ClassYou'reTesting_MethodBeingTested_WhatYou'reTesting_WhatYouExpectToHappen`

For example:
```csharp
public void Execution_ShuntYard_SimpleAddition_ReturnsCorrectAnswer()
```
You can use this as a rough guide as there are some edge-cases where this might not fit

### Test Structure:
Test methods should be implemented to match the following structure:
```csharp
[Test]
public void <TestMethodName>()
{
    //Arrange
    //  Do your data setup here e.g.
    int numberToTest = 5;
    //Act
    //  Do your test
    numberToTest += 5;
    //Assert
    //  Check it did what it should
    Assert.That(numberToTest, Is.EqualTo(10);
}
```
In modern NUnit versions, most tests should use the `Assert.That()` method which uses the parameters to perform the assertion. 
For more information on how you can use this method see their documentation [here](https://docs.nunit.org/articles/nunit/writing-tests/assertions/assertion-models/constraint.html)