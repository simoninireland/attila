\ $Id$

\ Cross-compiler header compiler for flat memory mode.
\ As with all memory models, it is vital that this file
\ matches the details contained in the underlying memory model
\ description, otherwise bad things will happen.
\ sd: can we mechanise this? Probably not...

<WORDLISTS ALSO CROSS DEFINITIONS

\ ---------- Header navigation ----------

\ Convert an txt to the address of its code field (no-op in this model)
<WORDLISTS ALSO CROSS
' NOOP IS >CFA
WORDLISTS>

\ (Code field manipulation words are in the image manager)

\ Convert an txt to a link pointer containing the xt of the next word in the
\ definition chain
: >LFA \ ( txt -- lfa )
    1 [CROSS] CELLS - ;

\ Convert a txt to its status field
: >STATUS \ ( txt -- addr )
    2 [CROSS] CELLS - 1+ ;

\ Convert an xt to a name string. addr will be CALIGNED
: >NAME \ ( txt -- addr namelen )
    [CROSS] >LFA 1 [CROSS] CELLS - DUP [CROSS] C@
    DUP >R
    - 1-
    1 [CROSS] CELLS - [CROSS] CALIGN
    R> ;

\ Convert a txt to indirect body (DOES> behaviour) if present
: >IBA \ ( txt -- iba )
    1 [CROSS] CELLS + ;

\ Access IBA
: IBA@ ( txt -- addr ) [CROSS] >IBA [CROSS] @ ;
: IBA! ( addr txt -- ) [CROSS] >IBA [CROSS] ! ;


\ ---------- Status and body management ----------

\ Mask-in the given mask to the status of a word
: SET-STATUS \ ( f txt -- )
    [CROSS] >STATUS DUP [CROSS] C@
    -ROT OR
    SWAP [CROSS] C! ;

\ Get the status of the given word
: GET-STATUS \ ( txt -- s )
    [CROSS] >STATUS [CROSS] C@ ;

\ Test whether the given word is IMMEDIATE
: IMMEDIATE? \ ( xt -- f )
    [CROSS] GET-STATUS IMMEDIATE-MASK AND 0<> ; 

\ Test whether the given word is REDIRECTABLE
: REDIRECTABLE? \ ( xt -- f )
    [CROSS] GET-STATUS REDIRECTABLE-MASK AND 0<> ; 

\ Convert txt to body address, accounting for iba if present
: >BODY \ ( xt -- addr )
    DUP [CROSS] >IBA
    SWAP [CROSS] REDIRECTABLE? IF
	1 [CROSS] CELLS +
    THEN ;


\ ---------- Header construction ----------

\ Create a header for the name word with the given code field
: (CROSS-HEADER,) \ ( addr n cf -- xt )
    [CROSS] CALIGNED         \ align TOP on the next cell boundary 
    >R                       \ stash the code field
    2DUP ." Compiling " TYPE CR
    DUP >R                   \ compile the name
    ?DUP 0> IF
	0 DO
	    DUP C@ [CROSS] CCOMPILE,
	    1+
	LOOP
    THEN
    DROP
    [CROSS] CALIGNED      
    R> [CROSS] CCOMPILE,              \ compile the name length
    0 [CROSS] CCOMPILE,               \ status byte
    [CROSS] CALIGNED
    [CROSS] LAST @ [CROSS] ACOMPILE,  \ compile a link pointer
    [CROSS] TOP                       \ the txt
    R> [CROSS] CFACOMPILE,            \ the code pointer
    DUP [CROSS] LAST ! ;              \ update LAST

<WORDLISTS ALSO CROSS
' (CROSS-HEADER,) IS (HEADER,)
WORDLISTS>

WORDLISTS>
