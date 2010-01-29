\ $Id$

\ File templates
\
\ Fill-in a template by executing Attila code between square brackets, in the
\ style of executing code within a colon-definition.

\ Parse the template
: <FROM-TEMPLATE \ ( -- )
    .s
    FALSE
    BEGIN
	PARSE-WORD
	2DUP S" FROM-TEMPLATE>" S=CI DUP IF
	    ROT 2DROP
	THEN NOT
    WHILE
	    2DUP S" [" S= IF
		\ go into evaluate mode
		." evaluate"
		2DROP DROP TRUE
	    ELSE
		2DUP S" ]" S= IF
		    \ go into output mode
		    ." output"
		    2DROP DROP FALSE
		ELSE
		    2 PICK IF
			\ evaluating, run the word
			." evaluating" SPACE 2DUP TYPE CR 
			2DUP FIND IF
			    ." found"
			    ROT 2DROP
			    INTERPRETING? IF
				.s EXECUTE
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
				TYPE SPACE S" ?" ABORT
			    THEN
			THEN
		    ELSE
			\ outputing, print
			." printing"
			INTERPRETING? IF
			    TYPE SPACE
			ELSE
			    POSTPONE SLITERAL [COMPILE] TYPE [COMPILE] SPACE
			THEN
		    THEN
		THEN
	    THEN
    REPEAT
    DROP .s ; IMMEDIATE
