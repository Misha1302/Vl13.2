grammar Grammar;

program: line* EOF;
line: NEWLINE* (include | expression | globalDecl | functionDecl | structDecl | varDecl | ret | varSet | NEWLINE | ';')+ NEWLINE*;

include: 'include' STRING;

varDecl: IDENTIFIER ':' type;
functionDecl: 'func' IDENTIFIER (varDecl (',' varDecl)*)? '->' type block;
globalDecl: 'global' varDecl;

block: '{' line* '}';
type: ampersand? IDENTIFIER;
ret: 'ret' expression?;

varSet: (IDENTIFIER | varDecl) '=' expression;

structDecl: 'struct' IDENTIFIER (varDecl (',' varDecl)*);

ampersand: '&';

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
    ;

STAR: '*';
DIV: '/';
MOD: '%';
PLUS: '+';
MINUS: '-';

IDENTIFIER: [a-zA-Z_][a-zA-Z_0-9.]*;
STRING: ('\'' (('\\\'')|.)*? '\'') | ('"' (('\\"')|.)*? '"');
FLOAT: '-'?[0-9]*[.][0-9]+;
INT: '-'?[0-9]+;

NEWLINE: [\r\n];
WHITESPACES: [ \t\n\r] -> skip;
SINGLELINECOMMENTS: '//' ~('\r' | '\n')* -> skip;