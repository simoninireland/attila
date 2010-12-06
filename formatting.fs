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

\ Formatted numeric output
\ Requires parser.fs, strings.fs, scratch.fs, counted-loops.fs


\ ---------- Standard number bases ----------
\ By convention the system is in DECIMAL when it boots

: BINARY   2 BASE ! ;
: DECIMAL 10 BASE ! ;
: HEX     16 BASE ! ;


\ ---------- Formatted numbers ----------

\ Initialise the scratch area ready for formatting numbers
: <# \ ( -- )
    CLEAR-SCRATCH ;

\ Convert the formatted number in the scratch area to a string and
\ place the address and length onto the stack. This will be
\ overwritten at the next use of the scratch area
: #> \ ( n -- addr len )
    DROP
    SCRATCH>
    2DUP REVERSE ;

\ Convert a number to a digit character.
: DIGIT> \ ( n -- c )
    DIGITS COUNT DROP SWAP CHARS + C@ ;

\ Add the least significant digit of a number to the scratch area,
\ leaving a quotient
: # \ ( n -- q )
    BASE @ /MOD SWAP DIGIT> HOLD ;

\ Convert the rest of the number
: #S \ ( n -- n' )
    BEGIN
	#
	DUP NOT
    UNTIL ;

\ Append a - sign if the number is negative
: SIGN \ ( n -- )
    0 < IF [CHAR] - HOLD THEN ;

\ Convert a number to a string using the normal formatting conventions
: NUMBER> \ ( n -- addr n )
    <# DUP ABS #S SWAP SIGN #> ;


\ ---------- Utility routines ----------

\ Print a number
: . \ ( n -- )
    NUMBER> TYPE ;

\ Print the contents of the cell at the given address
: ? \ ( addr -- )
    @ . ;

\ Print in decimal, hex and binary without changing the current number base
: .DEC BASE @ SWAP DECIMAL . BASE ! ;
: .HEX BASE @ SWAP HEX     . BASE ! ;
: .BIN BASE @ SWAP BINARY  . BASE ! ;

\ Print the contents of the stack, top item on the right
: .S \ ( -- )
    DEPTH DUP [CHAR] < EMIT <# #S #> TYPE [CHAR] > EMIT SPACE
    DUP 0 > IF
	DUP 0 DO
	    DUP I - PICK . SPACE
	LOOP
    THEN DROP ;
