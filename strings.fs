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

\ Various character and string operations
\ Requires: ascii.fs, variables.fs counted-loops.fs interpret-compile.fs


\ ---------- Compiling and printing ----------

\ Common things to output
: SPACE  BL       EMIT ;
: CR     NL       EMIT ;
: QUOTES [CHAR] " EMIT ;
: SPACES \ ( n -- )
    0 DO SPACE LOOP ;

\ Print a string when interpreting, or compile the code to do so
:NONAME \ ( "str" -- )
    BL CONSUME
    [CHAR] " PARSE TYPE ;
:NONAME \ ( "str" -- )
    POSTPONE S"
    [COMPILE] TYPE ;
INTERPRET/COMPILE ."

\ Immediately print anything up to the next closing bracket
: .( BL CONSUME [CHAR] ) PARSE TYPE CR ; IMMEDIATE

\ Place a string into data memory. Safe for empty strings
: S, \ ( addr n -- )
    DUP C,
    ?DUP 0> IF
	0 DO
	    DUP C@ C, 1 CHARS +
	LOOP
    THEN
    DROP ;

\ Append one string to another. There must be enough memory at addr1,
\ and the string mustn't overflow the 256 length limit
: S+ \ ( addr1 n1 addr2 n2 -- )
    2OVER CHARS +
    SWAP DUP >R
    CMOVE
    R> + SWAP 1 CHARS - C! ;
    
    
\ ---------- String operations ----------

\ The null string
: NULLSTRING 0 0 ;

\ Copy a string to the given address (including the count, so addr2 is a
\ counted string *not* just the characters)
: SMOVE \ ( addr1 n addr2 -- )
    2DUP C!
    1+ SWAP CMOVE ;

\ Turn an address into a counted string. Safe for null strings
: COUNT \ ( addr -- addr' len )
    DUP 0= IF
	DROP NULLSTRING
    ELSE
	DUP C@
	SWAP 1 CHARS + SWAP
    THEN ;

\ Check two strings for equality using the given comparison word
VARIABLE (SCOMPARER)
: (SCOMPARE) \ ( addr1 len1 addr2 len2 xt -- f )
    (SCOMPARER) XT!
    2 PICK = IF
	SWAP 0 DO
	    OVER C@ OVER C@ (SCOMPARER) XT@ EXECUTE IF
		1 CHARS + SWAP 1 CHARS + SWAP
	    ELSE
		2DROP 0 EXIT
	    THEN
	LOOP
	1
    ELSE
	DROP 0
    THEN
    ROT 2DROP ;


\ Check for string equality
: S= ['] C= (SCOMPARE) ;

\ Check for case-insensitive equality
: S=CI ['] C=CI (SCOMPARE) ;

\ Check for inequality
: S<> S= NOT ;

\ Reverse the characters in the given string
: REVERSE \ ( addr len -- )
    2DUP CHARS + 1 CHARS - SWAP
    2/ 0 DO                   \ start end
	2DUP 2DUP C@ SWAP C@  \ start end start end ce cs
	-ROT C!               \ start end start ce
	SWAP C!               \ start end
	1 CHARS - SWAP 1 CHARS + SWAP
    LOOP
    2DROP ;

\ Find the first index of the given character in a string, returning
\ its index or -1 if not found
: INDEX \ ( addr n c -- i )
    1 NEGATE
    3 ROLL 3 ROLL                         \ c -1 addr n 
    0 DO                                  \ c -1 addr
	DUP C@ 3 PICK C= IF
	    SWAP DROP I SWAP
	    LEAVE
	ELSE
	    1 CHARS +
	THEN
    LOOP
    DROP NIP ;

\ Translate the characters in addr n1 using the pair of strings
\ addr2 n2 and addr3 n3, with any character appearing in the second
\ being replace by the corresponding character in the third, which
\ must therefore be the same length
: TRANSLATE \ ( addr1 n1 addr2 n2 addr3 n3 -- )
    2 PICK OVER <> IF
	S" Translating strings of unequal length" ABORT
    THEN
    5 PICK 5 PICK 0 DO
	DUP C@
	5 PICK 5 PICK -ROT INDEX
	DUP 0 >= IF
	    CHARS 3 PICK + C@ OVER C!
	ELSE
	    DROP
	THEN
	1 CHARS +
    LOOP
    7 0 DO DROP LOOP ;
