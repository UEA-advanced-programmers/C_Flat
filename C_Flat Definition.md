# C_Flat Language Definition
## Language Syntax:

### Variable types:
<table>
    <tr>
        <th>Token</th>
        <th>Implementation</th>
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
        <td>N/A</td>
        <td>Equality</td>
    </tr>
    <tr>
        <td>!</td>
        <td>N/A</td>
        <td>Not</td>
    </tr>
    <tr>
        <td>!=</td>
        <td>N/A</td>
        <td>Inequality</td>
    </tr>
    <tr>
        <td><</td>
        <td>N/A</td>
        <td>Less than</td>
    </tr>
    <tr>
        <td>></td>
        <td>N/A</td>
        <td>More than</td>
    </tr>
    <tr>
        <td>&</td>
        <td>N/A</td>
        <td>logical and</td>
    </tr>
    <tr>
        <td>|</td>
        <td>N/A</td>
        <td>logical or</td>
    </tr>
    <tr>
        <td>=</td>
        <td>N/A</td>
        <td>Assignment operator</td>
    </tr>
    <tr>
        <td>;</td>
        <td>N/A</td>
        <td>end of statement</td>
    </tr>
    <tr>
        <td>{</td>
        <td>N/A</td>
        <td>Begin block</td>
    </tr>
    <tr>
        <td>}</td>
        <td>N/A</td>
        <td>End block</td>
    </tr>
</table>

## Simplified EBNF as of Week 7:

### Statements:
`<Statement>::= <Declaration> ';' | <ConditionalStatement>`

### Numerical expressions:

`<Expression>::= <Term> {('+'|'-') <Term>}`

`<Term>::= <Factor> {('*'|'/') <Factor>}`

`<Factor>::= '('<Expression>')' | <Number> | '-'<Factor>`

`<Number>::= <Digit> { '.' <Digit>}`

`<Digit> ::= #'[0-9]'`

### Logical expressions:

`<Logic-Statement>::= <Boolean> {<Condition>}`

`<Condition>::= ( '==' | '!=' | '&' | '|' ) <Boolean>`

`<Boolean>::= '!’<Logic-Statement> | 'true' | 'false' | <Expression-Query> | '('<Logic-Statement>')'`

`<Expression-Query> ::= <Expression> ( '==' | '!=' | '>'| '<' ) <Expression>`

### Conditional Statements:

`<ConditionalStatement>::= 'if’ ‘('<Logic-Statement> ’)’ ‘{' <block> '}' { 'else' ‘{‘ <block> ’}’ }`

`<block>::= *{ <statement> ';' }`

### Variables:
`<Declaration>::= 'var' <Identifier> '=' (<Expression> | <Logic-Statement> ) ';'`
`<Identifier>::= <Word>`
`<Word>::=[a-zA-Z]+`
