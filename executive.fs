\ $Id$

\ The standard outer executive

\ The outer executive
: OUTER \ ( -- )
    BEGIN
	PARSE-WORD ?DUP IF
	    2DUP FIND ?DUP IF
		ROT 2DROP
		INTERPRETING? OVER IMMEDIATE? OR IF
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
