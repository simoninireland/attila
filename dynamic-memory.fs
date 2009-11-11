\ $Id: dynamic-memory.fs,v 1.5 2007/05/18 19:02:13 sd Exp $

\ This file is part of Attila, a minimal threaded interpretive language
\ Copyright (c) 2007, UCD Dublin. All rights reserved.
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

\ Dynamic memory
\
\ This file provides very simple dynamic memory allocation, with no
\ garbage collection. It is based on the classical chain-of-blocks
\ idea, where each heap block is either used or free and allocation
\ uses the first free block of adequate size, splitting the block
\ into a used and free portion if necessary. Freeing a block will
\ cause the block to coalesce with adjacent free blocks, so all
\ blocks occupy the largest available contiguous free space. This
\ reduces -- but definitely does not eliminate -- heap fragmentation.
\
\ Each block is laid out as follows:
\    1 cell      size of block
\    1 byte      status
\    1 cell      address of previous block (0 if first)
\    1 cell      address of next block (0 if last)
\    n bytes     data segment                        <-- block addr


\ The size of the heap (32k)
1024 32 * VALUE HEAP-SIZE

\ The smallest data block we consider worthwhile
8 CELLS VALUE SMALLEST-HEAP-BLOCK

\ The size in bytes of a block header
3 CELLS 1+ VALUE HEAP-BLOCK-HEADER-SIZE

\ Return the header address of a block
: BLOCK>HEADER \ ( addr -- haddr )
    HEAP-BLOCK-HEADER-SIZE - ;

\ Return the addresses of the various header fields
: BLOCK>SIZE     BLOCK>HEADER ;
: BLOCK>STATUS   BLOCK>HEADER CELL+ ;
: BLOCK>NEXT     BLOCK>HEADER CELL+ 1+ ;
: BLOCK>PREVIOUS BLOCK>HEADER 2 CELLS + 1+ ;

\ Status flags
: BLOCK-FREE? BLOCK>STATUS C@ ;
: BLOCK-FREE BLOCK>STATUS 1 SWAP C! ;
: BLOCK-USED BLOCK>STATUS 0 SWAP C! ;

\ Return a block's data size
: BLOCK-SIZE BLOCK>SIZE @ ;

\ The heap itself
CREATE (HEAP) HEAP-SIZE ALLOT
(HEAP) HEAP-BLOCK-HEADER-SIZE + VALUE HEAP

\ Initialise the heap
: HEAP-INITIALISE
    HEAP-SIZE HEAP-BLOCK-HEADER-SIZE - HEAP BLOCK>SIZE !
    1 HEAP BLOCK>STATUS C!
    0 HEAP BLOCK>NEXT !
    0 HEAP BLOCK>PREVIOUS ! ;
HEAP-INITIALISE

\ Search for an unused block of sufficient size to hold the given
\ data, returning its address or 0
: FIND-BLOCK-FOR \ ( addr ns -- addr2 | 0 )
    BEGIN
	OVER 0<>
    WHILE
	    OVER BLOCK-FREE? IF
		OVER BLOCK-SIZE OVER >= IF
		    DROP EXIT
		THEN
	    THEN
	    SWAP BLOCK>NEXT @ SWAP
    REPEAT
    DROP ;

\ Decide whether a block is worth splitting. We split when the size of
\ the new block thus created will be greater than the smallest block
\ we consider useful
: BLOCK-WORTH-SPLITTING? \ ( addr ns -- f )
    >R BLOCK-SIZE R> - SMALLEST-HEAP-BLOCK > ;

\ Split a free block into two parts, one of the size requested and one with
\ the remaining size of the block, with all the correct links to
\ rebuild the block chain:
\    - calculate new block size and address, accounting for header
\    - set old size to that requested
\    - set new size to remainder
\    - point new next to old next
\    - point new next's previous to new, if new next is non-zero
\    - point old next to new
\    - point new previous to old
\    - mark new as in use
\ We only split a block if the new block produced will be of a useful size
: SPLIT-BLOCK \ ( addr ns -- )
    2DUP BLOCK-WORTH-SPLITTING? IF
	OVER BLOCK>SIZE @ OVER - HEAP-BLOCK-HEADER-SIZE - \ addr n n1
	>R 2DUP + HEAP-BLOCK-HEADER-SIZE + R>             \ addr n addr1 n1
	>R >R OVER BLOCK>SIZE ! R> R>                     \ addr addr1 n1
	OVER BLOCK>SIZE !                                 \ addr addr1
	OVER BLOCK>NEXT @ OVER BLOCK>NEXT !
	OVER BLOCK>NEXT @ ?DUP IF
	    BLOCK>PREVIOUS OVER SWAP !
	THEN
	OVER BLOCK>NEXT OVER SWAP !
	2DUP BLOCK>PREVIOUS !
	BLOCK-FREE BLOCK-USED
    THEN ;

\ Allocate a block of the given size, returning its address or 0 if no
\ block could be allocated. The block may be bigger than requested
: ALLOCATE ( n -- addr | 0 )
    HEAP OVER FIND-BLOCK-FOR DUP IF
	\ we found a block, so split it if necessary
	SWAP 2DUP SPLIT-BLOCK
    THEN
    DROP ;
    
\ Join a free block with its next block, if the next block is free
: JOIN-BLOCKS \ ( addr -- )
    DUP BLOCK>NEXT @                             \ addr addr1
    DUP BLOCK-FREE? IF
	DUP BLOCK-SIZE HEAP-BLOCK-HEADER-SIZE +  \ addr addr1 n1
	SWAP BLOCK>NEXT @                        \ addr n1 addr2
	DUP IF
	    DUP BLOCK>PREVIOUS 3 PICK SWAP !
	THEN
	2 PICK BLOCK>NEXT !                      \ addr n1
	OVER BLOCK-SIZE + SWAP BLOCK>SIZE !
    ELSE
	2DROP
    THEN ;

\ Free a block, performing any possible joining front and back
: FREE \ ( addr -- )
    DUP BLOCK-FREE
    DUP JOIN-BLOCKS
    BLOCK>PREVIOUS @ ?DUP IF
	DUP BLOCK-FREE? IF JOIN-BLOCKS ELSE DROP THEN
    THEN ;

\ Resize a block. This either resizes the block in place or
\ re-allocates it and moves its contents to the new space, or fails
\ and returns 0 without changing the original block. The code is
\ optimised to reduce the amount of unnecessary copying, possibly
\ at the expense of memory fragementation
\ TBD
: RESIZE 2DROP 0 ;


