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

\ Cross-compiler target image manager for flat memory images
\
\ This image is maintained in memory at the granularity of target cells (so
\ ALIGN etc align on cell boundaries). Each cell is associated with an output
\ word that emits it when the image is saved.
\
\ The cells in the image need not be the same size as those in the host,
\ but cannot (at present) be larger.

\ ---------- Building and maintaining the target ----------

\ The number of host bytes needed to store one target cell:
\    - 1 CELL for output word
\    - 1 CELL for data
2 CELLS CONSTANT /IMAGECELL

\ The pointer to the image data block. This is filled-in by (INITIALISE-IMAGE)
\ when the image block is created
0 VALUE IMAGE

\ The highest image address
0 VALUE HIGHEST-IMAGE-ADDRESS

\ The highest address actually stored to in the image
VARIABLE HIGHEST-MODIFIED-IMAGE-ADDRESS
: T>HIGHEST ( taddr -- taddr )
    DUP HIGHEST-MODIFIED-IMAGE-ADDRESS [FORTH] @ > IF
	DUP HIGHEST-MODIFIED-IMAGE-ADDRESS [FORTH] !
    THEN ;

\ Return the number of cells and bytes the image actually contains
: IMAGEBYTES \ ( -- n )
    HIGHEST-MODIFIED-IMAGE-ADDRESS [FORTH] @ ;
: IMAGECELLS \ ( -- n )
    IMAGEBYTES /CELL /MOD SWAP IF 1+ THEN ;

\ Check for address overflow
: T>OVERFLOW ( taddr -- taddr )
    DUP HIGHEST-IMAGE-ADDRESS > IF
	S" Image memory overflow" ABORT
    THEN ;

\ Return the host address of the given target image address, checking
\ for overflow
: T> \ ( taddr -- addr )
    T>OVERFLOW
    T>HIGHEST
    /CELL /MOD
    /IMAGECELL * + IMAGE + 1 CELLS + ;

\ Return the address of the output word associated with the given target address
: T>EMIT \ ( taddr -- addr )
    /CELL /MOD
    NIP /IMAGECELL * IMAGE + ;


\ ---------- Accessing the image ----------

\ Defer the words that actually emit parts of the image
DEFER EMIT-CELL
DEFER EMIT-ADDRESS
DEFER EMIT-CFA

\ Reading and writing the output word for a cell
: E! T>EMIT [FORTH] ! ;
: E@ T>EMIT [FORTH] @ ;

\ Align a target address
: T>ALIGN ( taddr -- taddr' )
    DUP /CELL MOD ?DUP 0<> IF /CELL SWAP - + THEN ;

\ Test for address alignment
: T>ALIGNED? \ ( addr -- addr )
    DUP DUP T>ALIGN <> IF
	. SPACE S" is not aligned on a cell boundary" ABORT
    THEN ;

\ Characters
: C! T> [FORTH] C! ;
: C@ T> [FORTH] C@ ;

\ Values, stored using target endianness. We have to maintain endianness at this
\ level, since otherwise code being cross-compiled that relied on endianness, for
\ example by addressing into data words, wouldn't work. Although it's quite a lot of
\ work in converting it shouldn't be a problem on a development-class system,
\ especially as the appropriate endianness is chosen when the cross-compiler
\ is loaded
: ! ( v taddr -- )
    T>ALIGNED?
    SWAP
    /CELL 0 DO
	[ BIGENDIAN? ] [IF]
	    2DUP
	    /CELL I - 1- 8 * RSHIFT 255 AND
	    SWAP I + C!
	[ELSE]
	    2DUP
	    255 AND
	    SWAP I + C!
	    8 RSHIFT
	[THEN]
    LOOP DROP
    ['] EMIT-CELL SWAP E! ; 
: @ ( taddr -- v )
    T>ALIGNED?
    0
    /CELL 0 DO
	[ BIGENDIAN? ] [IF]
	    8 LSHIFT
	    OVER I + C@ OR
	[ELSE]
	    OVER I + C@
	    I 8 * LSHIFT OR
	[THEN]
    LOOP NIP ;

\ Incrementing a location in one operation
: +! ( n taddr -- )
    DUP ROT
    @ +
    SWAP ! ;

\ Real addresses, stored as values
: RA! ! ;
: RA@ @ ;

\ Attila addresses, stored as offsets within the image
: A!
    DUP ROT !
    ['] EMIT-ADDRESS SWAP E! ; 
: A@ @ ;
: A+! ( n taddr -- )
    DUP ROT
    @ +
    SWAP A! ;

\ xts, stored as addresses
: XT! A! ;
: XT@ A@ ;

\ CFAs, stored as pointers to host-side counted strings
: (CFA@) T> [FORTH] @ ;
: (CFA!) 
    DUP ROT T> [FORTH] !
    ['] EMIT-CFA SWAP E! ;


\ ---------- Initialising and finalising the image ----------

\ Initialise the image for the given bumber of host cells
: (INITIALISE-IMAGE) \ ( cells -- )
    \ allocate the image and point IMAGE at it
    [FORTH] HERE      TO IMAGE
    DUP 1+ /CELL * 1- TO HIGHEST-IMAGE-ADDRESS
    DUP /IMAGECELL * [FORTH] ALLOT
    
    \ all cells are treated as data cells initially
    0 DO
	['] EMIT-CELL I /CELL * E!
    LOOP
    0 HIGHEST-MODIFIED-IMAGE-ADDRESS [FORTH] ! ;
 
\ Finalise the image -- nothing to do in this model
: (FINALISE-IMAGE) ( -- )
;
