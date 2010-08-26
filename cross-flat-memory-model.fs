\ $Id$

\ Cross-compiler header compiler for flat memory model.
\ As with all memory models, it is vital that this file
\ matches the details contained in the underlying memory model
\ description, otherwise bad things will happen.
\ sd: can we mechanise this? Probably not...


\ ---------- Header navigation ----------

\ Convert an txt to the address of its code field (no-op in this model)
' NOOP IS >CFA

\ (Code field manipulation words are in the image manager)

\ Convert an txt to a link pointer containing the xt of the next word in the
\ definition chain
: >LFA \ ( txt -- lfa )
    1 CELLS - ;

\ Convert a txt to its status field
: >STATUS \ ( txt -- addr )
    2 CELLS - 1+ ;

\ Convert an xt to a name string. addr will be CALIGNED
: >NAME \ ( txt -- addr namelen )
    >LFA 1 CELLS - DUP C@
    DUP >R
    - 1-
    1 CELLS - CALIGN
    R> ;

\ Convert a txt to indirect body (DOES> behaviour) if present
: >IBA \ ( txt -- iba )
    1 CELLS + ;

\ Access IBA
: IBA@ ( txt -- addr ) >IBA @ ;
: IBA! ( addr txt -- ) >IBA ! ;


\ ---------- Status and body management ----------

\ Mask-in the given mask to the status of a word
: SET-STATUS \ ( f txt -- )
    >STATUS DUP C@
    -ROT OR
    SWAP C! ;

\ Get the status of the given word
: GET-STATUS \ ( txt -- s )
    >STATUS C@ ;

\ Convert txt to body address, accounting for iba if present
: >BODY \ ( xt -- addr )
    DUP >IBA
    SWAP REDIRECTABLE? IF
	1 CELLS +
    THEN ;


\ ---------- Header construction ----------

\ Cross-compile a header for the name word with the given code field
: (CROSS-PRIMITIVE-HEADER,) \ ( addr n cf -- txt )
    CALIGNED         \ align TOP on the next cell boundary 
    >R                       \ stash the code field
    2DUP ." Compiling " TYPE CR
    DUP >R                   \ compile the name
    ?DUP 0> IF
	0 DO
	    DUP [FORTH] C@ CCOMPILE,
	    1 CHARS +
	LOOP
    THEN
    DROP ." name "
    CALIGNED      
    R> CCOMPILE,  ." len "            \ compile the name length
    0 CCOMPILE,     ." status "       \ status byte
    CALIGNED
    LASTXT DUP 0= IF                  \ compile the link pointer
	COMPILE, ." start of chain "
    ELSE
	ACOMPILE, ." link "
    THEN 
    TOP                       \ the txt
    R> DUP . SPACE CFACOMPILE,       ." cf "     \ the code pointer
    DUP LASTXT!        ." last" ;      \ update LAST

\ Create a header and a locator
: (CROSS-HEADER,) ( addr n cf -- txt )
    >R 2DUP R> (CROSS-PRIMITIVE-HEADER,)
    CREATE-WORD-LOCATOR ;

' (CROSS-HEADER,) IS (HEADER,)

