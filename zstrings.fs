\ $Id$

\ Null-terminated strings
\ Routines to convert to and from the usual counted string format
\ and the C style of null-terminated string (zstrings). Useful for
\ accessing  low-level routines, but also for storing strings longer
\ than the single-byte count of "normal" strings

\ The NULL character
0 CONSTANT NULL

\ Find the end address of a zstring -- the address of the terminating
\ null
: Z> \ ( zaddr -- zaddr1 )
    BEGIN
	DUP C@ NULL <>
    WHILE
	    1+
    REPEAT ;

\ Convert a zstring to a normal address-plus-count
\ The count may be more than a single byte
: ZCOUNT \ ( zaddr -- addr n )
    DUP Z> OVER - ;

\ Move a zstring at zaddr1 to zaddr2, assuming there's enough memory
: ZMOVE \ ( zaddr1 zaddr2 -- )
    OVER ZCOUNT 1+ NIP CMOVE ;

\ Concatenate zstring2 to the end of zstring1, assuming there's
\ enough memory
: Z+ \ ( zaddr1 zaddr2 -- )
    2DUP >R Z> R> ZCOUNT >R SWAP R> CMOVE
    >R ZCOUNT
    R> ZCOUNT NIP + + 0 SWAP C! ; 
    
\ Concatenate a string to the end of a zstring, assuming there's
\ enough memory
: Z+S \ ( zaddr addr n -- )
    >R >R ZCOUNT R> R>        \ zaddr zn addr n
    DUP ROT                   \ zaddr zn n addr n
    4 PICK Z> SWAP CMOVE      \ zaddr zn n
    + + NULL SWAP C! ;

\ Convert a string to a zstring at zaddr, assuming there's enough memory
: S>Z \ ( addr n zaddr -- )
    DUP >R
    SWAP DUP >R CMOVE
    NULL R> R> + C! ;

\ Compile a zstring into data memory
: Z, \ ( zaddr -- )
    HERE SWAP
    ZCOUNT 1+ ALLOT
    SWAP ZMOVE ;

\ Display a zstring
: ZTYPE \ ( zaddr -- )
    ZCOUNT TYPE ;
