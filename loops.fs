\ $Id$

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

\ Control loops
\
\ Compile various boolean-controlled loops, with enough information
\ for early exits etc to function correctly.
\
\ Note that these words are all immediate, and only work at
\ compile-time.
\ REQUIRE: cs-stack.fs

\ General loop begin
: BEGIN \ ( -- )
    0 (CS-START) ; IMMEDIATE

\ Unconditional branch back to previous BEGIN
: AGAIN \ ( -- )
    [COMPILE] (BRANCH) >BEGIN
    (CS-END) ; IMMEDIATE

\ Test the top of the stack and repeat until true
: UNTIL \ ( startaddr -- )
    [COMPILE] (?BRANCH) >BEGIN
    (CS-END) ; IMMEDIATE

\ Test the top of the stack and escape if false
: WHILE \ ( -- )
    [COMPILE] (?BRANCH) >END ; IMMEDIATE

\ Complete a while loop
: REPEAT \ ( -- )
    POSTPONE AGAIN ; IMMEDIATE
