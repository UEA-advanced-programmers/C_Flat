# C_Flat Language Definition
## Language Syntax:

### Variable types:
<table>
    <tr>
        <th>Input Type</th>
        <th>Code Representation</th>
    </tr>
    <tr>
        <td>Number</td>
        <td>Double</td>
    </tr>
    <tr>
        <td>String</td>
        <td>String</td>
    </tr>
    <tr>
        <td>Boolean</td>
        <td>bool</td>
    </tr>
</table>

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
`<Statement>::= <Declaration> | <Assignment> | <Function-Definition> | <Function-Call> | <Conditional-Statement> | <While-Loop>
`

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

`<Declaration>::= 'var’ (<Identifier> | <Assignment>) ‘;’`

`<Identifier>::= <Word>`

`<Word>::= 1*(a-zA-Z) - <Keywords>`
`<Assignment>::= <Identifier> '=' (<Expression> | ' " '<Word>' " ' | <Logic-Statement>) ‘;’`

### Functions:

`<Function-Definition>::= 'func' <Identifier> '('#<Parameter>')' <Block>`

`<Parameter>::= 'var' <Identifier>`

`<Function-Call>::= <Identifier> '(' *{<Identifier>} ')' ';'`

### Loops:

`<While-Loop>::= 'while' '(' <Logic-Statement> ')' <Block>`

### Keywords:

`<Keyword>::= 'if' || 'else' || 'while' || 'var' || 'func' | 'true' | 'false`

### Blocks:

`<Block>::= '{' *{<statement>';'} '}'`