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

\ Test cases for sudoku solver

TESTCASES" Sudoku solving"

SUDOKU test
SUDOKU solved


TESTING" Validity"

\ Valid
{ test SUDOKU-FROM-FILE examples/sudoku/test/1.sudoku
  test CHECK-SUDOKU -> TRUE }
{ test SUDOKU-FROM-FILE examples/sudoku/test/3.sudoku
  test CHECK-SUDOKU -> TRUE }

\ Invalid in several different ways
test SUDOKU-FROM-FILE examples/sudoku/test/2.sudoku
{ test CHECK-SUDOKU -> FALSE }
{ 0 test CHECK-ROW -> FALSE }
{ 1 test CHECK-ROW -> TRUE }
{ 0 test CHECK-COLUMN -> TRUE }
{ 8 test CHECK-COLUMN -> FALSE }
{ 0 0 test CHECK-SQUARE -> TRUE }
{ 6 0 test CHECK-SQUARE -> FALSE }


TESTING" Solving"

\ Solvable
test SUDOKU-FROM-FILE examples/sudoku/test/1.sudoku
test solved COPY-SUDOKU
{ solved SOLVE-SUDOKU
  solved CHECK-SUDOKU
  -> TRUE TRUE }

\ Solvable, but way harder
test SUDOKU-FROM-FILE examples/sudoku/test/3.sudoku
test solved COPY-SUDOKU
{ solved SOLVE-SUDOKU
  solved CHECK-SUDOKU
  -> TRUE TRUE }

\ Invalid
test SUDOKU-FROM-FILE examples/sudoku/test/2.sudoku
test solved COPY-SUDOKU
{ solved SOLVE-SUDOKU -> FALSE }


