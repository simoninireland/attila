\ $Id$

\ Cross-compiler target image for gcc
\
\ This image is maintained in memory at the granularity of target cells (so
\ ALIGN etc align on cell boundaries). Each cell is associated with an output
\ word that emits it when the image is saved. For cells holding numbers this
\ emits the value directly; for cells holding Attila (image) addresses, xts etc
\ this emits an address of the C-level data structure.
\
\ The image has a single code and data space, with everything allocated sequentially.
\
\ The image has a fixed maximum size.

\ ---------- Building and maintaining the target ----------

\ Maximum size of image in cells
64 1024 * CONSTANT MAXIMAGECELLS

\ The parts of the image
VARIABLE (TARGET-TOP)                    \ the current compilation address
: (TARGET-HERE) [CROSS] (TARGET-TOP) ;   \ current data address, same as compilation

\ The number of host bytes needed to store one target cell:
\    - 1 (host) CELL for output word
\    - 1 (target) CELL for data
1 CELLS 1 [CROSS] CELLS + CONSTANT /IMAGECELL

\ The image data block
DATA IMAGE [CROSS] MAXIMAGECELLS [CROSS] /IMAGECELL * ALLOT

\ Return the host address of the given target image address
: TARGET> \ ( taddr -- addr )
    [CROSS] /CELL /MOD
    [CROSS] /IMAGECELL * + [CROSS] IMAGE + 1 CELLS + ;

\ Return the address of the output word associated with the given target address
: TARGET>EMIT \ ( taddr -- addr )
    [CROSS] /CELL /MOD
    NIP [CROSS] /IMAGECELL * [CROSS] IMAGE + ;

\ Return the number of cells the image actually contains
: IMAGECELLS \ ( -- n )
    [CROSS] (TARGET-TOP) @ [CROSS] /CELL /MOD SWAP IF 1+ THEN ;

\ Reading and writing to and from the image's address
: ! [CROSS] TARGET> ! ;
: @ [CROSS] TARGET> @ ;
: C! [CROSS] TARGET> C! ;
: C@ [CROSS] TARGET> C@ ;

\ Reading and writing the output word for a cell
: E! [CROSS] TARGET>EMIT ! ;
: E@ [CROSS] TARGET>EMIT @ ;


\ ---------- Saving the image ----------

\ Emit a cell as data: argument is a cell value
: EMIT-CELL \ ( t -- )
    S" (CELL) 0x" TYPE
    .HEX
    S" ," TYPE NL ;

\ Emit a cell as an address within the image: argument is a taddr
: EMIT-ADDRESS \ ( taddr -- )
    [CROSS] /CELL /MOD
    S" (CELL) &image[" TYPE
    .
    S" ]" TYPE
    ?DUP 0<> IF
	SPACE S" + " TYPE .
    THEN
    S" ," NL ;

\ Emit a cfa as an address: argument is a counted string address
: EMIT-CFA \ ( caddr -- )
    COUNT
    S" (CELL) &" TYPE
    TYPE
    S" ," TYPE NL ;

\ Emit the image as a C data structure
: EMIT-IMAGE \ ( -- )
    S" CELL image[] = {" TYPE NL
    [CROSS] IMAGECELLS 0 DO
	I [CROSS] /CELL * [CROSS] @
	I [CROSS] /CELL * [CROSS] E@ EXECUTE
    LOOP
    S" };" TYPE NL ;


\ ---------- Platform and compilation primitives ----------

\ Cross-compiler access to important target addresses
: TOP [CROSS] (TARGET-TOP) @ ;
: HERE [CROSS] (TARGET-HERE) @ ;

\ Compile a character
: CCOMPILE, \ ( c -- )
    [CROSS] TOP [CROSS] C!
    1 [CROSS] (TARGET-TOP) +!
    [CROSS] TOP [CROSS] /CELL MOD 0= IF
	\ next cell, initialise as a data cell (may be overridden later)
	0 [CROSS] TOP [CROSS] !
	[ ALSO CROSS ' EMIT-CELL PREVIOUS ] LITERAL [CROSS] TOP [CROSS] E!
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

\ Compilation primitives for cells, real addresses, strings, addresses, xts, cfas
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
    [ ALSO CROSS ' EMIT-ADDRESS PREVIOUS ] LITERAL [CROSS] TOP [CROSS] E!
    [CROSS] COMPILE, ;
: XTCOMPILE, [CROSS] ACOMPILE, ;
: CFACOMPILE, \ ( caddr -- )
    [CROSS] CALIGNED
    [ ALSO CROSS ' EMIT-CFA PREVIOUS ] LITERAL [CROSS] TOP [CROSS] E!
    [CROSS] COMPILE, ;

\ In this model, data and code compilation are the same
: C,      [CROSS] COMPILE, ;
: ,       [CROSS] COMPILE, ;
: R,      [CROSS] RCOMPILE, ;
: A,      [CROSS] ACOMPILE, ;
: XT,     [CROSS] XTCOMPILE, ;
: ALIGN   [CROSS] CALIGN ;
: ALIGNED [CROSS] CALIGNED ;

\ ALLOTting data space simply compiles zeros
: ALLOT ( n -- )
    0 DO
	0 [CROSS] C,
    LOOP ;


\ ---------- Initialising and finalising the image ----------

\ Initialise the image
: (INITIALISE-IMAGE) \ ( -- )
    0 [CROSS] (TARGET-TOP) !
    [ ALSO CROSS ' EMIT-CELL PREVIOUS ] LITERAL [CROSS] TOP [CROSS] E! ;

\ Finalise the image prior to being output -- nothing needed in this model
: (FINALISE-IMAGE) ;


