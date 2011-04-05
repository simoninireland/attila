\ $Id$

\ This file is part of Attila, a retargetable threaded interpreter
\ Copyright (c) 2007--2011, Simon Dobson <simon.dobson@computer.org>.
\ All rights reserved.
\
\ Attila is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation; either version 2
\ of the License, or (at your option) any later version.
\
\ Attila is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.
\
\ You should have received a copy of the GNU General Public License
\ along with this program; if not, write to the Free Software
\ Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

\ A simple, flat memory model with code and data in a single segment.

\ ---------- Basic memory access and allocation ----------

\ TOP and HERE are the same in this model, as are CEILING and THERE
: TOP     (TOP) @ ;
: HERE    TOP ;
: CEILING (CEILING) @ ;
: THERE   CEILING ;

\ Access to last-defined word
\ sd: seems like the wrong place for this....
: LASTXT LAST @ ;

\ Return the amount of memory we allocate to allocate enough for the
\ given amount
: >SEGMENT-SIZE ( n -- n' )
    SEGMENT-SIZE /MOD SWAP IF 1+ THEN SEGMENT-SIZE * ;

\ Ensure that there is enough memory to CALLOT n bytes, creating a new segment
\ and resetting if necessary. Note that this doesn't actually *allocate*
\ memory, it just makes sure it *can be* allocated. TOP is not guaranteed
\ to be CALIGNED afterwards
: (CALLOT) ( n -- )
    DUP CEILING TOP - > IF 
	\ not enough room, try to create a new segment
	>SEGMENT-SIZE CREATE-SEGMENT ?DUP IF
	    \ new segment, re-set the variables
	    OVER (TOP) !
	    + 1- (CEILING) !
	ELSE
	    \ can't get a new segment, die
	    1 DIE
	THEN
    ELSE
	\ enough memory already
	DROP
    THEN ;

\ Allocate a number of bytes in code memory, checking for overflow, Safe for zero
\ and negative amounts, and checks for out-of-memory issues. Use (CALLOT) to
\ ensure there's enough to allocate before calling CALLOT
: CALLOT ( n -- )
    DUP 0> IF
	DUP CEILING TOP - > IF
	    \ out of memory
	    1 DIE
	ELSE
	    \ enough memory, allocate it
	    (TOP) +!
	THEN
    ELSE
	\ do nothing for non-positive allocations
	DROP
    THEN ;

\ ALLOTting data space is just like CALLOTting code space in this model
: (ALLOT) ( n -- ) (CALLOT) ;
: ALLOT   ( n -- )  CALLOT  ;


\ ---------- Code compilation ----------

\ Compile a character. This is the basic unit of compilation,
\ for code and data (in this model)
: CCOMPILE, \ ( c -- )
    TOP
    /CHAR CALLOT
    C! ;

\ Align code address on cell boundaries
: CALIGN \ ( addr -- aaddr )
    DUP /CELL /MOD DROP ?DUP 0<> IF /CELL SWAP - + THEN ;
: CALIGNED \ ( -- )
    TOP CALIGN TOP - ?DUP IF
	0 DO
	    0 CCOMPILE,
	LOOP
    THEN ;

\ Compilation primitives for cells, real addresses, strings, addresses, xts
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

\ Compile a sequence of characters -- like a string, but without
\ the leading length. Safe for empty sequences
: CSEQCOMPILE, \ ( addr n -- )
    ?DUP 0> IF
	0 DO
	    DUP C@ CCOMPILE,
	    /CHAR +
	LOOP
    THEN
    DROP ;

\ Compile a string. Safe for empty strings
: SCOMPILE, \ ( addr n -- )
    CALIGNED DUP CCOMPILE, CSEQCOMPILE, ;

\ There's no distinction between addresses at run-time
: RACOMPILE,  COMPILE, ;
: ACOMPILE,   COMPILE, ;
: XTCOMPILE,  ACOMPILE, ;
: CFACOMPILE, COMPILE, ;

\ CTCOMPILE, is defined in the inner interpreter


\ ---------- Data compilation ----------

\ In this model, data and code compilation are the same
: C,      CCOMPILE, ;
: ,       COMPILE, ;
: RA,     RACOMPILE, ;
: A,      ACOMPILE, ;
: XT,     XTCOMPILE, ;
: CSEQ,   CSEQCOMPILE, ;
: S,      SCOMPILE, ;
: ALIGN   CALIGN ;
: ALIGNED CALIGNED ;


\ ---------- Memory access ----------

\ Memory access also doesn't distinguish between address types
\ !, @, C@, C!, 2@ amd 2! are in core.fs: these words are the "manipulators"
\ for architectures that need to handle the different types differently
: A!   ! ;          \ Attila addresses
: A@   @ ;
: RA!  ! ;          \ "real" (hardware) addresses
: RA@  @ ;
: XT!  ! ;          \ xts
: XT@  @ ;
\ See also CFA! and CFA@, IBA! and IBA@, which manipulate word headers


	    