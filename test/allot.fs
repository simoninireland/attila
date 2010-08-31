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

\ Tests of static data memory allocation and access

TESTCASES" Static data memory and access operations"

TESTING" Sizes and calculations"

{ 0 CELLS -> 0 }
{ 1 CELLS -> /CELL }
{ 2 CELLS -> /CELL 2* }

{ 0 CHARS -> 0 }
{ 1 CHARS -> /CHAR }
{ 2 CHARS -> /CHAR 2* }


TESTING" Allotting"

ALIGNED
{ HERE
  1 CHARS ALLOT
  HERE SWAP - -> /CHAR }
{ HERE
  1 CELLS ALLOT
  HERE SWAP - -> /CELL }
{ HERE
  0 ALLOT
  HERE SWAP - -> 0 }
{ HERE
  -1 ALLOT
  HERE SWAP - -> 0 }


TESTING" Data compilation"

\ Data compilation must leave HERE aligned
ALIGNED
{ 1 ,
  HERE HERE ALIGN = -> TRUE }

\ Cell-sized data compilation
ALIGNED
{ HERE
  1 ,
  @ -> 1 }
{ HERE
  1 ,
  HERE SWAP - -> /CELL }

\ Character-sized data compilation
{ HERE
  1 C,
  C@ -> 1 }
ALIGNED
{ HERE
  1 C,
  HERE SWAP - -> /CHAR }


TESTING" Code-space data compilation"

\ Data compilation must leave HERE aligned
CALIGNED
{ 1 COMPILE,
  TOP TOP CALIGN = -> TRUE }

\ Cell-sized data compilation
CALIGNED
{ TOP
  1 COMPILE,
  @ -> 1 }
{ TOP
  1 COMPILE,
  TOP SWAP - -> /CELL }

\ Character-sized data compilation
{ TOP
  1 CCOMPILE,
  C@ -> 1 }
CALIGNED
{ TOP
  1 CCOMPILE,
  TOP SWAP - -> /CHAR }


TESTING" Memory access"
\ sd: We stick to data allocations as code-space storing may fail

ALIGNED
{ HERE
  1 , 2 ,
  DUP @
  SWAP 1 CELLS + @ -> 1 2 }
{ HERE
  1 C, 2 C,
  DUP C@
  SWAP 1 CHARS + C@ -> 1 2 }
ALIGNED
{ HERE
  1 , 2 C,
  DUP @
  SWAP 1 CELLS + C@ -> 1 2 }

\ , should compile at an aligned address
ALIGNED
{ HERE
  1 C, 2 ,
  DUP C@
  SWAP 1 CHARS + ALIGN @ -> 1 2 }

ALIGNED
{ HERE
  1 ,
  3 OVER  !
  @ -> 3 }
{ HERE
  1 C,
  3 OVER C!
  C@ -> 3 }
ALIGNED
{ HERE
  1 , 2 ,
  3 OVER !
  1 CELLS + @ -> 2 }

ALIGNED
{ HERE
  1 , 2 ,
  2@ -> 2 1 }
{ HERE
  1 , 2 ,
  DUP 3 4 -ROT 2!
  2@ -> 3 4 }

{ HERE
  1 ,
  4 OVER +!
  @ -> 5 }


TESTING" Block movement"

\ Create a block of 20 cells, with intermediate pointers
DATA BLOCK1 20 CELLS ALLOT
BLOCK1 10 CELLS + CONSTANT BLOCK2
BLOCK1  8 CELLS + CONSTANT BLOCK3

: RANGE ( n1 n2 -- n1 n1+1 n1+2 ... n2-1 )
    SWAP DO I LOOP ;

: BLOCK-SETUP ( n0 n1 n2 ... nm-1 m addr -- )
    OVER 0 DO
	OVER I - 1+ -ROLL
	OVER I CELLS + !
    LOOP
    2DROP ;

: BLOCK-TEST ( n0 n1 n2 ... nm-1 m addr -- f )
    OVER 0 DO
	OVER I - 1+ -ROLL
	OVER I CELLS + @ <> IF
	    DROP
	    I - 1- NDROP
	    FALSE EXIT
	THEN
    LOOP
    2DROP TRUE ;

\ Test moving non-overlapping blocks
{ 0 10 RANGE 10 BLOCK1 BLOCK-SETUP
  BLOCK1 BLOCK2 10 CELLS CMOVE TRUE ->
  0 10 RANGE 10 BLOCK2 BLOCK-TEST }

\ Check we don't overrun
{ 999 BLOCK1 9 CELLS + !
  0 8 RANGE 8 BLOCK2 BLOCK-SETUP
  BLOCK2 BLOCK1 8 CELLS CMOVE
  BLOCK1 9 CELLS + @ -> 999 }

\ Test moving overlapping blocks 
{ 0 10 RANGE 10 BLOCK1 BLOCK-SETUP
  BLOCK1 BLOCK3 10 CELLS CMOVE> TRUE ->
  0 10 RANGE 10 BLOCK3 BLOCK-TEST }

