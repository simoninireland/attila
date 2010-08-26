\ $Id$

\ The standard outer executive
\ sd: this could be modularised to allow easier modification and hooking

\ The outer executive
: OUTER \ ( -- )
    BEGIN
	PARSE-WORD ?DUP IF
	    2DUP FIND ?DUP IF
		2SWAP 2DROP
		0< INTERPRETING? OR IF
		    EXECUTE
		ELSE
		    CTCOMPILE,
		THEN
	    ELSE
		2DUP NUMBER? IF
		    ROT 2DROP
		    INTERPRETING? NOT IF
			POSTPONE LITERAL
		    THEN
		ELSE
		    TYPE S" ?" ABORT
		THEN
	    THEN
	THEN
    EXHAUSTED? UNTIL ;
