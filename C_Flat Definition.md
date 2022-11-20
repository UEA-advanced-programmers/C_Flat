# C_Flat Language Definition
## Language Syntax:

### Variables (In Progress):
<table>
    <tr>
        <th>Input Type</th>
        <th>Code Representation</th>
    </tr>
    <tr>
        <td>Number</td>
        <td>var myNumber = 10;</td>
    </tr>
    <tr>
        <td>String</td>
        <td>var myString = "Hello World!";</td>
    </tr>
    <tr>
        <td>Boolean</td>
        <td>var myBoolean = true;</td>
    </tr>
</table>

In the table above, the variables are being declared with the keyword `var` and assigned a value straight away, however we can also declare a variable without assigning a value.

<pre>var myVariable;</pre>

This can later be assigned a value.

<pre>myVariable = 10;</pre>

Variable declaration and assignment should always be followed by a semicolon.

### Functions (Not yet implemented):

Functions are declared using the keyword `func`, followed by a function name and any parameters in parentheses

<pre>func myFunction(var myParameter)
{
    var doSomething;
} </pre>

When we call this function, we simply using the function name and supply the correct parameters, if any are required.

<pre>myFunc(myParameter);</pre>

A function call should always be followed by a semicolon.

### Conditional Statement (In Progress):

A simple conditional statement starts with the keyword `if`, followed by a logic statement in parentheses. After this we can have as many statements as we want, inside curly braces.

<pre>if(true)
{
    var doSomething;
}</pre>

This conditional statement can be extended by using the keyword `else`, again followed by as many statements as we require, inside curly braces.

<pre>if(true)
{
    var doSomething;
}
else
{
    var doSomethingElse;
}</pre>

### Loops (In Progress):

A loop can be created using the keyword `while`, followed by a logic statement wrapped in parentheses. In turn, this is followed by as many statements as we like, inside curly braces.

<pre>while(true)
{
    var doSomething;
}</pre>

### Operations (Lower precedence evaluates first):
<table>
    <tr>
        <th>Symbol</th>
        <th>Precedence</th>
        <th>Operation</th>
    </tr>
    <tr>
        <td>+</td>
        <td>3</td>
        <td>Addition</td>
    </tr>
    <tr>
        <td>-</td>
        <td>3</td>
        <td>Subtraction</td>
    </tr>
    <tr>
        <td>*</td>
        <td>2</td>
        <td>Multiplication</td>
    </tr>
    <tr>
        <td>/</td>
        <td>2</td>
        <td>Division</td>
    </tr>
    <tr>
        <td>(</td>
        <td>0</td>
        <td>Open Parentheses</td>
    </tr>
    <tr>
        <td>)</td>
        <td>0</td>
        <td>Close Parentheses</td>
    </tr>
    <tr>
        <td>==</td>
        <td>5</td>
        <td>Equality</td>
    </tr>
    <tr>
        <td>!</td>
        <td>1</td>
        <td>Not</td>
    </tr>
    <tr>
        <td>!=</td>
        <td>5</td>
        <td>Inequality</td>
    </tr>
    <tr>
        <td><</td>
        <td>4</td>
        <td>Less than</td>
    </tr>
    <tr>
        <td>></td>
        <td>4</td>
        <td>More than</td>
    </tr>
    <tr>
        <td>&</td>
        <td>6</td>
        <td>Logical and</td>
    </tr>
    <tr>
        <td>|</td>
        <td>7</td>
        <td>Logical or</td>
    </tr>
    <tr>
        <td>=</td>
        <td>8</td>
        <td>Assignment Operator</td>
    </tr>
    <tr>
        <td>-</td>
        <td>1</td>
        <td>Unary Negation</td>
    </tr>
</table>

## Simplified EBNF as of Week 7:

### Statements:
`<Statement>::= <Declaration> | <Assignment> | <Function-Definition> | <Function-Call> | <Conditional-Statement> | <While-Loop>`

### Numerical expressions:

`<Expression>::= <Term> {('+'|'-') <Term>}`

`<Term>::= <Factor> {('*'|'/') <Factor>}`

`<Factor>::= '('<Expression>')' | <Number> | '-'<Factor>`

`<Number>::= <Digit> { '.' <Digit>}`

`<Digit> ::= 1*(0-9)`

### Logical expressions:

`<Logic-Statement>::= <Boolean> {<Condition>}`

`<Condition>::= ( '==' | '!=' | '&' | '|' ) <Boolean>`

`<Boolean>::= '!’<Logic-Statement> | 'true' | 'false' | <Expression-Query> | '('<Logic-Statement>')' | <Identifier>`

`<Expression-Query> ::= (<Expression> | <Identifier>) ( '==' | '!=' | '>'| '<' ) (<Expression> | <Identifier>)`

### Conditional Statements:

`<Conditional-Statement>::='if’ ‘('<Logic-Statement>’)’ <Block> {'else' <Block> }`

### Variables:

`<Declaration>::= 'var’ (<Identifier> ';'| <Assignment>)`

`<Identifier>::= <Word>`

`<Word>::= 1*(a-zA-Z) - <Keywords>`

`<Assignment>::= <Identifier> '=' (<Expression> | ' " '<Word>' " ' | <Logic-Statement>) ‘;’`

### Functions:

`<Function-Definition>::= 'func' <Identifier> '('#<Parameter>')' <Block>`

`<Parameter>::= 'var' <Identifier>`

`<Function-Call>::= <Identifier> '(' *{<Identifier> | <Logic-Statement> | <Expression>} ')' ';'`

### Loops:

`<While-Loop>::= 'while' '(' <Logic-Statement> ')' <Block>`

### Keywords:

`<Keyword>::= 'if' | 'else' | 'while' | 'var' | 'func' | 'true' | 'false`

### Blocks:

`<Block>::= '{' *{<statement>} '}'`