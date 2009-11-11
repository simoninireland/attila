\ $Id: formatting.fs,v 1.7 2007/05/18 19:02:13 sd Exp $

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

\ Formatted numeric output
\ Requires strings.fs, scratch.fs, counted-loops.fs

\ ---------- The numeric character set ----------

VARIABLE MAX-BASE
DATA NUMERIC-CHARACTER-SET
SCRATCH SCRATCH-POINTER !                                  \ manually initialise
PARSE-WORD 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ >SCRATCH   \ copy to scratch area
#SCRATCH MAX-BASE !                                        \ avoid overruns
SCRATCH> DUP ALLOT NUMERIC-CHARACTER-SET SWAP CMOVE        \ move into character set


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
: >DIGIT \ ( n -- c )
    NUMERIC-CHARACTER-SET + C@ ;

\ Add the least significant digit of a number to the scratch area,
\ leaving a quotient
: # \ ( n -- q )
    BASE @ MAX-BASE @ MIN /MOD SWAP >DIGIT HOLD ;

\ Convert the rest of the number
: #S \ ( n -- n' )
    BEGIN
	#
	DUP NOT
    UNTIL ;

\ Append a - sign if the number is negative
: SIGN \ ( n -- )
    0 < IF [CHAR] - HOLD THEN ;


\ ---------- Utility routines ----------

\ Print a number
: . \ ( n -- )
    <# DUP ABS #S SWAP SIGN #> TYPE ;

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


