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

\ Sudoku solver


\ Dimensions
9 CONSTANT GRID-SIZE

\ "Not present" marker for empty cells
0 CONSTANT NP

\ A sudoku grid
: SUDOKU ( "name" -- )
    DATA
    GRID-SIZE DUP * CELLS ALLOT ;

\ Copy one grid to another
: COPY-SUDOKU ( sud1 sud2 -- )
    GRID-SIZE DUP * CELLS CMOVE ;

\ The cell at row i, column j, numbered from 0
: SUDOKU-CELL ( i j sud -- addr )
    -ROT GRID-SIZE * CELLS -ROT CELLS + + ;

\ Access cells
: SUDOKU! ( v i j sud -- ) SUDOKU-CELL ! ;
: SUDOKU@ ( i j sud -- v ) SUDOKU-CELL @ ;

\ Pretty-print a puzzle grid
: .SUDOKU ( sud -- )
    GRID-SIZE 0 DO
	GRID-SIZE 0 DO
	    DUP J I -ROT SUDOKU@
	    DUP NP = IF
		DROP ." ."
	    ELSE
		.
	    THEN
	    SPACE

	    \ extra space between sub-squares
	    I 1+ 3 MOD 0= IF
		SPACE
	    THEN
	LOOP
	CR

	\ extra space between sub-squares
	I 1+ 3 MOD 0= IF
	    CR
	THEN
    LOOP
    DROP ;

\ The rest of the system
include examples/sudoku/read-sudoku.fs
include examples/sudoku/check-sudoku.fs
include examples/sudoku/solve-sudoku.fs
