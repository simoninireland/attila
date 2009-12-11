\ $Id$

\ The standard outer executive

\ ---------- Compilation helpers ----------

\ Test whether we're interpreting
: INTERPRETING? \ ( -- f )
    STATE @ INTERPRETATION-STATE = ;

\ Find the xt of the next word in the input source, or abort
: ' \ ( "word" -- xt )
    PARSE-WORD 2DUP FIND
    ?DUP IF
	ROT 2DROP
    ELSE
	TYPE
	S" ?" ABORT
    THEN ;


\ ---------- The outer interpreter (executive) ----------

\ The outer executive
: OUTER \ ( -- )
    BEGIN
	PARSE-WORD ?DUP NOT IF
	    2DUP FIND ?DUP IF
		ROT 2DROP INTERPRETING?
		OVER IMMEDIATE? OR IF
		    EXECUTE
		ELSE
		    XTCOMPILE,
		THEN
	    ELSE
		2DUP NUMBER? IF
		    ROT 2DROP INTERPRETING? NOT IF
			LITERAL
		    THEN
		ELSE
		    TYPE S" ?" ABORT
		THEN
	    THEN
	THEN
    EXHAUSTED? UNTIL ;
