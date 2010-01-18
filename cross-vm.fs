\ $Id$

\ VM description generator for cross-compiler

<WORDLISTS ALSO CODE-GENERATOR DEFINITIONS

\ Translate characters unsafe for C
: SAFE-IDENTIFIER \ ( addr n -- )
    2DUP
    S" -><+@;:[]{}()!?/"
    S" ________________" TRANSLATE ;

\ Parse a word and make it a safe C identifier
: PARSE-IDENTIFIER \ ( "name" -- addr n )
    PARSE-WORD [CODE-GENERATOR] SAFE-IDENTIFIER ;

\ The defining words used in the vm and arch files
: CONSTANT \ ( n "name" -- )
    [CODE-GENERATOR] PARSE-IDENTIFIER
    ." #define " TYPE SPACE . CR ;
: USER \ ( n "name" -- )
    [CODE-GENERATOR] PARSE-IDENTIFIER
    ." #define USER_" TYPE SPACE . CR ;

\ We allow C inclusions
: CHEADER: \ ( -- )
    BEGIN
	REFILL IF
	    SOURCE 1- S" ;CHEADER" S=CI? IF
		2DROP
		REFILL DROP
		FALSE
	    ELSE
		TRUE
	    THEN
	ELSE
	    S" Input ended before ;CHEADER" ABORT
	THEN
    WHILE
	    TYPE CR
    REPEAT ;

WORDLISTS>
