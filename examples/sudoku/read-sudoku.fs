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

\ Sudoku reader functions
\
\ Rather than store the puzzles as data we define a syntax for
\ defining sudoku and then just execute the files describing the
\ puzzle

\ Read a sudoku from the terminal, represented as a list of GRID-SIZE-squared
\ digits from 0 to 9 plus . for an empty square
: READ-SUDOKU ( sud -- )
    GRID-SIZE 0 DO
	GRID-SIZE 0 DO
	    PARSE-WORD IF
		\ convert to number
		C@ DUP DIGIT? IF
		    NIP
		    DUP 1 GRID-SIZE 1+ WITHIN IF
			\ valid digit, store
			J I 3 PICK SUDOKU!
		    ELSE
			DROP S" Invalid value" ABORT
		    THEN
		ELSE
		    [CHAR] . C= IF
			NP J I 3 PICK SUDOKU!
		    ELSE
			S" Not a digit or empty cell marker" ABORT
		    THEN
		THEN
	    ELSE
		S" Incomplete grid" ABORT
	    THEN
	LOOP
    LOOP
    DROP ;

\ Read a sudoku from an open file
: (SUDOKU-FROM-FILE) ( sud fh -- )
    (<FROM)
    SWAP READ-SUDOKU
    FROM> ;

\ read a puzzle grid from the named file
: SUDOKU-FROM-FILE ( sud "fn" -- )
    PARSE-WORD
    2DUP R/O OPEN-FILE 0= IF
	ROT 2DROP (SUDOKU-FROM-FILE)
    ELSE
	DROP TYPE SPACE S" can't be opened" ABORT
    THEN ;

