\ $Id$

\ Source file inclusion

\ ---------- Loading sourcefiles ----------

\ Load a file
: (LOAD) \ ( fh -- )
    (<INPUT)
    EXECUTIVE @ EXECUTE
    INPUT> ;

\ Load a file
: LOAD \ ( addr len -- )
    2DUP OPEN-FILE IF
	TYPE SPACE S" can't be loaded" ABORT
    ELSE
	ROT 2DROP
	(LOAD)
    THEN ;

    
\ ---------- Including files ----------
\ sd: no include path yet

\ Include a file, making use of the include path
: INCLUDED \ ( addr len -- )
    LOAD ;

\ Load the next file from the input stream
: INCLUDE \ ( "filename" -- )
    PARSE-WORD INCLUDED ;
