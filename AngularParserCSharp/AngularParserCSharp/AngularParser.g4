parser grammar AngularParser;

options { tokenVocab=AngularLexer; }

program 
	: assignment EOF
	;

assignment
	: primary (ASSIGNMENT primary)?
	;

primary
	: constant afterPrimary?
	| array afterPrimary?
	| object afterPrimary?
	| functionCall afterPrimary?
	| identifier afterPrimary?
	;

afterPrimary
	: DOT primary # AfterPrimaryIdentifier
	| ARRAY_OPENING primary ARRAY_CLOSING # AfterPrimaryComputed
	;

functionCall
	: identifier PARENTHESES_OPEN assignment? (COMMA assignment)* PARENTHESES_CLOSE
	;

constant 
	: NUMBER  # NumberLiteral
	| STRING # StringLiteral
	;

identifier
	: IDENTIFIER # IdentifierToken
	;

array
	: ARRAY_OPENING ARRAY_CLOSING
	| ARRAY_OPENING assignment (COMMA assignment)* COMMA? ARRAY_CLOSING
	;

nameValuePair
	: constant COLON assignment
	| identifier COLON assignment
	;

object
	: OBJECT_OPENING OBJECT_CLOSING
	| OBJECT_OPENING nameValuePair (COMMA nameValuePair)* COMMA? OBJECT_CLOSING
	;