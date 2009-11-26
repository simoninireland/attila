\ $Id$

\ The primitive cross-compiler
\
\ Primitives are stored in Attila source files alongide Attila code.
\ When loaded into the cross-compiler they are converted into
\ stylised C functions and output to a file ready for later compilation.
\
\ This code performs the same functions as tools/primgen does for
\ bootstrapping Attila.

\ ---------- Constants and data structures ----------

\ Create a primitive locator
\ Primitives are stored as lists of counted strings separated by blank
\ string:
\    counted string primitive name
\    list of arguments
\    list of results
\    counted string primitive code text
: PRIMITIVE-LOCATOR \ ( "name" -- )
    DATA
    0 ,    \ address of list of arguments
    0 ,    \ address of list of results
    0 , ;  \ address of code text

\ Compile the various elements in the locator, fixing its pointers
: PRIMITIVE-NAME, \ ( addr n xt -- )
    DROP S, ;
: PRIMITIVE-PARAMETERS, \ ( xt -- elem )
    0 CELL-ELEMENT
    DUP -ROT ! ;
: PRIMITIVE-RESULTS, \ ( xt -- elem )
    0 CELL-ELEMENT
    DUP -ROT 1 CELLS + ! ;
    
\ Return the primitive name
: PRIMITIVE-NAME \ ( loc -- addr n )
    3 CELLS + COUNT ;

\ Return lists of parameters and results
: PRIMITIVE-ARGUMENTS \ ( loc -- elem )
    @ ;
: PRIMITIVE-RESULTS \ ( loc -- elem )
    1 CELLS + @ ;

\ Return the primitive code text
: PRIMITIVE-CODE-TEXT \ ( loc -- addr n )
    2 CELLS + @ COUNT ;


\ ---------- Some handy helper words ----------

\ Test whether two strings are equal, leaving a flag and the first string
: S=? \ ( addr1 n1 addr2 n2 -- add1 n1 f )
    2OVER S= ;

\ Move a counted string to a memory address, on the assumption that there
\ is sufficient space
: SMOVE \ ( addr1 n addr2 -- )
    2DUP C!
    1+ SWAP CMOVE ;

\ Return the address of the character after the end of the given counted string
: CHARACTER-AFTER \ ( caddr -- addr )
    COUNT CHARS + ;

\ Catenate the first string to the second (counted) string
: S+ \ ( addr n caddr -- )
    DUP CHARACTER-AFTER     \ addr n caddr addr2
    ROT                     \ addr addr2 n caddr
    DUP C@                  \ addr addr2 n caddr oldn
    2 PICK + SWAP C!
    CMOVE ;


\ ---------- Primitive name generator ----------

VARIABLE (#PRIM) 0 (#PRIM) !    \ uniques
DATA (PRIMNAME) 256 ALLOT       \ buffer for name

\ Create a new primitive name
: PRIMNAME \ ( -- addr n )
    S" prim" (PRIMNAME) SMOVE
    (#PRIM) @ <# #S #> (PRIMNAME) S+
    1 (#PRIM) +! 
    (PRIMNAME) COUNT ;


\ ---------- Primitive parser ----------

\ Parse a primitive, creating a locator word that holds all its details
: PRIMITIVE:
    \ ALSO TARGET DEFINITIONS
    PRIMITIVE-LOCATOR

    PARSE-WORD S" (" S=? IF        \ compile primitive name, creating one if necessary
	2DROP PRIMNAME
    THEN
    LASTXT PRIMITIVE-NAME,

    LASTXT PRIMITIVE-PARAMETERS,   \ compile the parameters list
    BEGIN
	PARSE-WORD S" --" S=? NOT
    WHILE
	    STRING-ELEMENT DUP -ROT ELEMENT+
    REPEAT
    2DROP DROP

    LASTXT PRIMITIVE-RESULTS,       \ compile the results list
    BEGIN
	PARSE-WORD S" )" S=? NOT
    WHILE
	    STRING-ELEMENT DUP -ROT ELEMENT+
    REPEAT
    2DROP DROP
;

    
	    


\ ---------- Primitive generation ----------

   