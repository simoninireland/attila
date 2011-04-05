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

\ High-level I/O operations
\
\ These words provide high-level access to I/O operations, both
\ for files and (mainly) for line-based input. Unlike the primitives
\ many of these operations are blocking, making them easier to
\ use in most programs.

\ ---------- Blocking file access ----------

\ Read data from a file, returning the number of characters read and any
\ error code (0 for success). Return ( -- 0 0 ) if reading (successfully)
\ beyond the eof
: READ-FILE ( addr n fh -- m ior )
    \ loop until we either fail or get something
    BEGIN
	\ primitive non-blocking read
	2 PICK 2 PICK 2 PICK (READ-FILE)
	
	\ check for error or eof
	DUP 0< IF
	    \ read error, exit
	    4 ROLL 4 ROLL DROP 2DROP EXIT
	ELSE
	    OVER 0< IF
		\ eof, return 0 0
		2DROP DROP 2DROP 0 0 EXIT
	    ELSE
		OVER 0> IF
		    \ characters, succeed
		    4 ROLL 4 ROLL DROP 2DROP  EXIT
		ELSE
		    \ no characters, loop
		    2DROP
		THEN
	    THEN
	THEN
    AGAIN ;
    
\ Write data to a file -- currently just a wrapper, but probably
\ needs to be refactored for non-blocking read
: WRITE-FILE ( addr n f -- ior )
    (WRITE-FILE) ;


\ ---------- Line-based I/O ----------

\ Empty the line buffer, forcing a refill at the next attempt to
\ read input
: EMPTY-TIB ( -- )
    0 TIB A@ C!
    -1 >IN ! ;

\ Display a prompt for user input. This only happens if we're reading
\ from the terminal (standard input)
: PROMPT ( -- )
    SOURCE-ID 0= IF
	32 EMIT ( SPACE ) S" ok " TYPE
    THEN ;

\ Read a line of text from the input source, returning a "data available" flag
: REFILL ( -- f )
    \ prompt for user input if appropriate
    PROMPT

    \ read a line of data into the TIB
    TIB A@ 1+ 0
    BEGIN
	\ read a character
	OVER 1 INPUTSOURCE @ READ-FILE

	\ check for errors or eof
	2DUP 0= SWAP 0= AND     \ eof
	SWAP 0<> OR NIP IF      \ read error
	    \ set for refill and exit
	    2DROP
	    EMPTY-TIB
	    FALSE EXIT
	THEN

	\ check for eol
	OVER C@ 10 C= IF
	    \ eol, store length and succeed
	    NIP TIB A@ C!
	    0 >IN !
	    TRUE EXIT
	THEN

	\ next character
	1+ SWAP 1+ SWAP
    AGAIN ;
	    
\ Return the input point and length remaining in the TIB
: SOURCE ( -- addr n )
    >IN @ 0< IF
	\ input pointer negative, we already need a refill
	TIB A@ 1+ 0
    ELSE
	\ characters remaining
	TIB A@ C@ >IN @ -

	\ offset to the correct address
	TIB A@ 1+ >IN @ + SWAP
    THEN ;
