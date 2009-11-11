\ $Id: counted-loops.fs,v 1.2 2007/05/17 15:33:45 sd Exp $

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

\ Counted loops
\
\ Loops over an index, with the test at the bottom so that we go through
\ the loop at least once.
\
\ Loops are scoped within a single colon-definition: you can access the
\ index of the loop with the word I, but *only* within the *same*
\ colon-definition as the DO ... LOOP code itself, not from any words
\ called from that code.
\ Requires: loops.fs, conditionals.fs

\ Set up the loop, remembering the limit and initial indices
: (DO) \ ( limit initial -- )
    R> ROT             \ ret limit initial
    SWAP >R >R
    >R ;

\ Increment the index and continue if it is strictly less than the limit
: (+LOOP) \ ( inc -- f )
    R> SWAP            \ ret inc
    1 RPICK R>         \ ret inc limit i
    -ROT +             \ ret limit i'
    DUP -ROT < IF      \ ret i'
        >R >R 0
    ELSE
        R> 2DROP >R 1
    THEN ;

\ Start the loop
: DO
    [COMPILE] (DO)
    2 (CS-START) ; IMMEDIATE \ after the (DO) so we don't re-do the setup code

\ End the loop, with arbitrary increment for the index
: +LOOP
    [COMPILE] (+LOOP)
    [COMPILE] (?BRANCH) >BEGIN
    (CS-END) ; IMMEDIATE

\ Increment the index by one
: LOOP
    1 POSTPONE LITERAL
    POSTPONE +LOOP ; IMMEDIATE

\ Retrieve the index of the loop
: I \ ( -- i )
    1 RPICK ;

\ Retrieve the index of the surrounding loop
: J \ ( -- j )
    3 RPICK ;

\ Retrieve the index of the surrounding, surrounding loop
: K \ ( -- k )
    5 RPICK ;
