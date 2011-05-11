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

\ Tests of ring buffers

TESTCASES" Ring buffers"

10 RING TEST-RING

TESTING" Simple access and sizing"

{ TEST-RING RING-EMPTY
  1 TEST-RING >RING
  TEST-RING RING> -> 1 TRUE }

{ TEST-RING RING-EMPTY
  TEST-RING RING-SIZE -> 0 }

{ TEST-RING RING-EMPTY
  TEST-RING RING> -> FALSE }

{ TEST-RING RING-EMPTY
  TEST-RING RING-MAX-SIZE -> 10 }

{ TEST-RING RING-EMPTY
  1 TEST-RING >RING
  2 TEST-RING >RING
  TEST-RING RING>
  TEST-RING RING>
  TEST-RING RING> -> 1 TRUE 2 TRUE FALSE }


TESTING" Wrap-around"

{ TEST-RING RING-EMPTY
  TRUE
  :NONAME 15 0 DO
	  I TEST-RING >RING
	  TEST-RING RING> NOT IF
	      DROP FALSE
	      EXIT
	  ELSE
	      DROP
	  THEN
      LOOP ;
    EXECUTE -> TRUE }

{ TEST-RING RING-EMPTY
  TRUE
  :NONAME 15 0 DO
	  I TEST-RING >RING
	  TEST-RING RING> DROP
	  I = NOT IF
	      DROP FALSE
	      EXIT
	  THEN
      LOOP ;
    EXECUTE -> TRUE }

{ TEST-RING RING-EMPTY
  TRUE
  :NONAME 15 0 DO
	  I TEST-RING >RING
	  TEST-RING RING-SIZE 1 <> IF
	      DROP FALSE
	      EXIT
	  THEN
	  TEST-RING RING> 2DROP
      LOOP ;
    EXECUTE -> TRUE }

  
TESTING" Overwriting"

{ TEST-RING RING-EMPTY
  :NONAME 15 0 DO I TEST-RING >RING LOOP ;
  EXECUTE
  TEST-RING RING-SIZE -> 10 }
  
{ TEST-RING RING-EMPTY
  :NONAME 15 0 DO I TEST-RING >RING LOOP ;
  EXECUTE
  TRUE
  :NONAME 10 0 DO TEST-RING RING> DROP I 5 + <> IF DROP FALSE EXIT THEN LOOP ;
  EXECUTE -> TRUE }
  


