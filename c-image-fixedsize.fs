\ $Id$

\ Cross-compiler target image for gcc
\
\ This image is maintained in memory at the granularity of target cells (so
\ ALIGN etc align on cell boundaries). Each cell is associated with an output
\ word that emits it when the image is saved. For cells holding numbers this
\ emits the value directly; for cells holding Attila (image) addresses, xts etc
\ this emits an address of the C-level data structure.
\
\ The cells in the image need not be the same size as those in the host,
\ but cannot (at present) be larger.
\
\ The image has a single code and data space, with everything allocated sequentially.
\
\ The image has a fixed maximum size.

\ ---------- Building and maintaining the target ----------

\ Maximum size of image in cells. May be re-defined before calling
\ (INITIALISE-IMAGE) to get a larger or smaller image 
10 1024 * VALUE MAXIMAGECELLS

\ Number of user variables to allocate space for -- again, may be
\ re-defined before calling (INITIALISE-IMAGE)
15 VALUE MAXUSERVARIABLES

\ The parts of the image
VARIABLE (TARGET-TOP)            \ the current compilation address
: (TARGET-HERE) (TARGET-TOP) ;   \ current data address, same as compilation
VARIABLE (TARGET-LAST)           \ the last txt defined

\ The number of host bytes needed to store one target cell:
\    - 1 CELL for output word
\    - 1 CELL for data
2 CELLS CONSTANT /IMAGECELL

\ The pointer to the image data block. This is filled-in by (INITIALISE-IMAGE)
\ when the image block is created
0 VALUE IMAGE

\ Return the host address of the given target image address
: TARGET> \ ( taddr -- addr )
    /CELL /MOD
    /IMAGECELL * + IMAGE + 1 CELLS + ;

\ Return the address of the output word associated with the given target address
: TARGET>EMIT \ ( taddr -- addr )
    /CELL /MOD
    NIP /IMAGECELL * IMAGE + ;

\ Return the number of cells the image actually contains
: IMAGECELLS \ ( -- n )
    (TARGET-TOP) @ /CELL /MOD SWAP IF 1+ THEN ;


\ ---------- Accessing the image ----------

\ Reading and writing to and from the image's addresses. All the other
\ image manipulations are built from these
: C! TARGET> [FORTH] C! ;
: C@ TARGET> [FORTH] C@ ;

\ The (target) address of the image's i'th user variable
: USERVAR
    /CELL * ;

\ Reading and writing the output word for a cell
: E! TARGET>EMIT [FORTH] ! ;
: E@ TARGET>EMIT [FORTH] @ ;


\ ---------- Saving the image ----------

<wordlists also forth
: .BYTE \ ( n -- )
    BASE @ HEX SWAP
    <# ABS # # #> TYPE
    BASE ! ;
wordlists>

\ Emit a cell, converting from the target's endianness to a literal C value
\ sd: we have to maintain target endianness up until this point to make sure
\ that any access to target memory during cross-compilation on a per-byte basis
\ gets the right values
\ sd: should be refactored using [IF] etc
: .HEX-ENDIAN ( t -- )
    BIGENDIAN? IF
	/CELL 0 DO
	    DUP
	    /CELL I - 1- 8 * RSHIFT .BYTE
	LOOP
    ELSE
	/CELL 0 DO
	    DUP
	    255 AND .BYTE
	    8 RSHIFT
	LOOP
    THEN DROP ;

\ Emit a cell as data: argument is a cell value
: EMIT-CELL \ ( t -- )
    ." (CELL) "
    DUP 0< IF
	." -" ABS
    THEN
    ." 0x" .HEX-ENDIAN ;

\ Emit a cell as an address within the image: argument is a taddr
: EMIT-ADDRESS \ ( taddr -- )
    /CELL /MOD                           \ aligned cell
    ." (CELL) &image[0x" .HEX ." ]"
    ?DUP 0<> IF                          \ byte within cell
	SPACE ." + " .
    THEN ;

\ Emit a cfa as an address: argument is a counted string address
: EMIT-CFA \ ( caddr -- )
    ." (CELL) &"
    COUNT TYPE ;


\ ---------- Platform and compilation primitives ----------

\ Cross-compiler access to important target addresses
: TOP  (TARGET-TOP) @ ;
: HERE (TARGET-HERE) @ ;
: LASTXT (TARGET-LAST) @ ;
: LASTXT! (TARGET-LAST) ! ;

\ Compile a character
: CCOMPILE, \ ( c -- )
    TOP C!
    1 (TARGET-TOP) +! ;

\ Align code address on cell boundaries
: CALIGN \ ( addr -- aaddr )
    DUP /CELL MOD ?DUP 0<> IF /CELL SWAP - + THEN ;
: CALIGNED \ ( -- )
    TOP CALIGN TOP - ?DUP IF
	0 DO
	    0 CCOMPILE,
	LOOP
    THEN ;
: CALIGNED? \ ( addr -- f )
    DUP CALIGN = ;

\ Compile value using target endianness
\ sd: should be refactored using [IF] etc
: COMPILE, \ ( n -- )
    CALIGNED
    /CELL 0 DO
	BIGENDIAN? IF
	    DUP /CELL I - 1- 8 * RSHIFT 255 AND CCOMPILE,
	ELSE
	    DUP 255 AND CCOMPILE,
	    8 RSHIFT
	THEN
    LOOP DROP ;

\ Real (target) addresses are stored as values
: RACOMPILE, COMPILE, ;

\ Forth addresses are stored as offsets into the image
: ACOMPILE, \ ( taddr -- )
    CALIGNED
    TOP TARGET> [FORTH] !    \ store address in host endiannness
    ['] EMIT-ADDRESS TOP E!
    /CELL (TARGET-TOP) +! ;

\ Target xts are stored as target addresses
: XTCOMPILE, ACOMPILE, ;

\ Strings need to be copied from host memory, hence the explicit use of host C@ and CHARS
: SCOMPILE, \ ( addr n -- )
    DUP CCOMPILE,
    0 DO
	DUP [FORTH] C@ CCOMPILE,
	1 [FORTH] CHARS +
    LOOP DROP CALIGNED ;

\ For CFAs we assume that the caddr is a counted-string address of
\ a symbol, and cross-compile it by writing the string into the
\ image and setting the output word to write it out correctly. 
: CFACOMPILE, \ ( cf -- )
    CALIGNED
    TOP TARGET> [FORTH] !  \ store counted string address in host endianness
    ['] EMIT-CFA TOP E!
    /CELL (TARGET-TOP) +! ;

\ In this model, data and code compilation are the same
: C,       CCOMPILE, ;
: ,        COMPILE, ;
: RA,      RACOMPILE, ;
: A,       ACOMPILE, ;
: XT,      XTCOMPILE, ;
: ALIGN    CALIGN ;
: ALIGNED  CALIGNED ;
: ALIGNED? CALIGNED? ;

\ Reading and writing, with alignment checking
: CHECK-ALIGNED \ ( addr -- addr )
    DUP ALIGNED? NOT IF
	. SPACE S" is not word-aligned" ABORT
    THEN ;
: !
    CHECK-ALIGNED
    DUP ROT TARGET> [FORTH] !
    ['] EMIT-CELL SWAP E! ; 
: @
    CHECK-ALIGNED TARGET> [FORTH] @ ;
: RA! ! ;
: RA@ @ ;
: A!
    DUP ROT !
    ['] EMIT-ADDRESS SWAP E! ; 
: A@ @ ;
: XT! A! ;
: XT@ A@ ;
: CFA@ \ ( txt -- cf )
    >CFA @ ;
: CFA! \ ( cf txt -- )
    >CFA DUP ROT !
    ['] EMIT-CFA SWAP E! ;

\ ALLOTting data space simply compiles zeros
: ALLOT ( n -- )
    0 DO
	0 C,
    LOOP ;


\ ---------- Initialising and finalising the image ----------

<CODE-GENERATOR ALSO CROSS

\ Initialise the image
: INITIALISE-IMAGE \ ( -- )
    \ allocate the image and point IMAGE at it
    [FORTH] HERE TO IMAGE
    MAXIMAGECELLS /IMAGECELL * [FORTH] ALLOT

    \ all cells are treated as cells initially
    MAXIMAGECELLS 0 DO
	['] EMIT-CELL I /CELL * E!
    LOOP
    
    \ initialise the image pointers
    0 (TARGET-TOP)  [FORTH] !
    0 (TARGET-LAST) [FORTH] !
    
    \ initialise the user variable space
    MAXUSERVARIABLES 0 DO
	0 COMPILE,
    LOOP ;

\ Finalise the image prior to being output
: FINALISE-IMAGE
    TOP 2 ( TOP ) USERVAR A! ;

\ Emit the image as a C data structure
: EMIT-IMAGE \ ( -- )
    ." CELL image[0x" IMAGE-SIZE .HEX ." ] = {" CR
    IMAGECELLS 0 DO
	." /* 0x" I .HEX SPACE ." */ "  
	I /CELL * DUP @
	SWAP          E@ EXECUTE ." ," CR
    LOOP
    ." };" CR ;

CODE-GENERATOR>

\ Emit a small part of the image around the given target address
: EMIT-AROUND \ ( taddr -- )
    11 0 DO
	I 5 - 2DUP CELLS + DUP 0< IF
	    2DROP
	ELSE
	    SWAP 0= IF
		[FORTH] [CHAR] > EMIT
	    ELSE
		SPACE
	    THEN
	    DUP /CELL / .HEX SPACE
	    DUP TOP < IF 
		DUP @
		SWAP E@ EXECUTE
	    THEN
	    CR
	THEN
    LOOP
    DROP ;

