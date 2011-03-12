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

\ A chain of values
\
\ These are used to store chains of things like strings in data memory.
\ Each link in the chain consists of a data block and a pointer to the next
\ link.
\
\ Given their nature the data in links can't be updated -- well not without
\ extreme care not to overflow the data block size, anyway...
\
\ You can use these functions directly to store strings. At the moment they
\ waste some space by using a cell instead of a byte for the length. This
\ might change, and we might restrict the data block size to a byte's-worth.

\ Return the next link in a chain, or zero
: NEXT-LINK-IN-CHAIN ( link -- link | 0 )
    @ ;

\ Return the address of the last link in a chain
: (LAST-LINK-IN-CHAIN) ( link -- link' )
    BEGIN
	DUP NEXT-LINK-IN-CHAIN ?DUP 0<>
    WHILE
	    NIP
    REPEAT ;

\ Create a link at the end of a chain, ready to data-compile the data block
: (NEW-LINK-IN-CHAIN) ( link -- )
    (LAST-LINK-IN-CHAIN)
    ALIGNED HERE SWAP A!     \ link previous link to HERE
    0 , ;                    \ store blank link

\ Add a link to a chain. The data added to the link is given by the
\ address and count pair
: ADD-LINK-TO-CHAIN ( addr n link -- )
    (NEW-LINK-IN-CHAIN)
    DUP ,                    \ store the size
    HERE OVER ALLOT          \ allot space for data block
    SWAP CMOVE ;             \ store the data block

\ Common typed links
: ADD-CELL-LINK-TO-CHAIN ( v link -- ) (NEW-LINK-IN-CHAIN) /CELL , , ;
: ADD-XT-LINK-TO-CHAIN ( xt link -- )  (NEW-LINK-IN-CHAIN) /CELL , XT, ;
: ADD-A-LINK-TO-CHAIN ( addr link -- ) (NEW-LINK-IN-CHAIN) /CELL , A, ;

\ Return the size and address of a link data block
: DATA-LINK-IN-CHAIN ( link -- addr n )
    /CELL +                   \ step over link
    DUP @ SWAP /CELL + SWAP ; \ data block address and size

\ Place the entire chain of data blocks on the stack, shallowest link first,
\ topped by a count
: ALL-DATA-LINKS-IN-CHAIN ( link -- addrj nj -- addr0 n0 j+1 )
    0 SWAP
    BEGIN
	NEXT-LINK-IN-CHAIN DUP 0<>
    WHILE ( c l )
	    DUP DATA-LINK-IN-CHAIN ( c l addr n )
	    2SWAP
	    SWAP 1+ SWAP
    REPEAT
    DROP ;
