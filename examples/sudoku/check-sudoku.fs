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

\ Sudoku validity checker

\ Check whether the given value can legitimately be encountered in
\ the collection of cells represented by the small set, returning
\ a flag and a new small set containing the element. NP cells are skipped
: VALUE-CAN-APPEAR? ( ss v -- ss' -1 | 0 )
    \ bypass empty cells
    DUP NP <> IF
	\ check if present
	2DUP SWAP SMALLSET-CONTAINS? IF
	    \ element already present and not zero, fail
	    2DROP FALSE
	ELSE
	    \ element not present, set it and return the new set
	    SWAP SMALLSET-ADD TRUE
	THEN
    ELSE
	\ nothing to do, return the initial set
	DROP TRUE
    THEN ;

\ Check that the given row is valid, containing no digit more than once
: CHECK-ROW ( r sud -- f )
    EMPTY-SMALLSET
    GRID-SIZE 0 DO ( r sud ss )
	\ get the column value
	>R 2DUP R> ROT         ( r sud ss r sud )
	I SWAP SUDOKU@         ( r sud ss v )

	\ check validity
	VALUE-CAN-APPEAR? NOT IF
	    \ illegal value, fail
	    2DROP FALSE EXIT
	THEN
    LOOP
    DROP 2DROP TRUE ;
	    
\ Check that the given column is valid, containing no digit more than once
: CHECK-COLUMN ( c sud -- f )
    EMPTY-SMALLSET
    GRID-SIZE 0 DO ( c sud ss )
	\ get the row value
	>R 2DUP R> ROT         ( c sud ss c sud )
	I ROT SUDOKU@          ( c sud ss v )

	\ check validity
	VALUE-CAN-APPEAR? NOT IF
	    \ illegal value, fail
	    2DROP FALSE EXIT
	THEN
    LOOP
    DROP 2DROP TRUE ;

\ Check the 3x3 square at position (r,c) as its top-left corner
: CHECK-SQUARE ( r c sud -- f )
    EMPTY-SMALLSET
    3 0 DO
	3 0 DO ( r c sud ss )
	    \ get the point
	    3 PICK J +      ( r c sud ss r' )
	    3 PICK I +      ( r c sud ss r' c' )
	    3 PICK SUDOKU@  ( r c sud ss v )
	    
	    \ check validity
	    VALUE-CAN-APPEAR? NOT IF
		\ illegal value, fail
		DROP 2DROP FALSE EXIT
	    THEN
	LOOP
    LOOP
    2DROP 2DROP TRUE ;

\ Check puzzle for validity
: CHECK-SUDOKU ( sud -- f )
    \ check rows and columns
    GRID-SIZE 0 DO
	I OVER CHECK-ROW NOT IF
	    DROP FALSE EXIT
	THEN
	I OVER CHECK-COLUMN NOT IF
	    DROP FALSE EXIT
	THEN
    LOOP

    \ check squares
    3 0 DO
	3 0 DO
	    DUP J 3 * I 3 * -ROT CHECK-SQUARE NOT IF
		DROP FALSE EXIT
	    THEN
	LOOP
    LOOP

    \ if we get here, all's well
    DROP TRUE ;

	
	
	    



