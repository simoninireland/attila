\ $Id: colon.fs,v 1.6 2007/05/18 20:33:25 sd Exp $

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

\ The initial dictionary containing the words necessary to bring up
\ the colon-definiton compiler.
\
\ This code is re-written in C in initial.c, but may be bootstrapped
\ at some point (like when we do a cross-compiler). For now, use it
\ for reference in case the C code isn't clear.

\ ---------- Error handling ----------

\ Abort with an error, re-starting the executive
: ABORT \ ( addr len -- )
    TYPE
    SPACE TYPE
    WARM ;


\ ---------- Compilation helpers ----------

\ Set a status mask for a colon definition
: SET-STATUS \ ( mask xt -- )
    >STATUS DUP C@
    -ROT OR
    SWAP C! ;

\ Make the previous word IMMEDIATE
: IMMEDIATE \ ( -- )
    IMMEDIATE_MASK LASTXT SET-STATUS ;

\ Make the previous word REDIRECTABLE
: REDIRECTABLE \ ( -- )
    REDIRECTABLE_MASK LASTXT SET-STATUS ;

\ Test whether we're interpreting
: INTERPRETING? \ ( -- f )
    STATE @ INTERPRETATION-STATE = ;

\ Test whether a word is IMMEDIATE
: IMMEDIATE? \ ( xt -- f )
    >STATUS C@ IMMEDIATE-MASK & ;

\ Find the xt of the next word in the input source, or abort
: ' \ ( "word" -- xt )
    PARSE-WORD 2DUP FIND
    ?DUP IF
	ROT 2DROP
    ELSE
	TYPE
	S" ?" ABORT
    THEN ;


\ ---------- Colon-definition words ----------

\ Compile the top of the stack as a literal
: LITERAL \ ( n -- )
    ['] (LITERAL) COMPILE,
    COMPILE, ; IMMEDIATE

\ Compile the string on the tp of the stack as a literal
: SLITERAL \ ( addr len -- )
    ['] (SLITERAL) COMPILE,
    S, ; IMMEDIATE

\ Compile a "-delimited string from the input source as a literal
: S" \ ( "string" -- )
    BL CONSUME
    [CHAR] " PARSE
    ?DUP IF
	SLITERAL
    ELSE
	S" String not delimited" ABORT
    THEN ;

\ Enter interpretation state
: [ \ ( -- )
    INTERPRETATION-STATE STATE ! ;

\ Enter comilation state
: ] \ ( -- )
    COMPILATION-STATE STATE ! ;

\ The colon-definer
: : \ ( "name" -- )
    [ ' (DOCOLON) >CFA @ ] PARSE-WORD HEADER, CFA,
    ] ;

\ Complete a colon-definition
: ; \ ( -- )
    0 COMPILE, \ NEXT
    [ ; IMMEDIATE


\ ---------- The outer interpreter ----------

\ The outer executive
: OUTER \ ( -- )
    BEGIN
	PARSE-WORD ?DUP NOT IF
	    2DUP FIND ?DUP IF
		ROT 2DROP INTERPRETING?
		OVER IMMEDIATE? OR IF
		    EXECUTE
		ELSE
		    COMPILE,
		THEN
	    ELSE
		2DUP NUMBER? IF
		    ROT 2DROP INTERPRETING? NOT IF
			LITERAL
		    THEN
		ELSE
		    TYPE S" ?" ABORT
		THEN
	    THEN
	THEN
    EXHAUSTED? UNTIL ;


\ ---------- Including files ----------

\ Load a file
: (LOAD) \ ( fh -- )
    INPUTSOURCE DUP @
    2 PICK 2 PICK !

    \ run the executive on its original stack
    >R >R >R
    EXECUTIVE @ EXECUTE
    R> R> R>

    -ROT FILE-CLOSE SWAP ! ;

\ Open a file using the given word as file-finder
: (OPEN-FILE) \ ( addr len opener -- fh )
    >R 2DUP R> EXECUTE
    DUP IF
	ROT 2DROP
    ELSE
	DROP TYPE SPACE S" not accessible" ABORT
    THEN ;

\ Load a file
: LOAD \ ( addr len -- )
    ['] FILE-OPEN (OPEN-FILE) (LOAD) ;

\ Include a file, making use of the include path
: INCLUDE \ ( addr len -- )
    ['] FILE-OPEN-INCLUDE (OPEN-FILE) (LOAD) ;

\ Load the next file from the input stream
: #INCLUDE \ ( "filename" -- )
    PARSE-WORD INCLUDE ; IMMEDIATE
