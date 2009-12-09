\ $Id$

\ File inclusion

\ ---------- Loading sourcefiles ----------

\ Load a file
: (LOAD) \ ( fh -- )
    INPUTSOURCE DUP @
    2 PICK 2 PICK !

    \ run the executive on its original stack
    >R >R >R
    EXECUTIVE @ EXECUTE
    R> R> R>

    -ROT FILE-CLOSE SWAP ! ;

\ Open a file using the given word as file-finder
: (OPEN-FILE) \ ( addr len opener -- fh )
    >R 2DUP R> EXECUTE
    DUP IF
	ROT 2DROP
    ELSE
	DROP TYPE SPACE S" not accessible" ABORT
    THEN ;

\ Load a file
: LOAD \ ( addr len -- )
    ['] FILE-OPEN (OPEN-FILE) (LOAD) ;


\ ---------- Including files ----------
\ sd: no include path yet

\ Include a file, making use of the include path
: INCLUDED \ ( addr len -- )
    ['] FILE-OPEN-INCLUDE (OPEN-FILE) (LOAD) ;

\ Load the next file from the input stream
: INCLUDE \ ( "filename" -- )
    PARSE-WORD INCLUDED ;

