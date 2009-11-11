\ $Id: scratch.fs,v 1.3 2007/05/18 19:02:13 sd Exp $

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

\ A scratch area for manipulating strings prior to safer storage
\ Requires createdoes.fs

\ ---------- Scratch area ----------

\ Size of the scratch area -- modify with care
256 VALUE SCRATCH-SIZE

\ The scratch area
DATA SCRATCH SCRATCH-SIZE ALLOT

\ The current character index
VARIABLE SCRATCH-POINTER

\ Clear the scratch area
: CLEAR-SCRATCH \ ( -- )
    SCRATCH SCRATCH-POINTER ! ;

\ Move a string into the scratch area
: >SCRATCH \ ( addr n -- )
    >R
    SCRATCH-POINTER @ R@ CMOVE
    R> SCRATCH-POINTER +! ;

\ Return the number of characters in the scratch area
: #SCRATCH \ ( -- n )
    SCRATCH-POINTER @ SCRATCH - ;

\ Add a specific character to the scratch area
: HOLD \ ( c -- )
    SCRATCH-POINTER @ C!
    1 SCRATCH-POINTER +! ;

\ Represent the string in the scratch area on the stack, ready for use
: SCRATCH> \ ( -- addr n )
    SCRATCH SCRATCH-POINTER @ OVER - ;
