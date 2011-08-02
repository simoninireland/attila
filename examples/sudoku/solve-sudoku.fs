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

\ Sudoku pretty-much-brute-force solver
\
\ The only intelligence here is to calculate all the possible values
\ for each cell, and only iterate through those rather than through
\ impossible options.


\ ---------- Possible value computations ----------

\ Return the top-left co-ordinates of the sub-square containing
\ the given point
: SUB-SQUARE-OF ( r c -- r' c' )
    3 / 3 * SWAP
    3 / 3 * SWAP ;

\ Remove values present in the given row from the given small set
: REMOVE-ROW-VALUES ( r sud ss -- ss' )
    GRID-SIZE 0 DO
	\ get the column value
	>R 2DUP R> -ROT  ( r sud ss r sud )
	I SWAP SUDOKU@   ( r sud ss v )
	SWAP SMALLSET-REMOVE
    LOOP
    -ROT 2DROP ;	

\ Remove values present in the given column from the given small set
: REMOVE-COLUMN-VALUES ( c sud ss -- ss' )
    GRID-SIZE 0 DO
	\ get the row value
	>R 2DUP R> -ROT  ( c sud ss c sud )
	I -ROT SUDOKU@    ( c sud ss v )
	SWAP SMALLSET-REMOVE
    LOOP
    -ROT 2DROP ;	

\ Remove values present in the given sub-square from the given small set
: REMOVE-SQUARE-VALUES ( r c sud ss -- ss' )
    3 0 DO
	3 0 DO
	    \ get the cell value
	    3 PICK J +
	    3 PICK I +
	    3 PICK SUDOKU@   ( r c sud ss v )
	    SWAP SMALLSET-REMOVE
	LOOP
    LOOP
    >R DROP 2DROP R> ;	

\ Return a small set of all the values that validly be attempted
\ in the given cell
: POSSIBLE-VALUES ( r c sud -- ss )
    >R
    FULL-SMALLSET    ( r c ss )
    2 PICK R@ ROT REMOVE-ROW-VALUES
    OVER   R@ ROT REMOVE-COLUMN-VALUES
    >R SUB-SQUARE-OF R> R> SWAP REMOVE-SQUARE-VALUES ;

\ Print the possible values
: .POSSIBLE ( ss -- )
    GRID-SIZE 1+ 1 DO
	I OVER SMALLSET-CONTAINS? IF
	    I . SPACE
	THEN
    LOOP
    DROP ;


\ ---------- Solver ----------

\ Helper functions
: 3DUP ( a b c -- a b c a b c )
    >R 2DUP R@ -ROT R> ;
: 3DROP ( a b c -- )
    DROP 2DROP ;
: NEXT-SQUARE ( r c -- r' c' )
    1+ DUP GRID-SIZE >= IF
	DROP
	1+ 0
    THEN ;
: VALID-SQUARE? ( r c -- f )
    DROP GRID-SIZE < ;

\ Solve the puzzle from the given cell onwards
: (SOLVE-SUDOKU) ( r c sud -- f )
    BEGIN
	3DUP DROP VALID-SQUARE?
    WHILE
	    3DUP SUDOKU@ NP = IF
		\ this cell is empty, cycle through its possibilities
		3DUP POSSIBLE-VALUES
		GRID-SIZE 1+ 1 DO       ( r c sud pss )
		    I OVER SMALLSET-CONTAINS? IF
			>R 3DUP R> 3 -ROLL     ( r c sud pss r c sud )
			I 3 -ROLL SUDOKU!      ( r c sud pss ) 
			>R 3DUP R> 3 -ROLL     ( r c sud pss r c sud ) 
			>R NEXT-SQUARE R> RECURSE IF
			    \ solved with this value, succeed
			    2DROP 2DROP TRUE EXIT
			THEN
		    THEN
		LOOP
		
		\ if we get here, we didn't find a solution, so
		\ re-set the cell to empty and fail
		DROP
		NP 3 -ROLL SUDOKU!
		FALSE EXIT
	    THEN

	    \ try the next cell
	    >R NEXT-SQUARE R>
    REPEAT

    \ if we get here, we succeeded
    3DROP TRUE ;

\ Solve a puzzle, returning a flag
: SOLVE-SUDOKU ( sud -- f )
    DUP CHECK-SUDOKU IF
	\ puzzle is initially valid, solve
	0 0 ROT (SOLVE-SUDOKU)
    ELSE
	\ invalid fail
	DROP FALSE
    THEN ;

		