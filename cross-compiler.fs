\ $Id$

\ The Attila cross-compiler
\

\ ---------- The word lists ----------

WORDLIST CONSTANT CROSS-WORDLIST                   \ Host words
CROSS-WORDLIST PARSE-WORD CROSS (VOCABULARY)
WORDLIST CONSTANT TARGET-WORDLIST                  \ Target locator words
TARGET-WORDLIST PARSE-WORD TARGET (VOCABULARY)

\ Find a word in a given wordlist aborting if we fail
: FIND-IN-WORDLIST \ ( addr n wid -- xt )
    >R 2DUP R> SEARCH-WORDLIST
    0<> IF
	ROT 2DROP
    ELSE
	TYPE SPACE
	S" not found in word list" ABORT
    THEN ;

\ Execute a host cross word without changing the search order
\ State-smart: do the word or compile it
:NONAME \ ( "name" -- )
    CROSS-WORDLIST FIND-IN-WORDLIST
    EXECUTE ;
:NONAME \ ( "name" -- )
    CROSS-WORDLIST FIND-IN-WORDLIST
    COMPILE, ;
INTERPRET/COMPILE [CROSS]


\ ---------- Target description ----------
\ sd: This should come from elsewhere

ONLY CROSS ALSO DEFINITIONS

8 CONSTANT #CELL

\ Single block of memory with TOP = HERE
0 VALUE TOP
: HERE TOP ;


\ ---------- Low-level compilation into the target image ----------

\ For debugging only
: EMIT-CELL \ ( n -- )
    S" (CELL) 0x" TYPE
    .HEX
    S" ," TYPE NL ;
: EMIT-SYMBOL-ADDRESS \ ( addr n -- )
    S" (CELL) &" TYPE
    TYPE
    S" ," TYPE NL ;

\ Compilation primitive that collects bytes and emits cells
\ sd: currently hard-coded as being little-endian
VARIABLE (CURRENT-CELL)
VARIABLE (CURRENT-BYTE)
: CCOMPILE, \ ( c -- )
    (CURRENT-BYTE) @ 8 * LSHIFT (CURRENT-CELL) +!
    1 (CURRENT-BYTE) +!
    (CURRENT-BYTE) @ #CELL >= IF
	(CURRENT-CELL) @ EMIT-CELL
	0 (CURRENT-BYTE) !
	0 (CURRENT-CELL) !
    THEN
    TOP 1+ TO TOP ;

\ Align code address on cell boundaries
: CALIGN \ ( addr -- aaddr )
    DUP #CELL MOD #CELL SWAP - + ;
: CALIGNED \ ( -- )
    TOP CALIGN TOP - ?DUP IF
	0 DO
	    0 CCOMPILE,
	LOOP
    THEN ;

\ Additional compilation primitives for cells and strings
: COMPILE, \ ( n -- )
    CALIGNED
    #CELL 0 DO
	DUP 255 AND CCOMPILE,
	8 RSHIFT
    LOOP DROP ;
: SCOMPILE, \ ( addr n -- )
    DUP CCOMPILE,
    0 DO
	DUP C@ CCOMPILE,
	1+
    LOOP
    DROP ;

\ To compile a code field we emit the address of the symbol pointed to 
: CFA, \ ( cf -- )
    COUNT EMIT-SYMBOL-ADDRESS ;

\ In this model, data and code compilation are the same
: C, COMPILE, ;
: , COMPILE, ;
: ALIGN CALIGN ;
: ALIGNED CALIGNED ;


\ ---------- Higher-level compilation into the target ----------



