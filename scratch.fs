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

\ A scratch area for manipulating strings prior to safer storage
\ Requires createdoes.fs

\ ---------- Scratch area ----------

\ Size of the scratch area -- modify with care. Note that the implementation
\ allows for scratch areas greater than 256, i.e with non-byte string
\ counts
256 CONSTANT SCRATCH-SIZE

\ The scratch area
DATA SCRATCH SCRATCH-SIZE ALLOT

\ The current character index
VARIABLE SCRATCH-POINTER

\ Clear the scratch area
: CLEAR-SCRATCH \ ( -- )
    SCRATCH SCRATCH-POINTER A! ;

\ Return the number of characters in the scratch area
: #SCRATCH \ ( -- n )
    SCRATCH-POINTER A@ SCRATCH - ;

\ Move a string into the scratch area, over-writing anything there
: >SCRATCH \ ( addr n -- )
    CLEAR-SCRATCH
    DUP #SCRATCH + SCRATCH-SIZE >= IF
	S" Scratch area overflow" ABORT
    THEN
    >R
    SCRATCH-POINTER A@ R@ CMOVE
    R> SCRATCH-POINTER +! ;

\ Add a specific character to the scratch area
: HOLD \ ( c -- )
    SCRATCH-POINTER A@ C!
    1 SCRATCH-POINTER +!
    #SCRATCH SCRATCH-SIZE >= IF
	S" Scratch area overflow" ABORT
    THEN ;

\ Add the given string
: SHOLD ( addr n -- )
    0 DO
	DUP I + C@ HOLD
    LOOP
    DROP ;

\ Represent the string in the scratch area on the stack, ready for use
: SCRATCH> \ ( -- addr n )
    SCRATCH #SCRATCH ;

\ Initialise the scratch area
CLEAR-SCRATCH
