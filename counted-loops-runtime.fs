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

\ The run-time support needed by counted loops

\ Set up the loop, remembering the limit and initial indices
: (DO) \ ( limit initial -- )
    R> -ROT             \ ret limit initial
    SWAP >R >R
    >R ;

\ Increment the index and continue if it is strictly less than the limit
: (+LOOP) \ ( inc -- f )
    R> SWAP            \ ret inc
    1 RPICK R>         \ ret inc limit i
    ROT +              \ ret limit i'
    DUP ROT < IF       \ ret i'
        >R >R 0
    ELSE
        R> 2DROP >R 1
    THEN ;

\ Retrieve the index of the loop
: I \ ( -- i )
    1 RPICK ;

\ Retrieve the index of the surrounding loop
: J \ ( -- j )
    3 RPICK ;

\ Retrieve the index of the surrounding, surrounding loop
: K \ ( -- k )
    5 RPICK ;
