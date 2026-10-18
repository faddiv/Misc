// ANTLR implementation of grammar/sql92-select.ebnf.
//
// The rules follow the reference grammar one to one, with the same names and order. ANTLR's ALL(*) prediction
// does not need the reference grammar's left-factoring, so a few rules spell their alternatives out separately
// (predicateSuffix, primary); the reference text is quoted above each of them. WHERE, GROUP BY, HAVING, CASE operand
// and ELSE get small rules of their own, so the AST builder can reach them by name.
//
// Precedence, lowest to highest: OR < AND < NOT < predicate < + - || < * / < unary + - < primary
grammar Sql92Select;

options { caseInsensitive = true; }

// ------------------------------------------------------------------ statement

statement : queryExpression orderByClause? SEMICOLON? EOF ;

orderByClause : ORDER BY orderItem (COMMA orderItem)* ;

orderItem : expression (ASC | DESC)? ;

// ------------------------------------------------------------------ query expressions

queryExpression : queryTerm (unionOperator queryTerm)* ;

unionOperator : (UNION | EXCEPT) ALL? ;

queryTerm : queryPrimary (intersectOperator queryPrimary)* ;

intersectOperator : INTERSECT ALL? ;

queryPrimary : querySpecification | LPAREN queryExpression RPAREN ;

// Subqueries inside expressions must start with SELECT.
subquery : LPAREN subqueryExpression RPAREN ;

subqueryExpression : subqueryTerm (unionOperator queryTerm)* ;

subqueryTerm : querySpecification (intersectOperator queryPrimary)* ;

querySpecification
    : SELECT setQuantifier? selectList
      FROM tableReference (COMMA tableReference)*
      whereClause?
      groupByClause?
      havingClause?
    ;

whereClause : WHERE expression ;

groupByClause : GROUP BY expression (COMMA expression)* ;

havingClause : HAVING expression ;

setQuantifier : DISTINCT | ALL ;

selectList : STAR | selectItem (COMMA selectItem)* ;

selectItem
    : qualifiedName DOT STAR            # qualifiedStar
    | expression (AS? identifier)?      # expressionItem
    ;

// Reference: alias ::= AS? identifier. Inlined here and in tablePrimary: a separate rule costs ANTLR a rule call
// and an extra prediction for every select item, which made parsing 2-4% slower.

// ------------------------------------------------------------------ FROM clause

tableReference : tablePrimary joinPart* ;

joinPart
    : CROSS JOIN tablePrimary                          # crossJoin
    | NATURAL joinType? JOIN tablePrimary              # naturalJoin
    | joinType? JOIN tablePrimary joinSpecification    # qualifiedJoin
    ;

joinType : INNER | LEFT OUTER? | RIGHT OUTER? | FULL OUTER? ;

joinSpecification
    : ON expression
    | USING LPAREN identifier (COMMA identifier)* RPAREN
    ;

tablePrimary
    : qualifiedName (AS? identifier)?                  # namedTable
    | LPAREN queryExpression RPAREN AS? identifier     # derivedTable    // a derived table needs an alias
    ;

// ------------------------------------------------------------------ expressions

expression : andExpression (OR andExpression)* ;

andExpression : notExpression (AND notExpression)* ;

notExpression : NOT notExpression | predicate ;

// A predicate has at most one suffix, so a = b = c is rejected.
predicate : additiveExpression predicateSuffix? ;

// Reference: predicateSuffix ::= comparisonOperator ( (ANY | SOME | ALL) subquery | additiveExpression )
//                              | IS NOT? NULL
//                              | NOT? ( BETWEEN additiveExpression AND additiveExpression
//                                     | IN '(' ( subqueryExpression | expression (',' expression)* ) ')'
//                                     | LIKE additiveExpression (ESCAPE additiveExpression)? )
predicateSuffix
    : comparisonOperator (ANY | SOME | ALL) subquery                                # quantifiedComparison
    | comparisonOperator additiveExpression                                         # comparison
    | IS NOT? NULL                                                                  # isNull
    | NOT? BETWEEN additiveExpression AND additiveExpression                        # between
    | NOT? IN subquery                                                              # inSubquery
    | NOT? IN LPAREN expression (COMMA expression)* RPAREN                          # inList
    | NOT? LIKE additiveExpression (ESCAPE additiveExpression)?                     # like
    ;

comparisonOperator : EQ | NEQ | LT | GT | LTE | GTE ;

additiveExpression : multiplicativeExpression (additiveOperator multiplicativeExpression)* ;

additiveOperator : PLUS | MINUS | CONCAT ;

multiplicativeExpression : unaryExpression (multiplicativeOperator unaryExpression)* ;

multiplicativeOperator : STAR | SLASH ;

unaryExpression : (PLUS | MINUS) unaryExpression | primary ;

// Reference: primary ::= ... | qualifiedName ('(' functionArguments? ')')?
//                          | '(' (subqueryExpression | expression) ')'
primary
    : NUMBER                                                # numberLiteral
    | STRING                                                # stringLiteral
    | NULL                                                  # nullLiteral
    | CASE caseOperand? whenClause+ elseClause? END         # caseExpression
    | CAST LPAREN expression AS dataType RPAREN             # castExpression
    | EXISTS subquery                                       # existsExpression
    | qualifiedName LPAREN functionArguments? RPAREN        # functionCall
    | qualifiedName                                         # columnReference
    | subquery                                              # scalarSubquery
    | LPAREN expression RPAREN                              # parenthesized
    ;

caseOperand : expression ;

whenClause : WHEN expression THEN expression ;

elseClause : ELSE expression ;

functionArguments : STAR | setQuantifier? expression (COMMA expression)* ;

dataType : IDENTIFIER+ (LPAREN NUMBER (COMMA NUMBER)* RPAREN)? ;

qualifiedName : identifier (DOT identifier)* ;

identifier : IDENTIFIER | QUOTED_IDENTIFIER ;

// ------------------------------------------------------------------ lexer

ALL : 'ALL' ;
AND : 'AND' ;
ANY : 'ANY' ;
AS : 'AS' ;
ASC : 'ASC' ;
BETWEEN : 'BETWEEN' ;
BY : 'BY' ;
CASE : 'CASE' ;
CAST : 'CAST' ;
CROSS : 'CROSS' ;
DESC : 'DESC' ;
DISTINCT : 'DISTINCT' ;
ELSE : 'ELSE' ;
END : 'END' ;
ESCAPE : 'ESCAPE' ;
EXCEPT : 'EXCEPT' ;
EXISTS : 'EXISTS' ;
FROM : 'FROM' ;
FULL : 'FULL' ;
GROUP : 'GROUP' ;
HAVING : 'HAVING' ;
IN : 'IN' ;
INNER : 'INNER' ;
INTERSECT : 'INTERSECT' ;
IS : 'IS' ;
JOIN : 'JOIN' ;
LEFT : 'LEFT' ;
LIKE : 'LIKE' ;
NATURAL : 'NATURAL' ;
NOT : 'NOT' ;
NULL : 'NULL' ;
ON : 'ON' ;
OR : 'OR' ;
ORDER : 'ORDER' ;
OUTER : 'OUTER' ;
RIGHT : 'RIGHT' ;
SELECT : 'SELECT' ;
SOME : 'SOME' ;
THEN : 'THEN' ;
UNION : 'UNION' ;
USING : 'USING' ;
WHEN : 'WHEN' ;
WHERE : 'WHERE' ;

NEQ : '<>' ;
LTE : '<=' ;
GTE : '>=' ;
EQ : '=' ;
LT : '<' ;
GT : '>' ;
CONCAT : '||' ;
PLUS : '+' ;
MINUS : '-' ;
STAR : '*' ;
SLASH : '/' ;
LPAREN : '(' ;
RPAREN : ')' ;
COMMA : ',' ;
DOT : '.' ;
SEMICOLON : ';' ;

NUMBER : ( DIGIT+ ( '.' DIGIT* )? | '.' DIGIT+ ) ( 'E' [+-]? DIGIT+ )? ;
STRING : '\'' ( ~'\'' | '\'\'' )* '\'' ;
QUOTED_IDENTIFIER : '"' ( ~'"' | '""' )+ '"' ;
IDENTIFIER : [A-Z_] [A-Z_0-9]* ;

LINE_COMMENT : '--' ~[\r\n]* -> skip ;
WS : [ \t\r\n\f]+ -> skip ;

fragment DIGIT : [0-9] ;
