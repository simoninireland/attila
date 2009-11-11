\ $Id: strings.fs,v 1.8 2007/06/15 15:32:17 sd Exp $

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

\ Various character and string operations
\ Requires: variables.fs counted-loops.fs interpret-compile.fs

\ ---------- Character operations ----------

\ Character constants
32 CONSTANT BL
13 CONSTANT NL

\ Push the first character of a string onto the stack
: CHAR \ ( addr len -- c )
    DROP C@ ;

\ Extract the first character of the next word in the input stream
: [CHAR] \ ( "word" -- )
    PARSE-WORD CHAR
    POSTPONE LITERAL ; IMMEDIATE

\ Common things to output
: SPACE BL EMIT ;
: CR NL EMIT ;
: SPACES \ ( n -- )
    0 DO SPACE LOOP ;


\ ---------- Compiling and printing ----------

\ Print a string when interpreting or compiling
:NONAME \ ( "str" -- )
    [CHAR] " PARSE TYPE ;
:NONAME \ ( "str" -- )
    POSTPONE S"
    [COMPILE] TYPE ; IMMEDIATE
INTERPRET/COMPILE ."


\ ---------- Moving memory around ----------

: CMOVE> \ ( addr1 addr2 n -- ) with addr1 < addr2
    DUP >R + SWAP R@ + SWAP
    R> 0 DO
	1- SWAP 1- SWAP
	OVER C@ OVER C!
    LOOP 2DROP ;

: CMOVE< \ ( addr1 addr2 n -- ) with addr2 < addr1
    0 DO
	OVER C@ OVER C!
	1+ SWAP 1+ SWAP
    LOOP 2DROP ;

\ Move n bytes starting at addr1 to addr2, handling any overlap
: CMOVE \ ( addr1 addr2 n-- )
    2 PICK 1 PICK < IF CMOVE> ELSE CMOVE< THEN ;

    
\ ---------- String operations ----------

\ Turn an address into a counted string
: COUNT \ ( addr -- addr' len )
    DUP C@
    SWAP 1+ SWAP ;

\ Check two strings for equality
: S= \ ( addr1 len1 addr2 len2 -- f )
    2 PICK = IF
	SWAP 0 DO  \ addr1 addr2
	    OVER C@ OVER C@ = IF
		1+ SWAP 1+
	    ELSE
		2DROP 0 EXIT
	    THEN
	LOOP
	1
    ELSE
	DROP 0
    THEN
    ROT 2DROP ;

\ Check for inequality
: S<> S= NOT ;

\ Reverse the characters in the given string
: REVERSE \ ( addr len -- )
    2DUP + 1- SWAP
    2/ 0 DO                   \ start end
	2DUP 2DUP C@ SWAP C@  \ start end start end ce cs
	-ROT C!               \ start end start ce
	SWAP C!               \ start end
	1- SWAP 1+ SWAP
    LOOP
    2DROP ;
