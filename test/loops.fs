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

\ Tests of loops

TESTCASES" Loops"

TESTING" BEGIN ... UNTIL loops"

{ :NONAME 3 BEGIN DUP 1- DUP 0= UNTIL ; EXECUTE -> 3 2 1 0 }


TESTING" BEGIN ... WHILE ... REPEAT loops"

{ :NONAME 3 BEGIN DUP 0 > WHILE DUP 1- REPEAT ; EXECUTE -> 3 2 1 0 }
{ :NONAME 3 BEGIN DUP 5 > WHILE DUP 1- REPEAT ; EXECUTE -> 3 }


TESTING" Counted loops"

{ :NONAME 3 0 DO I    LOOP ; EXECUTE -> 0 1 2 }
{ :NONAME 6 0 DO I 2 +LOOP ; EXECUTE -> 0 2 4 }

\ go through DO loops at least once
{ :NONAME 0 0 DO I LOOP ; EXECUTE -> 0 }
{ :NONAME 0 6 DO I LOOP ; EXECUTE -> 6 }


TESTING" Nested loop index access"

{ :NONAME 2 0 DO I 2 0 DO I               LOOP LOOP ; EXECUTE -> 0 0 1 1 0 1 }
{ :NONAME 2 0 DO I 2 0 DO J               LOOP LOOP ; EXECUTE -> 0 0 0 1 1 1 }
{ :NONAME 2 0 DO I 2 0 DO J 2 0 DO K LOOP LOOP LOOP ; EXECUTE -> 0 0 0 0 0 0 0 1 1 1 1 1 1 1 }


TESTING" Leaving loops from the middle"

{ :NONAME 10 BEGIN              1- DUP 5 = IF LEAVE THEN DUP 0= UNTIL  1+ ; EXECUTE -> 6 }
{ :NONAME 10 BEGIN DUP 0> WHILE 1- DUP 5 = IF LEAVE THEN        REPEAT 1+ ; EXECUTE -> 6 }

\ using EXIT means we shouldn't execute the last 1+
{ :NONAME 10 BEGIN              1- DUP 5 = IF EXIT THEN DUP 0= UNTIL  1+ ; EXECUTE -> 5 }
{ :NONAME 10 BEGIN DUP 0> WHILE 1- DUP 5 = IF EXIT THEN        REPEAT 1+ ; EXECUTE -> 5 }

{ :NONAME 10 0 DO I DUP 2 = IF LEAVE THEN LOOP 1+ ; EXECUTE -> 0 1 3 }
{ :NONAME 10 0 DO I DUP 2 = IF EXIT  THEN LOOP 1+ ; EXECUTE -> 0 1 2 }



