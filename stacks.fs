\ $Id: stacks.fs,v 1.6 2007/05/24 21:51:03 sd Exp $

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

\ General stacks
\
\ A collection of words for creating and manipulating stacks. These
\ complement rather than replace the data and return stack words,
\ which have been retained for performance reasons.
\
\ Since we're used by control structures we can't use them ourselves,
\ which explains the manual construction of conditionals to handle errors
\ and loops to handle mass popping.
\ Requires: createdoes.fs

\ ---------- Stack creation and layout ----------

\ Create a stack of a constant size in the dictionary
: STACK \ ( cells "name" -- )
    CREATE
    DUP ,                  \ size in cells
    0 ,                    \ current number of entries
    CELLS ALLOT            \ stack space
  DOES> \ ( -- st )
    2 CELLS + ;

\ Return the address of the number of stack elements
: ST>N \ ( st -- iaddr )
    CELL - ;

\ Return the maximum number of cells
: ST>MAXN \ ( st -- imax )
    2 CELLS - @ ;

\ Check if a push will result in an overflow
: ST-OVERFLOW? \ ( st -- f )
    DUP ST>N @ SWAP ST>MAXN = ;

\ Check if a pop will result in an underflow
: ST-UNDERFLOW? \ ( st -- f )
    ST>N @ 0 = ;

\ Return the address of the top of the stack, where the
\ next element will be pushed to
: ST>TOPADDR \ ( st -- addr )
    DUP ST>N @ CELLS + ;

\ Update the value of the stack index
: +STN \ ( n st -- )
    ST>N DUP
    @ -ROT + SWAP ! ;


\ ---------- Access ----------

\ Push an element from the data stack to the given stack
: >ST \ ( v st  -- )
    DUP ROT ST-OVERFLOW?
    [ ' (?BRANCH) COMPILE, TOP 0 COMPILE, ]   \ IF
	2DROP S" Stack overflow" ABORT
    [ DUP JUMP> SWAP ! ]                      \ THEN
    OVER ST>TOPADDR !
    1 SWAP +STN ;

\ Pop the top entry from the given stack
: ST> \ ( st -- v )
    DUP ST-UNDERFLOW?
    [ ' (?BRANCH) COMPILE, TOP 0 COMPILE, ]   \ IF
	2DROP S" Stack underflow" ABORT
    [ DUP JUMP> SWAP ! ]                      \ THEN
    DUP ST>TOPADDR CELL - @
    1 NEGATE -ROT +STN ;

\ Peek at the top entry of the given stack
: ST@ \ ( st -- v )
    DUP ST>N @ 0 =
    [ ' (?BRANCH) COMPILE, TOP 0 COMPILE, ]   \ IF
	2DROP S" Stack underflow" ABORT
    [ DUP JUMP> SWAP ! ]                      \ THEN
    ST>TOPADDR CELL - @ ;
    
\ Return the depth of the stack in cells
: #ST \ ( st -- n )
    ST>N @ ;

\ Return the address of the i-th cell, counting from 0
\ Very un-TIL-like, but useful....
: ST>ADDR \ ( i st -- addr )
    2DUP ST>N @ >
    [ ' (?BRANCH) COMPILE, TOP 0 COMPILE, ]   \ IF
	2DROP S" Stack pick underflow" ABORT
    [ DUP JUMP> SWAP ! ]                      \ THEN
    ST>TOPADDR SWAP 1+ CELLS - ;

\ Pick the i'th item from the stack, counting from 0
: ST-PICK \ ( i st -- n )
    ST>ADDR @ ;

\ Update-in-place the i'th element on the stack, counting from 0
\ Very un-TIL-like, but useful....
: ST-UPDATE \ ( v i st -- )
    ST>ADDR ! ;

\ Drop n items, or empty the stack, whichever is smaller
: ST-NDROP \ ( n st -- )
    OVER 0>
    [ ' (?BRANCH) COMPILE, TOP 0 COMPILE, ]                        \ IF
        DUP ST>N -ROT MIN
    NEGATE SWAP +STN
    [ ' (BRANCH) COMPILE, TOP 0 COMPILE, SWAP DUP JUMP> SWAP ! ]   \ ELSE
        2DROP
    [ DUP JUMP> SWAP ! ] ;                                         \ THEN

\ Drop 1 item
: ST-DROP \ ( st -- )
    1 SWAP ST-NDROP ;

\ Drop all items
: ST-CLEAR \ ( st -- )
    ST>N 0 SWAP ! ; 

\ Copy the complete stack onto the data stack, with a count
: ST-ALL> \ ( st -- st-1 ... st0 n )
    DUP >R #ST DUP >R >R
    [ TOP ]                                                      \ BEGIN
    R@ ?DUP
    [ ' (?BRANCH) COMPILE, TOP 0 COMPILE, ]                      \ WHILE
    1- 2 RPICK ST-PICK
    R> 1- >R
    [ ' (BRANCH) COMPILE, SWAP >JUMP COMPILE, DUP JUMP> SWAP ! ] \ REPEAT
    R> R> R> -ROT 2DROP ;

