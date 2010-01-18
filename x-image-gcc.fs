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

<WORDLISTS ONLY FORTH ALSO DEFINITIONS

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
    [CROSS] /CELL /MOD
    /IMAGECELL * + IMAGE + 1 CELLS + ;

\ Return the address of the output word associated with the given target address
: TARGET>EMIT \ ( taddr -- addr )
    [CROSS] /CELL /MOD
    NIP /IMAGECELL * IMAGE + ;

\ Return the number of cells the image actually contains
: IMAGECELLS \ ( -- n )
    (TARGET-TOP) @ [CROSS] /CELL /MOD SWAP IF 1+ THEN ;


\ ---------- Accessing the image ----------

<WORDLISTS
ALSO CROSS DEFINITIONS

\ Reading and writing to and from the image's addresses
: C! TARGET> C! ;
: C@ TARGET> C@ ;

\ The (target) address of the image's i'th user variable
: USERVAR
    [CROSS] /CELL * ;
    
WORDLISTS>

\ Reading and writing the output word for a cell
: E! TARGET>EMIT ! ;
: E@ TARGET>EMIT @ ;


\ ---------- Saving the image ----------

\ Emit a cell as data: argument is a cell value
: EMIT-CELL \ ( t -- )
    ." (CELL) 0x" .HEX ;

\ Emit a cell as an address within the image: argument is a taddr
: EMIT-ADDRESS \ ( taddr -- )
    [CROSS] /CELL /MOD [CROSS] /CELL *   \ aligned cell
    ." (CELL) &image[0x" .HEX ." ]"
    ?DUP 0<> IF                          \ byte within cell
	SPACE ." + " .
    THEN ;

\ Emit a cfa as an address: argument is a counted string address
: EMIT-CFA \ ( caddr -- )
    ." (CELL) &"
    COUNT TYPE ;


\ ---------- Platform and compilation primitives ----------

<WORDLISTS
ALSO CROSS DEFINITIONS

\ Cross-compiler access to important target addresses
: TOP  (TARGET-TOP) @ ;
: HERE (TARGET-HERE) @ ;
: LAST (TARGET-LAST) ;
: LASTXT [CROSS] LAST @ ;

\ Compile a character
: CCOMPILE, \ ( c -- )
    [CROSS] TOP [CROSS] C!
    1 (TARGET-TOP) +!
    [CROSS] TOP [CROSS] /CELL MOD 0= IF
	\ next cell, initialise as a data cell (may be overridden later)
	['] EMIT-CELL [CROSS] TOP E!
    THEN ;

\ Align code address on cell boundaries
: CALIGN \ ( addr -- aaddr )
    DUP [CROSS] /CELL MOD ?DUP 0<> IF [CROSS] /CELL SWAP - + THEN ;
: CALIGNED \ ( -- )
    [CROSS] TOP [CROSS] CALIGN [CROSS] TOP - ?DUP IF
	0 DO
	    0 [CROSS] CCOMPILE,
	LOOP
    THEN ;
: CALIGNED? \ ( addr -- f )
    DUP [CROSS] CALIGN = ;

\ Compilation primitives for cells, real addresses, strings, addresses, xts
\ sd: currently hard-coded as being little-endian
: COMPILE, \ ( n -- )
    [CROSS] CALIGNED
    [CROSS] /CELL 0 DO
	DUP 255 AND [CROSS] CCOMPILE,
	8 RSHIFT
    LOOP DROP ;
: RCOMPILE, [CROSS] COMPILE, ;
: SCOMPILE, \ ( addr n -- )
    DUP [CROSS] CCOMPILE,
    0 DO
	DUP C@ [CROSS] CCOMPILE,
	1+
    LOOP
    DROP ;
: ACOMPILE, \ ( taddr -- )
    [CROSS] CALIGNED
    ['] EMIT-ADDRESS [CROSS] TOP E!
    [CROSS] COMPILE, ;
: XTCOMPILE, [CROSS] ACOMPILE, ;
    
\ For CFAs we assume that the caddr is a counted-string address of
\ a symbol, and cross-compile it by writing the string into the
\ image and setting the output word to write it out correctly
: CFACOMPILE, \ ( cf -- )
    [CROSS] CALIGNED
    ['] EMIT-CFA [CROSS] TOP E!
    [CROSS] COMPILE, ;

\ In this model, data and code compilation are the same
: C,       [CROSS] CCOMPILE, ;
: ,        [CROSS] COMPILE, ;
: R,       [CROSS] RCOMPILE, ;
: A,       [CROSS] ACOMPILE, ;
: XT,      [CROSS] XTCOMPILE, ;
: ALIGN    [CROSS] CALIGN ;
: ALIGNED  [CROSS] CALIGNED ;
: ALIGNED? [CROSS] CALIGNED? ;

\ Reading and writing, with alignment checking
: CHECK-ALIGNED \ ( addr -- addr )
    DUP [CROSS] ALIGNED? NOT IF
	. SPACE S" is not word-aligned" ABORT
    THEN ;
: !
    [CROSS] CHECK-ALIGNED
    DUP ROT TARGET> !
    ['] EMIT-CELL SWAP E! ; 
: @
    [CROSS] CHECK-ALIGNED TARGET> @ ;
: R! [CROSS] ! ;
: R@ [CROSS] @ ;
: A!
    [CROSS] CHECK-ALIGNED
    DUP ROT TARGET> !
    ['] EMIT-ADDRESS SWAP E! ; 
: A@ [CROSS] @ ;
: XT! [CROSS] A! ;
: XT@ [CROSS] A@ ;
: CFA@ \ ( txt -- cf )
    [CROSS] >CFA [CROSS] @ ;
: CFA! \ ( cf txt -- )
    [CROSS] >CFA DUP ROT [CROSS] !
    ['] EMIT-CFA SWAP E! ;

\ ALLOTting data space simply compiles zeros
: ALLOT ( n -- )
    0 DO
	0 [CROSS] C,
    LOOP ;

WORDLISTS>


\ ---------- Initialising and finalising the image ----------

\ Initialise the image
: INITIALISE-IMAGE \ ( -- )
    \ allocate the image and point IMAGE at it
    HERE TO IMAGE
    MAXIMAGECELLS /IMAGECELL * ALLOT
    
    \ initialise the image pointers
    0 (TARGET-TOP) !
    0 (TARGET-LAST) !
    
    \ initialise the user variable space
    MAXUSERVARIABLES 0 DO
	0 [CROSS] COMPILE,
	['] EMIT-CELL I E! 
    LOOP

    \ initialise the next cell as data
    ['] EMIT-CELL [CROSS] TOP E! ;

\ Finalise the image prior to being output
: FINALISE-IMAGE
  \  [CROSS-COMPILER] ['] COLD 0 [CROSS] !    \ cold-start vector 
  \  [CROSS] TOP 2 [CROSS] !                  \ (TOP)
;

\ Emit the image as a C data structure
: EMIT-IMAGE \ ( -- )
    ." CELL image[] = {" CR
    IMAGECELLS 0 DO
	I [CROSS] /CELL * [CROSS] @
	I [CROSS] /CELL * E@ EXECUTE ." ," CR
    LOOP
    ." };" CR ;

\ Emit a small part of the image around the given target address
: EMIT-AROUND \ ( taddr -- )
    11 0 DO
	DUP I 5 - DUP 0= IF
	    [CHAR] > EMIT
	ELSE
	    SPACE
	THEN
	[CROSS] CELLS +
	DUP .HEX SPACE
	DUP [CROSS] TOP < IF 
	    DUP [CROSS] @
	    SWAP E@ EXECUTE
	THEN
	CR
    LOOP
    DROP ;

WORDLISTS>
