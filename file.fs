\ $Id$

\ Additional file management functions
\
\ The SAVE-INPUT and RESTORE-INPUT functions let programs
\ transparently save and restore the current input file
\ and position.
\
\ <TO .. TO> and <FROM ... FROM> re-direct the
\ virtual machine's standard input and output respectively,
\ and can be used to replace the standard streams with
\ files for a period.

\ ---------- Save/restore ----------

\ Save the current input state
: SAVE-INPUT \ ( -- fh pos 2 )
    6 USERVAR ( INPUTSOURCE ) @
    DUP FILE-POSITION DROP
    2 ;

\ Restore the previously-save input state, if possible
: RESTORE-INPUT \ ( fh pos 2 -- f )
    DROP
    OVER REPOSITION-FILE ?DUP IF
	NIP
    ELSE
	6 USERVAR ( INPUTSOURCE )  !
	TRUE
    THEN ;


\ ---------- Re-directing I/O ----------

\ Re-direct subsequent output to the given file
: (<TO) \ ( fh -- ofh )
    7 USERVAR ( OUTPUTSINK ) @
    SWAP 7 USERVAR ( OUTPUTSINK ) ! ;
: <TO \ ( "name" -- ofh )
    PARSE-WORD 2DUP W/O CREATE-FILE IF
	DROP
	TYPE 32 EMIT S" cannot be re-directed to" ABORT
    ELSE
	ROT 2DROP (<TO)
    THEN ;

\ Restore previous output stream
: TO> \ ( ofh -- ) 
    7 USERVAR ( OUTPUTSINK ) @ CLOSE-FILE DROP
    7 USERVAR ( OUTPUTSINK ) ! ;

\ Re-direct subsequent input from the given file
: (<FROM) \ ( fh -- )
    6 USERVAR ( INPUTSOURCE ) @
    SWAP 6 USERVAR ( INPUTSOURCE ) ! ;
: <FROM \ ( "name" -- )
    PARSE-WORD 2DUP R/O OPEN-FILE IF
	DROP
	TYPE 32 EMIT S" cannot be re-directed from" ABORT
    ELSE
	ROT 2DROP (<FROM)
    THEN ;

\ Restore previous output stream
: FROM> \ ( ofh -- ) 
    6 USERVAR ( INPUTSOURCE ) @ CLOSE-FILE DROP
    6 USERVAR ( INPUTSOURCE ) ! ;

