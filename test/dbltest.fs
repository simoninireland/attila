\ test some double primitives

\ Copyright (C) 1996,1998,1999,2000,2003 Free Software Foundation, Inc.

\ Modified for kForth by David N. Williams, March 30, 2006.
\ Backported for kForth 1.3.x by K. Myneni, May 31, 2006.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation; either version 2
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program; if not, write to the Free Software
\ Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

include tester.4th
\ include coretest.4th
decimal

\ dnw, from core.4th
0 INVERT			CONSTANT MAX-UINT
0 INVERT 1 RSHIFT		CONSTANT MAX-INT
0 INVERT 1 RSHIFT INVERT	CONSTANT MIN-INT

\ fm/mod, sm/rem, um/mod, s>d, m*, um* already covered in coretest.fs

testing M+ D+ D- DNEGATE

{ 0 0 0 m+ -> 0 0 }
{ 0 0 1 m+ -> 1 0 }
{ 0 0 -1 m+ -> -1 -1 }
{ 1 0 -1 m+ -> 0 0 }
{ MAX-UINT 0 1 m+ -> 0 1 }
{ MAX-UINT MAX-UINT 1 m+ -> 0 0 }

{ 0 0 0 0 d+ -> 0 0 }
{ 1 0 -1 -1 d+ -> 0 0 }
{ -1 -1 1 0 d+ -> 0 0 }
{ -1 -1 -1 -1 d+ -> -2 -1 }
{ MAX-UINT 0 2dup d+ -> MAX-UINT 1- 1 }
{ MAX-UINT 1 1 1 d+ -> 0 3 }

{ 0 0 0 0 d- -> 0 0 }
{ 0 0 1 0 d- -> -1 -1 }
{ 0 0 -1 -1 d- -> 1 0 }
{ 1 0 0 0 d- -> 1 0 }
{ 1 0 1 0 d- -> 0 0 }
{ -1 -1 -1 -1 d- -> 0 0 }
{ 1 0 -1 -1 d- -> 2 0 }
{ -1 -1 1 0 d- -> -2 -1 }
{ 0 2 1 0 d- -> MAX-UINT 1 }

{ 0 0 dnegate -> 0 0 }
{ 1 0 dnegate -> -1 -1 }
{ -2 -1 dnegate -> 2 0 }
{ 0 1 dnegate -> 0 -1 }
{ 1 1 dnegate -> MAX-UINT -2 }

comment Skipping D2* D2/
(
{ 1 0 d2* -> 2 0 }
{ -10 -1 d2* -> -20 -1 }
{ MAX-UINT 1 d2* -> MAX-UINT 1- 3 }

{ 0 0 d2/ -> 0 0 }
{ 1 0 d2/ -> 0 0 }
{ -1 -1 d2/ -> -1 -1 }
{ MAX-UINT 3 d2/ -> MAX-UINT 1 }
)

testing D= D<

{ 0 0 0 0 d= -> true }
{ 0 0 1 0 d= -> false }
{ 0 1 0 0 d= -> false }
{ 1 1 0 0 d= -> false }

{ 1 0 1 0 d< -> false }
{ 0 0 1 0 d< -> true }
{ 1 0 0 1 d< -> true }
{ 0 1 1 0 d< -> false }
{ -1 -1 0 0 d< -> true }

\ added by dnw:
{  0  3 -1  2 d< -> false }
{ -1  2  0  3 d< -> true }
{ -1  2 -1  2 d< -> false }
\ to catch use of D-, which overflows
{  0 MIN-INT -1 MAX-INT d< -> true }
{  0  3 dnegate ->  0 -3 }
{  1  2 dnegate -> -1 -3 }
\ to catch failure to use low word unsigned compare
{  0  3 -1  3 d< -> true }
{  0 -3 -1 -3 d< -> true }
{ -1  3  0  3 d< -> false }
{ -1 -3  0 -3 d< -> false }

comment Skipping D<> D> D>= D<=
(
{ 0 0 0 0 d<> -> false }
{ 0 0 1 0 d<> -> true }
{ 0 1 0 0 d<> -> true }
{ 1 1 0 0 d<> -> true }

{ 1 0 1 0 d> -> false }
{ 0 0 1 0 d> -> false }
{ 1 0 0 1 d> -> false }
{ 0 1 1 0 d> -> true }
{ -1 -1 0 0 d> -> false }

{ 1 0 1 0 d>= -> true }
{ 0 0 1 0 d>= -> false }
{ 1 0 0 1 d>= -> false }
{ 0 1 1 0 d>= -> true }
{ -1 -1 0 0 d>= -> false }

{ 1 0 1 0 d<= -> true }
{ 0 0 1 0 d<= -> true }
{ 1 0 0 1 d<= -> true }
{ 0 1 1 0 d<= -> false }
{ -1 -1 0 0 d<= -> true }
)

\ Since the d-comparisons, the du-comparisons, and the d0-comparisons
\ are generated from the same source, we only test the ANS words in
\ the following.

testing D0= 

{ 0 0 d0= -> true }
{ 1 0 d0= -> false }
{ 0 1 d0= -> false }
{ 1 1 d0= -> false }
{ -1 -1 d0= -> false }

comment Skipping D0<
(
{ 0 0 d0< -> false }
{ -1 -1 d0< -> true }
{ -1 0 d0< -> false }
{ 0 min-int d0< -> true }
)

comment Skipping DU<
(
{ 1 0 1 0 du< -> false }
{ 0 0 1 0 du< -> true }
{ 1 0 0 1 du< -> true }
{ 0 1 1 0 du< -> false }
{ -1 -1 0 0 du< -> false }
)