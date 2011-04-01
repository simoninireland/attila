\ $Id$

\ This file is part of Attila, a retargetable threaded interpreter
\ Copyright (c) 2007--2010, Simon Dobson <simon.dobson@computer.org>.
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

\ A simple, flat memory model with headers, for indirected threading.

\ ---------- Memory management ----------

\ TOP and HERE are the same in this model, as are CEILING and THERE
: TOP     (TOP) @ ;
: HERE    TOP ;
: CEILING (CEILING) @ ;
: THERE   CEILING ;

\ Access to last-defined word
: LASTXT LAST @ ;

\ Allocate a number of bytes in code memory, checking for overflow
\ sd: bounds checking should be a conditional-compilation option?
: CALLOT ( n -- )
    (TOP) +!
    TOP CEILING >= IF
	1 DIE   \ error code 1: out of memory
    THEN ;

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

\ ALLOTting data space. Safe for zero and negative amounts. The allocated
\ space isn't zeroed
: ALLOT ( n -- )
    DUP 0> IF
	CALLOT
    ELSE
	DROP
    THEN ;

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


	    