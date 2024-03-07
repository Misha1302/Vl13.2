grammar Grammar;

program: line* EOF;
line: NEWLINE* (include | label | jmp | if | struct | expression | globalDecl | for | while | functionDecl | structDecl | varDecl | ret | varSet | NEWLINE) ';'? NEWLINE*;

include: 'include' STRING;

varDecl: IDENTIFIER ':' type;
functionDecl: 'func' IDENTIFIER (varDecl (',' varDecl)*)? '->' type block;
globalDecl: 'global' varDecl;

block: ('=>' line) | ('{' line* '}');
type: ampersand? IDENTIFIER;
ret: 'ret' expression?;

struct: 'struct' IDENTIFIER (varDecl (',' varDecl)*)?;

varSet: (IDENTIFIER | varDecl) '=' expression;

structDecl: 'struct' IDENTIFIER (varDecl (',' varDecl)*);

ampersand: '&';

label: 'label' IDENTIFIER;
jmp: 'jmp' IDENTIFIER;

if: 'if' expression block else?;
else: 'else' (if | block);
while: 'while' expression block;
for: 'for' line line line block;

expression:
    IDENTIFIER                                                                                          #identifierExpr
    | INT                                                                                               #intExpr
    | FLOAT                                                                                             #floatExpr
    | STRING                                                                                            #stringExpr
    | expression (STAR | DIV | MOD) expression                                                          #mulDivModExpr
    | expression (PLUS | MINUS) expression                                                              #sumSubExpr
    | ampersand IDENTIFIER                                                                              #getAddressExpr
    | expression '(' (expression (',' expression)*)? ')'                                                #callExpr
    | expression '<' type (',' type)* '>' '(' (expression (',' expression)*)? ')'                       #addressCallExpr
    | '(' expression ')'                                                                                #parentsExpr
    | expression (LT | LE | GT | GE) expression                                                         #cmpExpr
    | expression (EQ | NEQ) expression                                                                  #eqExpr
    | if                                                                                                #ifExpr
    ;

LT: '<'; LE: '<='; GT: '>'; GE: '>=';
EQ: '=='; NEQ: '!=';

STAR: '*';
DIV: '/';
MOD: '%';
PLUS: '+';
MINUS: '-';

IDENTIFIER: [a-zA-Z_][a-zA-Z_0-9.]*;
STRING: ('\'' (('\\\'')|.)*? '\'') | ('"' (('\\"')|.)*? '"');
FLOAT: '-'?[0-9_]*[.][0-9_]+;
INT: '-'?[0-9_]+;

NEWLINE: [\r\n] -> skip;
WHITESPACES: [ \t\n\r] -> skip;
SINGLELINECOMMENTS: '//' ~('\r' | '\n')* -> skip;