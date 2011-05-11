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

\ Non-blocking cell-sized ring buffers
\
\ Rings are fixed-sized LIFOs. Writing more elements than the ring
\ can hold overwrites older elements
\ sd: should these be abstracted over the size of the element?

\ ---------- Creation ----------

\ Create a ring buffer in data memory holding the given number of cells
: (RING) ( n -- )
    DUP ,             \ size
    0 ,               \ write point
    0 ,               \ read point
    1+ CELLS ALLOT ;  \ the ring itself, one more cell than its size

\ Create a new named ring buffer of the given size
: RING ( n "name" -- )
    DATA (RING) ;


\ ---------- Element ----------

\ Return the address of the read and write points
: (RING-WRITE-POINT) 1 CELLS + ;
: (RING-READ-POINT)  2 CELLS + ;

\ Return the address of the given ring element
: (RING-ELEMENT) ( i r -- addr )
    3 CELLS +
    SWAP CELLS + ;

\ Return the maximum size of the ring
: RING-MAX-SIZE ( r -- n ) @ ;
    

\ ---------- Access ----------

\ Increment the read point and return it, wrapping-around if necessary
: RING-BUMP-READ-POINT ( r -- i )
    >R
    R@ (RING-READ-POINT) @ 1+ R@ RING-MAX-SIZE 1+ MOD
    DUP R> (RING-READ-POINT) ! ;

\ Increment the write point and return it, wrapping-around if needed and
\ bumping the read point if points collide
: RING-BUMP-WRITE-POINT ( r -- i )
    >R
    R@ (RING-WRITE-POINT) @ 1+ R@ RING-MAX-SIZE 1+ MOD 
    DUP R@ (RING-WRITE-POINT) !

    \ bump the read point if necessary
    DUP R@ (RING-READ-POINT) @ = IF
	R> RING-BUMP-READ-POINT DROP
    ELSE
	RDROP
    THEN ;

\ Empty the ring
: RING-EMPTY ( r -- )
    >R
    0 R@ (RING-WRITE-POINT) !
    0 R> (RING-READ-POINT)  ! ;

\ Return the actual number of elements in a ring
: RING-SIZE ( r -- n )
    >R
    R@ (RING-WRITE-POINT) @
    R@ (RING-READ-POINT)  @
    2DUP = IF
	2DROP 0
    ELSE
	2DUP > IF
	    \ write not wrapped-around, size of the difference
	    -
	ELSE
	    \ write point wrapped-around, size is the write point
	    \ plus the difference from the read point to the end
	    R@ RING-MAX-SIZE SWAP -
	    + 1+
	THEN
    THEN RDROP ;

\ Write an element into the ring, overwriting the oldest element
\ if necessary
: >RING ( v r -- )
    >R
    R@ RING-BUMP-WRITE-POINT R> (RING-ELEMENT) ! ;

\ Read an element from the ring if possible, returning a success flag
\ and any value
: RING> ( r -- v -1 | 0 )
    >R
    R@ RING-SIZE 0= IF
	FALSE
    ELSE
	R@ RING-BUMP-READ-POINT R@ (RING-ELEMENT) @
	TRUE
    THEN RDROP ;
