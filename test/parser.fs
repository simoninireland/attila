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

\ Tests of parsers

TESTCASES" Parsers"

TESTING" Digit parsing"

DECIMAL
{ [CHAR] 0 DIGIT? -> 0 TRUE }
{ [CHAR] 9 DIGIT? -> 9 TRUE }
{ [CHAR] A DIGIT? -> FALSE }
HEX
{ [CHAR] 0 DIGIT? -> 0 TRUE }
{ [CHAR] A DIGIT? -> A TRUE }
{ [CHAR] a DIGIT? -> A TRUE }
{ [CHAR] F DIGIT? -> F TRUE }
{ [CHAR] G DIGIT? -> FALSE }
DECIMAL


TESTING" Unsigned number parsing"

DECIMAL
{ PARSE-WORD 0    UNSIGNED-NUMBER? -> 0 TRUE }
{ PARSE-WORD 1    UNSIGNED-NUMBER? -> 1 TRUE }
{ PARSE-WORD 01   UNSIGNED-NUMBER? -> 1 TRUE }
{ PARSE-WORD A    UNSIGNED-NUMBER? -> FALSE }
{ PARSE-WORD 1234 UNSIGNED-NUMBER? -> 1234 TRUE }
{ PARSE-WORD 123A UNSIGNED-NUMBER? -> FALSE }
{ PARSE-WORD -1   UNSIGNED-NUMBER? -> FALSE }
HEX
{ PARSE-WORD A    UNSIGNED-NUMBER? -> A TRUE }
DECIMAL


TESTING" Number parsing"

{ PARSE-WORD 0  NUMBER? -> 0        TRUE }
{ PARSE-WORD 1  NUMBER? -> 1        TRUE }
{ PARSE-WORD -1 NUMBER? -> 1 NEGATE TRUE }
{ PARSE-WORD A  NUMBER? -> FALSE }
