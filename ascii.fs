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

\ Basic character operations, ASCII version 

\ Character constants
32 CONSTANT BL
10 CONSTANT NL

\ Case testing
: UC? 65 ( A )  90 ( Z ) 1+ WITHIN ;
: LC? 97 ( a ) 122 ( z )  1+ WITHIN ;

\ Case conversion
: >UC \ ( c -- uc )
    DUP LC? IF
	97 ( a ) -  65 ( A ) +
    THEN ;
: >LC \ ( c -- lc )
    DUP UC? IF
	65 ( A )  - 97 ( a ) +
    THEN ;

\ Test characters for equality. This is sometimes character-set dependent (although
\ we're probably just being overly picky)
: C= = ;
