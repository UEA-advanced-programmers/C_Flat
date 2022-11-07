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
</table>

## Simplified EBNF as of Week 6:
### Statements:
`<Statement>::=<Expression> | <Logic-Statement>`

### Numerical expressions:

`<Expression>::= <Term> {('+'|'-') <Term>}`

`<Term>::= <Factor> {('*'|'/') <Factor>}`

`<Factor>::= '('<Expression>')' | <Number> | '-'<Factor>`

`<Number>::= <Digit> { '.' <Digit>}`

`<Digit> ::= #'[0-9]'`

### Logical expressions:

`<Logic-Statement>::= <Boolean> {<Condition>}`

`<Condition>::= ('==' | '&' | '|') <Boolean>`

`<Boolean>::= '!’<Logic-Statement> | 'true' | 'false' | <Expression-Query> | '('<Logic-Statement>')'`

`<Expression-Query> ::= <Expression> ('=='|'>'|'<') <Expression>`
