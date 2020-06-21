lexer grammar AngularLexer;

@lexer::members
{
    protected const int EOF = Eof;
    protected const int HIDDEN = Hidden;
}

fragment SCIENTIFIC_NOTATION
	: [eE] '-'? [0-9]+
	;

fragment FLOAT_PART
	: DOT [0-9]+
	;

fragment ZERO_BASED
	: FLOAT_PART SCIENTIFIC_NOTATION?
	;

fragment INTEGER_FLOAT
	: [0-9]+ FLOAT_PART? SCIENTIFIC_NOTATION? 
	;

	
NUMBER
	: '-'? (ZERO_BASED | INTEGER_FLOAT)
	;

fragment HEX_DIGIT
	: [0-9]
	| [A-F]
	| [a-f];

fragment ESCAPED_CHARACTER 
	: '\\\''
	| '\\"'
	| '\\n'
	| '\\f'
	| '\\r'
	| '\\t'
	| '\\v'
	;

fragment UNICODE_CHARACTER
	: '\\u' HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT
	;

fragment SPECIAL_CHARACTER
	: ESCAPED_CHARACTER
	| UNICODE_CHARACTER
	;

fragment SINGLE_QUOTE_STRING
	: '\'' (~[''\\] | SPECIAL_CHARACTER)* '\''
	;
	
fragment DOUBLE_QUOTE_STRING
	: '"' (~[""\\] | SPECIAL_CHARACTER)* '"'
	;
	
STRING
	: SINGLE_QUOTE_STRING 
	| DOUBLE_QUOTE_STRING
	;

fragment IDENTIFIER_BEGIN
	: [a-z]
	| [A-Z]
	| '_'
	| '$';
IDENTIFIER 
	: IDENTIFIER_BEGIN (IDENTIFIER_BEGIN | [0-9])*
	;

ARRAY_OPENING
	: '['
	;
	
ARRAY_CLOSING
	: ']'
	;

COLON
	: ':'
	;

COMMA
	: ','
	;
	
OBJECT_OPENING
	: '{'
	;

OBJECT_CLOSING
	: '}'
	;

DOT
	: '.'
	;
	
PARENTHESES_OPEN
	: '('
	;

PARENTHESES_CLOSE
	: ')'
	;

ASSIGNMENT
	: '='
	;

WHITESPACE
	: (' ' | '\n' | '\r' | '\t' | '\u000B' | '\u00A0' ) -> channel(HIDDEN);