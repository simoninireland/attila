\ $Id$

\ Source file inclusion

\ ---------- Loading sourcefiles ----------

\ Load a file
: (LOAD) \ ( fh -- )
    (<FROM)
    EXECUTIVE @ EXECUTE
    FROM> ;

\ Load a file
: LOAD \ ( addr len -- )
    2DUP R/O OPEN-FILE IF
	TYPE 32 EMIT S" can't be loaded" ABORT
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
