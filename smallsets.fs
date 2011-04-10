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

\ Small sets represented as bit fields, numbered from 0
\
\ Using a but field lets us represent sets of up to /CELL 8 *
\ numbers in a single cell, and implement the set operations
\ very efficiently using bitwise operations

\ The maximum number of elements in a small set
/CELL 8 * CONSTANT SMALLSET-SIZE

\ Create new small sets
: EMPTY-SMALLSET ( -- ss ) 0 ;
: FULL-SMALLSET ( -- ss ) EMPTY-SMALLSET INVERT ;
    
\ Add element b to ss1 to give ss2
: SMALLSET-ADD ( b ss1 -- ss2 )
    1 -ROT LSHIFT OR ;
    
\ Remove element b from ss1 to give ss2
: SMALLSET-REMOVE ( b ss1 -- ss2 )
    1 -ROT LSHIFT INVERT AND ;

\ Check whether ss1 contains element b
: SMALLSET-CONTAINS? ( b ss1 -- f )
    SWAP RSHIFT 1 AND 0> ;

\ Return the union of ss1 and ss2
: SMALLSET-UNION ( ss1 ss2 -- ss3 )
    OR ;
    
\ Return the intersection of ss1 and ss2
: SMALLSET-INTERSECTION ( ss1 ss2 -- ss3 )
    AND ;

\ Return the element of ss1 not contained in ss2
: SMALLSET-NEGATE ( ss1 ss2 -- ss3 )
    INVERT SMALLSET-INTERSECTION ;



