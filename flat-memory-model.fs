\ $Id$

\ A simple, flat memory model with headers.
\
\ A "flat" memory model in which headers, code and data live
\ live in a single contiguous block.
\
\     xt -> code       1 cell
\     ha -> namelen    1 byte
\           name       namelen bytes
\    lfa -> link       1 cell
\           status     1 byte
\  [ iba -> altbody    1 cell ] (REDIRECTABLE words only)
\   body -> body       n bytes

\ ---------- Header access ----------

\ Convert an xt to the address of its code field (no-op in this model)
: >CFA ; \ ( xt -- cfa )

\ Convert an xt to an ha
: >HA \ ( xt -- ha )
    1 CELLS + ;

\ Convert an xt to a name string
: >NAME \ ( xt -- addr namelen )
    >HA DUP 1+ SWAP C@ ;

\ Convert an xt to a link pointer
: >LFA \ ( xt -- lfa )
    1 CELLS + DUP C@ + 1+ ;

\ Convert xt to its status field
: >STATUS \ ( xt -- addr )
    >LFA 1 CELLS + ;

\ Convert xt to indirect body (DOES> behaviour) if present
\ (We don't check whether there actually *is* an IBA)
: >IBA \ ( xt -- iba )
    >STATUS 1+ ;

\ Convert xt to body address, accounting for iba if present
: >BODY \ ( xt -- addr )
    DUP >IBA
    SWAP REDIRECTABLE? IF
	1 CELLS +
    THEN ;

\ Convert ha to xt
: HA> \ ( ha -- xt )
    1 CELLS - ;


\ ---------- Header construction ----------

\ Create a header for the name word with the given code field
: (HEADER,) \ ( addr n cf -- xt )
    >R                  \ stash the code field
    DUP >R              \ compile the name
    0 DO
	DUP C@ CCOMPILE,
	1+
    LOOP
    R> CCOMPILE,        \ compile the name length
    0 COMPILE,          \ compile an empty link pointer
    TOP                 \ the xt
    R> COMPILE, ;       \ the code pointer


\ ---------- Finding words ----------

\ Traverse list of words
