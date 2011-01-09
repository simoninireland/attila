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

\ Tests of continuations

TESTCASES" Continuations"

TESTING" Simple continuation capture"

DATA TESTCC 100 CELLS ALLOT

{ 1 2 3 TESTCC ' NOOP CALL-CC -> 1 2 3 TESTCC }

{ 1 2 3 TESTCC ' DROP CALL-CC   -> 1 2 3 }
{ 1 2 3 TESTCC ' DROP CALL-CC 4 -> 1 2 3 4 }

: TEST-CC-SIMPLE
    TESTCC ['] DROP CALL-CC ;
{ 1 2 3 TEST-CC-SIMPLE   -> 1 2 3 }
{ 1 2 3 TEST-CC-SIMPLE 4 -> 1 2 3 4 }


TESTING" Continuation restoration"

: TEST-CC-MAKE-FALSE
    2DROP FALSE ;
: TEST-CC-AFTER
    TESTCC ['] TEST-CC-MAKE-FALSE CALL-CC IF 1 ELSE 2 THEN ;
{ TRUE TEST-CC-AFTER -> 2 }

: TEST-CC-AFTER2
    TRUE TESTCC ['] TEST-CC-MAKE-FALSE CALL-CC IF 1 ELSE 2 THEN ;
{ TEST-CC-AFTER2 -> 2 }
{ TRUE  TESTCC RUN-CONTINUATION -> 1 }
{ FALSE TESTCC RUN-CONTINUATION -> 1 } \ runs in previous stack


TESTING" Multiple re-runs"

VARIABLE TEST-CC-VAR
0 TEST-CC-VAR !
: TEST-CC-ADD
    1 TESTCC ['] DROP CALL-CC
    TEST-CC-VAR +! ;
{ TEST-CC-ADD             TEST-CC-VAR @ -> 1 }
{ TESTCC RUN-CONTINUATION TEST-CC-VAR @ -> 2 }
{ TESTCC RUN-CONTINUATION TEST-CC-VAR @ -> 3 }



