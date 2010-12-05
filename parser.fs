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

\ Parsers for words, numbers, whitespace, delimiters etc
\ Note that these parsers can't use the string-handling functions
\ since they must be pre-loaded before them

\ ---------- Character reading ----------

\ Peek at the next character from the TIB, returning it or 0 if there are none
: PEEK-CHAR ( -- c | 0 )
    SOURCE 0= IF
	0
    ELSE
	C@
    THEN ;

\ Read a character from the TIB, returning it or 0 if there are none
: READ-CHAR ( -- c | 0 )
    PEEK-CHAR DUP IF
	1 >IN +!
    THEN ;

\ Read at most n characters into addr, returning the number actually retrieved
: ACCEPT ( addr n -- m )
    0 DO
	READ-CHAR ?DUP 0= IF
	    \ no character, finish reading
	    DROP I LEAVE
	ELSE
	    \ store character, move on
	    OVER C!
	    /CHAR +
	THEN
    LOOP ;
    
\ Consume all leading instances of the given character in the TIB
: CONSUME ( c -- )
    BEGIN
	PEEK-CHAR ?DUP 0= IF
	    DROP LEAVE
	ELSE
	    OVER C= NOT IF
		DROP LEAVE
	    THEN
	THEN
	READ-CHAR DROP
    AGAIN ;

\ Consume all leading whitespace in the TIB
: CONSUME-WS ( -- )
    BEGIN
	PEEK-CHAR ?DUP 0= IF
	    LEAVE
	ELSE
	    WS? NOT IF
		LEAVE
	    THEN
	THEN
	READ-CHAR DROP
    AGAIN ;


\ ---------- Words ----------

\ Parse up to the next instance of the given character, or the end of the
\ available input
: PARSE ( c -- addr n | 0 )
    \ read until we have some input to parse
    BEGIN
	SOURCE ?DUP 0=
    WHILE
	    EXHAUSTED? IF
		DROP 0 EXIT       \ no more input, fail
	    ELSE
		REFILL 0= IF
		    DROP 0 EXIT
		THEN
	    THEN
    REPEAT

    0 SWAP
    0 DO
	READ-CHAR 3 PICK C= IF
	    LEAVE                \ leave having consumed delimiter
	                         \ but without it counting in the
	                         \ string to be returned
	ELSE
	    1+
	THEN
    LOOP
    -ROT DROP ;

\ Parse a word delimited on either side by whitespace or the end-of-line
: PARSE-WORD ( -- addr n | 0 )
    \ read until we have a line containing non-whitespace
    BEGIN
	CONSUME-WS
	SOURCE ?DUP 0=
    WHILE
	    EXHAUSTED? IF
		0 EXIT       \ no more input, fail
	    ELSE
		REFILL 0= IF
		    0 EXIT
		THEN
	    THEN
    REPEAT

    \ parse the next word
    0 SWAP                   \ the length we've accepted
    0 DO
	READ-CHAR WS? IF
	    EXIT
	ELSE
	    1+
	THEN
    LOOP ;


\ ---------- Digits ----------

\ List of all possible digits, stored as a data block *not* a string constant,
\ so executing it will give ( -- caddr ) not ( -- addr n ). The first n characters
\ are the digits on base n
DATA DIGITS S" 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ" S,

\ Test whether the given character is a digit in the current number base,
\ case-insensitively, returning the digit and -1, or 0 if it's invalid
: DIGIT? ( c -- d -1 | 0 )
    >UC
    DIGITS 1+                    \ first character
    DIGITS C@ BASE @ MIN 0 DO    \ max digits or number base, whichever's less
	2DUP I + C@ C= IF
	    2DROP I TRUE EXIT
	THEN
    LOOP
    2DROP FALSE ;
	

\ ---------- Numbers ----------

\ Test whether the given character is a sign
: SIGN? ( c -- f )
    DUP  [CHAR] + C=
    SWAP [CHAR] - C= OR ;

\ Convert the given string into a number on the current number base
: UNSIGNED-NUMBER? ( addr n -- i -1 | 0 )
    0 SWAP
    0 DO
	OVER I + C@ DIGIT? IF
	    SWAP BASE @ * +
	ELSE
	    2DROP FALSE EXIT
	THEN
    LOOP
    NIP TRUE ;

\ Convert the given string into a number, taking account of any
\ leading sign
: NUMBER? ( addr n -- i -1 | 0 )
    \ turn an (optional) sign into a multiplier
    1 ROT \ multiplier
    OVER C@ SIGN? IF
	\ got a sign, deal with it
	OVER C@ [CHAR] - C= IF
	    \ negate the multiplier for a minus sign
	    -ROT NEGATE ROT
	THEN
	\ move past the sign
	1- SWAP /CHAR + SWAP
    THEN

    \ parse the remaining (unsigned) number and multiply
    \ by the multiplier derived from the sign
    UNSIGNED-NUMBER? IF
	* TRUE
    ELSE
	DROP FALSE
    THEN ;

    


	