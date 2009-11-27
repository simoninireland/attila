\ $Id$

\ A simple, flat memory model with headers.
\
\ A "flat" memory model in which headers, code and data live
\ live in a single contiguous block.
\
\           name       namelen bytes (CALIGNED)
\           namelen    1 byte        (CALIGNED)
\           status     1 byte
\    lfa -> link       1 cell
\     xt -> code       1 cell        (CALIGNED)
\  [ iba -> altbody    1 cell ]      (REDIRECTABLE words only)
\   body -> body       n bytes

\ ---------- Header access ----------

\ Convert an xt to the address of its code field (no-op in this model)
: >CFA ; \ ( xt -- cfa )

\ Return  the code field of a word. This is treated as an abstract value
\ in Attila, but is actually a real address to primitive code
: CFA@ \ ( xt -- cf )
    >CFA @ ;

\ Convert an xt to a link pointer containing the xt of the next word in the
\ definition chain
: >LFA \ ( xt -- lfa )
    1 CELLS - ;

\ Convert xt to its status field. The namelen and status are adjacent and
\ CALIGNED
: >STATUS \ ( xt -- addr )
    2 CELLS - 1+ ;

\ Convert an xt to a name string. addr will be aligned
: >NAME \ ( xt -- addr namelen )
    >LFA 1 CELLS - DUP C@
    DUP >R
    - 1-
    7 - ALIGN        \ ensure we're aligned on the previous cell boundary
    R> ;

\ Convert xt to indirect body (DOES> behaviour) if present
\ (We don't check whether there actually *is* an IBA)
: >IBA \ ( xt -- iba )
    1 CELLS + ;

\ Convert xt to body address, accounting for iba if present
: >BODY \ ( xt -- addr )
    DUP >IBA
    SWAP REDIRECTABLE? IF
	1 CELLS +
    THEN ;


\ ---------- Header construction ----------

\ Create a header for the name word with the given code field
: (HEADER,) \ ( addr n cf -- xt )
    CALIGNED            \ align TOP on the next cell boundary 
    >R                  \ stash the code field
    DUP >R              \ compile the name
    0 DO
	DUP C@ CCOMPILE,
	1+
    LOOP
    CALIGNED      
    R> CCOMPILE,        \ compile the name length
    0 CCOMPILE,         \ status byte
    CALIGNED
    0 COMPILE,          \ compile an empty link pointer
    TOP                 \ the xt
    R> CFA, ;           \ the code pointer


\ ---------- Finding words ----------

\ Traverse list of words
: (FIND) \ ( addr namelen xt -- 0 | xt 1 | xt -1 )
    BEGIN
	DUP 0<>
    WHILE
	    >R 2DUP R@ ROT R>  \ addr n xt addr n xt
	    >NAME S= IF        \ addr n xt
		ROT 2DROP
		1 OVER IMMEDIATE? IF
		    NEGATE
		THEN
	    ELSE
		>LFA @
	    THEN
    REPEAT

    DUP 0= IF
	ROT 2DROP
    THEN ;


